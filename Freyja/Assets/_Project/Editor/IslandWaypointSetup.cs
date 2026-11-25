using System.Collections.Generic;
using System.Linq;
using Freyja.Prototype;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public static class IslandWaypointSetup
{
    private const string ScenePath = "Assets/_Project/Scenes/Ilha_Principal.unity";
    private const string DataPath = "Assets/_Project/Data/IlhaWaypointNavigation.asset";
    private const string NetworkName = "Waypoints da ilha";
    private const int Walkable = 1;

    [MenuItem("Freyja/Navegação/Gerar waypoints da ilha")]
    private static void GenerateFromMenu() => Generate();

    public static string Generate()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath || EditorApplication.isPlaying)
            throw new System.InvalidOperationException("Abra a cena principal em Edit Mode.");

        var island = GameObject.Find("Island023").transform;
        var buildings = island.Find("Construções — 12 casas e 5 serviços");
        var cast = island.Find("Elenco da ilha 045");
        var residents = island.Find("Moradores060");
        var pier = island.Find("Cais e píer");
        var terrain = island.GetComponentInChildren<Terrain>();

        var sources = new List<NavMeshBuildSource>();
        NavMeshBuilder.CollectSources(island, -1, NavMeshCollectGeometry.PhysicsColliders, 0,
            new List<NavMeshBuildMarkup>(), sources);
        sources.RemoveAll(source => source.component is Collider c &&
            (c.isTrigger || c.transform.IsChildOf(cast) || c.transform.IsChildOf(residents) ||
             c.GetComponentInParent<ShopkeeperRoutine>() != null));
        for (int i = 0; i < sources.Count; i++)
        {
            var source = sources[i];
            var target = source.component.transform;
            if (target.parent == buildings || target == pier.Find("Colisão do galpão"))
            {
                source.area = NavMesh.GetAreaFromName("Not Walkable");
                sources[i] = source;
            }
        }

        var settings = NavMesh.GetSettingsByIndex(0);
        settings.agentRadius = 0.25f;
        settings.agentHeight = 1.65f;
        settings.agentClimb = 0.4f;
        settings.agentSlope = 46f;
        settings.overrideVoxelSize = true;
        settings.voxelSize = 0.09f;
        settings.minRegionArea = 1f;

        // The terrain collider continues under the sea at y = 0.
        var bounds = new Bounds(new Vector3(0f, 15.5f, -8f), new Vector3(170f, 30f, 150f));
        var data = NavMeshBuilder.BuildNavMeshData(settings, sources, bounds, Vector3.zero, Quaternion.identity);
        if (data == null)
            throw new System.InvalidOperationException("A malha de navegação não foi gerada.");

        var networkRoot = GameObject.Find(NetworkName);
        if (networkRoot == null)
            networkRoot = new GameObject(NetworkName);
        var network = networkRoot.GetComponent<IslandWaypointNetwork>();
        if (network == null)
            network = networkRoot.AddComponent<IslandWaypointNetwork>();

        var previous = AssetDatabase.LoadAssetAtPath<NavMeshData>(DataPath);
        if (previous == null)
        {
            AssetDatabase.CreateAsset(data, DataPath);
        }
        else
        {
            EditorUtility.CopySerialized(data, previous);
            Object.DestroyImmediate(data);
            data = previous;
        }

        var networkProperties = new SerializedObject(network);
        networkProperties.FindProperty("navigation").objectReferenceValue = data;
        networkProperties.ApplyModifiedProperties();

        var instance = NavMesh.AddNavMeshData(data);
        string report;
        try
        {
            var landmarks = Group(networkRoot.transform, "Destinos principais");
            var entrances = Group(networkRoot.transform, "Acessos");
            var grid = Group(networkRoot.transform, "Distribuição pelo terreno");
            var isolated = Group(networkRoot.transform, "Áreas isoladas");
            var expected = new Dictionary<string, Vector3>
            {
                { "Largo", new Vector3(-2, 7, -3) },
                { "Píer — convés", new Vector3(-36.73f, 1.84f, -55.8f) },
                { "Píer — saída", new Vector3(-32.5f, 1.87f, -41.5f) },
                { "Curva do cais", Ground(terrain, -39, -32) },
                { "Venda — banca", new Vector3(-8, 7, -1) },
                { "Igreja — patamar", new Vector3(-1.66f, 14.9f, 25) },
                { "Rua leste — praia", Ground(terrain, 27, -22) },
                { "Rua leste — subida", Ground(terrain, 42, 12) },
                { "Casa isolada — ramal", Ground(terrain, -43, 30) },
                { "Colina — crista", Ground(terrain, 5, 50) },
                { "L3 — pé da escada", new Vector3(35.047f, 14.118f, 26.012f) },
                { "L3 — varanda", new Vector3(40.639f, 16.82f, 24.415f) }
            };

            if (!NavMesh.SamplePosition(expected["Largo"], out var center, 0.5f, Walkable))
                throw new System.InvalidOperationException("Largo ausente da malha.");

            var route = new NavMeshPath();
            var failures = new List<string>();
            var activeGroups = new HashSet<Transform> { landmarks, entrances, grid, isolated };
            var retained = new HashSet<Transform>();
            foreach (var point in expected)
                AddPoint(point.Key, point.Value, 2.2f, landmarks);

            foreach (Transform building in buildings)
            {
                var door = building.Find("Door023");
                if (door == null)
                    continue;
                var relative = Vector3.ProjectOnPlane(door.position - building.position, Vector3.up);
                Vector3 outward = relative.sqrMagnitude > 0.01f ? relative.normalized : -building.forward;
                Vector3 approach = door.position + outward * 1.2f;
                if (building.name == "L1")
                    approach = new Vector3(34f, 3.5f, -26f);
                AddPoint(building.name, approach, 3f, entrances);
            }

            for (int x = -75; x <= 75; x += 5)
            for (int z = -80; z <= 70; z += 5)
            {
                var ground = Ground(terrain, x, z);
                if (ground.y >= 0.5f)
                    AddPoint($"Terreno {x}, {z}", ground, 0.8f, grid);
            }

            foreach (Transform group in networkRoot.transform)
                if (activeGroups.Contains(group))
                    foreach (var old in group.Cast<Transform>().Where(t => !retained.Contains(t)).ToArray())
                        Undo.DestroyObjectImmediate(old.gameObject);

            report = $"{landmarks.childCount} destinos principais, {entrances.childCount} acessos, " +
                     $"{grid.childCount} pontos no terreno e {isolated.childCount} pontos isolados. " +
                     $"Acessos sem projeção: {string.Join(", ", failures)}";

            void AddPoint(string name, Vector3 origin, float distance, Transform group)
            {
                if (!NavMesh.SamplePosition(origin, out var hit, distance, Walkable))
                {
                    if (group != grid)
                        failures.Add(name);
                    return;
                }

                bool connected = NavMesh.CalculatePath(center.position, hit.position, Walkable, route) &&
                                 route.status == NavMeshPathStatus.PathComplete;
                var targetGroup = connected ? group : isolated;
                var oldGroup = connected ? isolated : group;
                var marker = targetGroup.Find(name) ?? oldGroup.Find(name);
                if (marker == null)
                    marker = new GameObject(name).transform;
                marker.SetParent(targetGroup, false);
                marker.position = hit.position;
                retained.Add(marker);
            }
        }
        finally
        {
            instance.Remove();
        }

        var locomotion = AssetDatabase.LoadAssetAtPath<AnimatorController>(
            "Assets/_Project/Art/Ilha_Tripo_011/Protagonista.controller");
        var idle = (AnimationClip)locomotion.layers[0].stateMachine.defaultState.motion;
        foreach (var actor in cast.Cast<Transform>().Concat(residents.Cast<Transform>()))
        {
            var walker = actor.GetComponent<IslandWaypointWalker>();
            if (walker == null)
                walker = actor.gameObject.AddComponent<IslandWaypointWalker>();

            var agent = actor.GetComponent<NavMeshAgent>();
            agent.enabled = false;
            agent.speed = 1.6f;
            agent.angularSpeed = 540f;
            agent.acceleration = 8f;
            agent.stoppingDistance = 0.12f;
            agent.radius = settings.agentRadius;
            agent.height = settings.agentHeight;
            agent.areaMask = Walkable;
            agent.autoTraverseOffMeshLink = false;
            EditorUtility.SetDirty(agent);

            var animator = actor.GetComponentInChildren<Animator>();
            var properties = new SerializedObject(walker);
            properties.FindProperty("network").objectReferenceValue = network;
            properties.FindProperty("animator").objectReferenceValue = animator;
            if (animator != null && !animator.parameters.Any(p => p.name == "Walking"))
            {
                properties.FindProperty("locomotionController").objectReferenceValue = locomotion;
                properties.FindProperty("locomotionIdle").objectReferenceValue = idle;
            }
            properties.ApplyModifiedProperties();
        }

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = networkRoot;
        Debug.Log(report, networkRoot);
        return report;
    }

    private static Transform Group(Transform parent, string name)
    {
        var group = parent.Find(name);
        if (group == null)
        {
            group = new GameObject(name).transform;
            group.SetParent(parent, false);
        }
        return group;
    }

    private static Vector3 Ground(Terrain terrain, float x, float z)
    {
        var point = new Vector3(x, 0f, z);
        point.y = terrain.SampleHeight(point) + terrain.transform.position.y;
        return point;
    }
}
