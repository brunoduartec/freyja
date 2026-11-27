using System;
using System.Collections.Generic;
using System.IO;
using Freyja.NpcScheduling;
using Freyja.NpcScheduling.Execution;
using Freyja.NpcScheduling.Unity;
using Freyja.SmartObjects;
using UnityEngine;
using UnityEngine.AI;

namespace Freyja.Prototype
{
    /// <summary>Temporary test harness for npc-animation-system; not used by production scheduling.</summary>
    public sealed class NpcAnimationDemoHarness : MonoBehaviour, IActivityEventSink
    {
        [Serializable]
        private sealed class Fixture
        {
            public string npcId;
            public IslandWaypointWalker walker;
            public NpcUnityActivityPorts ports;
            public NpcAnimatorBindings animations;
            public SmartObjectReservationOwner owner;
            public Transform movementDestination;
        }

        [SerializeField] private NpcScheduleTextAssetProvider dataProvider;
        [SerializeField] private Fixture[] fixtures = Array.Empty<Fixture>();
        [SerializeField] private float runSpeed = 3f;
        [SerializeField] private bool runOnStart;

        private ActivityExecution[] executions = Array.Empty<ActivityExecution>();
        private Transform[] initialPositions = Array.Empty<Transform>();
        private GameTimeSnapshot demoTime = new GameTimeSnapshot(1, 0, 0);
        private IDictionary<string, ActivityRecipe> recipes;
        private string harnessStatus = "idle";
        private bool moveToPrimaryDestination = true;
        private bool movementDiagnosticReported;

        public string HarnessStatus => harnessStatus;

        public void ConfigureDataProvider(NpcScheduleTextAssetProvider provider)
        {
            dataProvider = provider;
        }

        private void Start()
        {
            initialPositions = new Transform[fixtures?.Length ?? 0];
            for (int i = 0; i < initialPositions.Length; i++)
            {
                IslandWaypointWalker walker = fixtures[i]?.walker;
                if (walker == null) continue;
                var target = new GameObject("NPC Animation Demo Return " + fixtures[i].npcId);
                target.hideFlags = HideFlags.HideAndDontSave;
                target.transform.position = walker.transform.position;
                initialPositions[i] = target.transform;
            }
            if (runOnStart) MoveAtRunSpeed();
        }

        private void OnDestroy()
        {
            for (int i = 0; i < initialPositions.Length; i++)
                if (initialPositions[i] != null) Destroy(initialPositions[i].gameObject);
        }

        private void Update()
        {
            bool allTerminal = executions.Length > 0;
            bool missingSittingBinding = false;
            bool residualOwnership = false;
            bool activityFailed = false;
            for (int i = 0; i < executions.Length; i++)
            {
                if (executions[i] == null) { allTerminal = false; continue; }
                ActivitySnapshot current = executions[i].Snapshot;
                if (executions[i].IsTerminal && current != null && current.HasResidualOwnership)
                {
                    bool cleanupCompleted = executions[i].RetryCleanup(Time.timeAsDouble, demoTime);
                    current = executions[i].Snapshot;
                    if (cleanupCompleted)
                        Debug.Log($"[NpcAnimation091] cleanup_completed npc={current.NpcId} exec={current.ExecutionId} resource={current.ResourceId ?? "-"}", this);
                }
                if (current != null && current.ReasonCode != null && current.ReasonCode.StartsWith("animation_binding_missing", StringComparison.Ordinal))
                    missingSittingBinding = true;
                bool expectedContenderRejection = IsExpectedContenderRejection(i, current);
                if (current != null && current.State == ActivityState.Failed && !expectedContenderRejection)
                    activityFailed = true;
                if (executions[i].IsTerminal && current != null && current.HasResidualOwnership)
                    residualOwnership = true;
                if (!executions[i].IsTerminal)
                {
                    ActivitySnapshot snapshot = executions[i].Tick(Time.timeAsDouble, demoTime);
                    if (snapshot != null && executions[i].IsTerminal)
                        Debug.Log($"[NpcAnimation091] npc={snapshot.NpcId} exec={snapshot.ExecutionId} state={snapshot.State} reason={snapshot.ReasonCode} residual={snapshot.ResidualOperationId} resource={snapshot.ResourceId}", this);
                }
                if (!executions[i].IsTerminal) allTerminal = false;
                ActivitySnapshot afterTick = executions[i].Snapshot;
                if (executions[i].IsTerminal && afterTick != null && afterTick.HasResidualOwnership)
                    residualOwnership = true;
                if (afterTick != null && afterTick.ReasonCode != null && afterTick.ReasonCode.StartsWith("animation_binding_missing", StringComparison.Ordinal))
                    missingSittingBinding = true;
                if (afterTick != null && afterTick.State == ActivityState.Failed && !IsExpectedContenderRejection(i, afterTick))
                {
                    activityFailed = true;
                    if (!movementDiagnosticReported && afterTick.ReasonCode == "path_unavailable")
                    {
                        IslandWaypointWalker failedWalker = i < fixtures.Length ? fixtures[i]?.walker : null;
                        Debug.LogError($"[NpcAnimation091] move_to diagnostic npc={afterTick.NpcId} networkReady={failedWalker?.Network?.IsReady} walkerReason={failedWalker?.LastReason} actorPosition={failedWalker?.transform.position}", this);
                        movementDiagnosticReported = true;
                    }
                }
            }
            if (allTerminal)
                harnessStatus = residualOwnership ? "cleanup_pending_resource_retained" :
                    missingSittingBinding ? "sitting_animation_binding_missing" :
                    activityFailed ? "activity_failed_cleanly" : "activity_lifecycle_settled";
        }

        private static bool IsExpectedContenderRejection(int fixtureIndex, ActivitySnapshot snapshot)
        {
            return fixtureIndex == 2 && snapshot != null && snapshot.State == ActivityState.Failed &&
                snapshot.ReasonCode == "target_unavailable";
        }

        [ContextMenu("NPC Animation 091/Move three NPCs at walk speed")]
        private void MoveAtWalkSpeed()
        {
            MoveAll(1.6f);
        }

        [ContextMenu("NPC Animation 091/Request run using real agent speed")]
        private void MoveAtRunSpeed()
        {
            MoveAll(Mathf.Max(runSpeed, 2.6f));
        }

        [ContextMenu("NPC Animation 091/Run two seats and third contender")]
        private void BeginSitLifecycle()
        {
            NpcScheduleTextAssetProvider provider = dataProvider != null
                ? dataProvider
                : GetComponent<NpcScheduleTextAssetProvider>();
            if (HasActiveOrResidualExecutions())
            {
                harnessStatus = "existing_activity_or_claim_must_finish";
                Debug.LogWarning("[NpcAnimation091] Refusing to replace executions while they are active or still own resources.", this);
                return;
            }
            if (fixtures == null || fixtures.Length < 3 || provider == null)
            {
                harnessStatus = "demo_setup_incomplete";
                Debug.LogError("[NpcAnimation091] Configure three fixtures and the existing schedule data provider.", this);
                return;
            }
            if (!provider.TryLoad(out _, out recipes, out string error))
            {
                harnessStatus = error ?? "schedule_data_unavailable";
                Debug.LogError("[NpcAnimation091] " + harnessStatus, this);
                return;
            }
            if (!recipes.TryGetValue("sit", out ActivityRecipe sitRecipe))
            {
                harnessStatus = "sit_recipe_missing";
                Debug.LogError("[NpcAnimation091] Existing sit recipe was not loaded.", this);
                return;
            }

            for (int i = 0; i < fixtures.Length; i++)
            {
                Fixture fixture = fixtures[i];
                if (fixture == null || fixture.walker == null || fixture.ports == null || fixture.animations == null || fixture.owner == null)
                {
                    harnessStatus = "fixture_dependency_missing";
                    Debug.LogError("[NpcAnimation091] Fixture " + i + " is incomplete.", this);
                    return;
                }
            }

            var nextExecutions = new ActivityExecution[fixtures.Length];
            for (int i = 0; i < fixtures.Length; i++)
            {
                Fixture fixture = fixtures[i];
                fixture.ports.Initialize(fixture.npcId, fixture.owner,
                    GetComponent<NpcLocationRegistry>(), fixture.animations, fixture.walker.transform);
                var intent = new ActivityIntent(fixture.npcId, "sit", "npc-animation-demo-region", 1, 0, 10, 1,
                    ScheduleOrigin.Base, "npc-animation-demo", "sit-test-" + i);
                nextExecutions[i] = new ActivityExecution("npc-animation-demo-exec-" + i, intent, sitRecipe,
                    false, fixture.ports, this);
            }
            executions = nextExecutions;
            movementDiagnosticReported = false;
            harnessStatus = "activity_core_running";
            Debug.Log("[NpcAnimation091] Started the actual sit recipe for two NPCs and a third contender using the shared Quaternius sitting bindings.", this);
        }

        [ContextMenu("NPC Animation 091/Advance schedule to end")]
        private void AdvanceScheduleToEnd()
        {
            demoTime = new GameTimeSnapshot(demoTime.AbsoluteDay, 10, 0);
            Debug.Log("[NpcAnimation091] Schedule clock advanced to the configured 00:10 end.", this);
        }

        [ContextMenu("NPC Animation 091/Cancel all activities")]
        private void CancelAllActivities()
        {
            for (int i = 0; i < executions.Length; i++) CancelExecution(i);
        }

        [ContextMenu("NPC Animation 091/Retry residual cleanup")]
        private void RetryResidualCleanup()
        {
            for (int i = 0; i < executions.Length; i++)
            {
                ActivityExecution execution = executions[i];
                ActivitySnapshot snapshot = execution?.Snapshot;
                if (execution == null || !execution.IsTerminal || snapshot == null || !snapshot.HasResidualOwnership) continue;
                bool released = execution.RetryCleanup(Time.timeAsDouble, demoTime);
                ActivitySnapshot after = execution.Snapshot;
                Debug.Log($"[NpcAnimation091] cleanup_retry npc={after.NpcId} exec={after.ExecutionId} released={released} residual={after.ResidualOperationId} resource={after.ResourceId} reason={after.ReasonCode}", this);
            }
        }

        public void Emit(ActivityExecutionEvent activityEvent)
        {
            if (activityEvent == null) return;
            Debug.Log($"[NpcAnimation091] event={activityEvent.Kind} npc={activityEvent.NpcId} exec={activityEvent.ExecutionId} step={activityEvent.Step} reason={activityEvent.ReasonCode}", this);
        }

        private void OnGUI()
        {
            int fixtureCount = fixtures?.Length ?? 0;
            GUI.Box(new Rect(12f, 12f, 550f, 58f + fixtureCount * 42f + 82f), "NPC Animation 091 | " + harnessStatus);
            if (fixtures == null) return;
            for (int i = 0; i < fixtures.Length; i++)
            {
                Fixture fixture = fixtures[i];
                if (fixture == null) continue;
                ActivitySnapshot snapshot = i < executions.Length && executions[i] != null ? executions[i].Snapshot : null;
                NpcAnimationDriver driver = fixture.walker != null ? fixture.walker.GetComponent<NpcAnimationDriver>() : null;
                string activity = snapshot == null ? "no activity" :
                    snapshot.State + " | " + (snapshot.CurrentOperation ?? "-") + " | " + (snapshot.ReasonCode ?? "-");
                string speed = fixture.walker != null ? fixture.walker.Velocity.magnitude.ToString("F2") : "-";
                string error = driver != null ? (driver.InitializationError ?? "ready") : "driver missing";
                GUI.Label(new Rect(24f, 34f + i * 42f, 445f, 38f),
                    $"{fixture.npcId} | v={speed} | {fixture.walker?.MoveStatus} | {activity} | {error}");
                if (GUI.Button(new Rect(468f, 40f + i * 42f, 78f, 26f), "Cancel")) CancelExecution(i);
            }
            float controlsY = 48f + fixtureCount * 42f;
            if (GUI.Button(new Rect(20f, controlsY, 160f, 24f), "Walk")) MoveAtWalkSpeed();
            if (GUI.Button(new Rect(190f, controlsY, 160f, 24f), "Run")) MoveAtRunSpeed();
            if (GUI.Button(new Rect(360f, controlsY, 176f, 24f), "Start sit lifecycle")) BeginSitLifecycle();
            if (GUI.Button(new Rect(20f, controlsY + 28f, 160f, 24f), "Schedule end")) AdvanceScheduleToEnd();
            if (GUI.Button(new Rect(190f, controlsY + 28f, 160f, 24f), "Cancel all")) CancelAllActivities();
            if (GUI.Button(new Rect(360f, controlsY + 28f, 176f, 24f), "Retry cleanup")) RetryResidualCleanup();
            if (GUI.Button(new Rect(20f, controlsY + 56f, 245f, 24f), "Capture locomotion evidence")) CaptureEvidence("locomotion");
            if (GUI.Button(new Rect(275f, controlsY + 56f, 261f, 24f), "Capture activity evidence")) CaptureEvidence("activity");
        }

        private void CancelExecution(int index)
        {
            if (index < 0 || index >= executions.Length || executions[index] == null || executions[index].IsTerminal) return;
            executions[index].Cancel("demo_cancel", Time.timeAsDouble, demoTime);
            ActivitySnapshot snapshot = executions[index].Snapshot;
            harnessStatus = snapshot != null && snapshot.HasResidualOwnership ? "cancelled_cleanup_pending" : "cancelled";
            if (snapshot != null)
                Debug.Log($"[NpcAnimation091] cancel npc={snapshot.NpcId} exec={snapshot.ExecutionId} state={snapshot.State} reason={snapshot.ReasonCode} residual={snapshot.ResidualOperationId} resource={snapshot.ResourceId}", this);
        }

        private bool HasActiveOrResidualExecutions()
        {
            foreach (ActivityExecution execution in executions)
            {
                if (execution == null) continue;
                ActivitySnapshot snapshot = execution.Snapshot;
                if (!execution.IsTerminal || snapshot != null && snapshot.HasResidualOwnership) return true;
            }
            return false;
        }

        private void MoveAll(float speed)
        {
            if (fixtures == null) return;
            for (int i = 0; i < fixtures.Length; i++)
            {
                Fixture fixture = fixtures[i];
                if (fixture == null || fixture.walker == null || fixture.movementDestination == null) continue;
                NavMeshAgent agent = fixture.walker.GetComponent<NavMeshAgent>();
                if (agent != null) agent.speed = speed;
                Transform destination = moveToPrimaryDestination ? fixture.movementDestination :
                    i < initialPositions.Length ? initialPositions[i] : null;
                if (destination == null) destination = fixture.movementDestination;
                bool started = fixture.walker.TryBeginMove(destination, out long id);
                Debug.Log($"[NpcAnimation091] move npc={fixture.npcId} op={id} speed={speed:F2} started={started} destination={(moveToPrimaryDestination ? "primary" : "initial")} reason={fixture.walker.LastReason}", fixture.walker);
            }
            moveToPrimaryDestination = !moveToPrimaryDestination;
            harnessStatus = speed >= 2.5f ? "run_requested" : "walk_requested";
        }

        private void CaptureEvidence(string label)
        {
            string projectPath = Directory.GetParent(Application.dataPath).FullName;
            string repositoryPath = Directory.GetParent(projectPath).FullName;
            string evidenceDirectory = Path.Combine(repositoryPath, "specs", "091-npc-animation-system", "evidence");
            Directory.CreateDirectory(evidenceDirectory);
            string path = Path.Combine(evidenceDirectory, "playmode-" + label + ".png");
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log("[NpcAnimation091] Screenshot queued: " + path, this);
        }
    }
}
