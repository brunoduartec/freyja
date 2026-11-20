using UnityEngine;

namespace Freyja.Prototype
{
    [DefaultExecutionOrder(10)]
    public sealed class PlayerWalkAnimation : MonoBehaviour
    {
        private static readonly int Walking = Animator.StringToHash("Walking");
        private static readonly int WalkRate = Animator.StringToHash("WalkRate");

        private static readonly int StairAscent = Animator.StringToHash("Base Layer.StairAscent");
        private static readonly int Walk = Animator.StringToHash("Base Layer.Walk");
        private static readonly int Idle = Animator.StringToHash("Base Layer.Idle");
        private static readonly int Run = Animator.StringToHash("Base Layer.Run");
        private static readonly int Jump = Animator.StringToHash("Base Layer.Jump");

        [SerializeField] private CaisPlayerController player;
        [SerializeField] private Animator animator;
        [SerializeField, Min(0.1f)] private float referenceSpeed = 1.6f;

        private StairFootPlacement stairs;
        private bool climbing;
        private int traversalState;
        private bool hasRun;
        private bool hasJump;

        private void Awake()
        {
            stairs = animator.GetComponent<StairFootPlacement>();
            hasRun = animator.HasState(0, Run);
            hasJump = animator.HasState(0, Jump);
        }

        private void Update()
        {
            float speed = player.GroundSpeed;
            bool walking = speed > 0.15f;
            animator.SetBool(Walking, walking);
            animator.SetFloat(WalkRate, walking ? Mathf.Clamp(speed / referenceSpeed, 0.35f, 2f) : 1f);

            int nextTraversal = player.IsJumping && hasJump ? Jump : player.IsRunning && hasRun ? Run : 0;
            if (nextTraversal != 0)
            {
                if (nextTraversal != traversalState)
                    animator.CrossFadeInFixedTime(nextTraversal, 0.08f);
                climbing = false;
            }
            else if (stairs != null && stairs.TryGetStairAnimationTime(out float phase, out bool descending))
            {
                animator.Play(StairAscent, 0, descending ? Mathf.Repeat(1f - phase, 1f) : phase);
                climbing = true;
            }
            else if (climbing || traversalState != 0)
            {
                animator.CrossFadeInFixedTime(walking ? Walk : Idle, 0.16f);
                climbing = false;
            }
            traversalState = nextTraversal;
        }
    }
}
