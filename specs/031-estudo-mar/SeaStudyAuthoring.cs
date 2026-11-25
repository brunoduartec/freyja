using System;
using System.Linq;
using Freyja.Environment;
using Unity.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public static class SeaStudyAuthoring
{
    private const string ScenePath = "Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity";
    private const string Folder = "Assets/_Project/Art/SeaStudy031/";
    private const int MaskSize = 1024;
    private const float MaskOrigin = -104;
    private const float MaskSpan = 208;
    private const float MaxDistance = 24;

    public static string Run()
    {
        if (Application.dataPath.IndexOf("/031-estudo-mar/", StringComparison.Ordinal) < 0 ||
            EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Open the study project in Edit Mode.");

        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != ScenePath || scene.isDirty)
            throw new InvalidOperationException("Open and save scene 011 before authoring.");

        var reference = GameObject.Find("Island023/Mar").GetComponent<MeshRenderer>();
        var camera = GameObject.Find("Cais Gameplay Camera").GetComponent<Camera>();
        var shader = Shader.Find("Freyja/Coastal Water");
        if (!shader || ShaderUtil.ShaderHasError(shader))
            throw new InvalidOperationException("Coastal Water did not compile.");

        float waterLevel = reference.bounds.center.y;
        var contacts = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)
            .Where(renderer => renderer.bounds.min.y < waterLevel && renderer.bounds.max.y > waterLevel)
            .Where(renderer => renderer.name == "Piece11" || renderer.name == "Estaca" ||
                               renderer.name.StartsWith("Rock_", StringComparison.Ordinal))
            .Select(renderer => renderer.GetComponent<MeshFilter>())
            .Where(filter => filter && filter.sharedMesh)
            .ToArray();
        var coast = SaveAsset(BakeCoast(Terrain.activeTerrains.Single(), waterLevel, contacts), "CoastDistance.asset");

        var matA = CreateMaterial(shader, coast, camera.backgroundColor, 0f, "A - Ondulacao na superficie.mat");
        var matB = CreateMaterial(shader, coast, camera.backgroundColor, 0.38f, "B - Onda geometrica.mat");
        var grid = SaveAsset(CreateGrid(), "SeaGrid.asset");
        var referenceFilter = reference.GetComponent<MeshFilter>();
        var surface = ConfigureRenderer(reference.transform, "A · Ondulação na superfície",
            referenceFilter.sharedMesh, matA);
        var geometry = ConfigureRenderer(reference.transform, "B · Onda geométrica", grid, matB);

        // The original plane spans -5 to +5 in local space. The grid uses the same transform.
        var controller = reference.GetComponent<SeaStudyComparison>();
        if (!controller)
            controller = reference.gameObject.AddComponent<SeaStudyComparison>();

        var serialized = new SerializedObject(controller);
        serialized.FindProperty("reference").objectReferenceValue = reference;
        serialized.FindProperty("surface").objectReferenceValue = surface;
        serialized.FindProperty("geometry").objectReferenceValue = geometry;
        serialized.FindProperty("initialMode").enumValueIndex = (int)SeaStudyComparison.Mode.Surface;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        reference.enabled = false;
        surface.enabled = true;
        geometry.enabled = false;
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return $"Saved {scene.path}. Mask contains {contacts.Length} mesh contacts.";
    }

    private static MeshRenderer ConfigureRenderer(Transform parent, string name, Mesh mesh, Material material)
    {
        var child = parent.Find(name);
        if (!child)
        {
            child = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)).transform;
            child.SetParent(parent, false);
        }

        child.GetComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = child.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        return renderer;
    }

    private static Material CreateMaterial(Shader shader, Texture2D coast, Color horizon, float amplitude, string file)
    {
        var material = new Material(shader);
        material.SetTexture("_Coast", coast);
        material.SetVector("_CoastMapping", new Vector4(MaskOrigin, MaskOrigin, 1f / MaskSpan, MaxDistance));
        material.SetColor("_HorizonColor", horizon);
        material.SetFloat("_WaveHeight", amplitude);
        return SaveAsset(material, file);
    }

    private static T SaveAsset<T>(T temporary, string file) where T : Object
    {
        string path = Folder + file;
        temporary.name = System.IO.Path.GetFileNameWithoutExtension(file);
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (!existing)
        {
            AssetDatabase.CreateAsset(temporary, path);
            return temporary;
        }

        EditorUtility.CopySerialized(temporary, existing);
        Object.DestroyImmediate(temporary);
        EditorUtility.SetDirty(existing);
        return existing;
    }

    private static Texture2D BakeCoast(Terrain terrain, float waterLevel, MeshFilter[] contacts)
    {
        const float step = MaskSpan / MaskSize;
        Vector3 origin = terrain.transform.position;
        TerrainData terrainData = terrain.terrainData;
        Vector3 size = terrainData.size;
        int sandLayer = Array.FindIndex(terrainData.terrainLayers, layer => layer && layer.name == "BeachSand");
        if (sandLayer < 0)
            throw new InvalidOperationException("The scene terrain requires the BeachSand layer.");

        int mapWidth = terrainData.alphamapWidth;
        int mapHeight = terrainData.alphamapHeight;
        float[,,] paint = terrainData.GetAlphamaps(0, 0, mapWidth, mapHeight);
        var land = new bool[MaskSize * MaskSize];
        var sand = new bool[land.Length];
        var coastDistances = new float[land.Length];
        var beachDistances = new float[land.Length];
        for (int z = 0; z < MaskSize; z++)
        {
            float nz = (MaskOrigin + (z + 0.5f) * step - origin.z) / size.z;
            if (nz < 0 || nz > 1)
                continue;

            int mapZ = Mathf.RoundToInt(nz * (mapHeight - 1));
            for (int x = 0; x < MaskSize; x++)
            {
                float nx = (MaskOrigin + (x + 0.5f) * step - origin.x) / size.x;
                if (nx < 0 || nx > 1)
                    continue;

                int i = z * MaskSize + x;
                land[i] = terrainData.GetInterpolatedHeight(nx, nz) + origin.y > waterLevel;
                int mapX = Mathf.RoundToInt(nx * (mapWidth - 1));
                sand[i] = land[i] && paint[mapZ, mapX, sandLayer] >= 0.45f;
            }
        }

        Array.Fill(coastDistances, MaxDistance);
        Array.Fill(beachDistances, MaxDistance);
        for (int z = 1; z < MaskSize - 1; z++)
        for (int x = 1; x < MaskSize - 1; x++)
        {
            int i = z * MaskSize + x;
            if (land[i])
                continue;
            if (Touches(land, i))
                coastDistances[i] = 0;
            if (Touches(sand, i))
                beachDistances[i] = 0;
        }

        foreach (var contact in contacts)
            MarkContact(coastDistances, contact, waterLevel, step);

        FillDistances(coastDistances, step);
        FillDistances(beachDistances, step);
        var pixels = new byte[land.Length * 2];
        for (int i = 0; i < land.Length; i++)
        {
            pixels[i * 2] = EncodeDistance(coastDistances[i]);
            pixels[i * 2 + 1] = EncodeDistance(beachDistances[i]);
        }

        var texture = new Texture2D(MaskSize, MaskSize, TextureFormat.RG16, false, true)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };
        texture.SetPixelData(pixels, 0);
        texture.Apply(false);
        return texture;
    }

    private static bool Touches(bool[] mask, int i)
    {
        return mask[i - 1] || mask[i + 1] || mask[i - MaskSize] || mask[i + MaskSize];
    }

    private static byte EncodeDistance(float distance)
    {
        return (byte)Mathf.RoundToInt(Mathf.Clamp01(distance / MaxDistance) * 255);
    }

    private static void FillDistances(float[] distances, float step)
    {
        // Two chamfer passes propagate the nearest shoreline seed out into the water.
        for (int z = 1; z < MaskSize - 1; z++)
        for (int x = 1; x < MaskSize - 1; x++)
            SpreadDistance(distances, z * MaskSize + x, -1, -MaskSize, step);

        for (int z = MaskSize - 2; z >= 1; z--)
        for (int x = MaskSize - 2; x >= 1; x--)
            SpreadDistance(distances, z * MaskSize + x, 1, MaskSize, step);
    }

    private static void MarkContact(float[] distances, MeshFilter filter, float waterLevel, float step)
    {
        using var data = MeshUtility.AcquireReadOnlyMeshData(filter.sharedMesh);
        var mesh = data[0];
        using var vertices = new NativeArray<Vector3>(mesh.vertexCount, Allocator.Temp);
        mesh.GetVertices(vertices);
        var transform = filter.transform.localToWorldMatrix;
        for (int submesh = 0; submesh < mesh.subMeshCount; submesh++)
        {
            var section = mesh.GetSubMesh(submesh);
            if (section.topology != MeshTopology.Triangles)
                continue;

            using var indices = new NativeArray<int>(section.indexCount, Allocator.Temp);
            mesh.GetIndices(indices, submesh);
            for (int i = 0; i < indices.Length; i += 3)
            {
                Vector3 a = transform.MultiplyPoint3x4(vertices[indices[i]]);
                Vector3 b = transform.MultiplyPoint3x4(vertices[indices[i + 1]]);
                Vector3 c = transform.MultiplyPoint3x4(vertices[indices[i + 2]]);
                bool crossesAB = Crosses(a, b, waterLevel, out Vector3 ab);
                bool crossesBC = Crosses(b, c, waterLevel, out Vector3 bc);
                bool crossesCA = Crosses(c, a, waterLevel, out Vector3 ca);
                if (crossesAB)
                    MarkSegment(distances, ab, crossesBC ? bc : ca, step);
                else if (crossesBC && crossesCA)
                    MarkSegment(distances, bc, ca, step);
            }
        }
    }

    private static bool Crosses(Vector3 a, Vector3 b, float level, out Vector3 intersection)
    {
        if ((a.y < level) == (b.y < level))
        {
            intersection = default;
            return false;
        }

        intersection = Vector3.LerpUnclamped(a, b, (level - a.y) / (b.y - a.y));
        return true;
    }

    private static void MarkSegment(float[] distances, Vector3 a, Vector3 b, float step)
    {
        int samples = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(a, b) / (step * 0.5f)));
        for (int sample = 0; sample <= samples; sample++)
        {
            Vector3 world = Vector3.Lerp(a, b, sample / (float)samples);
            int x = Mathf.FloorToInt((world.x - MaskOrigin) / step);
            int z = Mathf.FloorToInt((world.z - MaskOrigin) / step);
            if (x > 0 && x < MaskSize - 1 && z > 0 && z < MaskSize - 1)
                distances[z * MaskSize + x] = 0;
        }
    }

    private static void SpreadDistance(float[] distances, int i, int side, int row, float step)
    {
        float axis = Mathf.Min(distances[i + side], distances[i + row]) + step;
        float diagonal = Mathf.Min(distances[i + row - 1], distances[i + row + 1]) + step * 1.414214f;
        distances[i] = Mathf.Min(distances[i], Mathf.Min(axis, diagonal));
    }

    private static Mesh CreateGrid()
    {
        const int resolution = 240;
        const int stride = resolution + 1;
        var vertices = new Vector3[stride * stride];
        var triangles = new int[resolution * resolution * 6];
        int write = 0;
        for (int z = 0; z <= resolution; z++)
        for (int x = 0; x <= resolution; x++)
        {
            int i = z * stride + x;
            vertices[i] = new Vector3(x * (10f / resolution) - 5f, 0, z * (10f / resolution) - 5f);
            if (z == resolution || x == resolution)
                continue;
            triangles[write++] = i;
            triangles[write++] = i + stride;
            triangles[write++] = i + 1;
            triangles[write++] = i + 1;
            triangles[write++] = i + stride;
            triangles[write++] = i + stride + 1;
        }

        var mesh = new Mesh { vertices = vertices, triangles = triangles };
        mesh.bounds = new Bounds(Vector3.zero, new Vector3(10f, 0.04f, 10f));
        return mesh;
    }
}
