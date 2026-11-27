using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Freyja.SmartObjects.Tests
{
    public sealed class SmartObjectAuthoringTests
    {
        private const string TempFolder = "Assets/__SmartObjectAuthoringTests";
        private readonly List<GameObject> created = new List<GameObject>();

        [SetUp]
        public void SetUp()
        {
            SmartObjectRegistry.ResetForTests();
            if (!AssetDatabase.IsValidFolder(TempFolder))
                AssetDatabase.CreateFolder("Assets", "__SmartObjectAuthoringTests");
        }

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject item in created)
                if (item != null) Object.DestroyImmediate(item);
            created.Clear();
            AssetDatabase.DeleteAsset(TempFolder);
            SmartObjectRegistry.ResetForTests();
        }

        [Test]
        public void PrefabRoundTripKeepsObjectAndSlotIdsAndChildReferences()
        {
            GameObject root = new GameObject("BenchAuthoringFixture");
            created.Add(root);
            Transform approachA = Child(root.transform, "Approach_A");
            Transform poseA = Child(root.transform, "Pose_A");
            Transform approachB = Child(root.transform, "Approach_B");
            Transform poseB = Child(root.transform, "Pose_B");
            SmartObject smartObject = root.AddComponent<SmartObject>();
            Set(smartObject, "slots", new List<InteractionSlot>
            {
                Slot("seat-left", approachA, poseA),
                Slot("seat-right", approachB, poseB)
            });
            string objectId = smartObject.ObjectId;

            const string prefabPath = TempFolder + "/BenchFixture.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Assert.That(prefab, Is.Not.Null);

            GameObject reloaded = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.That(reloaded, Is.Not.Null);
            SmartObject saved = reloaded.GetComponent<SmartObject>();
            Assert.That(saved.ObjectId, Is.EqualTo(objectId));
            Assert.That(saved.Slots.Count, Is.EqualTo(2));
            Assert.That(saved.Slots[0].SlotId, Is.EqualTo("seat-left"));
            Assert.That(saved.Slots[1].SlotId, Is.EqualTo("seat-right"));
            Assert.That(saved.Slots[0].Approach, Is.EqualTo(reloaded.transform.Find("Approach_A")));
            Assert.That(saved.Slots[0].Pose, Is.EqualTo(reloaded.transform.Find("Pose_A")));
            Assert.That(saved.Slots[1].Approach, Is.EqualTo(reloaded.transform.Find("Approach_B")));
            Assert.That(saved.Slots[1].Pose, Is.EqualTo(reloaded.transform.Find("Pose_B")));
            Assert.That(saved.Slots[0].SupportsActivity("sit"), Is.True);
        }

        [Test]
        public void EmptySlotIdIsGeneratedOnceAndPersistsAcrossValidation()
        {
            var root = new GameObject("GeneratedSlotFixture");
            root.SetActive(false);
            created.Add(root);
            Transform approach = Child(root.transform, "Approach");
            Transform pose = Child(root.transform, "Pose");
            SmartObject smartObject = root.AddComponent<SmartObject>();
            Set(smartObject, "slots", new List<InteractionSlot> { Slot("", approach, pose) });

            smartObject.EnsureStableIds();
            string id = smartObject.Slots[0].SlotId;
            smartObject.EnsureStableIds();

            Assert.That(string.IsNullOrWhiteSpace(id), Is.False);
            Assert.That(smartObject.Slots[0].SlotId, Is.EqualTo(id));
        }

        private static Transform Child(Transform parent, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child.transform;
        }

        private static InteractionSlot Slot(string id, Transform approach, Transform pose)
        {
            var slot = new InteractionSlot();
            Set(slot, "slotId", id);
            Set(slot, "activityIds", new List<string> { "sit" });
            Set(slot, "approach", approach);
            Set(slot, "pose", pose);
            return slot;
        }

        private static void Set(object target, string field, object value) =>
            target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
