using System.Collections;
using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Freyja.SmartObjects.Tests
{
    public sealed class SmartObjectLifecycleTests
    {
        private GameObject target;
        private GameObject ownerObject;
        private GameObject actor;
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
            if (actor != null) Object.Destroy(actor);
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

        [UnityTest]
        public IEnumerator NpcGameObjectDisableClearsAdapterStateForReactivation()
        {
            actor = new GameObject("npc-lifecycle-owner");
            actor.SetActive(false);
            var actorOwner = actor.AddComponent<SmartObjectReservationOwner>();
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "Assembly-CSharp");
            Type portsType = assembly.GetType("Freyja.NpcScheduling.Unity.NpcUnityActivityPorts");
            Type controllerType = assembly.GetType("Freyja.NpcScheduling.Unity.NpcActivityController");
            Component ports = actor.AddComponent(portsType);
            Component controller = actor.AddComponent(controllerType);
            SetField(controller, "activityPorts", ports);

            actor.SetActive(true);
            Assert.That(ReserveFor(actorOwner, out ReservationHandle handle), Is.True);
            SeedAdapterClaim(ports, handle, actorOwner);
            actor.SetActive(false);

            Assert.That(handle.IsValid, Is.False);
            Assert.That(actorOwner.ReservationCount, Is.Zero);
            Assert.That(smartObject.Slots[0].State, Is.EqualTo(InteractionSlotState.Available));
            Assert.That(GetDictionaryCount(ports, "claims"), Is.Zero);
            Assert.That(GetDictionaryCount(ports, "operations"), Is.Zero);

            actor.SetActive(true);
            Assert.That((bool)controllerType.GetProperty("IsInitializationPending").GetValue(controller), Is.True);
            Assert.That((bool)controllerType.GetProperty("IsReady").GetValue(controller), Is.False);
            actor.SetActive(false);
            Object.Destroy(actor);
            actor = null;
            yield return null;
        }

        private bool Reserve(out ReservationHandle handle) => SmartObjectRegistry.TryReserve(smartObject, "seat", "sit", owner, out handle, out _);

        private bool ReserveFor(SmartObjectReservationOwner reservationOwner, out ReservationHandle handle) =>
            SmartObjectRegistry.TryReserve(smartObject, "seat", "sit", reservationOwner, out handle, out _);

        private static void SeedAdapterClaim(Component ports, ReservationHandle handle, SmartObjectReservationOwner reservationOwner)
        {
            Type claimType = ports.GetType().GetNestedType("Claim", BindingFlags.NonPublic);
            object claim = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(claimType);
            claimType.GetField("Handle").SetValue(claim, handle);
            claimType.GetField("Owner").SetValue(claim, reservationOwner);
            claimType.GetField("NpcId").SetValue(claim, "npc-test");
            claimType.GetField("ExecutionId").SetValue(claim, "exec-test");
            AddDictionaryEntry(ports, "claims", "claim-test", claim);

            Type operationType = ports.GetType().GetNestedType("Operation", BindingFlags.NonPublic);
            object operation = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(operationType);
            AddDictionaryEntry(ports, "operations", "operation-test", operation);
        }

        private static void AddDictionaryEntry(object target, string fieldName, string key, object value)
        {
            object dictionary = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
            dictionary.GetType().GetMethod("Add").Invoke(dictionary, new[] { (object)key, value });
        }

        private static int GetDictionaryCount(object target, string fieldName)
        {
            object dictionary = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
            return (int)dictionary.GetType().GetProperty("Count").GetValue(dictionary);
        }

        private static void SetField(object target, string name, object value) =>
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
