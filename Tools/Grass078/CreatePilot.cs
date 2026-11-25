using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using Freyja.Environment;

const string scenePath = "Assets/_Project/Scenes/Grama_Piloto_078.unity";
const string artPath = "Assets/_Project/Art/Grass029/";
if (Application.isPlaying || SceneManager.GetActiveScene().isDirty)
    throw new System.InvalidOperationException("Save and stop the active scene before creating the grass pilot.");
if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath))
    throw new System.InvalidOperationException("The pilot scene already exists.");

var shader = Shader.Find("Freyja/Lit Coastal Grass");
if (!shader || !shader.isSupported)
    throw new System.InvalidOperationException("The Built-in grass shader is unavailable.");

var material = AssetDatabase.LoadAssetAtPath<Material>(artPath + "LitCoastalGrass.mat");
if (!material)
{
    material = new Material(shader) { enableInstancing = true };
    AssetDatabase.CreateAsset(material, artPath + "LitCoastalGrass.mat");
}

var meshes = AssetDatabase.LoadAllAssetsAtPath(artPath + "CoastalGrass.fbx").OfType<Mesh>().ToDictionary(mesh => mesh.name);
var layout = ScriptableObject.CreateInstance<CoastalGrassLayout>();
var cells = new List<CoastalGrassLayout.Cell>();
var random = new System.Random(78);
for (int row = -1; row < 1; row++)
for (int column = -1; column < 1; column++)
{
    const int side = 8;
    var batches = Enumerable.Range(0, 3).Select(index => new List<Matrix4x4>()).ToArray();
    for (int i = 0; i < 1400; i++)
    {
        float x = column * side + (float)random.NextDouble() * side;
        float z = row * side + (float)random.NextDouble() * side;
        if (Mathf.Abs(x) < 0.7f || Mathf.Abs(x - 4.7f) < 0.9f && Mathf.Abs(z) < 1.2f)
            continue;
        float size = Mathf.Lerp(0.7f, 1.2f, (float)random.NextDouble());
        batches[i % batches.Length].Add(Matrix4x4.TRS(new Vector3(x, 0, z),
            Quaternion.Euler(0, (float)random.NextDouble() * 360, 0), Vector3.one * size));
    }
    cells.Add(new CoastalGrassLayout.Cell
    {
        Bounds = new Bounds(new Vector3((column + 0.5f) * side, 0.25f, (row + 0.5f) * side), new Vector3(side + 0.5f, 1, side + 0.5f)),
        Batches = batches.Select((batch, index) => new CoastalGrassLayout.Batch
        {
            MeshIndex = index, Instances = batch.ToArray()
        }).ToArray()
    });
}

layout.SetCells(cells.ToArray());
var previous = SceneManager.GetActiveScene();
var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
SceneManager.SetActiveScene(scene);
try
{
    var camera = new GameObject("Pilot Gameplay Camera", typeof(Camera)).GetComponent<Camera>();
    camera.tag = "MainCamera";
    camera.transform.position = new Vector3(5.2f, 4.2f, -11f);
    camera.transform.LookAt(new Vector3(0.5f, 0.1f, 0));
    camera.fieldOfView = 55;
    camera.clearFlags = CameraClearFlags.SolidColor;
    camera.backgroundColor = new Color(0.29f, 0.38f, 0.48f);
    camera.targetTexture = AssetDatabase.LoadAssetAtPath<RenderTexture>("Assets/_Project/Art/Ilha_Tripo_011/Mundo_480x270.renderTexture");

    RenderSettings.ambientMode = AmbientMode.Trilight;
    RenderSettings.ambientSkyColor = new Color(0.55f, 0.62f, 0.67f);
    RenderSettings.ambientEquatorColor = new Color(0.35f, 0.42f, 0.49f);
    RenderSettings.ambientGroundColor = new Color(0.24f, 0.26f, 0.19f);
    var sun = new GameObject("Pilot Sun", typeof(Light)).GetComponent<Light>();
    sun.type = LightType.Directional;
    sun.intensity = 1.1f;
    sun.color = new Color(1f, 0.91f, 0.76f);
    sun.shadows = LightShadows.Hard;
    sun.transform.rotation = Quaternion.Euler(38, -65, 0);
    RenderSettings.sun = sun;
    var local = new GameObject("Pilot Lamp", typeof(Light)).GetComponent<Light>();
    local.type = LightType.Point;
    local.transform.position = new Vector3(4.1f, 1.5f, -2.6f);
    local.range = 4.7f;
    local.intensity = 2.2f;
    local.color = new Color(1, 0.62f, 0.27f);
    local.enabled = false;

    var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
    floor.name = "Pilot Ground";
    floor.transform.localScale = new Vector3(1.6f, 1, 1.6f);
    var blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
    blocker.name = "Pilot Shadow Caster";
    blocker.transform.position = new Vector3(4.7f, 1.1f, 0);
    blocker.transform.localScale = new Vector3(1.5f, 2.2f, 2);
    new GameObject("Global Wind", typeof(GlobalWindController));
    var grass = new GameObject("Pilot Grass", typeof(CoastalGrass)).GetComponent<CoastalGrass>();
    grass.Configure(camera, layout, AssetDatabase.LoadAssetAtPath<Material>(artPath + "CoastalGrass.mat"),
        new[] { meshes["CoastalTuftFan"], meshes["CoastalTuftTall"], meshes["CoastalTuftSimple"] });
    string layoutPath = artPath + "PilotLayout078.asset";
    AssetDatabase.CreateAsset(layout, layoutPath);
    var settings = new SerializedObject(grass);
    settings.FindProperty("litMaterial").objectReferenceValue = material;
    settings.FindProperty("coastalElevation").floatValue = 0;
    settings.FindProperty("wildElevation").floatValue = 0;
    SerializedProperty lights = settings.FindProperty("localLights");
    lights.arraySize = 1;
    lights.GetArrayElementAtIndex(0).objectReferenceValue = local;
    settings.ApplyModifiedPropertiesWithoutUndo();
    if (!EditorSceneManager.SaveScene(scene, scenePath))
        throw new System.InvalidOperationException("Could not save the grass pilot.");
    AssetDatabase.SaveAssets();
}
finally
{
    SceneManager.SetActiveScene(previous);
    EditorSceneManager.CloseScene(scene, true);
}
return scenePath;
