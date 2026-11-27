using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Freyja.NpcAnimation.Tests
{
    public sealed class NpcAnimationProfileTests
    {
        private NpcAnimationProfile profile;
        private GameObject actor;

        [SetUp]
        public void SetUp() => profile = ScriptableObject.CreateInstance<NpcAnimationProfile>();

        [TearDown]
        public void TearDown()
        {
            if (profile != null) Object.DestroyImmediate(profile);
            if (actor != null) Object.DestroyImmediate(actor);
        }

        [TestCase("sit_down")]
        [TestCase("sit_idle")]
        [TestCase("stand_up")]
        public void MissingSeatedActionFailsWithStableReason(string animationId)
        {
            Assert.That(profile.TryGetAction(animationId, out _, out string reason), Is.False);
            Assert.That(reason, Is.EqualTo("animation_binding_missing"));
        }

        [Test]
        public void DuplicateSemanticActionFailsValidation()
        {
            SetField(profile, "actions", new[] { CreateAction("sit_down"), CreateAction("sit_down") });
            SetField(profile, "lookupBuilt", false);

            Assert.That(profile.TryGetAction("sit_down", out _, out string reason), Is.False);
            Assert.That(reason, Is.EqualTo("animation_binding_duplicate"));
        }

        [Test]
        public void NonHumanoidOrMissingAvatarIsRejectedBeforeControllerUse()
        {
            actor = new GameObject("npc-animation-profile-avatar-test");
            Animator animator = actor.AddComponent<Animator>();
            Assert.That(profile.ValidateLocomotion(animator), Is.EqualTo("animation_avatar_incompatible"));
        }

        [TestCase(false, false, 1f, 0.05f, 0f)]
        [TestCase(true, true, 1f, 0.05f, 0f)]
        [TestCase(true, false, 0.04f, 0.05f, 0f)]
        [TestCase(true, false, 1.25f, 0.05f, 1.25f)]
        public void SpeedResolutionUsesActualVelocityAndIdleGate(bool routeActive, bool pathPending,
            float effectiveSpeed, float stopThreshold, float expected)
        {
            float actual = NpcAnimationProfile.ResolveLocomotionSpeed(routeActive, pathPending,
                effectiveSpeed, stopThreshold);
            Assert.That(actual, Is.EqualTo(expected).Within(0.0001f));
        }

        private static NpcAnimationActionBinding CreateAction(string id)
        {
            var action = new NpcAnimationActionBinding();
            SetField(action, "semanticId", id);
            SetField(action, "stateFullPath", "Base Layer." + id);
            return action;
        }

        private static void SetField(object target, string name, object value)
        {
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }
    }
}
