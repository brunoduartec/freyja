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

var terrain = island.GetComponentInChildren<Terrain>();
Vector2Int DensityCell(Vector3 point) => new(Mathf.FloorToInt(point.x * 8), Mathf.FloorToInt(point.z * 8));
var occupied = new HashSet<Vector2Int>(bakedRoots.Select(DensityCell));
foreach (var mesh in tufts.Where((mesh, index) => valid[index]))
    occupied.Add(DensityCell(mesh.transform.position));

var candidates = new List<(int region, GrassDensityLayout.Tuft tuft)>();
int sourceIndex = 0;
for (int region = 0; region < existingLayout.Cells.Length; region++)
{
    foreach (var batch in existingLayout.Cells[region].Batches)
    foreach (var matrix in batch.Instances)
    {
        int current = sourceIndex++;
        Vector3 root = bakedRoots[current];
        if (!bakedValid[current] || root.y < 2.4f)
            continue;

        for (int attempt = 0; attempt < 5; attempt++)
        {
            int salt = 1741 + attempt * 257;
            float angle = CoastalGrass.PlacementScore(root, salt) * Mathf.PI * 2;
            float radius = Mathf.Lerp(0.16f, 0.36f, CoastalGrass.PlacementScore(root, salt + 9));
            Vector3 point = root + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            float noise = density.SampleCoverage(point);
            if (CoastalGrass.PlacementScore(point, salt + 23) >= noise || !occupied.Add(DensityCell(point)))
                continue;

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
}

bool[] validDensity = CoastalGrassAuthoring.FilterGrassSites(island, candidates.Select(item => item.tuft.Root).ToArray(), false);
var byRegion = candidates.Where((item, index) => validDensity[index]).ToLookup(item => item.region, item => item.tuft);
var densityRegions = Enumerable.Range(0, existingLayout.Cells.Length)
    .Select(region => new GrassDensityLayout.Region { Tufts = byRegion[region].ToArray() }).ToArray();
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
return new { scene = scene.path, grass = grass.name, localLights = localLights.Length, authoredTufts = tufts.Length, migratedTufts = valid.Count(value => value), bakedTufts = bakedRoots.Length, blockedBaked = bakedValid.Count(value => !value), addedCandidates = validDensity.Count(value => value) };
