using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;
using Freyja.Prototype;

if(EditorApplication.isPlaying||EditorApplication.isCompiling)throw new InvalidOperationException("Editor deve estar parado e compilado.");
var scene=SceneManager.GetActiveScene();
if(scene.path!="Assets/_Project/Scenes/Cais_Caminhada.unity")throw new InvalidOperationException("Abrir somente a cena de caminhada.");
if(GameObject.Find("PlayerPrototype")!=null)throw new InvalidOperationException("Cena ja montada; nao duplicar jogador.");
string modelPath="Assets/_Project/Art/Characters/Prototype/Player_Provisorio.fbx";
AssetDatabase.ImportAsset(modelPath,ImportAssetOptions.ForceSynchronousImport);
var importer=(ModelImporter)AssetImporter.GetAtPath(modelPath);
importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;
var palette=JObject.Parse(File.ReadAllText("Assets/_Project/Art/Characters/Prototype/Player_Provisorio_palette.json"));
foreach(var p in ((JObject)palette["palette"]).Properties()) {
 string path="Assets/_Project/Art/Characters/Prototype/"+p.Name+".mat";
 var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
 if(mat==null){mat=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(mat,path);}
 mat.color=new Color((float)p.Value["r"],(float)p.Value["g"],(float)p.Value["b"]).gamma;
 mat.SetFloat("_Glossiness",0.08f);EditorUtility.SetDirty(mat);
 importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),p.Name),mat);
}
importer.SaveAndReimport();
foreach(var old in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(old.gameObject);

int solid=LayerMask.NameToLayer("CaisSolid"),playerLayer=LayerMask.NameToLayer("CaisPlayer");
var playerGo=new GameObject("PlayerPrototype");playerGo.layer=playerLayer;
playerGo.transform.SetPositionAndRotation(new Vector3(0,0.94f,7.5f),Quaternion.Euler(0,180,0));
var body=playerGo.AddComponent<CharacterController>();body.height=1.8f;body.radius=0.3f;body.center=new Vector3(0,.9f,0);body.stepOffset=.25f;body.skinWidth=.04f;body.slopeLimit=60;body.minMoveDistance=0;
var player=playerGo.AddComponent<CaisPlayerController>();
var model=AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
var visual=(GameObject)PrefabUtility.InstantiatePrefab(model);
visual.name="ProvisionalVisual";visual.transform.SetParent(playerGo.transform,false);visual.transform.localPosition=Vector3.zero;visual.transform.localRotation=Quaternion.identity;visual.transform.localScale=Vector3.one;
foreach(var t in visual.GetComponentsInChildren<Transform>())t.gameObject.layer=playerLayer;
foreach(var c in visual.GetComponentsInChildren<Collider>())UnityEngine.Object.DestroyImmediate(c);

var cameraGo=new GameObject("Cais Gameplay Camera",typeof(Camera),typeof(AudioListener));cameraGo.tag="MainCamera";
var camera=cameraGo.GetComponent<Camera>();camera.fieldOfView=58;camera.nearClipPlane=.08f;camera.farClipPlane=150;camera.allowMSAA=false;camera.allowHDR=false;camera.backgroundColor=new Color(.53f,.70f,.79f);camera.clearFlags=CameraClearFlags.SolidColor;
var rig=cameraGo.AddComponent<CaisCameraRig>();rig.Target=playerGo.transform;rig.ViewCamera=camera;rig.ObstacleMask=1<<solid;rig.ControlEnabled=false;
player.CameraTransform=cameraGo.transform;player.ControlEnabled=false;
rig.ResetView(180);

var spawn=new GameObject("SpawnPoint");spawn.transform.SetPositionAndRotation(playerGo.transform.position,playerGo.transform.rotation);
var end=new GameObject("WalkEndpoint");end.transform.position=new Vector3(-.5f,1.85f,-7f);
var canvasGo=new GameObject("Controls HUD",typeof(Canvas),typeof(CanvasScaler));
var canvas=canvasGo.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
var scaler=canvasGo.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=.5f;
var panelGo=new GameObject("Instruction Panel",typeof(RectTransform),typeof(Image));panelGo.transform.SetParent(canvasGo.transform,false);
var panel=panelGo.GetComponent<RectTransform>();panel.anchorMin=new Vector2(0,1);panel.anchorMax=new Vector2(1,1);panel.pivot=new Vector2(.5f,1);panel.anchoredPosition=Vector2.zero;panel.sizeDelta=new Vector2(0,104);
panelGo.GetComponent<Image>().color=new Color(.035f,.08f,.09f,.88f);
var textGo=new GameObject("Controls Text",typeof(RectTransform),typeof(Text));textGo.transform.SetParent(panelGo.transform,false);
var rect=textGo.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=new Vector2(24,9);rect.offsetMax=new Vector2(-24,-9);
var text=textGo.GetComponent<Text>();text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=20;text.color=new Color(.96f,.94f,.84f);text.alignment=TextAnchor.MiddleLeft;text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Truncate;
text.text="CAMINHAR PELO CAIS\nClique para controlar\nWASD  caminhar   |   Mouse  olhar   |   Esc  liberar cursor";
var sessionGo=new GameObject("Cais Session");var session=sessionGo.AddComponent<CaisSessionController>();
var asset=AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
if(!AssetDatabase.IsValidFolder("Assets/_Project/Input"))AssetDatabase.CreateFolder("Assets/_Project","Input");
InputActionReference Ref(string actionPath,string filename) {
 string path="Assets/_Project/Input/"+filename+".asset";
 var reference=AssetDatabase.LoadAssetAtPath<InputActionReference>(path);
 if(reference==null){reference=InputActionReference.Create(asset.FindAction(actionPath,true));AssetDatabase.CreateAsset(reference,path);}
 return reference;
}
var so=new SerializedObject(session);
so.FindProperty("player").objectReferenceValue=player;
so.FindProperty("cameraRig").objectReferenceValue=rig;
so.FindProperty("spawnPoint").objectReferenceValue=spawn.transform;
so.FindProperty("solidMask").intValue=1<<solid;
so.FindProperty("controlsText").objectReferenceValue=text;
so.FindProperty("moveAction").objectReferenceValue=Ref("Player/Move","CaisMove");
so.FindProperty("lookAction").objectReferenceValue=Ref("Player/Look","CaisLook");
so.FindProperty("releaseAction").objectReferenceValue=Ref("UI/Cancel","CaisRelease");
so.FindProperty("resumeAction").objectReferenceValue=Ref("UI/Click","CaisResume");
so.ApplyModifiedPropertiesWithoutUndo();
PrefabUtility.SaveAsPrefabAssetAndConnect(playerGo,"Assets/_Project/Prefabs/PlayerPrototype.prefab",InteractionMode.AutomatedAction);
AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene);
if(SceneView.lastActiveSceneView!=null){SceneView.lastActiveSceneView.pivot=playerGo.transform.position+Vector3.up;SceneView.lastActiveSceneView.rotation=camera.transform.rotation;SceneView.lastActiveSceneView.size=7;SceneView.lastActiveSceneView.orthographic=false;}
var bounds=visual.GetComponentsInChildren<Renderer>()[0].bounds;
foreach(var r in visual.GetComponentsInChildren<Renderer>())bounds.Encapsulate(r.bounds);
return "scene="+scene.path+"; cameras="+UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Length+"; listeners="+UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length+"; visualHeight="+bounds.size.y+"; colliders="+UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Length;
