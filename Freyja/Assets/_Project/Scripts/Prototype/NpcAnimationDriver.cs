using Freyja.NpcAnimation;
using UnityEngine;

namespace Freyja.Prototype
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(IslandWaypointWalker))]
    public sealed class NpcAnimationDriver : MonoBehaviour
    {
        [SerializeField] private NpcAnimationProfile profile;
        [SerializeField] private IslandWaypointWalker walker;
        [SerializeField] private Animator animator;

        private int speedParameterHash;
        private bool ready;
        private bool errorReported;
        private bool animationOperationActive;

        public float EffectiveSpeed { get; private set; }
        public string InitializationError { get; private set; }

        private void Awake()
        {
            if (walker == null) walker = GetComponent<IslandWaypointWalker>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        private void OnEnable() => Initialize();

        private void OnDisable()
        {
            EffectiveSpeed = 0f;
            animationOperationActive = false;
        }

        private void Update()
        {
            if (!ready || animationOperationActive || walker == null || animator == null || !animator.isActiveAndEnabled) return;
            EffectiveSpeed = NpcAnimationProfile.ResolveLocomotionSpeed(
                walker.MoveStatus == WaypointMoveStatus.Running,
                walker.IsPathPending,
                walker.Velocity.magnitude,
                profile.StopThreshold);
            animator.SetFloat(speedParameterHash, EffectiveSpeed, profile.DampingSeconds, Time.deltaTime);
        }

        public void SetAnimationOperationActive(bool active)
        {
            animationOperationActive = active;
            if (ready && animator != null && animator.isActiveAndEnabled && active)
            {
                EffectiveSpeed = 0f;
                animator.SetFloat(speedParameterHash, 0f);
            }
        }

        private void Initialize()
        {
            ready = false;
            InitializationError = null;
            if (walker == null) { Fail("animation_walker_missing"); return; }
            if (animator == null) { Fail("animator_unavailable"); return; }

            // The walker/NavMeshAgent owns root translation during travel.
            animator.applyRootMotion = false;
            if (profile == null) profile = NpcAnimationProfile.LoadSharedDefault();
            if (profile == null) { Fail("animation_profile_missing"); return; }
            if (profile.MotionController != null)
                animator.runtimeAnimatorController = profile.MotionController;
            string validationError = profile.ValidateLocomotion(animator);
            if (validationError != null) { Fail(validationError); return; }

            speedParameterHash = Animator.StringToHash(profile.LocomotionSpeedParameter);
            ready = true;
            errorReported = false;
        }

        private void Fail(string reason)
        {
            InitializationError = reason;
            if (errorReported) return;
            errorReported = true;
            Debug.LogError("NPC animation driver: " + reason, this);
        }
    }
}
