using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Freyja.Environment;

var grass = UnityEngine.Object.FindFirstObjectByType<CoastalGrass>();
var wind = GlobalWindController.Active;
if (!Application.isPlaying || !grass || !wind)
    throw new InvalidOperationException("Enter Play Mode in Ilha_Principal.");
var grassSettings = new SerializedObject(grass);
var layout = (CoastalGrassLayout)grassSettings.FindProperty("layout").objectReferenceValue;
var regionField = typeof(CoastalGrass).GetField("regions", BindingFlags.NonPublic | BindingFlags.Instance);
var regions = (Array)regionField.GetValue(grass);
FieldInfo batchesField = regions.GetType().GetElementType().GetField("Batches");
FieldInfo boundsField = regions.GetType().GetElementType().GetField("Bounds");
var results = new List<object>();

void Verify(string name, bool passed, object detail = null)
{
    results.Add(new { name, passed, detail });
    if (!passed)
        UnityEngine.Debug.LogError(name);
}

ulong[] Digests()
{
    var hashes = new ulong[regions.Length];
    for (int i = 0; i < regions.Length; i++)
    {
        var batches = (Matrix4x4[][])batchesField.GetValue(regions.GetValue(i));
        ulong hash = 14695981039346656037ul;
        foreach (var matrices in batches)
        {
            hash = unchecked((hash ^ (ulong)matrices.Length) * 1099511628211ul);
            foreach (var matrix in matrices)
                for (int entry = 0; entry < 16; entry++)
                    hash = unchecked((hash ^ (uint)BitConverter.SingleToInt32Bits(matrix[entry])) * 1099511628211ul);
        }
        hashes[i] = hash;
    }
    return hashes;
}

int CountMatrix(Vector3 point)
{
    int count = 0;
    foreach (var region in regions)
    {
        var batches = (Matrix4x4[][])batchesField.GetValue(region);
        foreach (var matrices in batches)
            foreach (var matrix in matrices)
                if ((Vector3)matrix.GetColumn(3) == point)
                    count++;
    }
    return count;
}

int initialCount = grass.BakedInstances;
ulong[] initial = Digests();
grass.RebuildPlacement();
Verify("mesma seed, mesmos lotes", initial.SequenceEqual(Digests()), initialCount);
var seed = grassSettings.FindProperty("variationSeed");
int savedSeed = seed.intValue;
seed.intValue = savedSeed + 1;
grassSettings.ApplyModifiedPropertiesWithoutUndo();
grass.RebuildPlacement();
Verify("seed diferente altera seleção", !initial.SequenceEqual(Digests()), grass.BakedInstances);
seed.intValue = savedSeed;
grassSettings.ApplyModifiedPropertiesWithoutUndo();
grass.RebuildPlacement();
Verify("seed original recupera seleção", initial.SequenceEqual(Digests()));

var layouts = layout.Cells.Where(c => c.Batches.Sum(b => b.Instances.Length) > 200).ToArray();
Vector3 p0 = layouts[0].Batches[0].Instances.Select(matrix => (Vector3)matrix.GetColumn(3)).First(point => CountMatrix(point) > 0);
Vector3 p1 = layouts.Last().Batches[0].Instances.Select(matrix => (Vector3)matrix.GetColumn(3)).First(point => CountMatrix(point) > 0);

var zone = new GameObject("Temporary Grass Exclusion Test");
zone.SetActive(false);
zone.transform.position = p0;
var exclusion = zone.AddComponent<GrassExclusionVolume>();
exclusion.Configure(grass, Vector3.zero, new Vector3(2, 2, 2));
var watch = Stopwatch.StartNew();
zone.SetActive(true);
watch.Stop();
int affected = grass.RebuiltRegions;
Verify("exclusão remove ponto", CountMatrix(p0) == 0 && grass.BakedInstances < initialCount,
    new { before = initialCount, during = grass.BakedInstances, affected, ms = watch.Elapsed.TotalMilliseconds });
ulong[] blocked = Digests();
int changed = blocked.Zip(initial, (a, b) => a != b).Count(value => value);
Verify("reconstrução localizada", affected > 0 && affected < regions.Length && changed <= affected,
    new { affected, totalRegions = regions.Length, changed });

Vector3 start = zone.transform.position;
zone.transform.position = p1;
var update = typeof(GrassExclusionVolume).GetMethod("LateUpdate", BindingFlags.NonPublic | BindingFlags.Instance);
watch.Restart();
update.Invoke(exclusion, null);
watch.Stop();
Verify("mover restaura origem e exclui destino", CountMatrix(start) > 0 && CountMatrix(p1) == 0,
    new { affected = grass.RebuiltRegions, ms = watch.Elapsed.TotalMilliseconds });
zone.SetActive(false);
UnityEngine.Object.Destroy(zone);
Verify("remover restaura todos os lotes", initial.SequenceEqual(Digests()));

float direction = GlobalWindController.ResolveHeading(Vector2.zero, 37);
float invalid = GlobalWindController.ResolveHeading(new Vector2(float.NaN, 1), 31);
Verify("direções nula e inválida mantêm orientação", direction == 37 && invalid == 31);

int outside = 0;
int invertedBounds = 0;
int[] profiles = new int[3];
var roots = new HashSet<Vector2Int>();
Terrain terrain = Terrain.activeTerrain;
TerrainData surface = terrain.terrainData;
Vector3 origin = terrain.transform.position;
int duplicates = 0;
int invalidSurface = 0;
float maxHeightError = 0;
float maxSlope = 0;
SerializedProperty meshes = grassSettings.FindProperty("tuftMeshes");
var meshBounds = Enumerable.Range(0, meshes.arraySize)
    .Select(i => ((Mesh)meshes.GetArrayElementAtIndex(i).objectReferenceValue).bounds).ToArray();
Vector3[] scales = new[] { "shortGrass", "wildGrass", "coastalGrass" }.Select(name =>
{
    var profile = grassSettings.FindProperty(name);
    float width = profile.FindPropertyRelative("width").floatValue;
    return new Vector3(width, profile.FindPropertyRelative("height").floatValue, width);
}).ToArray();
foreach (var region in regions)
{
    var bounds = (Bounds)boundsField.GetValue(region);
    var batches = (Matrix4x4[][])batchesField.GetValue(region);
    for (int i = 0; i < batches.Length; i++)
    {
        if (i % 5 >= 3)
            continue;
        profiles[i / 5] += batches[i].Length;
        foreach (Matrix4x4 matrix in batches[i])
        {
            Vector3 pivot = matrix.GetColumn(3);
            var key = new Vector2Int(Mathf.RoundToInt(pivot.x * 1024), Mathf.RoundToInt(pivot.z * 1024));
            if (!roots.Add(key))
                duplicates++;
            float u = (pivot.x - origin.x) / surface.size.x;
            float v = (pivot.z - origin.z) / surface.size.z;
            float heightError = Mathf.Abs(pivot.y - (surface.GetInterpolatedHeight(u, v) + origin.y));
            float slope = surface.GetSteepness(u, v);
            maxHeightError = Mathf.Max(maxHeightError, heightError);
            maxSlope = Mathf.Max(maxSlope, slope);
            if (heightError > 0.19f || slope > 53 || pivot.y < 1.4f)
                invalidSurface++;
            Bounds mesh = meshBounds[i % 5];
            Vector3 scale = scales[i / 5];
            for (int corner = 0; corner < 8; corner++)
            {
                var sign = new Vector3((corner & 1) == 0 ? -1 : 1, (corner & 2) == 0 ? -1 : 1, (corner & 4) == 0 ? -1 : 1);
                Vector3 world = matrix.MultiplyPoint3x4(Vector3.Scale(mesh.center + Vector3.Scale(mesh.extents, sign), scale));
                Vector3 margin = new Vector3(0.12f, 0, 0.12f);
                if (!bounds.Contains(world + margin) || !bounds.Contains(world - margin))
                    outside++;
            }
            if (bounds.extents.x <= 0 || bounds.extents.z <= 0)
                invertedBounds++;
        }
    }
}
Verify("bounds cobrem deslocamento máximo", outside == 0 && invertedBounds == 0, new { outside, invertedBounds });
Verify("altura, inclinação e nível de água", invalidSurface == 0, new { invalidSurface, maxHeightError, maxSlope });
Verify("raízes únicas nos limites das regiões", duplicates == 0, new { duplicates, roots = roots.Count });
var windId = Shader.PropertyToID("_FreyjaWind");
wind.enabled = false;
Verify("desativar controlador limpa o vento", Shader.GetGlobalVector(windId) == Vector4.zero);
wind.enabled = true;
Verify("reativar controlador republica o vento", GlobalWindController.Active == wind && Shader.GetGlobalVector(windId).z > 0);
Verify("três perfis presentes", profiles.All(count => count > 0), profiles);
int legacy = UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
    .Count(renderer => renderer.enabled && renderer.sharedMaterials.Any(material => material && material.shader.name == "Freyja/Coastal Grass"));
Verify("zero relva antiga em renderers ativos", legacy == 0, legacy);

var folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../../specs/078-grama-vento-global/evidence"));
Directory.CreateDirectory(folder);
var noise = (GrassDensityLayout)grassSettings.FindProperty("densityLayout").objectReferenceValue;
if (noise)
{
    float min = float.PositiveInfinity;
    float max = 0;
    float seamDifference = 0;
    const int resolution = 160;
    var map = new Texture2D(resolution, resolution, TextureFormat.RGB24, false);
    var pixels = new Color[resolution * resolution];
    for (int y = 0; y < resolution; y++)
    for (int x = 0; x < resolution; x++)
    {
        var point = origin + new Vector3(x * surface.size.x / (resolution - 1), 0, y * surface.size.z / (resolution - 1));
        float coverage = noise.SampleCoverage(point);
        min = Mathf.Min(min, coverage);
        max = Mathf.Max(max, coverage);
        pixels[y * resolution + x] = Color.Lerp(new Color(0.09f, 0.18f, 0.13f), new Color(0.82f, 0.91f, 0.61f), coverage);
    }
    for (int x = -7; x <= 7; x++)
    for (int z = -7; z <= 7; z++)
    {
        var point = new Vector3(x * 8, 0, z * 8 + 1.75f);
        seamDifference = Mathf.Max(seamDifference, Mathf.Abs(
            noise.SampleCoverage(point - Vector3.right * 0.005f) -
            noise.SampleCoverage(point + Vector3.right * 0.005f)));
    }
    Verify("ruído espacial contínuo, com variação de densidade", max - min > 0.2f && seamDifference < 0.02f,
        new { min, max, seamDifference });
    map.SetPixels(pixels);
    map.Apply();
    File.WriteAllBytes(Path.Combine(folder, "mapa-ruido.png"), map.EncodeToPNG());
    UnityEngine.Object.Destroy(map);
}

var data = Newtonsoft.Json.JsonConvert.SerializeObject(results, Newtonsoft.Json.Formatting.Indented);
File.WriteAllText(Path.Combine(folder, "placement.json"), data);
return data;
