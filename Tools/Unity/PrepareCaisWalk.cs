using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

if(EditorApplication.isPlaying || EditorApplication.isCompiling) throw new InvalidOperationException("Editor deve estar parado e compilado.");
var current=SceneManager.GetActiveScene();
if(current.isDirty) throw new InvalidOperationException("Preservar a cena ativa com alteracoes nao salvas antes de continuar.");
void Folder(string path) {
 if(AssetDatabase.IsValidFolder(path)) return;
 var parent=Path.GetDirectoryName(path).Replace('\\','/'); Folder(parent); AssetDatabase.CreateFolder(parent,Path.GetFileName(path));
}
foreach(var path in new[]{"Assets/_Project/Scripts/Prototype","Assets/_Project/Art/Characters/Prototype","Assets/_Project/Prefabs"})Folder(path);
string target="Assets/_Project/Scenes/Cais_Caminhada.unity";
if(File.Exists(target)) throw new InvalidOperationException("Cena de caminhada ja existe; nao sobrescrever.");
var source=EditorSceneManager.OpenScene("Assets/_Project/Scenes/Cais_Iteracao02.unity",OpenSceneMode.Single);
EditorSceneManager.SaveScene(source,target,true);
var scene=EditorSceneManager.OpenScene(target,OpenSceneMode.Single);
var tags=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
var layers=tags.FindProperty("layers");
int Layer(string name) {
 var existing=LayerMask.NameToLayer(name); if(existing>=0)return existing;
 for(int i=8;i<layers.arraySize;i++)if(string.IsNullOrEmpty(layers.GetArrayElementAtIndex(i).stringValue)){layers.GetArrayElementAtIndex(i).stringValue=name; tags.ApplyModifiedProperties();return i;}
 throw new InvalidOperationException("Sem layer disponivel para "+name);
}
int solid=Layer("CaisSolid"),player=Layer("CaisPlayer");
int added=0;
foreach(var filter in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None)) {
 var go=filter.gameObject;var n=go.name;
 bool water=n=="Mar"||n.StartsWith("Mar.");
 bool obstacle=n.StartsWith("Caixa_")||n.StartsWith("Reforco_caixa")||n.StartsWith("Pedra_")||n.StartsWith("Barco_casco")||n.StartsWith("Estaca_");
 bool cliff=n.StartsWith("Costao_frontal");
 if(water)continue;
 if(obstacle && go.GetComponent<Collider>()==null){var b=go.AddComponent<BoxCollider>();b.center=filter.sharedMesh.bounds.center;b.size=filter.sharedMesh.bounds.size;added++;}
 if(cliff && go.GetComponent<Collider>()==null){go.AddComponent<MeshCollider>();added++;}
 if(go.GetComponent<Collider>()!=null)go.layer=solid;
}
EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
return "prepared="+scene.path+"; addedColliders="+added+"; totalColliders="+UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Length+"; solidLayer="+solid+"; playerLayer="+player;
