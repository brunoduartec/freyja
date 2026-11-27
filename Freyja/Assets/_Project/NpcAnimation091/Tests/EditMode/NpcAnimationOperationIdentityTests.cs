using NUnit.Framework;

namespace Freyja.NpcAnimation.Tests
{
    public sealed class NpcAnimationOperationIdentityTests
    {
        [Test]
        public void StaleOperationCannotPollCompleteOrCancelReplacement()
        {
            object oldOperation = new object();
            object replacement = new object();
            var stale = new NpcAnimationOperationIdentity(oldOperation, "npc-a", "exec-001");
            var current = new NpcAnimationOperationIdentity(replacement, "npc-a", "exec-002");

            Assert.That(stale.IsCurrent(replacement, "npc-a", "exec-002"), Is.False);
            Assert.That(stale.TrySucceed(replacement, "npc-a", "exec-002"), Is.False);
            Assert.That(stale.TryCancel(replacement, "npc-a", "exec-002"), Is.False);
            Assert.That(current.State, Is.EqualTo(NpcAnimationOperationState.Running));
            Assert.That(current.IsCurrent(replacement, "npc-a", "exec-002"), Is.True);
        }

        [Test]
        public void CancelIsExactAndIdempotent()
        {
            object operation = new object();
            var identity = new NpcAnimationOperationIdentity(operation, "npc-a", "exec-001");

            Assert.That(identity.TryCancel(operation, "npc-b", "exec-001"), Is.False);
            Assert.That(identity.TryCancel(operation, "npc-a", "exec-001"), Is.True);
            Assert.That(identity.TryCancel(operation, "npc-a", "exec-001"), Is.True);
            Assert.That(identity.State, Is.EqualTo(NpcAnimationOperationState.Cancelled));
        }

        [Test]
        public void LateCancelOfCompletedTicketIsNoOp()
        {
            object operation = new object();
            var identity = new NpcAnimationOperationIdentity(operation, "npc-a", "exec-001");

            Assert.That(identity.TrySucceed(operation, "npc-a", "exec-001"), Is.True);
            Assert.That(identity.TryCancel(operation, "npc-a", "exec-001"), Is.True);
            Assert.That(identity.State, Is.EqualTo(NpcAnimationOperationState.Succeeded));
        }
    }
}
