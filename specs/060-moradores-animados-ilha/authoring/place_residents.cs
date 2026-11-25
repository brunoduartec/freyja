using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Newtonsoft.Json.Linq;

var feature="/Users/bruno.duartec/Documents/Projects/Freyja/.utmp/060-moradores-animados-ilha/specs/060-moradores-animados-ilha";
var rows=JArray.Parse(File.ReadAllText(feature+"/authoring/characters.json"));
var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if(scene.path!="Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity"||EditorApplication.isPlaying)throw new Exception("Unexpected scene or Play Mode");
var island=GameObject.Find("Island023").transform;
var group=island.Find("Moradores060");
if(group==null){var go=new GameObject("Moradores060");group=go.transform;group.SetParent(island,false);}
var prefabDirectory="Assets/_Project/Prefabs/Moradores060";
Directory.CreateDirectory(Path.Combine(Application.dataPath,"_Project/Prefabs/Moradores060"));
AssetDatabase.Refresh();
var reports=new List<object>();
foreach(var row in rows)
{
    string name=(string)row["asset"];
    string directory="Assets/_Project/Art/Moradores060/"+name;
    var asset=AssetDatabase.LoadAssetAtPath<GameObject>(directory+"/"+name+".fbx");
    var wrapper=new GameObject((string)row["label"]);
    var visual=(GameObject)PrefabUtility.InstantiatePrefab(asset);
    visual.transform.SetParent(wrapper.transform,false);
    var animator=visual.GetComponent<Animator>();
    animator.runtimeAnimatorController=AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(directory+"/"+name+".controller");
    animator.applyRootMotion=false;
    animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
    var material=AssetDatabase.LoadAssetAtPath<Material>(directory+"/"+name+".mat");
    foreach(var renderer in visual.GetComponentsInChildren<SkinnedMeshRenderer>())
    {
        renderer.sharedMaterials=renderer.sharedMaterials.Select(m=>material).ToArray();
        renderer.updateWhenOffscreen=true;
    }
    float height=(float)row["height"];
    var capsule=wrapper.AddComponent<CapsuleCollider>();
    capsule.height=height;
    capsule.radius=height<1.55f?0.18f:0.23f;
    capsule.center=new Vector3(0,height*0.5f,0);
    string prefabPath=prefabDirectory+"/"+name+".prefab";
    PrefabUtility.SaveAsPrefabAsset(wrapper,prefabPath);
    UnityEngine.Object.DestroyImmediate(wrapper);
    var old=group.Find((string)row["label"]);
    if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
    var npc=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));
    npc.name=(string)row["label"];
    npc.transform.SetParent(group,false);
    var origin=new Vector3((float)row["x"],(float)row["groundY"]+1f,(float)row["z"]);
    var floor=Physics.RaycastAll(origin,Vector3.down,7f,~0,QueryTriggerInteraction.Ignore)
        .Where(h=>h.normal.y>0.8f&&!h.collider.transform.IsChildOf(group))
        .OrderBy(h=>Mathf.Abs(h.point.y-(float)row["groundY"])).First();
    npc.transform.position=floor.point+Vector3.up*(float)row["groundOffset"];
    npc.transform.rotation=Quaternion.Euler(0,(float)row["yaw"],0);
    PrefabUtility.RecordPrefabInstancePropertyModifications(npc.transform);
    reports.Add(new{name=npc.name,id=(string)row["id"],position=new[]{npc.transform.position.x,npc.transform.position.y,npc.transform.position.z},yaw=(float)row["yaw"],height,prefab=prefabPath,ground=floor.collider.name,normal=floor.normal.y});
}
EditorSceneManager.MarkSceneDirty(scene);
AssetDatabase.SaveAssets();
EditorSceneManager.SaveScene(scene);
string json=Newtonsoft.Json.JsonConvert.SerializeObject(reports,Newtonsoft.Json.Formatting.Indented);
File.WriteAllText(feature+"/evidence/placements.json",json);
return json;
