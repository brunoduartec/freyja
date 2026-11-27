using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Freyja.SmartObjects.Tests
{
    public sealed class SmartObjectLifecycleTests
    {
        private GameObject target;
        private GameObject ownerObject;
        private SmartObject smartObject;
        private SmartObjectReservationOwner owner;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            SmartObjectRegistry.ResetForTests();
            target = new GameObject("lifecycle-object");
            target.SetActive(false);
            smartObject = target.AddComponent<SmartObject>();
            var slot = new InteractionSlot();
            typeof(InteractionSlot).GetField("slotId", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(slot, "seat");
            typeof(InteractionSlot).GetField("activityIds", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(slot, new System.Collections.Generic.List<string> { "sit" });
            typeof(InteractionSlot).GetField("approach", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(slot, target.transform);
            typeof(InteractionSlot).GetField("pose", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(slot, target.transform);
            typeof(SmartObject).GetField("objectId", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(smartObject, "lifecycle");
            typeof(SmartObject).GetField("slots", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(smartObject, new System.Collections.Generic.List<InteractionSlot> { slot });
            target.SetActive(true);
            ownerObject = new GameObject("lifecycle-owner");
            owner = ownerObject.AddComponent<SmartObjectReservationOwner>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (target != null) Object.Destroy(target);
            if (ownerObject != null) Object.Destroy(ownerObject);
            yield return null;
            SmartObjectRegistry.ResetForTests();
        }

        [UnityTest]
        public IEnumerator ObjectDisableReactivationAndDestroyClearClaims()
        {
            Assert.That(Reserve(out ReservationHandle first), Is.True);
            target.SetActive(false);
            Assert.That(first.IsValid, Is.False);
            Assert.That(owner.ReservationCount, Is.Zero);
            target.SetActive(true);
            Assert.That(Reserve(out ReservationHandle second), Is.True);
            Object.Destroy(target);
            yield return null;
            Assert.That(second.IsValid, Is.False);
            Assert.That(owner.ReservationCount, Is.Zero);
        }

        [UnityTest]
        public IEnumerator OwnerDisableAndDestroyReleaseOnlyOwnedClaim()
        {
            Assert.That(Reserve(out ReservationHandle handle), Is.True);
            ownerObject.SetActive(false);
            Assert.That(handle.IsValid, Is.False);
            Assert.That(smartObject.Slots[0].State, Is.EqualTo(InteractionSlotState.Available));
            ownerObject.SetActive(true);
            Assert.That(Reserve(out ReservationHandle second), Is.True);
            Object.Destroy(ownerObject);
            yield return null;
            Assert.That(second.IsValid, Is.False);
            Assert.That(smartObject.Slots[0].State, Is.EqualTo(InteractionSlotState.Available));
        }

        [UnityTest]
        public IEnumerator SubsystemResetInvalidatesOldHandles()
        {
            Assert.That(Reserve(out ReservationHandle handle), Is.True);
            SmartObjectRegistry.ResetForTests();
            Assert.That(handle.IsValid, Is.False);
            Assert.That(owner.ReservationCount, Is.Zero);
            yield return null;
        }

        private bool Reserve(out ReservationHandle handle) => SmartObjectRegistry.TryReserve(smartObject, "seat", "sit", owner, out handle, out _);
    }
}
