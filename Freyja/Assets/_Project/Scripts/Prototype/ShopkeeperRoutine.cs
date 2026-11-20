using UnityEngine;

namespace Freyja.Prototype
{
    public sealed class ShopkeeperRoutine : MonoBehaviour
    {
        private static readonly int Walking = Animator.StringToHash("Walking");
        private static readonly int WalkRate = Animator.StringToHash("WalkRate");

        [SerializeField] private Animator animator;
        [SerializeField] private Vector3 counterEnd;
        [SerializeField, Min(0.1f)] private float speed = 0.85f;
        [SerializeField, Min(0f)] private float pauseSeconds = 3f;

        private Vector3 counterStart;
        private Quaternion restRotation;
        private float pauseRemaining;
        private bool towardEnd = true;

        private void Start()
        {
            if (animator == null || (counterEnd - transform.position).sqrMagnitude < 0.01f)
            {
                enabled = false;
                return;
            }

            counterStart = transform.position;
            restRotation = transform.rotation;
            pauseRemaining = pauseSeconds;
            animator.SetFloat(WalkRate, speed / 1.6f);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (pauseRemaining > 0f)
            {
                pauseRemaining -= dt;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, restRotation, 150f * dt);
                return;
            }

            Vector3 destination = towardEnd ? counterEnd : counterStart;
            Vector3 direction = Vector3.ProjectOnPlane(destination - transform.position, Vector3.up);
            if (direction.sqrMagnitude > 0.0001f)
            {
                Quaternion heading = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, heading, 150f * dt);
                if (Quaternion.Angle(transform.rotation, heading) > 12f)
                    return;
            }

            animator.SetBool(Walking, true);
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * dt);
            if (transform.position != destination)
                return;

            animator.SetBool(Walking, false);
            towardEnd = !towardEnd;
            pauseRemaining = pauseSeconds;
        }
    }
}
