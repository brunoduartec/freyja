// Execute through Unity MCP only after the final Blender export is confirmed
// and the user releases the editor. This script is a prepared integration, not
// an automatic build step. Replace the FBX node placeholders after inspecting
// the final export; editor-state and FBX preflights run before opening scene 004.
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

const string scenePath = "Assets/_Project/Scenes/Ilha_Relevo_004.unity";
const string fbxPath = "Assets/_Project/Art/Environment/IlhaRelevo/Ilha_Relevo_004.fbx";
const bool replaceCharacterVisual = false; // Wait for the separate character FBX and explicit integration step.
const string playerVisualFbxPath = "__SET_PLAYER_VISUAL_FBX_PATH__";
const string prefabPath = "Assets/_Project/Prefabs/IlhaRelevo.prefab";
const string fbxTerrainName = "Terrain_004_Walkable_Island";
const string fbxHouseName = "Casa_Recanto_004";
const string fbxHouseRoofName = "Casa_Recanto_Telhado_004";
const string oldIslandRootName = "IlhaBruta";
const string oldWaterRootName = "Mar_IlhaBrutaVisual";
const string caisRootName = "Cais_VisualReview";
const string dockMarkerName = "Cais_Rampa_Juncao";
const string playerRootName = "PlayerPrototype";
const string oldPlayerVisualName = "ProvisionalVisual";
const string newPlayerVisualName = "CharacterVisual_004";
const string cameraRootName = "Cais Gameplay Camera";
const string sessionRootName = "Cais Session";
const string spawnRootName = "SpawnPoint";
const string sessionComponentName = "CaisSessionController";
const float spawnHeightOffset = 0.6f;
const float recoveryHorizontalMargin = 40f;
const float recoveryVerticalMargin = 80f;
const float waterMargin = 40f;
const float cameraFarMargin = 30f;
const float targetCharacterVisualHeight = 1.7f;
const float characterVisualYawOffset = 0f;
const int dockJoinRaySteps = 8;
const float dockJoinHalfWidth = 0.45f;
const float dockJoinMaxStep = 0.25f;

var blockNames = new[] { "B1", "B2", "B3", "B4", "A1", "A2", "A3", "A4", "L1", "L2", "L3", "L4" };
var dockDiagnosticPoint = new Vector3(0f, 1f, -4f); // Blender (0,4,1) converted to Unity; diagnostic only.
var solidLayer = LayerMask.NameToLayer("CaisSolid");
string FullAssetPath(string path) => Path.Combine(
    Directory.GetParent(Application.dataPath).FullName,
    path.Replace('/', Path.DirectorySeparatorChar));

// Do not interrupt a live play session or silently discard scene edits.
if (EditorApplication.isPlayingOrWillChangePlaymode)
    return "ABORT: Unity está em Play Mode ou mudando de estado; mantenha a sessão e tente novamente quando liberada.";
var dirtyScenes = Enumerable.Range(0, SceneManager.sceneCount)
    .Select(SceneManager.GetSceneAt)
    .Where(scene => scene.isDirty)
    .Select(scene => scene.path)
    .ToArray();
if (dirtyScenes.Length > 0)
    return "ABORT: há cenas com alterações não salvas: " + string.Join(", ", dirtyScenes);
if (solidLayer < 0)
    return "ABORT: layer CaisSolid não existe.";
if (!File.Exists(FullAssetPath(fbxPath)))
    return "ABORT: FBX final ainda não existe: " + fbxPath;
AssetDatabase.Refresh();
var fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
if (!fbxAsset)
    return "ABORT: FBX existe, mas ainda não foi importado no AssetDatabase: " + fbxPath;
if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath))
    return "ABORT: cena 004 não existe: " + scenePath;
GameObject playerVisualAsset = null;
if (replaceCharacterVisual)
{
    if (playerVisualFbxPath.StartsWith("__SET_"))
        return "ABORT: defina o caminho do FBX visual do personagem 004 e confirme que o arquivo exportado existe.";
    if (!File.Exists(FullAssetPath(playerVisualFbxPath)))
        return "ABORT: FBX visual do personagem 004 não existe: " + playerVisualFbxPath;
    playerVisualAsset = AssetDatabase.LoadAssetAtPath<GameObject>(playerVisualFbxPath);
    if (!playerVisualAsset)
        return "ABORT: FBX visual do personagem existe, mas ainda não foi importado: " + playerVisualFbxPath;
    if (playerVisualAsset.GetComponentsInChildren<Renderer>(true).Length == 0)
        return "ABORT: FBX visual não contém Renderer.";
}
if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath))
    return "ABORT: prefab destino já existe; preservado: " + prefabPath;
if (fbxTerrainName.StartsWith("__SET_") || fbxHouseName.StartsWith("__SET_") || fbxHouseRoofName.StartsWith("__SET_"))
    return "ABORT: preencha os nomes de terreno e casinha informados no export Blender.";

var modelNodes = fbxAsset.GetComponentsInChildren<Transform>(true);
Transform GetUniqueNode(string name) => modelNodes.SingleOrDefault(node => node.name == name);
var terrainAssetNode = GetUniqueNode(fbxTerrainName);
var houseAssetNode = GetUniqueNode(fbxHouseName);
var houseRoofAssetNode = GetUniqueNode(fbxHouseRoofName);
if (!terrainAssetNode || !terrainAssetNode.GetComponent<MeshFilter>()?.sharedMesh)
    return "ABORT: terreno FBX ausente, duplicado ou sem MeshFilter: " + fbxTerrainName;
if (!houseAssetNode || !houseAssetNode.GetComponent<MeshFilter>()?.sharedMesh)
    return "ABORT: casinha FBX ausente, duplicada ou sem MeshFilter: " + fbxHouseName;
if (!houseRoofAssetNode || !houseRoofAssetNode.GetComponent<MeshFilter>()?.sharedMesh)
    return "ABORT: telhado FBX ausente, duplicado ou sem MeshFilter: " + fbxHouseRoofName;
var missingBlocks = blockNames.Where(name =>
    modelNodes.Count(node => node.name == name && node.GetComponent<MeshFilter>()?.sharedMesh) != 1).ToArray();
if (missingBlocks.Length > 0)
    return "ABORT: esperado um MeshFilter por bloco B/A/L; ausentes ou duplicados: " + string.Join(", ", missingBlocks);

// Check the current 004 copy before switching scenes. Do not use scene 003 or
// alter shared prefabs; the only old island object disabled below is its 003
// prefab instance at the 004 scene root.
var previousActiveScene = SceneManager.GetActiveScene();
var targetScene = previousActiveScene.path == scenePath
    ? previousActiveScene
    : EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
if (!targetScene.IsValid() || targetScene.path != scenePath)
    return "ABORT: não foi possível abrir a cena 004.";
var roots = targetScene.GetRootGameObjects();
var caisRoot = roots.SingleOrDefault(go => go.name == caisRootName);
var playerRoot = roots.SingleOrDefault(go => go.name == playerRootName);
var cameraRoot = roots.SingleOrDefault(go => go.name == cameraRootName);
var sessionRoot = roots.SingleOrDefault(go => go.name == sessionRootName);
var spawnRoot = roots.SingleOrDefault(go => go.name == spawnRootName);
var spawnMarker = spawnRoot
    ? spawnRoot.GetComponentsInChildren<Transform>(true).SingleOrDefault(transform => transform.name == spawnRootName)
    : null;
var dockMarker = roots.SelectMany(go => go.GetComponentsInChildren<Transform>(true))
    .SingleOrDefault(transform => transform.name == dockMarkerName);
var oldIsland = roots.SingleOrDefault(go => go.name == oldIslandRootName);
var oldWater = roots.SingleOrDefault(go => go.name == oldWaterRootName);
if (!caisRoot || !playerRoot || !cameraRoot || !sessionRoot || !spawnRoot || !spawnMarker || !dockMarker)
    return "ABORT: faltam raízes do cais, jogador, câmera, sessão, SpawnPoint ou marcador Cais_Rampa_Juncao na cópia 004.";
if (!oldIsland || !PrefabUtility.IsPartOfPrefabInstance(oldIsland))
    return "ABORT: instância de prefab 003 IlhaBruta esperada não encontrada na raiz da cena 004.";
var characterController = playerRoot.GetComponent<CharacterController>();
if (!characterController)
    return "ABORT: CharacterController ausente no PlayerPrototype.";
MonoBehaviour playerController = null;
Transform oldPlayerVisual = null;
SerializedProperty sessionPlayerProperty = null;
SerializedProperty cameraTargetProperty = null;
if (replaceCharacterVisual)
{
    if (PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(playerRoot) != "Assets/_Project/Prefabs/PlayerPrototype.prefab")
        return "ABORT: PlayerPrototype não é a instância do prefab compartilhado esperada.";
    playerController = playerRoot.GetComponents<MonoBehaviour>()
        .SingleOrDefault(component => component && component.GetType().Name == "CaisPlayerController");
    oldPlayerVisual = playerRoot.GetComponentsInChildren<Transform>(true)
        .SingleOrDefault(transform => transform.name == oldPlayerVisualName);
    if (!playerController || !oldPlayerVisual)
        return "ABORT: CaisPlayerController ou visual provisório não encontrado no PlayerPrototype.";
    if (playerRoot.GetComponentsInChildren<Transform>(true).Any(transform => transform.name == newPlayerVisualName))
        return "ABORT: visual de personagem 004 já existe na cena; preservado.";
}
if (roots.Any(go => go.name == "IlhaRelevo"))
    return "ABORT: a cena já contém uma integração IlhaRelevo; nenhuma alteração foi feita.";
var camera = cameraRoot.GetComponentInChildren<Camera>(true);
var session = sessionRoot.GetComponentsInChildren<MonoBehaviour>(true)
    .SingleOrDefault(component => component && component.GetType().Name == sessionComponentName);
if (!camera || !session)
    return "ABORT: câmera ou CaisSessionController ausente na cena 004.";
var sessionData = new SerializedObject(session);
var boundsProperty = sessionData.FindProperty("recoveryBounds");
if (boundsProperty == null || boundsProperty.propertyType != SerializedPropertyType.Bounds)
    return "ABORT: campo recoveryBounds ausente no CaisSessionController.";
if (replaceCharacterVisual)
{
    sessionPlayerProperty = sessionData.FindProperty("player");
    if (sessionPlayerProperty == null || sessionPlayerProperty.objectReferenceValue != playerController)
        return "ABORT: referência do CaisSessionController ao CaisPlayerController não aponta para o jogador esperado.";
    var cameraRig = cameraRoot.GetComponentsInChildren<MonoBehaviour>(true)
        .SingleOrDefault(component => component && component.GetType().Name == "CaisCameraRig");
    var cameraRigData = cameraRig ? new SerializedObject(cameraRig) : null;
    cameraTargetProperty = cameraRigData?.FindProperty("target");
    if (cameraTargetProperty == null || cameraTargetProperty.objectReferenceValue != playerRoot.transform)
        return "ABORT: alvo existente do CaisCameraRig não aponta para o PlayerPrototype.";
}

// All FBX and scene preflight checks are complete. Keep the FBX origin and
// rotation at zero as in the 003 integration; the anchor is diagnostic only.
var islandRoot = new GameObject("IlhaRelevo");
SceneManager.MoveGameObjectToScene(islandRoot, targetScene);
var modelInstance = PrefabUtility.InstantiatePrefab(fbxAsset, targetScene) as GameObject;
if (!modelInstance)
{
    Object.DestroyImmediate(islandRoot);
    return "ABORT: não foi possível instanciar o FBX 004.";
}
modelInstance.transform.SetParent(islandRoot.transform, false);
islandRoot.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
islandRoot.transform.localScale = Vector3.one;
modelInstance.transform.localPosition = Vector3.zero;
modelInstance.transform.localRotation = Quaternion.identity;
modelInstance.transform.localScale = Vector3.one;
Transform FindModelNode(string name) =>
    modelInstance.GetComponentsInChildren<Transform>(true).SingleOrDefault(node => node.name == name);
GameObject CreateColliderObject(Transform modelNode, string suffix)
{
    var colliderObject = new GameObject(modelNode.name + suffix);
    colliderObject.transform.SetParent(islandRoot.transform, false);
    colliderObject.transform.SetPositionAndRotation(modelNode.position, modelNode.rotation);
    colliderObject.transform.localScale = modelNode.lossyScale;
    colliderObject.layer = solidLayer;
    return colliderObject;
}

var terrain = FindModelNode(fbxTerrainName);
var terrainMesh = terrain ? terrain.GetComponent<MeshFilter>()?.sharedMesh : null;
if (!terrain || !terrainMesh)
{
    Object.DestroyImmediate(islandRoot);
    return "ABORT: malha terrestre não encontrada na instância FBX.";
}
var terrainRenderer = terrain.GetComponent<Renderer>();
if (!terrainRenderer)
{
    Object.DestroyImmediate(islandRoot);
    return "ABORT: Renderer do terreno ausente.";
}
var terrainCollider = CreateColliderObject(terrain, "_Collider").AddComponent<MeshCollider>();
terrainCollider.convex = false;
terrainCollider.sharedMesh = terrainMesh;
foreach (var name in blockNames.Concat(new[] { fbxHouseName, fbxHouseRoofName }))
{
    var node = FindModelNode(name);
    var mesh = node ? node.GetComponent<MeshFilter>()?.sharedMesh : null;
    if (!node || !mesh)
    {
        Object.DestroyImmediate(islandRoot);
        return "ABORT: bloco/casinha ausente na instância FBX: " + name;
    }
    var collider = CreateColliderObject(node, "_Collider").AddComponent<BoxCollider>();
    collider.center = mesh.bounds.center;
    collider.size = mesh.bounds.size;
}

// Test the approximate dock-anchor-to-ramp segment with downward physics rays.
// Rays must hit either the new land collider or existing CaisSolid geometry,
// and consecutive centerline samples must stay within the existing step size.
var oldWaterMaterial = oldWater ? oldWater.GetComponent<Renderer>()?.sharedMaterial : null;
var waterLevel = oldWater ? oldWater.transform.position.y : dockMarker.position.y - 0.25f;
oldIsland.SetActive(false);
if (oldWater) oldWater.SetActive(false);
Physics.SyncTransforms();
var rayMask = 1 << solidLayer;
var anchorToRamp = dockMarker.position - dockDiagnosticPoint;
var lateral = Vector3.Cross(anchorToRamp.normalized, Vector3.up).normalized;
var centerlineHits = new float[dockJoinRaySteps + 1];
var rayFailure = "";
for (var i = 0; i <= dockJoinRaySteps && rayFailure.Length == 0; i++)
{
    var t = i / (float)dockJoinRaySteps;
    var center = Vector3.Lerp(dockDiagnosticPoint, dockMarker.position, t);
    for (var side = -1; side <= 1; side++)
    {
        var point = center + lateral * (side * dockJoinHalfWidth);
        var origin = new Vector3(point.x, Mathf.Max(dockDiagnosticPoint.y, dockMarker.position.y) + 8f, point.z);
        if (!Physics.Raycast(origin, Vector3.down, out var hit, 32f, rayMask, QueryTriggerInteraction.Ignore))
        {
            rayFailure = "sem superfície CaisSolid no ray " + i + "/" + side;
            break;
        }
        var belongsToNewLand = hit.collider.transform.IsChildOf(islandRoot.transform);
        var belongsToDock = hit.collider.transform.IsChildOf(caisRoot.transform)
            || hit.collider.transform == dockMarker
            || hit.collider.transform.IsChildOf(dockMarker);
        if (!belongsToNewLand && !belongsToDock)
        {
            rayFailure = "ray atingiu objeto inesperado " + hit.collider.name + " em " + i + "/" + side;
            break;
        }
        if (side == 0) centerlineHits[i] = hit.point.y;
    }
    if (i > 0 && rayFailure.Length == 0 && Mathf.Abs(centerlineHits[i] - centerlineHits[i - 1]) > dockJoinMaxStep)
        rayFailure = "desnível > " + dockJoinMaxStep + " m entre rays " + (i - 1) + " e " + i;
}
if (rayFailure.Length > 0)
{
    oldIsland.SetActive(true);
    if (oldWater) oldWater.SetActive(true);
    Object.DestroyImmediate(islandRoot);
    Physics.SyncTransforms();
    return "ABORT: junção cais/terreno falhou: " + rayFailure + ". Cena não salva; conferir offsets FBX e colisões.";
}

var terrainBounds = terrainRenderer.bounds;
var recoveryBounds = terrainBounds;
recoveryBounds.Encapsulate(new Bounds(dockMarker.position, Vector3.one));
recoveryBounds.Expand(new Vector3(recoveryHorizontalMargin, recoveryVerticalMargin, recoveryHorizontalMargin));

// Keep the existing player, dock, ramp and shared Cais prefab. Only disable
// the old 003 island prefab instance and the obsolete scene-level water plane.
var water = GameObject.CreatePrimitive(PrimitiveType.Plane);
water.name = "Mar_IlhaRelevoVisual";
var waterCollider = water.GetComponent<Collider>();
if (waterCollider) Object.DestroyImmediate(waterCollider);
water.transform.position = new Vector3(terrainBounds.center.x, waterLevel, terrainBounds.center.z);
water.transform.localScale = new Vector3((terrainBounds.size.x + waterMargin * 2f) / 10f, 1f,
    (terrainBounds.size.z + waterMargin * 2f) / 10f);
if (oldWaterMaterial) water.GetComponent<Renderer>().sharedMaterial = oldWaterMaterial;
SceneManager.MoveGameObjectToScene(water, targetScene);

// Retain the existing spawn if it remains inside the expanded safe bounds;
// otherwise align it to the dock junction at a small clear height.
if (!recoveryBounds.Contains(spawnMarker.position))
{
    var safeSpawnPosition = dockMarker.position + Vector3.up * spawnHeightOffset;
    if (!recoveryBounds.Contains(safeSpawnPosition))
    {
        oldIsland.SetActive(true);
        if (oldWater) oldWater.SetActive(true);
        Object.DestroyImmediate(water);
        Object.DestroyImmediate(islandRoot);
        Physics.SyncTransforms();
        return "ABORT: spawn atual e spawn junto ao cais ficam fora dos bounds derivados.";
    }
    spawnMarker.SetPositionAndRotation(safeSpawnPosition, dockMarker.rotation);
}
if (!recoveryBounds.Contains(spawnMarker.position))
{
    oldIsland.SetActive(true);
    if (oldWater) oldWater.SetActive(true);
    Object.DestroyImmediate(water);
    Object.DestroyImmediate(islandRoot);
    Physics.SyncTransforms();
    return "ABORT: SpawnPoint continua fora dos bounds derivados do terreno e cais.";
}
sessionData.Update();
boundsProperty = sessionData.FindProperty("recoveryBounds");
boundsProperty.boundsValue = recoveryBounds;
sessionData.ApplyModifiedPropertiesWithoutUndo();
camera.farClipPlane = Mathf.Max(camera.farClipPlane, terrainBounds.size.magnitude + cameraFarMargin);

if (replaceCharacterVisual)
{
// Replace only the scene-instance visual child. The shared PlayerPrototype
// prefab, CharacterController, controller component, session reference and
// camera target remain untouched. The imported FBX has no gameplay role.
var visualRoot = new GameObject(newPlayerVisualName);
Undo.RegisterCreatedObjectUndo(visualRoot, "Add character visual for island 004");
Undo.SetTransformParent(visualRoot.transform, playerRoot.transform, "Attach character visual to PlayerPrototype");
visualRoot.transform.localPosition = Vector3.zero;
visualRoot.transform.localRotation = Quaternion.Euler(0f, characterVisualYawOffset, 0f);
visualRoot.transform.localScale = Vector3.one;
var visualInstance = PrefabUtility.InstantiatePrefab(playerVisualAsset, targetScene) as GameObject;
if (!visualInstance)
{
    oldIsland.SetActive(true);
    if (oldWater) oldWater.SetActive(true);
    Object.DestroyImmediate(water);
    Object.DestroyImmediate(visualRoot);
    Object.DestroyImmediate(islandRoot);
    Physics.SyncTransforms();
    return "ABORT: não foi possível instanciar o FBX visual; cena não salva.";
}
Undo.SetTransformParent(visualInstance.transform, visualRoot.transform, "Nest imported character visual");
visualInstance.transform.localPosition = Vector3.zero;
visualInstance.transform.localRotation = Quaternion.identity;
visualInstance.transform.localScale = Vector3.one;
var visualRenderers = visualInstance.GetComponentsInChildren<Renderer>(true).Where(renderer => renderer && renderer.enabled).ToArray();
if (visualRenderers.Length == 0)
{
    oldIsland.SetActive(true);
    if (oldWater) oldWater.SetActive(true);
    Object.DestroyImmediate(water);
    Object.DestroyImmediate(visualRoot);
    Object.DestroyImmediate(islandRoot);
    Physics.SyncTransforms();
    return "ABORT: FBX visual não contém Renderer ativo; cena não salva.";
}
var visualBounds = visualRenderers.Select(renderer => renderer.bounds).Aggregate((a, b) => { a.Encapsulate(b); return a; });
visualRoot.transform.localScale = Vector3.one * (targetCharacterVisualHeight / Mathf.Max(0.001f, visualBounds.size.y));
visualBounds = visualRenderers.Select(renderer => renderer.bounds).Aggregate((a, b) => { a.Encapsulate(b); return a; });
visualRoot.transform.position += new Vector3(
    playerRoot.transform.position.x - visualBounds.center.x,
    playerRoot.transform.position.y - visualBounds.min.y,
    playerRoot.transform.position.z - visualBounds.center.z);
foreach (var collider in visualInstance.GetComponentsInChildren<Collider>(true))
    collider.enabled = false;
foreach (var transform in visualRoot.GetComponentsInChildren<Transform>(true))
    transform.gameObject.layer = playerRoot.layer;
Undo.RecordObject(oldPlayerVisual.gameObject, "Hide provisional character visual in scene 004");
oldPlayerVisual.gameObject.SetActive(false);
PrefabUtility.RecordPrefabInstancePropertyModifications(oldPlayerVisual.gameObject);
PrefabUtility.RecordPrefabInstancePropertyModifications(playerRoot.transform);
if (sessionPlayerProperty.objectReferenceValue != playerController || cameraTargetProperty.objectReferenceValue != playerRoot.transform)
    return "ERROR: referência de sessão/câmera mudou inesperadamente; revisar antes de salvar.";
}

bool prefabSaved;
PrefabUtility.SaveAsPrefabAssetAndConnect(islandRoot, prefabPath, InteractionMode.AutomatedAction, out prefabSaved);
if (!prefabSaved)
    return "ERROR: falha ao salvar o prefab 004; a cena ainda não foi salva.";
EditorSceneManager.MarkSceneDirty(targetScene);
if (!EditorSceneManager.SaveScene(targetScene))
    return "ERROR: prefab criado, mas falha ao salvar a cena 004.";
AssetDatabase.SaveAssets();
AssetDatabase.Refresh();
Physics.SyncTransforms();
return "OK: prefab=" + prefabPath
    + "; cena=" + targetScene.path
    + "; terreno=MeshCollider não convexo/CaisSolid"
    + "; blocos+casinha+telhado=BoxCollider/CaisSolid"
    + "; rotas sem collider adicional"
    + "; instância antiga desativada=" + oldIslandRootName
    + "; junção validada por " + ((dockJoinRaySteps + 1) * 3) + " rays CaisSolid"
    + "; água dimensionada pelo terreno, visual sem collider"
    + "; recoveryBounds=" + recoveryBounds
    + "; SpawnPoint=" + spawnMarker.position
    + "; cameraFarClip=" + camera.farClipPlane
    + (replaceCharacterVisual
        ? "; visual=" + playerVisualFbxPath + " altura=" + targetCharacterVisualHeight + "m (só override da cena 004)"
        : "; visual provisório preservado; substituição personagem pendente")
    + "; cais, rampa, jogador, controles e assets anteriores preservados.";
