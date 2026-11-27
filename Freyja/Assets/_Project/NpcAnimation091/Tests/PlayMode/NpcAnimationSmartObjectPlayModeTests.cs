using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Freyja.SmartObjects;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Freyja.NpcAnimation.Tests
{
    public sealed class NpcAnimationSmartObjectPlayModeTests
    {
        private GameObject bankRoot;
        private GameObject[] owners;
        private SmartObject smartObject;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            bankRoot = new GameObject("npc-animation-test-bank");
            bankRoot.SetActive(false);
            smartObject = bankRoot.AddComponent<SmartObject>();
            SetField(smartObject, "objectId", "npc-animation-test-bank");
            SetField(smartObject, "regionId", "test-region");

            var slots = new List<InteractionSlot>();
            for (int i = 0; i < 2; i++)
            {
                var slot = new InteractionSlot();
                SetField(slot, "slotId", "seat-" + i);
                SetField(slot, "activityIds", new List<string> { "sit" });
                SetField(slot, "approach", CreatePoint("approach-" + i, i * 2f, 0f));
                SetField(slot, "pose", CreatePoint("pose-" + i, i * 2f, 1f));
                slots.Add(slot);
            }
            SetField(smartObject, "slots", slots);
            bankRoot.SetActive(true);

            owners = new GameObject[3];
            for (int i = 0; i < owners.Length; i++)
            {
                owners[i] = new GameObject("npc-animation-test-owner-" + i);
                owners[i].AddComponent<SmartObjectReservationOwner>();
            }
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (bankRoot != null) Object.Destroy(bankRoot);
            if (owners != null)
                foreach (GameObject owner in owners)
                    if (owner != null) Object.Destroy(owner);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TwoClaimsOccupyDistinctSlotsAndThirdClaimIsRejected()
        {
            IReadOnlyList<SmartObjectCandidate> candidates = SmartObjectRegistry.FindCandidates(
                Vector3.zero, "sit", "test-region");
            Assert.That(candidates.Count, Is.EqualTo(2));
            Assert.That(Reserve(candidates[0].Slot.SlotId, owners[0], out ReservationHandle first), Is.True);
            Assert.That(Reserve(candidates[1].Slot.SlotId, owners[1], out ReservationHandle second), Is.True);
            Assert.That(first.SlotId, Is.Not.EqualTo(second.SlotId));
            Assert.That(first.TryOccupy(), Is.True);
            Assert.That(second.TryOccupy(), Is.True);

            bool thirdReserved = Reserve(first.SlotId, owners[2], out _);
            Assert.That(thirdReserved, Is.False);
            Assert.That(SmartObjectRegistry.FindCandidates(Vector3.zero, "sit", "test-region"), Is.Empty);
            Assert.That(owners[0].GetComponent<SmartObjectReservationOwner>().ReservationCount, Is.EqualTo(1));
            Assert.That(owners[1].GetComponent<SmartObjectReservationOwner>().ReservationCount, Is.EqualTo(1));
            Assert.That(owners[2].GetComponent<SmartObjectReservationOwner>().ReservationCount, Is.Zero);
            yield return null;
        }

        private bool Reserve(string slotId, GameObject ownerObject, out ReservationHandle handle)
        {
            return SmartObjectRegistry.TryReserve(smartObject, slotId, "sit",
                ownerObject.GetComponent<SmartObjectReservationOwner>(), out handle, out _);
        }

        private Transform CreatePoint(string name, float x, float z)
        {
            var point = new GameObject(name);
            point.transform.SetParent(bankRoot.transform, false);
            point.transform.localPosition = new Vector3(x, 0f, z);
            return point.transform;
        }

        private static void SetField(object target, string name, object value)
        {
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }
    }
}
