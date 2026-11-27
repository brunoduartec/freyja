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

        private readonly RaycastHit[] hits = new RaycastHit[16];
        private NavMeshPath path;
        private NavMeshAgent agent;
        private RuntimeAnimatorController originalController;
        private AnimatorOverrideController walkingController;
        private bool animating;
        private float blockedTime;
        private long moveSequence;
        private long activeMoveId;
        private WaypointMoveStatus moveStatus;

        public bool IsWalking => agent != null && agent.enabled && agent.isOnNavMesh && agent.hasPath;
        public IslandWaypointNetwork Network => network;

        private void Awake()
        {
            path = new NavMeshPath();
            agent = GetComponent<NavMeshAgent>();
            agent.enabled = false;
        }

        [ContextMenu("Caminhar até destino")]
        private void WalkToDestination() => TryMoveTo(destination);

        public bool TryMoveTo(Transform waypoint)
        {
            return TryBeginMove(waypoint,out _);
        }

        public bool TryBeginMove(Transform waypoint,out long operationId)
        {
            operationId=++moveSequence;
            activeMoveId=operationId;
            moveStatus=WaypointMoveStatus.Running;
            if(!TryMoveToInternal(waypoint))
            {
                moveStatus=WaypointMoveStatus.Failed;
                operationId=activeMoveId;
                return false;
            }
            return true;
        }

        public WaypointMoveStatus GetMoveStatus(long operationId)
        {
            return operationId==activeMoveId?moveStatus:WaypointMoveStatus.Unknown;
        }

        public bool CancelMove(long operationId)
        {
            if(operationId!=activeMoveId)return false;
            if(moveStatus==WaypointMoveStatus.Running)FinishMove(WaypointMoveStatus.Cancelled);
            return true;
        }

        private bool TryMoveToInternal(Transform waypoint)
        {
            if (!isActiveAndEnabled || agent == null || network == null || !network.IsReady)
                return false;

            // Smart Object approach Transforms can live under their own prefabs, not under the waypoint network.
            // They are valid destinations when both endpoints sample onto the loaded NavMesh and a complete path exists.
            if (waypoint == null ||
                !NavMesh.SamplePosition(waypoint.position, out var end, 0.2f, agent.areaMask) ||
                !NavMesh.SamplePosition(transform.position, out var start, 0.35f, agent.areaMask) ||
                !NavMesh.CalculatePath(start.position, end.position, agent.areaMask, path) ||
                path.status != NavMeshPathStatus.PathComplete)
            {
                Stop();
                return false;
            }

            agent.enabled = true;
            if (!agent.isOnNavMesh || !agent.SetPath(path))
            {
                Stop();
                return false;
            }

            destination = waypoint;
            agent.isStopped = false;
            blockedTime = 0f;
            return true;
        }

        public void Stop()
        {
            if(moveStatus==WaypointMoveStatus.Running)moveStatus=WaypointMoveStatus.Cancelled;
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

        private void FinishMove(WaypointMoveStatus status)
        {
            moveStatus=status;
            StopAgent();
        }

        private void OnDisable() => Stop();

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
            if(moveStatus!=WaypointMoveStatus.Running)return;
            if(agent==null||!agent.enabled||!agent.isOnNavMesh)
            {
                FinishMove(WaypointMoveStatus.Failed);
                return;
            }
            if(agent.pathPending)
                return;

            if (!agent.hasPath || agent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                FinishMove(WaypointMoveStatus.Failed);
                return;
            }
            if(agent.remainingDistance<=agent.stoppingDistance+0.02f)
            {
                FinishMove(WaypointMoveStatus.Succeeded);
                return;
            }

            Vector3 direction = Vector3.ProjectOnPlane(agent.steeringTarget - transform.position, Vector3.up);
            bool blocked = IsObstructed(direction);
            agent.isStopped = blocked;
            blockedTime = blocked ? blockedTime + Time.deltaTime : 0f;
            SetWalkingAnimation(!blocked && agent.velocity.sqrMagnitude > 0.01f);
            if (blockedTime >= 2f)
                FinishMove(WaypointMoveStatus.Failed);
        }

        private bool IsObstructed(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.0025f)
                return false;

            float radius = agent.radius * 0.8f;
            Vector3 feet = transform.position;
            int count = Physics.CapsuleCastNonAlloc(feet + Vector3.up * 0.7f,
                feet + Vector3.up * (agent.height - radius), radius, direction.normalized,
                hits, 0.2f + agent.speed * Time.deltaTime, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                if (!hits[i].collider.transform.IsChildOf(transform) && hits[i].normal.y < 0.65f)
                    return true;
            }

            return false;
        }

        private void SetWalkingAnimation(bool walking)
        {
            if (animator == null || animating == walking)
                return;

            if (walking && locomotionController != null && walkingController == null)
            {
                originalController = animator.runtimeAnimatorController;
                var idle = originalController.animationClips[0];
                walkingController = new AnimatorOverrideController(locomotionController);
                walkingController[locomotionIdle] = idle;
                animator.runtimeAnimatorController = walkingController;
            }

            animating = walking;
            animator.SetBool(Walking, walking);
            animator.SetFloat(WalkRate, agent.speed / 1.6f);
        }
    }
}
