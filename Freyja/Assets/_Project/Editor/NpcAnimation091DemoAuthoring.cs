using System;
using System.Collections.Generic;
using Freyja.NpcAnimation;
using Freyja.NpcScheduling.Unity;
using Freyja.Prototype;
using Freyja.SmartObjects;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace Freyja.Editor
{
    internal static class NpcAnimation091DemoAuthoring
    {
        private const string ScenePath = "Assets/_Project/Scenes/NpcAnimation_Demo.unity";
        private const string NavMeshPath = "Assets/_Project/Art/NpcAnimation091/NpcAnimation_Demo_NavMesh.asset";
        private const string FloorMaterialPath = "Assets/_Project/Art/NpcAnimation091/DemoFloor.mat";
        private const string ProfilePath = "Assets/_Project/Resources/NpcAnimation091/SharedResidentProfile.asset";
        private const string ControllerPath = "Assets/_Project/Art/NpcAnimation091/SharedResident.controller";

        [MenuItem("Freyja/NPC Animation 091/Create Three Resident Demo Scene")]
        private static void CreateDemoScene()
        {
            NpcAnimationProfile profile = AssetDatabase.LoadAssetAtPath<NpcAnimationProfile>(ProfilePath);
            RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
            if (profile == null || controller == null || profile.MotionController != controller ||
                profile.IdleClip == null || profile.WalkClip == null || profile.RunClip == null ||
                !profile.TryGetAction("sit_down", out _, out _) ||
                !profile.TryGetAction("sit_idle", out _, out _) ||
                !profile.TryGetAction("stand_up", out _, out _))
            {
                Debug.LogError("Create shared animation assets first and verify the profile has idle/walk/run/sit_down/sit_idle/stand_up bindings.");
                return;
            }
            if (!AssetDatabase.Contains(profile))
            {
                Debug.LogError("NPC Animation 091: the shared profile must be a saved project asset before the demo scene can reference it.");
                return;
            }

            Scene previousScene = SceneManager.GetActiveScene();
            Scene existingDemo = SceneManager.GetSceneByPath(ScenePath);
            NewSceneMode newSceneMode = NewSceneMode.Additive;
            if (existingDemo.IsValid() && existingDemo.isLoaded)
            {
                if (existingDemo.isDirty && !EditorSceneManager.SaveScene(existingDemo))
                {
                    Debug.LogError("NPC Animation 091: could not save the open demo scene before regenerating it.");
                    return;
                }

                Scene otherLoadedScene = default;
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene candidate = SceneManager.GetSceneAt(i);
                    if (candidate.IsValid() && candidate.isLoaded && candidate != existingDemo)
                    {
                        otherLoadedScene = candidate;
                        break;
                    }
                }

                if (otherLoadedScene.IsValid())
                {
                    if (previousScene == existingDemo)
                    {
                        SceneManager.SetActiveScene(otherLoadedScene);
                        previousScene = otherLoadedScene;
                    }
                    if (!EditorSceneManager.CloseScene(existingDemo, true))
                    {
                        Debug.LogError("NPC Animation 091: could not close the previous demo scene before regenerating it.");
                        return;
                    }
                }
                else
                {
                    // Unity cannot close the only loaded scene; replace it with the new isolated demo instead.
                    newSceneMode = NewSceneMode.Single;
                    previousScene = default;
                }
            }
            else if (SceneManager.sceneCount == 1 && previousScene.IsValid() &&
                     string.IsNullOrEmpty(previousScene.path) && IsGeneratedDemoScene(previousScene))
            {
                // Recover only the known temporary scene produced by this feature's prior failed save.
                newSceneMode = NewSceneMode.Single;
                previousScene = default;
            }

            Scene demoScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, newSceneMode);
            try
            {
                GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
                MoveToScene(demoScene, floor);
                floor.name = "Demo NavMesh Floor";
                floor.transform.localScale = new Vector3(2f, 1f, 2f);
                Shader floorShader = Shader.Find("Standard");
                if (floorShader == null)
                {
                    Debug.LogError("NPC Animation 091: Built-in Standard shader is unavailable.");
                    return;
                }
                Material floorMaterial = AssetDatabase.LoadAssetAtPath<Material>(FloorMaterialPath);
                if (floorMaterial == null)
                {
                    floorMaterial = new Material(floorShader) { name = "NPC Animation Demo Floor Material" };
                    AssetDatabase.CreateAsset(floorMaterial, FloorMaterialPath);
                    floorMaterial = AssetDatabase.LoadAssetAtPath<Material>(FloorMaterialPath);
                }
                floorMaterial.shader = floorShader;
                floorMaterial.color = new Color(0.29f, 0.34f, 0.28f);
                EditorUtility.SetDirty(floorMaterial);
                floor.GetComponent<Renderer>().sharedMaterial = floorMaterial;
                EditorUtility.SetDirty(floor);

                GameObject networkRoot = new GameObject("NPC Animation Demo Navigation");
                MoveToScene(demoScene, networkRoot);
                var network = networkRoot.AddComponent<IslandWaypointNetwork>();
                Transform destinationA = CreateChild(networkRoot.transform, "Destination_Left", new Vector3(4f, 0f, -3f));
                Transform destinationB = CreateChild(networkRoot.transform, "Destination_Center", new Vector3(0f, 0f, -4f));
                Transform destinationC = CreateChild(networkRoot.transform, "Destination_Right", new Vector3(-4f, 0f, -3f));
                NavMeshData navMesh = BuildAndSaveNavMesh(floor);
                if (navMesh == null) return;
                var networkObject = new SerializedObject(network);
                SerializedProperty navigationProperty = networkObject.FindProperty("navigation");
                navigationProperty.objectReferenceValue = navMesh;
                networkObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(network);
                if (navigationProperty.objectReferenceValue == null)
                {
                    Debug.LogError("NPC Animation 091: the generated NavMesh asset could not be assigned to the scene network.");
                    return;
                }

                GameObject benchPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/_Project/Prefabs/SmartObjects085/BenchProvisional.prefab");
                GameObject bench = PrefabUtility.InstantiatePrefab(benchPrefab, demoScene) as GameObject;
                if (bench == null)
                {
                    Debug.LogError("BenchProvisional prefab is missing.");
                    return;
                }
                bench.name = "Two Seat Test Bench";
                bench.transform.SetPositionAndRotation(new Vector3(0f, 0f, 2f), Quaternion.identity);
                bench.transform.localScale = new Vector3(1f, 0.6f, 1f);
                ConfigureBenchGeometry(bench);
                ConfigureBenchSeatPoints(bench);
                SmartObject smartObject = bench.GetComponent<SmartObject>();
                var smartObjectData = new SerializedObject(smartObject);
                smartObjectData.FindProperty("objectId").stringValue = "npc-animation-demo-bench";
                smartObjectData.FindProperty("regionId").stringValue = "npc-animation-demo-region";
                smartObjectData.ApplyModifiedPropertiesWithoutUndo();

                GameObject providerObject = new GameObject("NPC Animation Demo Runtime");
                MoveToScene(demoScene, providerObject);
                var provider = providerObject.AddComponent<NpcScheduleTextAssetProvider>();
                SetTextAsset(provider, "activityCatalog", "Assets/_Project/NpcScheduling/Data/activities.json");
                SetTextArray(provider, "templates", "Assets/_Project/NpcScheduling/Data/templates/resident.json");
                SetTextArray(provider, "npcSchedules", "Assets/_Project/NpcScheduling/Data/npcs/demo_resident.json");
                SetTextArray(provider, "overrides");
                SetTextAsset(provider, "activityRecipes", "Assets/_Project/NpcScheduling/Data/execution/recipes.json");

                var locationRegistry = providerObject.AddComponent<NpcLocationRegistry>();
                var locationData = new SerializedObject(locationRegistry);
                locationData.FindProperty("waypointNetwork").objectReferenceValue = network;
                locationData.FindProperty("points").arraySize = 0;
                locationData.ApplyModifiedPropertiesWithoutUndo();

                string[] prefabPaths =
                {
                    "Assets/_Project/Prefabs/Moradores060/NunoPires.prefab",
                    "Assets/_Project/Prefabs/Moradores060/AmeliaFaria.prefab",
                    "Assets/_Project/Prefabs/Moradores060/MauroNeves.prefab"
                };
                Vector3[] starts = { new Vector3(-4f, 0f, -4f), new Vector3(0f, 0f, -5f), new Vector3(4f, 0f, -4f) };
                Transform[] destinations = { destinationA, destinationB, destinationC };
                var fixtureActors = new List<GameObject>();
                for (int i = 0; i < prefabPaths.Length; i++)
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPaths[i]);
                    GameObject visual = PrefabUtility.InstantiatePrefab(prefab, demoScene) as GameObject;
                    if (prefab == null || visual == null)
                    {
                        Debug.LogError("Resident prefab is missing: " + prefabPaths[i]);
                        return;
                    }
                    // Keep gameplay components on a plain scene root. Unity can otherwise drop
                    // ScriptableObject references assigned to components added to prefab instances.
                    var actor = new GameObject("NPC Animation Test " + prefab.name);
                    MoveToScene(demoScene, actor);
                    actor.transform.SetPositionAndRotation(starts[i], Quaternion.identity);
                    visual.name = prefab.name + " Visual";
                    visual.transform.SetParent(actor.transform, false);

                    NavMeshAgent agent = actor.GetComponent<NavMeshAgent>();
                    if (agent == null) agent = Undo.AddComponent<NavMeshAgent>(actor);
                    if (agent == null)
                    {
                        Debug.LogError("NPC Animation 091: Unity could not add a NavMeshAgent to " + actor.name + ".");
                        return;
                    }
                    agent.agentTypeID = NavMesh.GetSettingsByID(0).agentTypeID;
                    agent.radius = 0.28f;
                    agent.height = 1.7f;
                    agent.baseOffset = 0f;
                    agent.speed = 1.6f;
                    agent.acceleration = 8f;
                    agent.angularSpeed = 360f;
                    agent.stoppingDistance = 0.05f;
                    agent.autoBraking = true;
                    agent.updatePosition = true;
                    agent.updateRotation = true;
                    agent.enabled = false;

                    IslandWaypointWalker walker = actor.GetComponent<IslandWaypointWalker>() ?? actor.AddComponent<IslandWaypointWalker>();
                    SetObjectReference(walker, "network", network);
                    SetObjectReference(walker, "destination", destinations[i]);
                    SmartObjectReservationOwner owner = actor.GetComponent<SmartObjectReservationOwner>() ?? actor.AddComponent<SmartObjectReservationOwner>();
                    Animator animator = actor.GetComponentInChildren<Animator>(true);
                    if (animator == null)
                    {
                        Debug.LogError("Resident has no Animator: " + prefabPaths[i]);
                        return;
                    }
                    animator.applyRootMotion = false;

                    var driver = actor.AddComponent<NpcAnimationDriver>();
                    SetObjectReference(driver, "walker", walker);
                    SetObjectReference(driver, "animator", animator);
                    var bindings = actor.AddComponent<NpcAnimatorBindings>();
                    SetObjectReference(bindings, "animator", animator);
                    var activityPorts = actor.AddComponent<NpcUnityActivityPorts>();
                    var activityPortsData = new SerializedObject(activityPorts);
                    activityPortsData.FindProperty("alignmentTolerance").floatValue = 0.90f;
                    activityPortsData.ApplyModifiedPropertiesWithoutUndo();
                    actor.SetActive(true);
                    fixtureActors.Add(actor);
                }

                var harness = providerObject.AddComponent<NpcAnimationDemoHarness>();
                var harnessData = new SerializedObject(harness);
                SerializedProperty fixtures = harnessData.FindProperty("fixtures");
                fixtures.arraySize = fixtureActors.Count;
                string[] ids = { "nuno-pires", "amelia-faria", "mauro-neves" };
                for (int i = 0; i < fixtureActors.Count; i++)
                {
                    GameObject actor = fixtureActors[i];
                    SerializedProperty item = fixtures.GetArrayElementAtIndex(i);
                    item.FindPropertyRelative("npcId").stringValue = ids[i];
                    item.FindPropertyRelative("walker").objectReferenceValue = actor.GetComponent<IslandWaypointWalker>();
                    item.FindPropertyRelative("ports").objectReferenceValue = actor.GetComponent<NpcUnityActivityPorts>();
                    item.FindPropertyRelative("animations").objectReferenceValue = actor.GetComponent<NpcAnimatorBindings>();
                    item.FindPropertyRelative("owner").objectReferenceValue = actor.GetComponent<SmartObjectReservationOwner>();
                    item.FindPropertyRelative("movementDestination").objectReferenceValue = destinations[i];
                }
                harnessData.FindProperty("runOnStart").boolValue = false;
                harnessData.FindProperty("dataProvider").objectReferenceValue = provider;
                harnessData.ApplyModifiedPropertiesWithoutUndo();
                harness.ConfigureDataProvider(provider);
                EditorUtility.SetDirty(harness);

                var lightObject = new GameObject("Demo Directional Light");
                MoveToScene(demoScene, lightObject);
                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.2f;
                lightObject.transform.rotation = Quaternion.Euler(45f, -35f, 0f);

                var cameraObject = new GameObject("NPC Animation Demo Camera");
                MoveToScene(demoScene, cameraObject);
                Camera camera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
                cameraObject.transform.position = new Vector3(8f, 8f, -9f);
                cameraObject.transform.rotation = Quaternion.LookRotation(new Vector3(0f, 1.1f, 1f) - cameraObject.transform.position);
                camera.fieldOfView = 48f;

                EditorSceneManager.MarkSceneDirty(demoScene);
                if (!EditorSceneManager.SaveScene(demoScene, ScenePath))
                {
                    Debug.LogError("NPC Animation 091: Unity could not save the generated demo scene to " + ScenePath + ".");
                    return;
                }
                AssetDatabase.SaveAssets();
                SceneManager.SetActiveScene(demoScene);
                Selection.activeGameObject = providerObject;
                Debug.Log("NPC Animation 091: isolated demo scene created with three real Humanoid residents, shared profile, real navigation, two-slot Smart Object and the imported Quaternius sitting lifecycle.", harness);
            }
            finally
            {
                if (demoScene.IsValid() && demoScene.isLoaded) SceneManager.SetActiveScene(demoScene);
                else if (previousScene.IsValid() && previousScene.isLoaded) SceneManager.SetActiveScene(previousScene);
            }
        }

        private static NavMeshData BuildAndSaveNavMesh(GameObject floor)
        {
            MeshFilter filter = floor.GetComponent<MeshFilter>();
            var source = new NavMeshBuildSource
            {
                shape = NavMeshBuildSourceShape.Mesh,
                sourceObject = filter.sharedMesh,
                transform = floor.transform.localToWorldMatrix,
                area = 0
            };
            NavMeshBuildSettings settings = NavMesh.GetSettingsByID(0);
            var data = NavMeshBuilder.BuildNavMeshData(settings, new List<NavMeshBuildSource> { source },
                new Bounds(Vector3.zero, new Vector3(24f, 6f, 24f)), Vector3.zero, Quaternion.identity);
            if (data == null)
            {
                Debug.LogError("NPC Animation 091: Unity could not bake the demo NavMesh.");
                return null;
            }
            AssetDatabase.DeleteAsset(NavMeshPath);
            AssetDatabase.CreateAsset(data, NavMeshPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(NavMeshPath, ImportAssetOptions.ForceSynchronousImport);
            NavMeshData persistentData = AssetDatabase.LoadAssetAtPath<NavMeshData>(NavMeshPath);
            if (persistentData == null)
            {
                Debug.LogError("NPC Animation 091: the saved NavMesh asset could not be reloaded.");
                return null;
            }
            return persistentData;
        }

        private static Transform CreateChild(Transform parent, string name, Vector3 localPosition)
        {
            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            child.localPosition = localPosition;
            return child;
        }

        private static void ConfigureBenchSeatPoints(GameObject bench)
        {
            if (bench == null) throw new ArgumentNullException(nameof(bench));
            // BenchProvisional source geometry: seat surface at y=0.54m, front edge at z=+0.235m,
            // backrest at z=-0.22m, and usable width 1.75m. At demo scale 0.6, the seat surface is
            // about 0.32m high. Keep seated roots at the front edge so stand-up can resample the
            // carved NavMesh nearby. The seated pose faces the open side of the bench; the
            // approach markers remain outside it.
            // Lower seated roots by 0.09m in world space so the retargeted pose settles onto
            // the seat instead of appearing to hover above it. Keep approach points on the floor.
            SetSeatPoint(bench.transform.Find("Pose_Sit_Left"), new Vector3(-0.43f, 0.25f, 0.2f), Quaternion.identity);
            SetSeatPoint(bench.transform.Find("Pose_Sit_Right"), new Vector3(0.43f, 0.25f, 0.2f), Quaternion.identity);
            SetSeatPoint(bench.transform.Find("Approach_Sit_Left"), new Vector3(-0.43f, 0f, 0.75f), Quaternion.identity);
            SetSeatPoint(bench.transform.Find("Approach_Sit_Right"), new Vector3(0.43f, 0f, 0.75f), Quaternion.identity);
        }

        private static void ConfigureBenchGeometry(GameObject bench)
        {
            Transform visual = bench.transform.Find("Visual_BenchProvisional");
            if (visual == null)
                throw new InvalidOperationException("BenchProvisional is missing its Visual_BenchProvisional model root.");

            // The prefab had this imported FBX root offset upward by 1.15m, while the FBX already
            // contains the bench in floor-relative coordinates. Clear that duplicate offset.
            visual.localPosition = Vector3.zero;
            PrefabUtility.RecordPrefabInstancePropertyModifications(visual);

            BoxCollider broadCollider = bench.transform.Find("Collider_Bench")?.GetComponent<BoxCollider>();
            if (broadCollider != null) broadCollider.enabled = false;

            CreateBenchCollider(bench.transform, "Collision_BenchSeat", new Vector3(0f, 0.5f, 0f),
                new Vector3(1.75f, 0.1f, 0.42f));
            CreateBenchCollider(bench.transform, "Collision_BenchBackrest", new Vector3(0f, 0.79f, -0.22f),
                new Vector3(1.75f, 0.66f, 0.1f));
            CreateBenchCollider(bench.transform, "Collision_BenchLeg_Left_Back", new Vector3(-0.7f, 0.245f, -0.17f),
                new Vector3(0.13f, 0.49f, 0.13f));
            CreateBenchCollider(bench.transform, "Collision_BenchLeg_Left_Front", new Vector3(-0.7f, 0.245f, 0.17f),
                new Vector3(0.13f, 0.49f, 0.13f));
            CreateBenchCollider(bench.transform, "Collision_BenchLeg_Right_Back", new Vector3(0.7f, 0.245f, -0.17f),
                new Vector3(0.13f, 0.49f, 0.13f));
            CreateBenchCollider(bench.transform, "Collision_BenchLeg_Right_Front", new Vector3(0.7f, 0.245f, 0.17f),
                new Vector3(0.13f, 0.49f, 0.13f));

            NavMeshObstacle obstacle = Undo.AddComponent<NavMeshObstacle>(bench);
            obstacle.shape = NavMeshObstacleShape.Box;
            // Extend below the floor so the carve intersects the baked NavMesh. Cover the full
            // bench footprint and agent clearance; approach markers remain outside this box.
            obstacle.center = new Vector3(0f, 0.55f, -0.03f);
            obstacle.size = new Vector3(1.86f, 2f, 0.72f);
            obstacle.carving = true;
            obstacle.carveOnlyStationary = false;
            obstacle.carvingMoveThreshold = 0.1f;
            obstacle.carvingTimeToStationary = 0.5f;
            PrefabUtility.RecordPrefabInstancePropertyModifications(obstacle);
        }

        private static void CreateBenchCollider(Transform parent, string objectName, Vector3 localPosition, Vector3 size)
        {
            var colliderObject = new GameObject(objectName);
            Undo.RegisterCreatedObjectUndo(colliderObject, "Create demo bench collision volume");
            colliderObject.transform.SetParent(parent, false);
            colliderObject.transform.localPosition = localPosition;
            BoxCollider collider = Undo.AddComponent<BoxCollider>(colliderObject);
            collider.size = size;
            PrefabUtility.RecordPrefabInstancePropertyModifications(colliderObject.transform);
            EditorUtility.SetDirty(colliderObject);
        }

        private static void SetSeatPoint(Transform point, Vector3 localPosition, Quaternion localRotation)
        {
            if (point == null) throw new InvalidOperationException("BenchProvisional is missing a configured seat or approach point.");
            point.localPosition = localPosition;
            point.localRotation = localRotation;
            PrefabUtility.RecordPrefabInstancePropertyModifications(point);
            EditorUtility.SetDirty(point);
        }

        private static bool IsGeneratedDemoScene(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.GetComponentInChildren<NpcAnimationDemoHarness>(true) != null)
                    return true;
            return false;
        }

        private static void MoveToScene(Scene scene, GameObject gameObject)
        {
            if (gameObject != null && scene.IsValid() && scene.isLoaded && gameObject.scene != scene)
                SceneManager.MoveGameObjectToScene(gameObject, scene);
        }

        private static void SetObjectReference(UnityEngine.Object target, string property, UnityEngine.Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.Update();
            SerializedProperty serializedProperty = serialized.FindProperty(property);
            if (serializedProperty == null)
                throw new InvalidOperationException("NPC Animation 091: serialized field not found: " + target.GetType().Name + "." + property);
            serializedProperty.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
            if (target is Component component)
                PrefabUtility.RecordPrefabInstancePropertyModifications(component);

            var verification = new SerializedObject(target);
            verification.Update();
            SerializedProperty persistedProperty = verification.FindProperty(property);
            if (persistedProperty == null || persistedProperty.objectReferenceValue != value)
                throw new InvalidOperationException("NPC Animation 091: failed to assign " + target.GetType().Name + "." + property + ".");
        }

        private static void SetTextAsset(NpcScheduleTextAssetProvider provider, string field, string path)
        {
            SetObjectReference(provider, field, AssetDatabase.LoadAssetAtPath<TextAsset>(path));
        }

        private static void SetTextArray(NpcScheduleTextAssetProvider provider, string field, params string[] paths)
        {
            var serialized = new SerializedObject(provider);
            SerializedProperty property = serialized.FindProperty(field);
            property.arraySize = paths.Length;
            for (int i = 0; i < paths.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<TextAsset>(paths[i]);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
