using System;
using System.Collections.Generic;
using UnityEngine;

namespace Freyja.NpcAnimation
{
    public enum NpcAnimationActionMode
    {
        Finite,
        Continuous
    }

    [Serializable]
    public sealed class NpcAnimationActionBinding
    {
        [SerializeField] private string semanticId;
        [SerializeField] private string stateFullPath;
        [SerializeField] private int layer;
        [SerializeField] private NpcAnimationActionMode mode;
        [SerializeField, Min(0f)] private float transitionSeconds = 0.1f;
        [SerializeField, Min(0.1f)] private float timeoutSeconds = 8f;

        public string SemanticId => semanticId;
        public string StateFullPath => stateFullPath;
        public int Layer => layer;
        public NpcAnimationActionMode Mode => mode;
        public float TransitionSeconds => transitionSeconds;
        public float TimeoutSeconds => timeoutSeconds;
    }

    [CreateAssetMenu(menuName = "Freyja/NPC Animation Profile", fileName = "NpcAnimationProfile")]
    public sealed class NpcAnimationProfile : ScriptableObject
    {
        private const string SharedResourcesPath = "NpcAnimation091/SharedResidentProfile";

        [SerializeField] private RuntimeAnimatorController motionController;
        [SerializeField] private AnimationClip idleClip;
        [SerializeField] private AnimationClip walkClip;
        [SerializeField] private AnimationClip runClip;
        [SerializeField] private string locomotionSpeedParameter = "Speed";
        [SerializeField, Min(0f)] private float stopThreshold = 0.05f;
        [SerializeField, Min(0f)] private float walkThreshold = 1.5f;
        [SerializeField, Min(0f)] private float runThreshold = 2.2f;
        [SerializeField, Min(0f)] private float dampingSeconds = 0.1f;
        [SerializeField] private string cancelStateFullPath = "Base Layer.Locomotion";
        [SerializeField] private NpcAnimationActionBinding[] actions = Array.Empty<NpcAnimationActionBinding>();

        private readonly Dictionary<string, NpcAnimationActionBinding> actionsById =
            new Dictionary<string, NpcAnimationActionBinding>(StringComparer.Ordinal);
        private readonly HashSet<string> duplicateIds = new HashSet<string>(StringComparer.Ordinal);
        private bool lookupBuilt;

        public RuntimeAnimatorController MotionController => motionController;
        public AnimationClip IdleClip => idleClip;
        public AnimationClip WalkClip => walkClip;
        public AnimationClip RunClip => runClip;
        public string LocomotionSpeedParameter => locomotionSpeedParameter;
        public float StopThreshold => stopThreshold;
        public float WalkThreshold => walkThreshold;
        public float RunThreshold => runThreshold;
        public float DampingSeconds => dampingSeconds;
        public string CancelStateFullPath => cancelStateFullPath;

        /// <summary>Returns the project's shared default profile for NPCs without an explicit override.</summary>
        public static NpcAnimationProfile LoadSharedDefault()
        {
            return Resources.Load<NpcAnimationProfile>(SharedResourcesPath);
        }

        public bool TryGetAction(string semanticId, out NpcAnimationActionBinding binding, out string reason)
        {
            BuildLookup();
            string key = semanticId ?? String.Empty;
            if (duplicateIds.Contains(key))
            {
                binding = null;
                reason = "animation_binding_duplicate";
                return false;
            }
            if (!actionsById.TryGetValue(key, out binding))
            {
                reason = "animation_binding_missing";
                return false;
            }
            reason = null;
            return true;
        }

        public string ValidateLocomotion(Animator animator)
        {
            if (animator == null || !animator.isActiveAndEnabled) return "animator_unavailable";
            if (animator.avatar == null || !animator.avatar.isValid || !animator.avatar.isHuman)
                return "animation_avatar_incompatible";
            if (motionController == null) return "animation_motion_controller_missing";
            if (idleClip == null) return "animation_idle_clip_missing";
            if (walkClip == null) return "animation_walk_clip_missing";
            if (runClip == null) return "animation_run_clip_missing";
            if (String.IsNullOrWhiteSpace(locomotionSpeedParameter)) return "animation_speed_parameter_missing";
            if (!IsFinite(stopThreshold) || !IsFinite(walkThreshold) || !IsFinite(runThreshold) || !IsFinite(dampingSeconds) ||
                stopThreshold < 0f || walkThreshold <= stopThreshold || runThreshold <= walkThreshold || dampingSeconds < 0f)
                return "animation_locomotion_thresholds_invalid";

            foreach (AnimatorControllerParameter parameter in animator.parameters)
                if (parameter.nameHash == Animator.StringToHash(locomotionSpeedParameter) &&
                    parameter.type == AnimatorControllerParameterType.Float) return null;
            return "animation_speed_parameter_invalid";
        }

        public static float ResolveLocomotionSpeed(bool routeActive, bool pathPending, float effectiveSpeed, float stopThreshold)
        {
            if (!routeActive || pathPending || !IsFinite(effectiveSpeed) || effectiveSpeed < 0f ||
                !IsFinite(stopThreshold) || effectiveSpeed <= Mathf.Max(0f, stopThreshold)) return 0f;
            return effectiveSpeed;
        }

        private void OnEnable()
        {
            lookupBuilt = false;
            BuildLookup();
        }

        private void OnValidate()
        {
            lookupBuilt = false;
            BuildLookup();
        }

        private void BuildLookup()
        {
            if (lookupBuilt) return;
            lookupBuilt = true;
            actionsById.Clear();
            duplicateIds.Clear();
            if (actions == null) return;

            foreach (NpcAnimationActionBinding action in actions)
            {
                if (action == null || String.IsNullOrWhiteSpace(action.SemanticId)) continue;
                string key = action.SemanticId;
                if (actionsById.ContainsKey(key) || duplicateIds.Contains(key))
                {
                    actionsById.Remove(key);
                    duplicateIds.Add(key);
                    continue;
                }
                actionsById.Add(key, action);
            }
        }

        private static bool IsFinite(float value) => !Single.IsNaN(value) && !Single.IsInfinity(value);
    }
}
