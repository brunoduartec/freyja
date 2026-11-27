using System;
using UnityEngine;

namespace Freyja.SmartObjects
{
    [DisallowMultipleComponent]
    public sealed class SmartObjectSeatedPose : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        private HumanPoseHandler handler;
        private float[] standingMuscles;
        private bool animatorWasEnabled;
        private bool seated;

        public bool IsSeated => seated;

        private void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        public bool Sit()
        {
            if (seated) return true;
            if (animator == null || animator.avatar == null || !animator.avatar.isHuman) return false;

            handler?.Dispose();
            handler = new HumanPoseHandler(animator.avatar, animator.transform);
            var pose = new HumanPose();
            handler.GetHumanPose(ref pose);
            standingMuscles = (float[])pose.muscles.Clone();
            animatorWasEnabled = animator.enabled;

            string[] names = HumanTrait.MuscleName;
            for (int i = 0; i < names.Length; i++)
            {
                switch (names[i])
                {
                    case "Left Upper Leg Front-Back":
                    case "Right Upper Leg Front-Back":
                        pose.muscles[i] = 0.8f;
                        break;
                    case "Left Lower Leg Stretch":
                    case "Right Lower Leg Stretch":
                        pose.muscles[i] = -0.8f;
                        break;
                }
            }

            handler.SetHumanPose(ref pose);
            animator.enabled = false;
            seated = true;
            return true;
        }

        public void StandUp()
        {
            if (!seated) return;
            if (handler != null && standingMuscles != null)
            {
                var pose = new HumanPose();
                handler.GetHumanPose(ref pose);
                Array.Copy(standingMuscles, pose.muscles, Math.Min(standingMuscles.Length, pose.muscles.Length));
                handler.SetHumanPose(ref pose);
            }
            if (animator != null) animator.enabled = animatorWasEnabled;
            seated = false;
        }

        private void OnDestroy()
        {
            StandUp();
            handler?.Dispose();
            handler = null;
        }
    }
}
