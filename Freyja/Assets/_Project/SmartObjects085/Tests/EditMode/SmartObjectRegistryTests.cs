using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Freyja.SmartObjects.Tests
{
    public sealed class SmartObjectRegistryTests
    {
        private readonly List<GameObject> created = new List<GameObject>();

        [SetUp] public void SetUp() => SmartObjectRegistry.ResetForTests();
        [TearDown]
        public void TearDown()
        {
            foreach (GameObject item in created) if (item != null) Object.DestroyImmediate(item);
            created.Clear();
            SmartObjectRegistry.ResetForTests();
        }

        [Test]
        public void RegistersAndFindsByIdRegionAndActivity()
        {
            SmartObject obj = Create("bench-a", "shore", "left", "sit", Vector3.zero);
            Assert.That(SmartObjectRegistry.TryGetById("bench-a", out SmartObject found), Is.True);
            Assert.That(found, Is.EqualTo(obj));
            Assert.That(SmartObjectRegistry.FindCandidates(Vector3.zero, "sit", "shore").Count, Is.EqualTo(1));
            Assert.That(SmartObjectRegistry.FindCandidates(Vector3.zero, "sit", "village").Count, Is.Zero);
            Assert.That(SmartObjectRegistry.FindCandidates(Vector3.zero, "fish").Count, Is.Zero);
        }

        [Test]
        public void OrdersByDistanceThenObjectIdThenLocalSlotId()
        {
            SmartObject a = Create("a", "", "z", "sit", new Vector3(2, 0, 0));
            AddSlot(a, "a", "sit");
            Create("b", "", "a", "sit", new Vector3(-2, 0, 0));
            Create("c", "", "a", "sit", new Vector3(1, 0, 0));
            var results = SmartObjectRegistry.FindCandidates(Vector3.zero, "sit");
            Assert.That(results.Count, Is.EqualTo(4));
            Assert.That(results[0].Object.ObjectId, Is.EqualTo("c"));
            Assert.That(results[1].Object.ObjectId, Is.EqualTo("a"));
            Assert.That(results[1].Slot.SlotId, Is.EqualTo("a"));
            Assert.That(results[2].Slot.SlotId, Is.EqualTo("z"));
            Assert.That(results[3].Object.ObjectId, Is.EqualTo("b"));
        }

        [Test]
        public void DuplicateIdsAreDiagnosedAndNeverReplaceLookup()
        {
            SmartObject first = Create("same", "", "one", "sit", Vector3.zero);
            LogAssert.Expect(LogType.Warning, "Duplicate SmartObject ID 'same' detected; registry lookup excludes this ID until repaired.");
            Create("same", "", "two", "sit", Vector3.one);
            Assert.That(SmartObjectRegistry.TryGetById("same", out _), Is.False);
            Assert.That(SmartObjectRegistry.FindCandidates(Vector3.zero, "sit").Count, Is.Zero);
            SmartObjectRegistry.Unregister(first);
            Assert.That(SmartObjectRegistry.TryGetById("same", out SmartObject remaining), Is.True);
            Assert.That(remaining, Is.Not.Null);
            Object.DestroyImmediate(first.gameObject);
        }

        [Test]
        public void DisabledObjectsAndSlotsAreNotOffered()
        {
            SmartObject obj = Create("one", "", "seat", "sit", Vector3.zero);
            obj.SetSlotEnabled("seat", false);
            Assert.That(SmartObjectRegistry.FindCandidates(Vector3.zero, "sit").Count, Is.Zero);
            obj.SetSlotEnabled("seat", true);
            obj.gameObject.SetActive(false);
            Assert.That(SmartObjectRegistry.FindCandidates(Vector3.zero, "sit").Count, Is.Zero);
        }

        [Test]
        public void DuplicateLocalSlotIdsAreExcludedFromDiscoveryAndReservation()
        {
            SmartObject obj = Create("duplicate-slots", "", "seat", "sit", Vector3.zero);
            AddSlot(obj, "seat", "sit");
            SmartObjectReservationOwner owner = CreateOwner("owner");

            Assert.That(SmartObjectRegistry.FindCandidates(Vector3.zero, "sit").Count, Is.Zero);
            Assert.That(SmartObjectRegistry.TryReserve(obj, "seat", "sit", owner, out _, out ReservationFailure failure), Is.False);
            Assert.That(failure, Is.EqualTo(ReservationFailure.InvalidConfiguration));
        }

        [Test]
        public void ThreeRequestsUseTwoNearSlotsThenFallBackToNextObject()
        {
            SmartObject benchA = Create("bench-a", "", "seat-left", "sit", Vector3.zero);
            AddSlot(benchA, "seat-right", "sit");
            SmartObject benchB = Create("bench-b", "", "seat-left", "sit", new Vector3(5f, 0f, 0f));
            AddSlot(benchB, "seat-right", "sit");
            SmartObjectReservationOwner[] owners =
            {
                CreateOwner("agent-a"), CreateOwner("agent-b"), CreateOwner("agent-c")
            };
            string[] objectIds = new string[owners.Length];

            for (int i = 0; i < owners.Length; i++)
            {
                var candidates = SmartObjectRegistry.FindCandidates(Vector3.zero, "sit");
                Assert.That(candidates.Count, Is.GreaterThan(0));
                SmartObjectCandidate candidate = candidates[0];
                Assert.That(SmartObjectRegistry.TryReserve(candidate.Object, candidate.Slot.SlotId,
                    candidate.ActivityId, owners[i], out _, out _), Is.True);
                objectIds[i] = candidate.Object.ObjectId;
            }

            Assert.That(objectIds, Is.EqualTo(new[] { "bench-a", "bench-a", "bench-b" }));
        }

        [Test]
        public void ExclusiveClaimCompatibilityOccupancyAndStaleHandles()
        {
            SmartObject obj = Create("one", "", "seat", "sit", Vector3.zero);
            SmartObjectReservationOwner owner = CreateOwner("owner-a");
            SmartObjectReservationOwner other = CreateOwner("owner-b");
            Assert.That(SmartObjectRegistry.TryReserve(obj, "seat", "fish", owner, out _, out ReservationFailure incompatible), Is.False);
            Assert.That(incompatible, Is.EqualTo(ReservationFailure.IncompatibleActivity));
            Assert.That(SmartObjectRegistry.TryReserve(obj, "seat", "sit", owner, out ReservationHandle old, out _), Is.True);
            Assert.That(SmartObjectRegistry.TryReserve(obj, "seat", "sit", other, out _, out ReservationFailure occupied), Is.False);
            Assert.That(occupied, Is.EqualTo(ReservationFailure.SlotUnavailable));
            Assert.That(other.Release(old), Is.False);
            Assert.That(old.TryOccupy(), Is.True);
            Assert.That(old.TryOccupy(), Is.False);
            Assert.That(owner.Release(old), Is.True);
            Assert.That(old.IsValid, Is.False);
            Assert.That(old.Release(), Is.False);
            Assert.That(SmartObjectRegistry.TryReserve(obj, "seat", "sit", other, out ReservationHandle fresh, out _), Is.True);
            Assert.That(old.Release(), Is.False);
            Assert.That(fresh.IsValid, Is.True);
        }

        [Test]
        public void MissingSlotAndInvalidOwnerAreRejected()
        {
            SmartObject obj = Create("one", "", "seat", "sit", Vector3.zero);
            SmartObjectReservationOwner owner = CreateOwner("owner");
            Assert.That(SmartObjectRegistry.TryReserve(obj, "missing", "sit", owner, out _, out ReservationFailure missing), Is.False);
            Assert.That(missing, Is.EqualTo(ReservationFailure.SlotNotFound));
            Assert.That(SmartObjectRegistry.TryReserve(obj, "seat", "sit", null, out _, out ReservationFailure invalid), Is.False);
            Assert.That(invalid, Is.EqualTo(ReservationFailure.InvalidInput));
        }

        [Test]
        public void SlotDisableInvalidatesItsCurrentHandle()
        {
            SmartObject obj = Create("one", "", "seat", "sit", Vector3.zero);
            SmartObjectReservationOwner owner = CreateOwner("owner");
            Assert.That(SmartObjectRegistry.TryReserve(obj, "seat", "sit", owner, out ReservationHandle handle, out _), Is.True);
            obj.SetSlotEnabled("seat", false);
            Assert.That(handle.IsValid, Is.False);
            Assert.That(owner.ReservationCount, Is.Zero);
            Assert.That(obj.Slots[0].State, Is.EqualTo(InteractionSlotState.Disabled));
        }

        [Test]
        public void EmptyObjectIdentityIsGeneratedOnce()
        {
            var go = new GameObject("generated-id");
            created.Add(go);
            SmartObject obj = go.AddComponent<SmartObject>();
            Assert.That(string.IsNullOrWhiteSpace(obj.ObjectId), Is.False);
            string stable = obj.ObjectId;
            go.transform.position = new Vector3(120f, 3f, -18f);
            obj.EnsureStableIds();
            Assert.That(obj.ObjectId, Is.EqualTo(stable));

            var other = new GameObject("other-generated-id");
            created.Add(other);
            SmartObject second = other.AddComponent<SmartObject>();
            Assert.That(second.ObjectId, Is.Not.EqualTo(stable));
        }

        private SmartObject Create(string id, string region, string slotId, string activity, Vector3 position)
        {
            var go = new GameObject(id + "-" + created.Count);
            created.Add(go);
            go.transform.position = position;
            go.SetActive(false);
            SmartObject obj = go.AddComponent<SmartObject>();
            var slot = new InteractionSlot();
            Set(slot, "slotId", slotId);
            Set(slot, "activityIds", new List<string> { activity });
            Set(slot, "approach", go.transform);
            Set(slot, "pose", go.transform);
            Set(obj, "objectId", id);
            Set(obj, "regionId", region);
            Set(obj, "slots", new List<InteractionSlot> { slot });
            go.SetActive(true);
            SmartObjectRegistry.Register(obj);
            return obj;
        }

        private SmartObjectReservationOwner CreateOwner(string name)
        {
            var go = new GameObject(name);
            created.Add(go);
            return go.AddComponent<SmartObjectReservationOwner>();
        }

        private static void AddSlot(SmartObject obj, string slotId, string activity)
        {
            var slot = new InteractionSlot();
            Set(slot, "slotId", slotId);
            Set(slot, "activityIds", new List<string> { activity });
            Set(slot, "approach", obj.transform);
            Set(slot, "pose", obj.transform);
            var slots = new List<InteractionSlot>(obj.Slots) { slot };
            Set(obj, "slots", slots);
        }

        private static void Set(object target, string field, object value)
        {
            target.GetType().GetField(field, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(target, value);
        }
    }
}
