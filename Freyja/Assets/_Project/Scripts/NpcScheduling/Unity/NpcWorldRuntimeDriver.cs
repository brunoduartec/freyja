using System;
using System.Collections.Generic;
using Freyja.Environment;
using Freyja.NpcScheduling.Execution;
using UnityEngine;

namespace Freyja.NpcScheduling.Unity
{
    [DisallowMultipleComponent]
    public sealed class NpcWorldRuntimeDriver : MonoBehaviour
    {
        [SerializeField] private CoastalDayNight clock;
        [SerializeField] private NpcActivityController[] hosts = Array.Empty<NpcActivityController>();
        [SerializeField] private string weather = "clear";
        [SerializeField, TextArea] private string runtimeStatus;

        private readonly List<NpcActivityController> activeHosts = new List<NpcActivityController>();
        private readonly HashSet<NpcActivityController> resumeAfterCleanup = new HashSet<NpcActivityController>();
        private readonly HashSet<string> reportedDiagnostics = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, int> hostIdCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly List<NpcActivityController> candidates = new List<NpcActivityController>();

        private WorldClockProjection projection;
        private WorldContextSnapshot worldContext;
        private GameTimeSnapshot currentTime;
        private bool hostsValidated;
        private bool deferHostValidation;
        private int lastStatusDay = -1;
        private int lastStatusMinute = -1;

        public bool IsReady => clock != null && hostsValidated && activeHosts.Count > 0;
        public GameTimeSnapshot CurrentGameTime => currentTime;
        public string RuntimeStatus => runtimeStatus;
#if UNITY_EDITOR
        public CoastalDayNight EditorClock => clock;
        public bool EditorHostsValidated => hostsValidated;
        public int EditorConfiguredHostCount => hosts == null ? 0 : hosts.Length;
        public int EditorInactiveHostCount
        {
            get
            {
                if (hosts == null) return 0;
                int count = 0;
                foreach (NpcActivityController host in hosts)
                    if (host == null || !host.isActiveAndEnabled) count++;
                return count;
            }
        }
        public int EditorRejectedHostCount => hostsValidated ? Math.Max(0, EditorConfiguredHostCount - EditorInactiveHostCount - activeHosts.Count) : 0;
        public NpcActivityController[] EditorGetHosts()
        {
            if (!hostsValidated) return Array.Empty<NpcActivityController>();
            var copy = new List<NpcActivityController>(activeHosts);
            copy.Sort((a, b) => string.Compare(a != null ? a.NpcId : null, b != null ? b.NpcId : null, StringComparison.Ordinal));
            return copy.ToArray();
        }
#endif

        private void OnEnable()
        {
            projection = new WorldClockProjection();
            worldContext = new WorldContextSnapshot(String.IsNullOrWhiteSpace(weather) ? "clear" : weather, null);
            currentTime = null;
            lastStatusDay = -1;
            lastStatusMinute = -1;
            runtimeStatus = String.Empty;
            activeHosts.Clear();
            candidates.Clear();
            hostIdCounts.Clear();
            reportedDiagnostics.Clear();
            hostsValidated = false;
            deferHostValidation = true;
        }

        private void Update()
        {
            double operationalSeconds = Time.timeAsDouble;
            if (!hostsValidated)
            {
                if (deferHostValidation)
                    deferHostValidation = false;
                else
                    ValidateHosts();
            }

            if (clock == null)
            {
                ReportOnce("missing_clock", "NPC world integration: missing CoastalDayNight reference.");
                return;
            }

            if (!projection.TryProject(clock.Hour, out GameTimeSnapshot time, out string clockError))
            {
                ReportOnce("clock:" + clockError, "NPC world integration clock: " + clockError + ". NPC schedules are not advanced until time is valid and monotonic.");
                return;
            }

            currentTime = time;
            UpdateStatus(time);
            for (int i = 0; i < activeHosts.Count; i++)
            {
                NpcActivityController host = activeHosts[i];
                if (host == null || !host.isActiveAndEnabled)
                    continue;
                if (!host.IsReady && host.IsInitializationPending)
                    continue;

                if (resumeAfterCleanup.Contains(host) && !host.HasWorldSnapshot)
                {
                    // A disabled/re-enabled NPC rebuilt its controller; its first current-world tick will resolve anew.
                    resumeAfterCleanup.Remove(host);
                }
                else if (resumeAfterCleanup.Contains(host))
                {
                    ActivitySnapshot snapshot = host.CurrentActivitySnapshot;
                    if (snapshot == null || !snapshot.HasResidualOwnership)
                    {
                        if (host.Resume(host.CurrentIntent))
                            resumeAfterCleanup.Remove(host);
                        else
                            ReportOnce("resume:" + host.NpcId, "NPC " + host.NpcId + " could not resume after interruption cleanup.");
                    }
                }

                if (!host.TickFromWorld(time, worldContext, operationalSeconds))
                    ReportOnce("tick:" + host.NpcId + ":" + host.InitializationError,
                        "NPC " + host.NpcId + " world tick failed: " + (host.InitializationError ?? "unknown_reason") + ".");
            }
        }

        private void OnDisable()
        {
            if (currentTime == null)
                return;

            double operationalSeconds = Time.timeAsDouble;
            for (int i = 0; i < activeHosts.Count; i++)
            {
                NpcActivityController host = activeHosts[i];
                if (host == null || !host.isActiveAndEnabled)
                    continue;

                ActivitySnapshot snapshot = host.CancelExternal("world_driver_disabled");
                if (snapshot == null)
                    continue;

                if (snapshot.HasResidualOwnership)
                    resumeAfterCleanup.Add(host);
                else
                    host.Resume(host.CurrentIntent);
            }
        }

        /// <summary>Exercises the existing cancel/cleanup/resume path without starting a parallel activity workflow.</summary>
        public bool InterruptNpcForValidation(string npcId)
        {
            if (String.IsNullOrWhiteSpace(npcId) || !hostsValidated || currentTime == null)
                return false;

            for (int i = 0; i < activeHosts.Count; i++)
            {
                NpcActivityController host = activeHosts[i];
                if (host == null || !String.Equals(host.NpcId, npcId, StringComparison.Ordinal))
                    continue;

                ActivitySnapshot snapshot = host.CancelExternal("validation_interruption");
                if (snapshot == null)
                    return false;

                if (snapshot.HasResidualOwnership)
                    resumeAfterCleanup.Add(host);
                else if (!host.Resume(host.CurrentIntent))
                    return false;

                Debug.Log("NPC integration interruption requested: npc=" + npcId + " execution=" +
                    (snapshot.ExecutionId ?? "-") + " state=" + snapshot.State + " residual=" + snapshot.HasResidualOwnership, this);
                return true;
            }

            return false;
        }

        private void ValidateHosts()
        {
            activeHosts.Clear();
            candidates.Clear();
            hostIdCounts.Clear();

            if (clock == null)
                ReportOnce("missing_clock", "NPC world integration: missing CoastalDayNight reference.");
            if (hosts == null || hosts.Length == 0)
            {
                ReportOnce("missing_hosts", "NPC world integration: no NPC hosts are configured.");
                hostsValidated = true;
                return;
            }

            var references = new HashSet<NpcActivityController>();
            for (int i = 0; i < hosts.Length; i++)
            {
                NpcActivityController host = hosts[i];
                if (host == null)
                {
                    ReportOnce("null_host:" + i, "NPC world integration: host reference at index " + i + " is missing.");
                    continue;
                }
                if (!references.Add(host))
                {
                    ReportOnce("duplicate_reference:" + i, "NPC world integration: the same controller is listed more than once at index " + i + ".");
                    continue;
                }
                if (!host.isActiveAndEnabled)
                {
                    ReportOnce("inactive_host:" + i, "NPC world integration: host at index " + i + " is inactive and will not be ticked.");
                    continue;
                }
                if (String.IsNullOrWhiteSpace(host.NpcId))
                {
                    ReportOnce("blank_id:" + i, "NPC world integration: host at index " + i + " has no stable NPC ID.");
                    continue;
                }

                hostIdCounts.TryGetValue(host.NpcId, out int count);
                hostIdCounts[host.NpcId] = count + 1;
                if (!host.IsReady)
                {
                    ReportOnce("not_ready:" + host.NpcId, "NPC " + host.NpcId + " host is not ready: " + (host.InitializationError ?? "initialization_pending") + ".");
                    continue;
                }

                candidates.Add(host);
            }

            foreach (KeyValuePair<string, int> pair in hostIdCounts)
                if (pair.Value > 1)
                    ReportOnce("duplicate_id:" + pair.Key, "NPC world integration: duplicate host ID '" + pair.Key + "'; every host with this ID is skipped.");

            for (int i = 0; i < candidates.Count; i++)
            {
                NpcActivityController host = candidates[i];
                if (hostIdCounts[host.NpcId] == 1)
                    activeHosts.Add(host);
            }

            hostsValidated = true;
            if (activeHosts.Count == 0)
                ReportOnce("no_valid_hosts", "NPC world integration has no valid, unique, initialized NPC hosts.");
            else
                runtimeStatus = "NPC world integration ready: " + activeHosts.Count + " unique host(s); waiting for first world tick.";
        }

        private void UpdateStatus(GameTimeSnapshot time)
        {
            if (lastStatusDay == time.AbsoluteDay && lastStatusMinute == time.MinuteOfDay)
                return;

            lastStatusDay = time.AbsoluteDay;
            lastStatusMinute = time.MinuteOfDay;
            runtimeStatus = "Day " + time.AbsoluteDay + " " + (time.MinuteOfDay / 60).ToString("D2") + ":" +
                (time.MinuteOfDay % 60).ToString("D2") + " | active NPC hosts " + activeHosts.Count + ".";
        }

        private void ReportOnce(string key, string message)
        {
            if (!reportedDiagnostics.Add(key))
                return;
            runtimeStatus = message;
            Debug.LogError(message, this);
        }
    }
}
