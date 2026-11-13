// Execute through the Unity MCP only after the final Blender export is confirmed.
// Creates the scene-specific integration for spec 003. This script intentionally
// refuses dirty source scenes, missing FBX nodes, or existing output prefabs.
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

const string sourceScenePath = "Assets/_Project/Scenes/Cais_Caminhada.unity";
const string targetScenePath = "Assets/_Project/Scenes/Ilha_Bruta_003.unity";
const string fbxPath = "Assets/_Project/Art/Environment/IlhaBruta/Ilha_Bruta_003.fbx";
const string prefabFolder = "Assets/_Project/Prefabs";
const string prefabPath = prefabFolder + "/IlhaBruta.prefab";
const string terrainName = "Terreno_Ilha_Caminhavel_260x180";
const string oldTerrainName = "Encosta_lowpoly.001";
const string oldSeaName = "Mar.001";
var blockNames = new[] { "B1", "B2", "B3", "B4", "A1", "A2", "A3", "A4", "L1", "L2", "L3", "L4" };

if (EditorApplication.isPlaying) return "ABORT: encerre o Play Mode antes de integrar.";
var sourceScene = SceneManager.GetActiveScene();
if (sourceScene.path != sourceScenePath) return "ABORT: a cena ativa deve ser Cais_Caminhada.unity; ativa=" + sourceScene.path;
if (sourceScene.isDirty) return "ABORT: Cais_Caminhada.unity tem alterações não salvas.";
if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath)) return "ABORT: prefab já existe; preservado: " + prefabPath;

AssetDatabase.Refresh();
var fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
if (!fbxAsset) return "ABORT: FBX não importado: " + fbxPath;

var modelTransforms = fbxAsset.GetComponentsInChildren<Transform>(true);
var terrainAssetNode = modelTransforms.SingleOrDefault(t => t.name == terrainName);
if (!terrainAssetNode || !terrainAssetNode.GetComponent<MeshFilter>()?.sharedMesh)
    return "ABORT: nó de terreno ausente ou sem MeshFilter: " + terrainName;
var missingBlocks = blockNames.Where(name =>
    modelTransforms.Count(t => t.name == name && t.GetComponent<MeshFilter>()?.sharedMesh) != 1).ToArray();
if (missingBlocks.Length > 0)
    return "ABORT: esperado um MeshFilter por bloco; ausentes/duplicados=" + string.Join(", ", missingBlocks);

var prefabDirectory = AssetDatabase.LoadAssetAtPath<DefaultAsset>(prefabFolder);
if (!prefabDirectory) AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");

var target = EditorSceneManager.OpenScene(targetScenePath, OpenSceneMode.Single);
if (!target.IsValid()) return "ABORT: não foi possível abrir a cena destino.";
var caisRoot = target.GetRootGameObjects().SingleOrDefault(go => go.name == "Cais_VisualReview");
var player = target.GetRootGameObjects().SingleOrDefault(go => go.name == "PlayerPrototype");
var cameraRoot = target.GetRootGameObjects().SingleOrDefault(go => go.name == "Cais Gameplay Camera");
var sessionRoot = target.GetRootGameObjects().SingleOrDefault(go => go.name == "Cais Session");
if (!caisRoot || !player || !cameraRoot || !sessionRoot)
    return "ABORT: a cena 003 não contém as raízes esperadas do cais, jogador, câmera e sessão.";
if (target.GetRootGameObjects().Any(go => go.name == "IlhaBruta" || go.name == "Mar_IlhaBrutaVisual"))
    return "ABORT: a cena já contém objetos da integração.";

Transform FindSceneChild(GameObject root, string name) =>
    root.GetComponentsInChildren<Transform>(true).SingleOrDefault(t => t.name == name);
var oldTerrain = FindSceneChild(caisRoot, oldTerrainName);
var oldSea = FindSceneChild(caisRoot, oldSeaName);
var oldSeaRenderer = oldSea ? oldSea.GetComponent<Renderer>() : null;
var oldSeaMaterial = oldSeaRenderer ? oldSeaRenderer.sharedMaterial : null;
if (!oldTerrain || !oldSea || !oldSeaMaterial)
    return "ABORT: não encontrei terreno/mar/material na instância Cais_VisualReview.";

var solidLayer = LayerMask.NameToLayer("CaisSolid");
if (solidLayer < 0) return "ABORT: layer CaisSolid não existe.";
var camera = cameraRoot.GetComponentInChildren<Camera>(true);
var session = sessionRoot.GetComponentsInChildren<MonoBehaviour>(true).FirstOrDefault(c => c && c.GetType().Name == "CaisSessionController");
if (!camera || !session) return "ABORT: câmera ou CaisSessionController ausente.";
var sessionData = new SerializedObject(session);
var boundsProperty = sessionData.FindProperty("recoveryBounds");
if (boundsProperty == null || boundsProperty.propertyType != SerializedPropertyType.Bounds)
    return "ABORT: campo recoveryBounds ausente no CaisSessionController.";

var islandRoot = new GameObject("IlhaBruta");
SceneManager.MoveGameObjectToScene(islandRoot, target);
var modelInstance = PrefabUtility.InstantiatePrefab(fbxAsset, target) as GameObject;
if (!modelInstance) { Object.DestroyImmediate(islandRoot); return "ABORT: não foi possível instanciar o FBX."; }
modelInstance.transform.SetParent(islandRoot.transform, false);
islandRoot.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
islandRoot.transform.localScale = Vector3.one;

Transform FindModelChild(string name) =>
    modelInstance.GetComponentsInChildren<Transform>(true).SingleOrDefault(t => t.name == name);
GameObject CreateColliderObject(Transform modelNode, string suffix) {
    var colliderObject = new GameObject(modelNode.name + suffix);
    colliderObject.transform.SetParent(islandRoot.transform, false);
    colliderObject.transform.position = modelNode.position;
    colliderObject.transform.rotation = modelNode.rotation;
    colliderObject.transform.localScale = modelNode.lossyScale;
    colliderObject.layer = solidLayer;
    return colliderObject;
}
var terrain = FindModelChild(terrainName);
var terrainMesh = terrain ? terrain.GetComponent<MeshFilter>()?.sharedMesh : null;
if (!terrain || !terrainMesh) { Object.DestroyImmediate(islandRoot); return "ABORT: terreno não encontrado na instância FBX."; }
var terrainColliderObject = CreateColliderObject(terrain, "_Collider");
var terrainCollider = terrainColliderObject.AddComponent<MeshCollider>();
terrainCollider.convex = false;
terrainCollider.sharedMesh = terrainMesh;

foreach (var name in blockNames) {
    var node = FindModelChild(name);
    var mesh = node ? node.GetComponent<MeshFilter>()?.sharedMesh : null;
    if (!node || !mesh) { Object.DestroyImmediate(islandRoot); return "ABORT: bloco ausente na instância FBX: " + name; }
    var colliderObject = CreateColliderObject(node, "_Collider");
    var collider = colliderObject.AddComponent<BoxCollider>();
    collider.center = mesh.bounds.center;
    collider.size = mesh.bounds.size;
}

var terrainRenderer = terrain.GetComponent<Renderer>();
if (!terrainRenderer) { Object.DestroyImmediate(islandRoot); return "ABORT: Renderer do terreno ausente."; }
var terrainBounds = terrainRenderer.bounds;
var expandedBounds = terrainBounds;
expandedBounds.Expand(new Vector3(40f, 80f, 40f));
var spawnRoot = target.GetRootGameObjects().SingleOrDefault(go => go.name == "SpawnPoint");
var spawn = spawnRoot ? FindSceneChild(spawnRoot, "SpawnPoint") : null;
if (!spawn || !expandedBounds.Contains(spawn.position)) {
    Object.DestroyImmediate(islandRoot);
    return "ABORT: spawn existente não está dentro dos novos recoveryBounds.";
}

// Disable only the overlapping ground and sea in this scene instance. Keep dock,
// landing beach, stairs, rocks, boat, player, and their shared prefab assets intact.
oldTerrain.gameObject.SetActive(false);
oldSea.gameObject.SetActive(false);

var water = GameObject.CreatePrimitive(PrimitiveType.Plane);
water.name = "Mar_IlhaBrutaVisual";
var waterCollider = water.GetComponent<Collider>();
if (waterCollider) Object.DestroyImmediate(waterCollider);
water.transform.position = new Vector3(terrainBounds.center.x, -0.25f, terrainBounds.center.z);
water.transform.localScale = new Vector3(60f, 1f, 60f);
water.GetComponent<Renderer>().sharedMaterial = oldSeaMaterial;
SceneManager.MoveGameObjectToScene(water, target);

var cameraDistance = terrainBounds.size.magnitude + 30f;
camera.farClipPlane = Mathf.Max(camera.farClipPlane, cameraDistance);
sessionData.Update();
boundsProperty = sessionData.FindProperty("recoveryBounds");
boundsProperty.boundsValue = expandedBounds;
sessionData.ApplyModifiedPropertiesWithoutUndo();

bool prefabSaved;
PrefabUtility.SaveAsPrefabAssetAndConnect(islandRoot, prefabPath, InteractionMode.AutomatedAction, out prefabSaved);
if (!prefabSaved) return "ERROR: falha ao salvar prefab; cena ainda não foi salva.";
EditorSceneManager.MarkSceneDirty(target);
if (!EditorSceneManager.SaveScene(target)) return "ERROR: prefab criado, mas falha ao salvar a cena destino.";
AssetDatabase.SaveAssets();
AssetDatabase.Refresh();
Physics.SyncTransforms();

return "OK: prefab=" + prefabPath
    + "; cena=" + target.path
    + "; terreno=" + terrainName + " MeshCollider/CaisSolid"
    + "; blocos=" + string.Join(",", blockNames) + " BoxCollider/CaisSolid"
    + "; antigos desativados=" + oldTerrainName + "," + oldSeaName
    + "; água visual sem collider, escala=600m"
    + "; recoveryBounds=" + expandedBounds
    + "; cameraFarClip=" + camera.farClipPlane
    + "; cais, barco, jogador, controles e spawn preservados.";
