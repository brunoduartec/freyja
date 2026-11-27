using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Freyja.SmartObjects;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

namespace Freyja.SmartObjects.Tests
{
    public sealed class SmartObjectDemoTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private readonly List<GameObject> createdObjects = new List<GameObject>();
        private NavMeshData navMeshData;
        private Transform networkTransform;
        private Component waypointNetwork;
        private Type walkerType;
        private Type harnessType;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            SmartObjectRegistry.ResetForTests();
            walkerType = FindType("Freyja.Prototype.IslandWaypointWalker");
            harnessType = FindType("Freyja.SmartObjects.Demo.SmartObjectDemoHarness");
            Assert.That(walkerType, Is.Not.Null, "IslandWaypointWalker must be available from Assembly-CSharp.");
            Assert.That(harnessType, Is.Not.Null, "SmartObjectDemoHarness must be available from Assembly-CSharp.");

            navMeshData = BuildFlatNavMesh();
            Assert.That(navMeshData, Is.Not.Null, "The Play Mode fixture must build its navigation surface.");

            var networkObject = CreateObject("demo-test-waypoint-network");
            networkObject.SetActive(false);
            waypointNetwork = networkObject.AddComponent(FindType("Freyja.Prototype.IslandWaypointNetwork"));
            SetField(waypointNetwork, "navigation", navMeshData);
            networkObject.SetActive(true);
            networkTransform = waypointNetwork.transform;
            Assert.That(NavMesh.SamplePosition(Vector3.zero, out _, 0.35f, NavMesh.AllAreas), Is.True,
                "The existing IslandWaypointNetwork must register the fixture NavMesh.");
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (GameObject created in createdObjects)
                if (created != null) UnityEngine.Object.Destroy(created);
            createdObjects.Clear();
            yield return null;
            if (navMeshData != null) UnityEngine.Object.Destroy(navMeshData);
            SmartObjectRegistry.ResetForTests();
        }

        [UnityTest]
        public IEnumerator ThreeRequestsReserveBothBenchASlotsThenFallbackToBenchBAndOccupyAfterArrival()
        {
            SmartObject benchA = CreateBench("bench-a", new Vector3(3f, 0f, 0f), "left", "right");
            SmartObject benchB = CreateBench("bench-b", new Vector3(7f, 0f, 0f), "only", null);
            GameObject[] actors =
            {
                CreateActor("requester-1", new Vector3(0f, 0f, -3f)),
                CreateActor("requester-2", Vector3.zero),
                CreateActor("requester-3", new Vector3(0f, 0f, 3f))
            };
            Component harness = CreateHarness(actors);

            yield return null;

            Assert.That(GetState(harness, 0), Is.EqualTo("Moving"));
            Assert.That(GetState(harness, 1), Is.EqualTo("Moving"));
            Assert.That(GetState(harness, 2), Is.EqualTo("Moving"));
            Assert.That(CountSlotsInState(benchA, InteractionSlotState.Reserved), Is.EqualTo(2));
            Assert.That(CountSlotsInState(benchB, InteractionSlotState.Reserved), Is.EqualTo(1));
            Assert.That(CountSlotsInState(benchA, InteractionSlotState.Occupied), Is.Zero,
                "Slots remain reserved while agents are still approaching.");
            Assert.That(CountSlotsInState(benchB, InteractionSlotState.Occupied), Is.Zero);

            // Requester 3 takes the diagonal route to bench B at the fixture agent's
            // conservative 1 m/s speed. Allow the harness timeout (30 s) plus a margin.
            float deadline = Time.realtimeSinceStartup + 32f;
            while (Time.realtimeSinceStartup < deadline &&
                   (GetState(harness, 0) != "OccupiedAtPoseReference" ||
                    GetState(harness, 1) != "OccupiedAtPoseReference" ||
                    GetState(harness, 2) != "OccupiedAtPoseReference"))
                yield return null;

            Assert.That(GetState(harness, 0), Is.EqualTo("OccupiedAtPoseReference"));
            Assert.That(GetState(harness, 1), Is.EqualTo("OccupiedAtPoseReference"));
            Assert.That(GetState(harness, 2), Is.EqualTo("OccupiedAtPoseReference"));
            Assert.That(CountSlotsInState(benchA, InteractionSlotState.Occupied), Is.EqualTo(2));
            Assert.That(CountSlotsInState(benchB, InteractionSlotState.Occupied), Is.EqualTo(1));
            foreach (GameObject actor in actors)
                Assert.That(actor.GetComponent<SmartObjectReservationOwner>().ReservationCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator CancellingOneRequestReleasesOnlyItsOwnReservation()
        {
            SmartObject bench = CreateBench("cancel-bench", new Vector3(3f, 0f, 0f), "left", "right");
            GameObject[] actors =
            {
                CreateActor("cancelled-requester", Vector3.zero),
                CreateActor("continuing-requester", new Vector3(0f, 0f, 0.5f))
            };
            Component harness = CreateHarness(actors);

            yield return null;

            Assert.That(CountSlotsInState(bench, InteractionSlotState.Reserved), Is.EqualTo(2));
            Assert.That(Invoke<bool>(harness, "CancelRequest", 0), Is.True);
            Assert.That(GetState(harness, 0), Is.EqualTo("Cancelled"));
            Assert.That(actors[0].GetComponent<SmartObjectReservationOwner>().ReservationCount, Is.Zero);
            Assert.That(actors[1].GetComponent<SmartObjectReservationOwner>().ReservationCount, Is.EqualTo(1));
            Assert.That(CountSlotsInState(bench, InteractionSlotState.Available), Is.EqualTo(1));
            Assert.That(CountSlotsInState(bench, InteractionSlotState.Reserved), Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator UnreachableApproachReleasesItsReservation()
        {
            SmartObject bench = CreateBench("unreachable-bench", new Vector3(3f, 0f, 0f), "seat", null);
            GameObject actor = CreateActor("unreachable-requester", Vector3.zero);
            Component harness = CreateHarness(actor);
            var unreachableProbe = CreateObject("unreachable-probe");
            unreachableProbe.transform.SetParent(networkTransform, false);
            unreachableProbe.transform.position = new Vector3(100f, 0f, 0f);
            SetField(harness, "unreachableProbe", unreachableProbe.transform);

            yield return null;

            Assert.That(actor.GetComponent<SmartObjectReservationOwner>().ReservationCount, Is.EqualTo(1));
            Assert.That(Invoke<bool>(harness, "TestUnreachablePath", 0), Is.True);
            Assert.That(GetState(harness, 0), Is.EqualTo("UnreachableReleased"));
            Assert.That(actor.GetComponent<SmartObjectReservationOwner>().ReservationCount, Is.Zero);
            Assert.That(CountSlotsInState(bench, InteractionSlotState.Available), Is.EqualTo(1));
        }

        private GameObject CreateObject(string name)
        {
            var created = new GameObject(name);
            createdObjects.Add(created);
            return created;
        }

        private SmartObject CreateBench(string id, Vector3 position, string firstSlot, string secondSlot)
        {
            var bench = CreateObject(id);
            bench.SetActive(false);
            var slots = new List<InteractionSlot>();
            AddSlot(slots, firstSlot, position + new Vector3(0f, 0f, -1.1f), bench.transform);
            if (secondSlot != null)
                AddSlot(slots, secondSlot, position + new Vector3(0f, 0f, 1.1f), bench.transform);

            SmartObject smartObject = bench.AddComponent<SmartObject>();
            SetField(smartObject, "objectId", id);
            SetField(smartObject, "slots", slots);
            bench.transform.position = position;
            bench.SetActive(true);
            return smartObject;
        }

        private void AddSlot(List<InteractionSlot> slots, string id, Vector3 approachPosition, Transform smartObjectRoot)
        {
            Transform approach = CreateObject($"{id}-approach").transform;
            approach.SetParent(smartObjectRoot, true);
            approach.position = approachPosition;
            Transform pose = CreateObject($"{id}-pose").transform;
            pose.SetParent(smartObjectRoot, true);
            pose.position = approachPosition;

            var slot = new InteractionSlot();
            SetField(slot, "slotId", id);
            SetField(slot, "activityIds", new List<string> { "sit" });
            SetField(slot, "approach", approach);
            SetField(slot, "pose", pose);
            slots.Add(slot);
        }

        private GameObject CreateActor(string name, Vector3 position)
        {
            GameObject actor = CreateObject(name);
            actor.transform.position = position;
            actor.AddComponent<SmartObjectReservationOwner>();
            NavMeshAgent agent = actor.AddComponent<NavMeshAgent>();
            agent.speed = 1f;
            agent.stoppingDistance = 0.1f;
            Component walker = actor.AddComponent(walkerType);
            SetField(walker, "network", waypointNetwork);
            return actor;
        }

        private Component CreateHarness(params GameObject[] actors)
        {
            Component harness = CreateObject("smart-object-demo-harness").AddComponent(harnessType);
            Type requestType = harnessType.GetNestedType("AgentRequest", BindingFlags.Public);
            var requests = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(requestType));
            foreach (GameObject actor in actors)
            {
                object request = Activator.CreateInstance(requestType);
                SetField(request, "actor", actor);
                SetField(request, "timeoutSeconds", 30f);
                requests.Add(request);
            }
            SetField(harness, "requests", requests);
            return harness;
        }

        private NavMeshData BuildFlatNavMesh()
        {
            NavMeshBuildSettings settings = NavMesh.GetSettingsByID(0);
            var source = new NavMeshBuildSource
            {
                shape = NavMeshBuildSourceShape.Box,
                transform = Matrix4x4.TRS(new Vector3(0f, -0.1f, 0f), Quaternion.identity, Vector3.one),
                size = new Vector3(30f, 0.2f, 30f),
                area = 0
            };
            var bounds = new Bounds(new Vector3(0f, 0f, 0f), new Vector3(40f, 10f, 40f));
            return NavMeshBuilder.BuildNavMeshData(settings, new List<NavMeshBuildSource> { source }, bounds,
                Vector3.zero, Quaternion.identity);
        }

        private static int CountSlotsInState(SmartObject smartObject, InteractionSlotState state)
        {
            int count = 0;
            foreach (InteractionSlot slot in smartObject.Slots)
                if (slot.State == state) count++;
            return count;
        }

        private static string GetState(Component harness, int index) =>
            Invoke<object>(harness, "GetRequestState", index).ToString();

        private static T Invoke<T>(Component target, string method, params object[] arguments)
        {
            MethodInfo info = target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.Public);
            Assert.That(info, Is.Not.Null, $"Expected public harness method '{method}'.");
            return (T)info.Invoke(target, arguments);
        }

        private static Type FindType(string fullName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName);
                if (type != null) return type;
            }
            return null;
        }

        private static void SetField(object target, string name, object value)
        {
            FieldInfo field = target.GetType().GetField(name, PrivateInstance | BindingFlags.Public);
            Assert.That(field, Is.Not.Null, $"Expected serialized field '{name}' on {target.GetType().Name}.");
            field.SetValue(target, value);
        }
    }
}
