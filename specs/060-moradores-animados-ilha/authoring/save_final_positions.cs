using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Newtonsoft.Json.Linq;

var feature="/Users/bruno.duartec/Documents/Projects/Freyja/.utmp/060-moradores-animados-ilha/specs/060-moradores-animados-ilha";
if(EditorApplication.isPlaying)throw new Exception("Stop Play Mode before saving.");
var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if(scene.path!="Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity")throw new Exception("Wrong scene.");
var group=GameObject.Find("Island023/Moradores060").transform;
var reports=new List<object>();
foreach(var row in JArray.Parse(File.ReadAllText(feature+"/authoring/characters.json")))
{
    var t=group.Find((string)row["label"]);
    var origin=new Vector3((float)row["x"],(float)row["groundY"]+1f,(float)row["z"]);
    var floor=Physics.RaycastAll(origin,Vector3.down,7f,~0,QueryTriggerInteraction.Ignore).Where(h=>h.normal.y>.8f&&!h.collider.transform.IsChildOf(group)).OrderBy(h=>Mathf.Abs(h.point.y-(float)row["groundY"])).First();
    t.SetPositionAndRotation(floor.point+Vector3.up*(float)row["groundOffset"],Quaternion.Euler(0,(float)row["yaw"],0));
    PrefabUtility.RecordPrefabInstancePropertyModifications(t);
    var animator=t.GetComponentInChildren<Animator>();
    reports.Add(new{name=t.name,id=(string)row["id"],position=new[]{t.position.x,t.position.y,t.position.z},yaw=(float)row["yaw"],height=(float)row["height"],avatarValid=animator.avatar.isValid,humanoid=animator.avatar.isHuman,triangles=animator.GetComponentsInChildren<SkinnedMeshRenderer>().Sum(r=>r.sharedMesh.triangles.Length/3),prefab=PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(t.gameObject)});
}
EditorSceneManager.MarkSceneDirty(scene);
AssetDatabase.SaveAssets();
EditorSceneManager.SaveScene(scene);
var json=Newtonsoft.Json.JsonConvert.SerializeObject(reports,Newtonsoft.Json.Formatting.Indented);
File.WriteAllText(feature+"/evidence/placements.json",json);
return json;
