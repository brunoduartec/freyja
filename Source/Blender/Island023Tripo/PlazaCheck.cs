using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class PlazaCheck023
{
    const string Evidence = "/Users/bruno.duartec/Documents/Projects/Freyja/Artifacts/Island023/Tripo/";

    public static object Main()
    {
        return PrepareAccess("plaza", new[] { "Venda", "Oficina" },
            () => new[] { Capture("praca-venda-oficina.png", new Vector3(-20, 16, -27), new Vector3(-4.5f, 10, 6)) });
    }

    public static object Remaining()
    {
        return PrepareAccess("remaining", new[] { "A4", "L2", "L4 — casa isolada", "Salão comunitário", "Armazém do cais (apoio em terra)" },
            () => new[]
            {
                Capture("ilha-modelos-14-18.png", new Vector3(12, 113, -153), new Vector3(0, 8, 0)),
                Capture("praca-modelos-14-18.png", new Vector3(-16, 20, -31), new Vector3(8, 9, 5)),
                Capture("cais-modelos-14-18.png", new Vector3(-67, 40, -92), new Vector3(-15, 2, -39)),
                Capture("casa-velho-16.png", new Vector3(-71, 38, 15), new Vector3(-49, 17, 39))
            });
    }

    static object PrepareAccess(string test, string[] lots, Func<string[]> capture)
    {
        var island = GameObject.Find("Island023");
        var buildings = island.transform.Find("Construções — 12 casas e 5 serviços");
        var terrain = island.GetComponentInChildren<Terrain>();
        var baseline = Evidence + "Terreno-antes-14-18.asset";
        if (test == "remaining" && !File.Exists(baseline))
            File.Copy(AssetDatabase.GetAssetPath(terrain.terrainData), baseline);
        var oldRoutes = JArray.Parse(File.ReadAllText(Evidence + "doors-layout.json"));
        var updated = new JArray();
        foreach (var lot in lots)
        {
            var plot = buildings.Find(lot);
            var entrance = plot.Find("Door023");
            if (!entrance) continue;
            var route = (JObject)oldRoutes.Single(r => (string)r["id"] == lot).DeepClone();
            var points = (JArray)route["points"];
            var oldEntrance = Read((JObject)points.Last);
            // Approach perpendicular to the facade for the last meter, clear of the display counter.
            var approach = entrance.position - plot.forward * 1.3f;
            points.RemoveAt(points.Count - 1);
            Append(points, Read((JObject)points.Last), oldEntrance - plot.forward * .6f);
            Append(points, Read((JObject)points.Last), approach);
            Append(points, approach, entrance.position);
            route["door"] = Point(entrance.position);
            updated.Add(route);
            PaintWalkway(terrain, oldEntrance, approach, entrance.position);
        }
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(island.scene);
        File.WriteAllText(Evidence + test + "-layout.json", updated.ToString());
        return new { testableDoors = updated.Count, screenshots = capture(), compilationFailed = EditorUtility.scriptCompilationFailed };
    }

    static Vector3 Read(JObject token) => new Vector3((float)token["x"], (float)token["y"], (float)token["z"]);
    static JObject Point(Vector3 point) => new JObject { ["x"] = point.x, ["y"] = point.y, ["z"] = point.z };

    static void Append(JArray points, Vector3 from, Vector3 to)
    {
        int samples = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(from, to) / .45f));
        for (int i = 1; i <= samples; i++) points.Add(Point(Vector3.Lerp(from, to, i / (float)samples)));
    }

    static void PaintWalkway(Terrain terrain, params Vector3[] points)
    {
        var data = terrain.terrainData;
        int sand = Array.FindIndex(data.terrainLayers, layer => layer && layer.name == "PathSand");
        if (sand < 0) return;
        var origin = terrain.transform.position;
        var size = data.size;
        int n = data.alphamapResolution - 1;
        float u = n / size.x, v = n / size.z;
        int x0 = Mathf.Clamp(Mathf.FloorToInt((points.Min(p => p.x) - origin.x - 1) * u), 0, n);
        int z0 = Mathf.Clamp(Mathf.FloorToInt((points.Min(p => p.z) - origin.z - 1) * v), 0, n);
        int x1 = Mathf.Clamp(Mathf.CeilToInt((points.Max(p => p.x) - origin.x + 1) * u), 0, n);
        int z1 = Mathf.Clamp(Mathf.CeilToInt((points.Max(p => p.z) - origin.z + 1) * v), 0, n);
        var weights = data.GetAlphamaps(x0, z0, x1 - x0 + 1, z1 - z0 + 1);
        for (int z = z0; z <= z1; z++)
        for (int x = x0; x <= x1; x++)
        {
            var p = new Vector2(origin.x + x / u, origin.z + z / v);
            float distance = float.MaxValue;
            for (int i = 1; i < points.Length; i++)
            {
                var a = new Vector2(points[i - 1].x, points[i - 1].z);
                var b = new Vector2(points[i].x, points[i].z);
                var ab = b - a;
                var t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(.0001f, ab.sqrMagnitude));
                distance = Mathf.Min(distance, Vector2.Distance(p, a + t * ab));
            }
            distance += (Mathf.PerlinNoise(p.x * 1.7f, p.y * 1.7f) - .5f) * .25f;
            float coverage = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.44f, .83f, distance));
            float grass = weights[z - z0, x - x0, 0];
            float delta = Mathf.Min(grass, Mathf.Max(0, coverage - weights[z - z0, x - x0, sand]));
            weights[z - z0, x - x0, 0] -= delta;
            weights[z - z0, x - x0, sand] += delta;
        }
        data.SetAlphamaps(x0, z0, weights);
        EditorUtility.SetDirty(data);
        foreach (var map in data.alphamapTextures) EditorUtility.SetDirty(map);
    }

    static string Capture(string filename, Vector3 eye, Vector3 focus)
    {
        var go = new GameObject("Câmera temporária para verificar a praça");
        var camera = go.AddComponent<Camera>();
        var target = new RenderTexture(1400, 960, 24);
        var image = new Texture2D(1400, 960, TextureFormat.RGB24, false);
        var earlier = RenderTexture.active;
        try
        {
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.13f,.46f,.53f);
            camera.fieldOfView = 43;
            camera.farClipPlane = 750;
            camera.allowHDR = false;
            camera.allowMSAA = false;
            camera.transform.position = eye;
            camera.transform.LookAt(focus);
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, 1400, 960), 0, 0);
            image.Apply();
            var file = Evidence + filename;
            File.WriteAllBytes(file, image.EncodeToPNG());
            return file;
        }
        finally
        {
            RenderTexture.active = earlier;
            camera.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(image);
            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
