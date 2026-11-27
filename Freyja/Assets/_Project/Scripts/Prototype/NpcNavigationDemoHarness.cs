using System;
using System.Collections.Generic;
using UnityEngine;

namespace Freyja.Prototype
{
    [Serializable]
    public struct NpcNavigationFixtureDiagnostic
    {
        public string NpcId;
        public long OperationId;
        public WaypointMoveStatus Status;
        public Vector3 RequestedPosition;
        public Vector3 EffectivePosition;
        public string PathState;
        public Vector3[] PathCorners;
        public Vector3 Velocity;
        public Vector3 Direction;
        public float NoProgressSeconds;
        public float ElapsedSeconds;
        public string LastReason;
    }

    /// <summary>
    /// Small, removable authoring harness. All movement remains owned by
    /// IslandWaypointWalker; this component only supplies configured test targets.
    /// </summary>
    public sealed class NpcNavigationDemoHarness : MonoBehaviour
    {
        [Serializable]
        private sealed class AgentFixture
        {
            public string npcId;
            public IslandWaypointWalker walker;
            public Transform destination;
            public Transform[] regionPoints;
            public Transform selectedApproachPoint;

            [NonSerialized] public int nextRegionPointIndex;
            [NonSerialized] public long operationId;
            [NonSerialized] public WaypointMoveStatus lastObservedStatus;
            [NonSerialized] public string harnessReason;
        }

        [SerializeField] private AgentFixture[] agents = new AgentFixture[3];
        [SerializeField, Min(0.05f)] private float regionMarkerRadius = 0.22f;
        [SerializeField, Min(0.05f)] private float approachMarkerRadius = 0.3f;

        [ContextMenu("Navigation Demo/Move to configured destinations")]
        private void MoveAllToDestinations()
        {
            if (!Application.isPlaying || agents == null)
                return;

            foreach (AgentFixture fixture in agents)
                TryMoveTo(fixture, fixture?.destination);
        }

        [ContextMenu("Navigation Demo/Move to next region point")]
        private void MoveAllToNextRegionPoints()
        {
            if (!Application.isPlaying || agents == null)
                return;

            foreach (AgentFixture fixture in agents)
                TryMoveToNextRegionPoint(fixture);
        }

        [ContextMenu("Navigation Demo/Move to selected approaches")]
        private void MoveAllToSelectedApproaches()
        {
            if (!Application.isPlaying || agents == null)
                return;

            foreach (AgentFixture fixture in agents)
                TryMoveTo(fixture, fixture?.selectedApproachPoint);
        }

        [ContextMenu("Navigation Demo/Cancel all movement")]
        private void CancelAllMovement()
        {
            if (!Application.isPlaying || agents == null)
                return;

            foreach (AgentFixture fixture in agents)
            {
                if (fixture?.walker == null || fixture.operationId == 0)
                    continue;

                if (fixture.walker.CancelMove(fixture.operationId))
                    ObserveStatusTransition(fixture);
            }
        }

        [ContextMenu("Navigation Demo/Reissue then cancel stale operation")]
        private void CancelAndReissueDestinations()
        {
            if (!Application.isPlaying || agents == null)
                return;

            foreach (AgentFixture fixture in agents)
            {
                if (fixture?.walker == null)
                    continue;

                long previousOperationId = fixture.operationId;
                TryMoveTo(fixture, fixture.destination);
                if (previousOperationId != 0)
                    fixture.walker.CancelMove(previousOperationId);
            }
        }

        private static bool TryMoveTo(AgentFixture fixture, Transform target)
        {
            if (fixture?.walker == null)
                return false;

            if (target == null)
            {
                fixture.harnessReason = "target_removed";
                return false;
            }

            fixture.harnessReason = "none";
            bool accepted = fixture.walker.TryBeginMove(target, out fixture.operationId);
            ObserveStatusTransition(fixture);
            return accepted;
        }

        private static bool TryMoveToNextRegionPoint(AgentFixture fixture)
        {
            if (fixture?.walker == null || fixture.regionPoints == null || fixture.regionPoints.Length == 0)
            {
                if (fixture != null)
                    fixture.harnessReason = "region_no_candidates";
                return false;
            }

            int start = Mathf.Clamp(fixture.nextRegionPointIndex, 0, fixture.regionPoints.Length - 1);
            for (int offset = 0; offset < fixture.regionPoints.Length; offset++)
            {
                int candidateIndex = (start + offset) % fixture.regionPoints.Length;
                Transform candidate = fixture.regionPoints[candidateIndex];
                if (candidate == null || !TryMoveTo(fixture, candidate))
                    continue;

                fixture.nextRegionPointIndex = (candidateIndex + 1) % fixture.regionPoints.Length;
                return true;
            }

            fixture.harnessReason = "region_no_reachable_candidate";
            return false;
        }

        /// <summary>Polls current operation state and logs each observed state change once.</summary>
        public NpcNavigationFixtureDiagnostic[] GetFixtureDiagnostics()
        {
            if (agents == null)
                return Array.Empty<NpcNavigationFixtureDiagnostic>();

            var diagnostics = new List<NpcNavigationFixtureDiagnostic>(agents.Length);
            foreach (AgentFixture fixture in agents)
            {
                if (fixture == null)
                    continue;

                WaypointMoveStatus status = fixture.walker != null && fixture.operationId != 0
                    ? fixture.walker.GetMoveStatus(fixture.operationId)
                    : WaypointMoveStatus.Unknown;
                ObserveStatusTransition(fixture, status);
                diagnostics.Add(new NpcNavigationFixtureDiagnostic
                {
                    NpcId = fixture.npcId,
                    OperationId = fixture.operationId,
                    Status = status,
                    RequestedPosition = fixture.walker != null ? fixture.walker.RequestedPosition : Vector3.zero,
                    EffectivePosition = fixture.walker != null ? fixture.walker.EffectivePosition : Vector3.zero,
                    PathState = fixture.walker != null
                        ? (fixture.walker.IsPathPending ? "pending" : fixture.walker.PathStatus.ToString())
                        : "unavailable",
                    PathCorners = fixture.walker != null ? fixture.walker.PathCorners : Array.Empty<Vector3>(),
                    Velocity = fixture.walker != null ? fixture.walker.Velocity : Vector3.zero,
                    Direction = fixture.walker != null ? fixture.walker.Direction : Vector3.zero,
                    NoProgressSeconds = fixture.walker != null ? fixture.walker.NoProgressSeconds : 0f,
                    ElapsedSeconds = fixture.walker != null ? fixture.walker.ElapsedSeconds : 0f,
                    LastReason = !string.IsNullOrEmpty(fixture.harnessReason) && fixture.harnessReason != "none"
                        ? fixture.harnessReason
                        : fixture.walker != null ? fixture.walker.LastReason : "walker_unavailable"
                });
            }

            return diagnostics.ToArray();
        }

        private static void ObserveStatusTransition(AgentFixture fixture)
        {
            WaypointMoveStatus status = fixture?.walker != null && fixture.operationId != 0
                ? fixture.walker.GetMoveStatus(fixture.operationId)
                : WaypointMoveStatus.Unknown;
            ObserveStatusTransition(fixture, status);
        }

        private static void ObserveStatusTransition(AgentFixture fixture, WaypointMoveStatus status)
        {
            if (fixture == null || fixture.lastObservedStatus == status)
                return;

            fixture.lastObservedStatus = status;
            Debug.Log($"[NpcNavigationDemo] {fixture.npcId} op={fixture.operationId} status={status} reason={fixture.walker?.LastReason ?? fixture.harnessReason}", fixture.walker);
        }

        private void OnDrawGizmosSelected()
        {
            if (agents == null)
                return;

            foreach (AgentFixture fixture in agents)
            {
                if (fixture == null)
                    continue;

                DrawPoint(fixture.destination, new Color(0.35f, 1f, 0.45f, 0.9f), regionMarkerRadius);
                DrawPoint(fixture.selectedApproachPoint, new Color(1f, 0.65f, 0.15f, 0.95f), approachMarkerRadius);
                if (fixture.walker != null)
                {
                    DrawPoint(fixture.walker.transform, Color.cyan,
                        Mathf.Max(fixture.walker.HorizontalArrivalTolerance, 0.05f));
                    DrawPointAt(fixture.walker.EffectivePosition, Color.white,
                        Mathf.Max(fixture.walker.VerticalArrivalTolerance, 0.05f));
                }

                if (fixture.regionPoints != null)
                {
                    foreach (Transform point in fixture.regionPoints)
                        DrawPoint(point, new Color(0.15f, 0.85f, 1f, 0.8f), regionMarkerRadius);
                }

                DrawCurrentPath(fixture.walker);
            }
        }

        private static void DrawPoint(Transform point, Color color, float radius)
        {
            if (point == null)
                return;

            Gizmos.color = color;
            Gizmos.DrawWireSphere(point.position, radius);
        }

        private static void DrawPointAt(Vector3 position, Color color, float radius)
        {
            Gizmos.color = color;
            Gizmos.DrawWireSphere(position, radius);
        }

        private static void DrawCurrentPath(IslandWaypointWalker walker)
        {
            if (walker == null)
                return;

            Vector3[] corners = walker.PathCorners;
            if (corners == null || corners.Length < 2)
                return;

            Gizmos.color = new Color(1f, 1f, 1f, 0.85f);
            for (int i = 1; i < corners.Length; i++)
                Gizmos.DrawLine(corners[i - 1], corners[i]);
        }
    }
}
