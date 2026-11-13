// Execute via Unity MCP after confirming the separate character FBX is stable.
// Adds only a scene-level visual child to PlayerPrototype in scene 004. Never
// edits or applies changes to the shared PlayerPrototype prefab asset.
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

const string targetScenePath = "Assets/_Project/Scenes/Ilha_Relevo_004.unity";
const string playerPrefabPath = "Assets/_Project/Prefabs/PlayerPrototype.prefab";
const string visualFbxPath = "Assets/_Project/Art/Characters/Player004/Player004_Male_Casual.fbx";
const string playerRootName = "PlayerPrototype";
const string oldVisualName = "ProvisionalVisual";
const string newVisualName = "CharacterVisual_004";
const float targetVisualHeight = 1.7f;
const float localYawOffset = 0f; // Imported FBX root has identity rotation; preserve existing player forward axis.

if (EditorApplication.isPlayingOrWillChangePlaymode)
    return "ABORT: encerre Play Mode antes de trocar a visualização.";
var scene = SceneManager.GetActiveScene();
if (scene.path != targetScenePath || scene.isDirty)
    return "ABORT: cena ativa deve ser 004 e limpa; ativa=" + scene.path + " dirty=" + scene.isDirty;
var dirtyScenes = Enumerable.Range(0, SceneManager.sceneCount)
    .Select(SceneManager.GetSceneAt).Where(s => s.isDirty).Select(s => s.path).ToArray();
if (dirtyScenes.Length > 0)
    return "ABORT: outras cenas têm alterações não salvas: " + string.Join(", ", dirtyScenes);
AssetDatabase.Refresh();
var visualAsset = AssetDatabase.LoadAssetAtPath<GameObject>(visualFbxPath);
if (!visualAsset)
    return "ABORT: FBX visual não importado: " + visualFbxPath;
var roots = scene.GetRootGameObjects();
var playerRoot = roots.SingleOrDefault(go => go.name == playerRootName);
if (!playerRoot || PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(playerRoot) != playerPrefabPath)
    return "ABORT: instância PlayerPrototype esperada não encontrada.";
if (!playerRoot.GetComponent<CharacterController>())
    return "ABORT: PlayerPrototype não contém CharacterController e CaisPlayerController na raiz.";
var playerController = playerRoot.GetComponents<MonoBehaviour>()
    .SingleOrDefault(c => c && c.GetType().Name == "CaisPlayerController");
if (!playerController)
    return "ABORT: CaisPlayerController ausente na raiz do jogador.";
var sessionRoot = roots.SingleOrDefault(go => go.name == "Cais Session");
var session = sessionRoot
    ? sessionRoot.GetComponentsInChildren<MonoBehaviour>(true).SingleOrDefault(c => c && c.GetType().Name == "CaisSessionController")
    : null;
var sessionPlayer = session ? new SerializedObject(session).FindProperty("player") : null;
if (sessionPlayer == null || sessionPlayer.objectReferenceValue != playerController)
    return "ABORT: CaisSessionController.player não aponta para o CaisPlayerController existente.";
var cameraRoot = roots.SingleOrDefault(go => go.name == "Cais Gameplay Camera");
var cameraRig = cameraRoot
    ? cameraRoot.GetComponentsInChildren<MonoBehaviour>(true).SingleOrDefault(c => c && c.GetType().Name == "CaisCameraRig")
    : null;
var cameraTarget = cameraRig ? new SerializedObject(cameraRig).FindProperty("target") : null;
if (cameraTarget == null || cameraTarget.objectReferenceValue != playerRoot.transform)
    return "ABORT: CaisCameraRig.target não aponta para o PlayerPrototype existente.";
var visuals = playerRoot.GetComponentsInChildren<Transform>(true);
var oldVisual = visuals.SingleOrDefault(t => t.name == oldVisualName);
if (!oldVisual)
    return "ABORT: visual provisório não encontrado: " + oldVisualName;
if (visuals.Any(t => t.name == newVisualName))
    return "ABORT: visual novo já existe; nenhuma alteração foi feita.";
var rendererCount = visualAsset.GetComponentsInChildren<Renderer>(true).Length;
if (rendererCount == 0)
    return "ABORT: FBX não contém Renderer.";

var visualRoot = new GameObject(newVisualName);
Undo.RegisterCreatedObjectUndo(visualRoot, "Add scene 004 character visual");
Undo.SetTransformParent(visualRoot.transform, playerRoot.transform, "Attach visual to PlayerPrototype");
visualRoot.transform.localPosition = Vector3.zero;
visualRoot.transform.localRotation = Quaternion.Euler(0f, localYawOffset, 0f);
visualRoot.transform.localScale = Vector3.one;
var instance = PrefabUtility.InstantiatePrefab(visualAsset, scene) as GameObject;
if (!instance)
{
    Object.DestroyImmediate(visualRoot);
    return "ABORT: não foi possível instanciar o FBX visual.";
}
Undo.SetTransformParent(instance.transform, visualRoot.transform, "Nest imported character visual");
instance.transform.localPosition = Vector3.zero;
instance.transform.localRotation = Quaternion.identity;
instance.transform.localScale = Vector3.one;
var renderers = instance.GetComponentsInChildren<Renderer>(true).Where(r => r && r.enabled).ToArray();
if (renderers.Length == 0)
{
    Object.DestroyImmediate(visualRoot);
    return "ABORT: FBX não tem Renderer ativo.";
}
var bounds = renderers.Select(r => r.bounds).Aggregate((a, b) => { a.Encapsulate(b); return a; });
if (bounds.size.y <= 0.01f)
{
    Object.DestroyImmediate(visualRoot);
    return "ABORT: altura do FBX inválida: " + bounds.size.y;
}
var uniformScale = targetVisualHeight / bounds.size.y;
visualRoot.transform.localScale = Vector3.one * uniformScale;
bounds = renderers.Select(r => r.bounds).Aggregate((a, b) => { a.Encapsulate(b); return a; });
visualRoot.transform.position += new Vector3(
    playerRoot.transform.position.x - bounds.center.x,
    playerRoot.transform.position.y - bounds.min.y,
    playerRoot.transform.position.z - bounds.center.z);
foreach (var collider in instance.GetComponentsInChildren<Collider>(true))
    collider.enabled = false;
foreach (var child in visualRoot.GetComponentsInChildren<Transform>(true))
    child.gameObject.layer = playerRoot.layer;

Undo.RecordObject(oldVisual.gameObject, "Hide provisional visual in scene 004");
oldVisual.gameObject.SetActive(false);
PrefabUtility.RecordPrefabInstancePropertyModifications(oldVisual.gameObject);
PrefabUtility.RecordPrefabInstancePropertyModifications(playerRoot.transform);
EditorSceneManager.MarkSceneDirty(scene);
if (!EditorSceneManager.SaveScene(scene))
    return "ERROR: não foi possível salvar a cena 004.";
AssetDatabase.SaveAssets();
Physics.SyncTransforms();
bounds = renderers.Select(r => r.bounds).Aggregate((a, b) => { a.Encapsulate(b); return a; });
return "OK: cena=" + scene.path
    + "; FBX=" + visualFbxPath
    + "; renderers=" + rendererCount
    + "; height=" + bounds.size.y.ToString("F3") + "m"
    + "; footY=" + bounds.min.y.ToString("F3") + " playerY=" + playerRoot.transform.position.y.ToString("F3")
    + "; centerXZ=" + bounds.center.x.ToString("F3") + "," + bounds.center.z.ToString("F3")
    + "; yawOffset=" + localYawOffset
    + "; controller/session/camera roots and shared PlayerPrototype prefab unchanged.";
