using UnityEngine;
using UnityEngine.AI;

namespace Freyja.Prototype
{
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

        public bool IsWalking => agent != null && agent.enabled && agent.isOnNavMesh && agent.hasPath;

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
            if (!isActiveAndEnabled || agent == null || network == null || !network.IsReady)
                return false;

            if (waypoint == null || !waypoint.IsChildOf(network.transform) ||
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
            if (agent != null && agent.enabled)
            {
                if (agent.isOnNavMesh)
                    agent.ResetPath();
                agent.enabled = false;
            }
            SetWalkingAnimation(false);
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
            if (agent == null || !agent.enabled || !agent.isOnNavMesh || agent.pathPending)
                return;

            if (!agent.hasPath || agent.pathStatus != NavMeshPathStatus.PathComplete ||
                agent.remainingDistance <= agent.stoppingDistance + 0.02f)
            {
                Stop();
                return;
            }

            Vector3 direction = Vector3.ProjectOnPlane(agent.steeringTarget - transform.position, Vector3.up);
            bool blocked = IsObstructed(direction);
            agent.isStopped = blocked;
            blockedTime = blocked ? blockedTime + Time.deltaTime : 0f;
            SetWalkingAnimation(!blocked && agent.velocity.sqrMagnitude > 0.01f);
            if (blockedTime >= 2f)
                Stop();
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
