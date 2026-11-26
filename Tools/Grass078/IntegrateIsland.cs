using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using Freyja.Environment;
using Freyja.Editor;

const string path = "Assets/_Project/Scenes/Ilha_Principal.unity";
if (Application.isPlaying || SceneManager.GetActiveScene().isDirty)
    throw new System.InvalidOperationException("Stop Play Mode and save any pending scene work.");
var scene = SceneManager.GetActiveScene().path == path ? SceneManager.GetActiveScene() : EditorSceneManager.OpenScene(path);
var grass = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<CoastalGrass>(true)).Single();
var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Grass029/LitCoastalGrass.mat");
if (!material || !material.enableInstancing)
    throw new System.InvalidOperationException("An instanced Built-in grass material is required.");

var settings = new SerializedObject(grass);
settings.FindProperty("litMaterial").objectReferenceValue = material;
var island = scene.GetRootGameObjects().Single(root => root.name == "Island023").transform;
var vegetation = scene.GetRootGameObjects().Single(root => root.name == "Vegetação 032");
var archive = vegetation.GetComponentsInChildren<Transform>(true).Single(node => node.name == "Cobertura base");
var tufts = archive.GetComponentsInChildren<MeshFilter>(true).Where(mesh => mesh.name == "Tufo costeiro").ToArray();
if (tufts.Length == 0 || tufts.Any(mesh => !mesh.sharedMesh || !mesh.GetComponent<MeshRenderer>()))
    throw new System.InvalidOperationException("The authored grass group contains unexpected renderers.");
bool[] valid = CoastalGrassAuthoring.FilterGrassSites(island, tufts.Select(mesh => mesh.transform.position).ToArray(), true);
var names = new[] { "CoastalTuftFan", "CoastalTuftTall", "CoastalTuftSimple" };
var authoredCells = tufts.Where((mesh, index) => valid[index])
    .GroupBy(mesh => new Vector2Int(Mathf.FloorToInt(mesh.transform.position.x / 8), Mathf.FloorToInt(mesh.transform.position.z / 8)))
    .Select(cell =>
    {
        var bounds = new Bounds(cell.First().transform.position, Vector3.one);
        foreach (var mesh in cell)
            bounds.Encapsulate(new Bounds(mesh.transform.position, Vector3.one));
        return new CoastalGrassLayout.Cell
        {
            Bounds = bounds,
            Batches = cell.GroupBy(mesh => System.Array.IndexOf(names, mesh.sharedMesh.name)).Select(batch => new CoastalGrassLayout.Batch
            {
                MeshIndex = batch.Key, Instances = batch.Select(mesh => mesh.transform.localToWorldMatrix).ToArray()
            }).ToArray()
        };
    }).ToArray();

const string authoredPath = "Assets/_Project/Art/Grass029/AuthoredCoastalTufts078.asset";
var authoredLayout = AssetDatabase.LoadAssetAtPath<CoastalGrassLayout>(authoredPath);
if (!authoredLayout)
{
    authoredLayout = ScriptableObject.CreateInstance<CoastalGrassLayout>();
    AssetDatabase.CreateAsset(authoredLayout, authoredPath);
}
Undo.RecordObject(authoredLayout, "Migrate authored coastal tufts");
authoredLayout.SetCells(authoredCells);
EditorUtility.SetDirty(authoredLayout);
AssetDatabase.SaveAssetIfDirty(authoredLayout);
var existingLayout = (CoastalGrassLayout)settings.FindProperty("layout").objectReferenceValue;
Vector3[] bakedRoots = existingLayout.Cells.SelectMany(cell => cell.Batches).SelectMany(batch => batch.Instances)
    .Select(matrix => (Vector3)matrix.GetColumn(3)).ToArray();
bool[] bakedValid = CoastalGrassAuthoring.FilterGrassSites(island, bakedRoots, false);
const string maskPath = "Assets/_Project/Art/Grass029/IslandSurfaceMask078.asset";
var mask = AssetDatabase.LoadAssetAtPath<GrassSurfaceMask>(maskPath);
if (!mask)
{
    mask = ScriptableObject.CreateInstance<GrassSurfaceMask>();
    AssetDatabase.CreateAsset(mask, maskPath);
}
Undo.RecordObject(mask, "Update grass surface mask");
mask.Configure(bakedRoots.Where((root, index) => !bakedValid[index]));
EditorUtility.SetDirty(mask);
AssetDatabase.SaveAssetIfDirty(mask);
settings.FindProperty("surfaceMask").objectReferenceValue = mask;

const string densityPath = "Assets/_Project/Art/Grass029/IslandDensity078.asset";
var density = AssetDatabase.LoadAssetAtPath<GrassDensityLayout>(densityPath);
if (!density)
{
    density = ScriptableObject.CreateInstance<GrassDensityLayout>();
    AssetDatabase.CreateAsset(density, densityPath);
}
var densitySettings = new SerializedObject(density);
var savedDensitySource = densitySettings.FindProperty("source").objectReferenceValue;
var savedDensityRegions = densitySettings.FindProperty("regions");
if (savedDensitySource != existingLayout || savedDensityRegions.arraySize != existingLayout.Cells.Length)
    throw new System.InvalidOperationException("The existing 078 density layout does not match the base grass regions; refusing to discard it.");
var preservedDensityRegions = new GrassDensityLayout.Region[savedDensityRegions.arraySize];
for (int region = 0; region < preservedDensityRegions.Length; region++)
{
    var tuftProperty = savedDensityRegions.GetArrayElementAtIndex(region).FindPropertyRelative("Tufts");
    var tufts = new GrassDensityLayout.Tuft[tuftProperty.arraySize];
    for (int i = 0; i < tufts.Length; i++)
    {
        var tuft = tuftProperty.GetArrayElementAtIndex(i);
        tufts[i] = new GrassDensityLayout.Tuft
        {
            Root = tuft.FindPropertyRelative("Root").vector3Value,
            Yaw = tuft.FindPropertyRelative("Yaw").floatValue,
            Scale = tuft.FindPropertyRelative("Scale").floatValue,
            MeshIndex = tuft.FindPropertyRelative("MeshIndex").intValue
        };
    }
    preservedDensityRegions[region] = new GrassDensityLayout.Region { Tufts = tufts };
}
const int expected078SupplementRoots = 72770;
int preservedSupplementRoots = preservedDensityRegions.Sum(region => region.Tufts.Length);
if (preservedSupplementRoots != expected078SupplementRoots)
    throw new System.InvalidOperationException($"Expected the untouched 078 source ({expected078SupplementRoots} supplements), found {preservedSupplementRoots}. Restore the 078 asset before rebaking.");

var terrain = island.GetComponentInChildren<Terrain>();
Vector2Int DensityCell(Vector3 point) => new(Mathf.FloorToInt(point.x * 10), Mathf.FloorToInt(point.z * 10));
Vector2Int RootKey(Vector3 point) => new(Mathf.RoundToInt(point.x * 1024), Mathf.RoundToInt(point.z * 1024));
var occupied = new HashSet<Vector2Int>(bakedRoots.Select(DensityCell));
var occupiedRootKeys = new HashSet<Vector2Int>(bakedRoots.Select(RootKey));
foreach (var mesh in tufts.Where((mesh, index) => valid[index]))
{
    occupied.Add(DensityCell(mesh.transform.position));
    occupiedRootKeys.Add(RootKey(mesh.transform.position));
}
foreach (var region in preservedDensityRegions)
foreach (var tuft in region.Tufts)
{
    occupied.Add(DensityCell(tuft.Root));
    occupiedRootKeys.Add(RootKey(tuft.Root));
}

// Keep all 078 supplements, then deterministically fill around base and
// supplemental roots under the same noise cutoff and 1 mm uniqueness key.
const int samplesPerRoot = 32;
const int samplesPerExistingSupplement = 32;
const int placementSeed = 1741;
var candidates = new List<(int region, GrassDensityLayout.Tuft tuft)>();
void Propose(int region, Vector3 root, int attempts, int seedOffset, float minRadius, float maxRadius)
{
    for (int attempt = 0; attempt < attempts; attempt++)
    {
        int salt = placementSeed + seedOffset + attempt * 257;
        float angle = CoastalGrass.PlacementScore(root, salt) * Mathf.PI * 2;
        float radius = Mathf.Lerp(minRadius, maxRadius, CoastalGrass.PlacementScore(root, salt + 9));
        Vector3 point = root + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
        float noise = density.SampleCoverage(point);
        Vector2Int densityCell = DensityCell(point);
        Vector2Int rootKey = RootKey(point);
        if (CoastalGrass.PlacementScore(point, salt + 23) >= noise ||
            occupied.Contains(densityCell) || occupiedRootKeys.Contains(rootKey))
            continue;

        occupied.Add(densityCell);
        occupiedRootKeys.Add(rootKey);
        point.y = terrain.SampleHeight(point) + terrain.transform.position.y - 0.005f;
        float style = CoastalGrass.PlacementScore(point, 82);
        candidates.Add((region, new GrassDensityLayout.Tuft
        {
            Root = point,
            Yaw = CoastalGrass.PlacementScore(point, 84) * 360,
            Scale = Mathf.Lerp(0.64f, 1.36f, style * 0.72f + noise * 0.28f),
            MeshIndex = style < 0.22f ? 0 : style > 0.74f ? 1 : 2
        }));
    }
}
int sourceIndex = 0;
for (int region = 0; region < existingLayout.Cells.Length; region++)
    foreach (var batch in existingLayout.Cells[region].Batches)
    foreach (var matrix in batch.Instances)
    {
        int current = sourceIndex++;
        Vector3 root = bakedRoots[current];
        if (bakedValid[current] && root.y >= 2.4f)
            Propose(region, root, samplesPerRoot, 0, 0.18f, 0.85f);
    }
for (int region = 0; region < preservedDensityRegions.Length; region++)
foreach (var tuft in preservedDensityRegions[region].Tufts)
    Propose(region, tuft.Root, samplesPerExistingSupplement, 1000003, 0.22f, 1.4f);

bool[] validDensity = CoastalGrassAuthoring.FilterGrassSites(island, candidates.Select(item => item.tuft.Root).ToArray(), false);
for (int i = 0; i < candidates.Count; i++)
    if (!validDensity[i])
    {
        occupied.Remove(DensityCell(candidates[i].tuft.Root));
        occupiedRootKeys.Remove(RootKey(candidates[i].tuft.Root));
    }
var acceptedCandidates = candidates.Where((item, index) => validDensity[index]).ToList();
var regionByCell = new Dictionary<Vector2Int, int>();
for (int region = 0; region < existingLayout.Cells.Length; region++)
{
    var firstMatrix = existingLayout.Cells[region].Batches.SelectMany(batch => batch.Instances).FirstOrDefault();
    Vector3 point = firstMatrix.GetColumn(3);
    Vector2Int cell = new(Mathf.FloorToInt(point.x / 8), Mathf.FloorToInt(point.z / 8));
    if (!regionByCell.ContainsKey(cell)) regionByCell.Add(cell, region);
}
int baseAndPreservedRoots = bakedValid.Count(value => value) + valid.Count(value => value) + preservedSupplementRoots;
const int targetHighRoots = 348642;
const int globalSampleBatchSize = 4096;
const int maxGlobalSampleAttempts = 10000000;
int globalSampleAttempts = 0;
while (baseAndPreservedRoots + acceptedCandidates.Count < targetHighRoots && globalSampleAttempts < maxGlobalSampleAttempts)
{
    var batch = new List<(int region, GrassDensityLayout.Tuft tuft)>(globalSampleBatchSize / 4);
    for (int sample = 0; sample < globalSampleBatchSize && globalSampleAttempts < maxGlobalSampleAttempts; sample++)
    {
        int index = ++globalSampleAttempts;
        var sampleKey = new Vector3(index * 0.001f, 0, index * 0.00137f);
        float x01 = CoastalGrass.PlacementScore(sampleKey, placementSeed + 191);
        float z01 = CoastalGrass.PlacementScore(sampleKey, placementSeed + 733);
        Vector3 point = terrain.transform.position + new Vector3(x01 * terrain.terrainData.size.x, 0, z01 * terrain.terrainData.size.z);
        Vector2Int cell = new(Mathf.FloorToInt(point.x / 8), Mathf.FloorToInt(point.z / 8));
        if (!regionByCell.TryGetValue(cell, out int region)) continue;

        float noise = density.SampleCoverage(point);
        int salt = placementSeed + index * 17;
        Vector2Int densityCell = DensityCell(point);
        Vector2Int rootKey = RootKey(point);
        if (CoastalGrass.PlacementScore(point, salt + 23) >= noise ||
            occupied.Contains(densityCell) || occupiedRootKeys.Contains(rootKey))
            continue;

        occupied.Add(densityCell);
        occupiedRootKeys.Add(rootKey);
        point.y = terrain.SampleHeight(point) + terrain.transform.position.y - 0.005f;
        float style = CoastalGrass.PlacementScore(point, 82);
        batch.Add((region, new GrassDensityLayout.Tuft
        {
            Root = point,
            Yaw = CoastalGrass.PlacementScore(point, 84) * 360,
            Scale = Mathf.Lerp(0.64f, 1.36f, style * 0.72f + noise * 0.28f),
            MeshIndex = style < 0.22f ? 0 : style > 0.74f ? 1 : 2
        }));
    }

    bool[] validBatch = CoastalGrassAuthoring.FilterGrassSites(island, batch.Select(item => item.tuft.Root).ToArray(), false);
    for (int i = 0; i < batch.Count; i++)
        if (!validBatch[i])
        {
            occupied.Remove(DensityCell(batch[i].tuft.Root));
            occupiedRootKeys.Remove(RootKey(batch[i].tuft.Root));
        }
    acceptedCandidates.AddRange(batch.Where((item, index) => validBatch[index]));
}
int newSupplementRoots = acceptedCandidates.Count;
var byRegion = acceptedCandidates.ToLookup(item => item.region, item => item.tuft);
var densityRegions = Enumerable.Range(0, existingLayout.Cells.Length)
    .Select(region => new GrassDensityLayout.Region
    {
        Tufts = preservedDensityRegions[region].Tufts.Concat(byRegion[region]).ToArray()
    }).ToArray();
int supplementRoots = densityRegions.Sum(region => region.Tufts.Length);
var uniqueHighRoots = new HashSet<Vector2Int>();
var rootOrigins = new Dictionary<Vector2Int, string>();
var duplicateDetails = new List<string>();
void TrackRoot(Vector3 point, string origin)
{
    Vector2Int key = RootKey(point);
    if (rootOrigins.TryGetValue(key, out string previous))
        duplicateDetails.Add($"{key}: {previous} <> {origin}");
    else
    {
        rootOrigins.Add(key, origin);
        uniqueHighRoots.Add(key);
    }
}
for (int i = 0; i < bakedRoots.Length; i++)
    if (bakedValid[i]) TrackRoot(bakedRoots[i], $"base[{i}]");
for (int i = 0; i < tufts.Length; i++)
    if (valid[i]) TrackRoot(tufts[i].transform.position, $"authored[{i}]");
for (int region = 0; region < densityRegions.Length; region++)
for (int tuft = 0; tuft < densityRegions[region].Tufts.Length; tuft++)
    TrackRoot(densityRegions[region].Tufts[tuft].Root, $"supplement[{region}:{tuft}]");
int eligibleRootCandidates = bakedValid.Count(value => value) + valid.Count(value => value) + supplementRoots;
int projectedHighRoots = uniqueHighRoots.Count;
int duplicateRootCandidates = eligibleRootCandidates - projectedHighRoots;
if (duplicateRootCandidates != 0)
    throw new System.InvalidOperationException($"The 080 bake contains {duplicateRootCandidates} duplicate eligible grass roots: {string.Join("; ", duplicateDetails.Take(8))}");
float targetRatio = projectedHighRoots / (float)targetHighRoots;
if (projectedHighRoots < 313778 || projectedHighRoots > 383506)
    throw new System.InvalidOperationException($"Projected High grass roots {projectedHighRoots} ({targetRatio:P1} of {targetHighRoots}) are outside the approved ±10% target; base={bakedValid.Count(value => value)}, authored={valid.Count(value => value)}, preserved078={preservedSupplementRoots}, rootSampleProposals={candidates.Count}, validNew={newSupplementRoots}, globalAttempts={globalSampleAttempts}.");
Undo.RecordObject(density, "Update high density grass layout");
density.Configure(existingLayout, densityRegions);
EditorUtility.SetDirty(density);
AssetDatabase.SaveAssetIfDirty(density);
settings.FindProperty("densityLayout").objectReferenceValue = density;
settings.FindProperty("authoredLayout").objectReferenceValue = authoredLayout;
foreach (var tuft in tufts)
{
    Undo.RecordObject(tuft.gameObject, "Archive authored coastal tuft");
    tuft.gameObject.SetActive(false);
}
Light[] localLights = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Light>(true))
    .Where(light => light.type == LightType.Point).ToArray();
SerializedProperty lights = settings.FindProperty("localLights");
lights.arraySize = localLights.Length;
for (int i = 0; i < localLights.Length; i++)
    lights.GetArrayElementAtIndex(i).objectReferenceValue = localLights[i];
settings.ApplyModifiedProperties();
if (!grass.TryGetComponent<GlobalWindController>(out _))
    Undo.AddComponent<GlobalWindController>(grass.gameObject);
if (!EditorSceneManager.SaveScene(scene))
    throw new System.InvalidOperationException("Could not save the grass configuration in Ilha_Principal.");
return new { scene = scene.path, grass = grass.name, localLights = localLights.Length, authoredTufts = tufts.Length, migratedTufts = valid.Count(value => value), bakedTufts = bakedRoots.Length, blockedBaked = bakedValid.Count(value => !value), preserved078Supplements = preservedSupplementRoots, addedCandidates = newSupplementRoots, eligibleRootCandidates, duplicateRootCandidates, projectedHighRoots, targetHighRoots, targetRatio };
