using UnityEngine;
using UnityEngine.AI;

namespace Freyja.Prototype
{
    public enum WaypointMoveStatus { Unknown, Running, Succeeded, Failed, Cancelled }

    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class IslandWaypointWalker : MonoBehaviour
    {
        private static readonly int Walking = Animator.StringToHash("Walking");
        private static readonly int WalkRate = Animator.StringToHash("WalkRate");

        [SerializeField] private IslandWaypointNetwork network;
        [SerializeField] private Animator animator;
        [SerializeField] private RuntimeAnimatorController locomotionController;
        [SerializeField] private AnimationClip locomotionIdle;
        [SerializeField] private Transform destination;
        [SerializeField, Min(0.01f)] private float horizontalArrivalTolerance = 0.2f;
        [SerializeField, Min(0.01f)] private float verticalArrivalTolerance = 0.25f;
        [SerializeField, Min(0.1f)] private float noProgressTimeout = 4f;
        [SerializeField, Min(0.1f)] private float pathPendingTimeout = 4f;
        [SerializeField, Min(0.001f)] private float progressDistanceEpsilon = 0.03f;

        private NavMeshPath path;
        private NavMeshAgent agent;
        private RuntimeAnimatorController originalController;
        private AnimatorOverrideController walkingController;
        private bool animating;
        private float noProgressSeconds;
        private float pathPendingSeconds;
        private float elapsedSeconds;
        private float lastRemainingDistance;
        private long moveSequence;
        private long activeMoveId;
        private WaypointMoveStatus moveStatus;
        private Vector3 requestedPosition;
        private Vector3 effectivePosition;
        private Vector3 lastPosition;
        private Vector3 movementDirection;
        private NavMeshPathStatus lastPathStatus = NavMeshPathStatus.PathInvalid;
        private string lastReason = "none";

        public bool IsWalking => moveStatus == WaypointMoveStatus.Running;
        public IslandWaypointNetwork Network => network;
        public long ActiveOperationId => activeMoveId;
        public WaypointMoveStatus MoveStatus => moveStatus;
        public Vector3 RequestedPosition => requestedPosition;
        public Vector3 EffectivePosition => effectivePosition;
        public float HorizontalArrivalTolerance => horizontalArrivalTolerance;
        public float VerticalArrivalTolerance => verticalArrivalTolerance;
        public NavMeshPathStatus PathStatus => agent != null && agent.enabled && agent.isOnNavMesh
            ? agent.pathStatus
            : lastPathStatus;
        public bool IsPathPending => agent != null && agent.enabled && agent.pathPending;
        public Vector3[] PathCorners => agent != null && agent.enabled && agent.isOnNavMesh && agent.hasPath
            ? agent.path.corners
            : path != null ? path.corners : System.Array.Empty<Vector3>();
        public Vector3 Velocity => agent != null && agent.enabled && agent.isOnNavMesh
            ? agent.velocity
            : Vector3.zero;
        public Vector3 Direction => movementDirection;
        public float NoProgressSeconds => noProgressSeconds;
        public float ElapsedSeconds => elapsedSeconds;
        public string LastReason => lastReason;

        private void Awake()
        {
            path = new NavMeshPath();
            agent = GetComponent<NavMeshAgent>();
            if (agent.enabled)
                agent.enabled = false;
        }

        [ContextMenu("Caminhar até destino")]
        private void WalkToDestination() => TryMoveTo(destination);

        public bool TryMoveTo(Transform waypoint)
        {
            return TryBeginMove(waypoint, out _);
        }

        public bool TryBeginMove(Transform waypoint, out long operationId)
        {
            if (moveStatus == WaypointMoveStatus.Running)
                FinishMove(WaypointMoveStatus.Cancelled, "replaced_by_new_operation");

            operationId = ++moveSequence;
            activeMoveId = operationId;
            moveStatus = WaypointMoveStatus.Running;
            destination = waypoint;
            requestedPosition = waypoint != null ? waypoint.position : Vector3.zero;
            effectivePosition = Vector3.zero;
            lastPosition = transform.position;
            movementDirection = Vector3.zero;
            noProgressSeconds = 0f;
            pathPendingSeconds = 0f;
            elapsedSeconds = 0f;
            lastRemainingDistance = float.PositiveInfinity;
            lastReason = "none";
            lastPathStatus = NavMeshPathStatus.PathInvalid;

            if (!TryMoveToInternal(waypoint))
            {
                moveStatus = WaypointMoveStatus.Failed;
                StopAgent();
                return false;
            }

            return true;
        }

        public WaypointMoveStatus GetMoveStatus(long operationId)
        {
            return operationId == activeMoveId ? moveStatus : WaypointMoveStatus.Unknown;
        }

        public bool CancelMove(long operationId)
        {
            if (operationId != activeMoveId)
                return false;

            if (moveStatus == WaypointMoveStatus.Running)
                FinishMove(WaypointMoveStatus.Cancelled, "cancelled");

            return moveStatus == WaypointMoveStatus.Cancelled;
        }

        private bool TryMoveToInternal(Transform waypoint)
        {
            if (!isActiveAndEnabled || waypoint == null || agent == null ||
                !agent.gameObject.activeInHierarchy)
            {
                lastReason = waypoint == null ? "target_removed" : "agent_unavailable";
                return false;
            }

            if (network == null || !network.IsReady)
            {
                lastReason = "navigation_unavailable";
                return false;
            }

            if (!NavMesh.SamplePosition(waypoint.position, out var end, 0.2f, agent.areaMask) ||
                !NavMesh.SamplePosition(transform.position, out var start, 0.35f, agent.areaMask))
            {
                lastReason = "path_invalid";
                return false;
            }

            if (!NavMesh.CalculatePath(start.position, end.position, agent.areaMask, path))
            {
                lastPathStatus = path.status;
                lastReason = "path_invalid";
                return false;
            }

            lastPathStatus = path.status;
            if (path.status != NavMeshPathStatus.PathComplete)
            {
                lastReason = "path_incomplete";
                return false;
            }

            effectivePosition = end.position;
            agent.enabled = true;
            if (!agent.isOnNavMesh || !agent.SetPath(path))
            {
                lastReason = "agent_unavailable";
                return false;
            }

            agent.isStopped = false;
            lastRemainingDistance = agent.remainingDistance;
            lastPosition = transform.position;
            return true;
        }

        public void Stop()
        {
            if (moveStatus == WaypointMoveStatus.Running)
                CancelMove(activeMoveId);
            else
                StopAgent();
        }

        private void StopAgent()
        {
            if (agent != null && agent.enabled)
            {
                if (agent.isOnNavMesh)
                    agent.ResetPath();
                agent.enabled = false;
            }

            SetWalkingAnimation(false);
        }

        private void FinishMove(WaypointMoveStatus status, string reason)
        {
            moveStatus = status;
            lastReason = reason;
            if (agent != null && agent.enabled && agent.isOnNavMesh)
                lastPathStatus = agent.pathStatus;
            StopAgent();
        }

        private void OnDisable()
        {
            if (moveStatus == WaypointMoveStatus.Running)
                FinishMove(WaypointMoveStatus.Cancelled, "walker_disabled");
            else
                StopAgent();
        }

        private void OnDestroy()
        {
            if (walkingController == null)
                return;
            if (animator != null && animator.runtimeAnimatorController == walkingController)
                animator.runtimeAnimatorController = originalController;
            Destroy(walkingController);
        }

        private void Update()
        {
            if (moveStatus != WaypointMoveStatus.Running)
                return;

            // Count real runtime seconds while running; pause the watchdog when game time is paused.
            float deltaTime = Time.timeScale > 0f ? Time.unscaledDeltaTime : 0f;
            elapsedSeconds += deltaTime;
            if (destination == null)
            {
                FinishMove(WaypointMoveStatus.Failed, "target_removed");
                return;
            }

            if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            {
                FinishMove(WaypointMoveStatus.Failed, "agent_unavailable");
                return;
            }

            if (agent.pathPending)
            {
                pathPendingSeconds += deltaTime;
                if (pathPendingSeconds >= pathPendingTimeout)
                    FinishMove(WaypointMoveStatus.Failed, "path_pending_timeout");
                return;
            }

            pathPendingSeconds = 0f;
            lastPathStatus = agent.pathStatus;
            if (agent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                FinishMove(WaypointMoveStatus.Failed, "path_invalid");
                return;
            }

            Vector3 offset = transform.position - effectivePosition;
            float horizontalDistanceSquared = offset.x * offset.x + offset.z * offset.z;
            if (horizontalDistanceSquared <= horizontalArrivalTolerance * horizontalArrivalTolerance &&
                Mathf.Abs(offset.y) <= verticalArrivalTolerance)
            {
                FinishMove(WaypointMoveStatus.Succeeded, "arrived");
                return;
            }

            float remainingDistance = agent.remainingDistance;
            bool madeProgress = (transform.position - lastPosition).sqrMagnitude >= progressDistanceEpsilon * progressDistanceEpsilon ||
                lastRemainingDistance - remainingDistance >= progressDistanceEpsilon;
            if (madeProgress)
            {
                noProgressSeconds = 0f;
                lastRemainingDistance = remainingDistance;
                lastPosition = transform.position;
            }
            else
            {
                noProgressSeconds += deltaTime;
            }

            Vector3 velocity = agent.velocity;
            movementDirection = velocity.sqrMagnitude > 0.0025f
                ? velocity.normalized
                : Vector3.zero;
            SetWalkingAnimation(velocity.sqrMagnitude > 0.01f);

            if (noProgressSeconds >= noProgressTimeout)
                FinishMove(WaypointMoveStatus.Failed, "no_progress_timeout");
        }

        private void SetWalkingAnimation(bool walking)
        {
            if (animator == null || animating == walking)
                return;

            if (walking && locomotionController != null && walkingController == null)
            {
                originalController = animator.runtimeAnimatorController;
                if (originalController != null && locomotionIdle != null)
                {
                    var clips = originalController.animationClips;
                    if (clips.Length > 0)
                    {
                        walkingController = new AnimatorOverrideController(locomotionController);
                        walkingController[locomotionIdle] = clips[0];
                        animator.runtimeAnimatorController = walkingController;
                    }
                }
            }

            animating = walking;
            animator.SetBool(Walking, walking);
            if (agent != null)
                animator.SetFloat(WalkRate, agent.speed / 1.6f);
        }
    }
}
