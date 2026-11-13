// Executar pelo MCP Unity: eval com file apontando para este arquivo.
// Monta somente a segunda iteracao visual; nao adiciona gameplay.
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;

var scenePath = "Assets/_Project/Scenes/Cais_Iteracao02.unity";
var current = SceneManager.GetActiveScene();
if (EditorApplication.isPlaying) throw new InvalidOperationException("Sair do Play Mode antes de montar a cena.");
if (current.isDirty && current.name != "Cais_Iteracao02") throw new InvalidOperationException("A cena ativa tem alteracoes nao salvas; preservar antes de continuar.");
if (File.Exists(scenePath)) throw new InvalidOperationException("A cena de revisao ja existe; nao sobrescrever silenciosamente.");

void EnsureFolder(string path)
{
    if (AssetDatabase.IsValidFolder(path)) return;
    var parent = Path.GetDirectoryName(path).Replace('\\', '/');
    EnsureFolder(parent);
    AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
}

EnsureFolder("Assets/_Project/Scenes");
EnsureFolder("Assets/_Project/Prefabs");
EnsureFolder("Assets/_Project/Art/Materials/Cais");
var modelPath = "Assets/_Project/Art/Environment/Cais/Cais_Iteracao01.fbx";
AssetDatabase.ImportAsset(modelPath, ImportAssetOptions.ForceSynchronousImport);
var importer = (ModelImporter)AssetImporter.GetAtPath(modelPath);
importer.globalScale = 1f;
importer.importCameras = false;
importer.importLights = false;
importer.importAnimation = false;
importer.isReadable = true; // MeshCollider e inspecao de escala nesta revisao.
importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
var shader = Shader.Find("Standard");
if (shader == null) throw new InvalidOperationException("Shader Built-in Standard indisponivel.");
var manifest = JObject.Parse(File.ReadAllText("Assets/_Project/Art/Environment/Cais/Cais_Iteracao01.materials.json"));
foreach (var entry in (JArray)manifest["materials"])
{
    var name = (string)entry["name"];
    var materialPath = "Assets/_Project/Art/Materials/Cais/" + name + ".mat";
    var mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
    if (mat == null) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, materialPath); }
    mat.color = new Color((float)entry["r"], (float)entry["g"], (float)entry["b"], 1f).gamma;
    mat.SetFloat("_Glossiness", 0.08f);
    mat.SetFloat("_Metallic", 0f);
    EditorUtility.SetDirty(mat);
    importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), name), mat);
}
importer.SaveAndReimport();
AssetDatabase.SaveAssets();
var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
if (model == null) throw new InvalidOperationException("Modelo nao importado.");

var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
scene.name = "Cais_Iteracao02";
var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
instance.name = "Cais_VisualReview";
foreach (var mesh in instance.GetComponentsInChildren<MeshFilter>())
{
    var name = mesh.gameObject.name;
    if (name.StartsWith("Cais_tabua") || name.StartsWith("Degrau_cais")) mesh.gameObject.AddComponent<BoxCollider>();
    else if (name.StartsWith("Encosta_lowpoly") || name.StartsWith("Inicio_do_caminho") || name.StartsWith("Praia_de_desembarque"))
        mesh.gameObject.AddComponent<MeshCollider>();
    mesh.gameObject.isStatic = true;
}
PrefabUtility.SaveAsPrefabAssetAndConnect(instance, "Assets/_Project/Prefabs/Cais_VisualReview.prefab", InteractionMode.AutomatedAction);

var hull = instance.GetComponentsInChildren<Renderer>().First(r => r.name.StartsWith("Barco_casco_aberto"));
var sea = instance.GetComponentsInChildren<Renderer>().First(r => r.name == "Mar" || r.name.StartsWith("Mar."));
var sx = Mathf.Sign(hull.bounds.center.x);
var sz = Mathf.Sign(hull.bounds.center.z);
var target = new Vector3(sx * 0.7f, 0.4f, sz * 0.6f);
var cameraObject = new GameObject("Review Camera", typeof(Camera), typeof(AudioListener));
var camera = cameraObject.GetComponent<Camera>();
cameraObject.tag = "MainCamera";
camera.transform.position = new Vector3(sx * 18f, 20f, sz * 25f);
camera.transform.LookAt(target);
camera.orthographic = true;
camera.orthographicSize = 9.375f;
camera.nearClipPlane = 0.1f;
camera.farClipPlane = 200f;
camera.clearFlags = CameraClearFlags.SolidColor;
camera.backgroundColor = new Color(0.53f, 0.70f, 0.79f);
camera.allowHDR = false;
camera.allowMSAA = false;

var sunObject = new GameObject("Coastal Sun", typeof(Light));
var sun = sunObject.GetComponent<Light>();
sun.type = LightType.Directional;
sun.transform.rotation = Quaternion.Euler(45f, -35f * sx, 0f);
sun.color = new Color(1f, 0.95f, 0.85f);
sun.intensity = 1.15f;
sun.shadows = LightShadows.Soft;
sun.shadowStrength = 0.7f;
RenderSettings.sun = sun;
RenderSettings.ambientMode = AmbientMode.Trilight;
RenderSettings.ambientSkyColor = new Color(0.58f, 0.66f, 0.72f);
RenderSettings.ambientEquatorColor = new Color(0.43f, 0.46f, 0.44f);
RenderSettings.ambientGroundColor = new Color(0.27f, 0.29f, 0.27f);
RenderSettings.skybox = null;
RenderSettings.fog = false;
if (SceneView.lastActiveSceneView != null)
{
    var view = SceneView.lastActiveSceneView;
    view.pivot = target;
    view.rotation = camera.transform.rotation;
    view.size = 18f;
    view.orthographic = true;
    view.Repaint();
}
EditorSceneManager.SaveScene(scene, scenePath);
AssetDatabase.SaveAssets();
Selection.activeGameObject = instance;

var evidence = Path.GetFullPath("../Source/Unity");
Directory.CreateDirectory(evidence);
var previous = RenderTexture.active;
var rt = new RenderTexture(1280, 960, 24, RenderTextureFormat.ARGB32);
var image = new Texture2D(1280, 960, TextureFormat.RGB24, false);
try
{
    camera.targetTexture = rt;
    camera.Render();
    RenderTexture.active = rt;
    image.ReadPixels(new Rect(0, 0, 1280, 960), 0, 0);
    image.Apply();
    File.WriteAllBytes(Path.Combine(evidence, "Cais_Iteracao02.png"), image.EncodeToPNG());
}
finally
{
    camera.targetTexture = null;
    RenderTexture.active = previous;
    UnityEngine.Object.DestroyImmediate(rt);
    UnityEngine.Object.DestroyImmediate(image);
}
EditorSceneManager.SaveScene(scene, scenePath);
return "Cena: " + scenePath + "; meshes: " + instance.GetComponentsInChildren<MeshFilter>().Length
    + "; colliders: " + instance.GetComponentsInChildren<Collider>().Length
    + "; barco: " + hull.bounds.center + " dimensoes " + hull.bounds.size
    + "; mar: " + sea.bounds.size + "; preview: " + evidence;
