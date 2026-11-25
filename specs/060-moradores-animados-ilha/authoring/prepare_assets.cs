using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using Newtonsoft.Json.Linq;

var feature="/Users/bruno.duartec/Documents/Projects/Freyja/.utmp/060-moradores-animados-ilha/specs/060-moradores-animados-ilha";
var rows=JArray.Parse(File.ReadAllText(feature+"/authoring/characters.json"));
AssetDatabase.Refresh();
var reports=new List<object>();
var library="Assets/ThirdParty/Kevin Iglesias/Human Basic Motions FREE/";
for(int index=0;index<rows.Count;index++)
{
    var row=rows[index];
    string name=(string)row["asset"];
    string directory="Assets/_Project/Art/Moradores060/"+name;
    string modelPath=directory+"/"+name+".fbx";
    var importer=(ModelImporter)AssetImporter.GetAtPath(modelPath);
    importer.globalScale=(float)row["height"];
    importer.animationType=ModelImporterAnimationType.Human;
    importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;
    importer.importAnimation=false;
    importer.importCameras=false;
    importer.importLights=false;
    importer.SaveAndReimport();
    var avatar=AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<Avatar>().First();
    if(!avatar.isValid||!avatar.isHuman)throw new Exception(name+" invalid avatar");
    string texturePath=directory+"/BaseColor.png";
    var textureImporter=(TextureImporter)AssetImporter.GetAtPath(texturePath);
    textureImporter.textureType=TextureImporterType.Default;
    textureImporter.sRGBTexture=true;
    textureImporter.filterMode=FilterMode.Point;
    textureImporter.wrapMode=TextureWrapMode.Clamp;
    textureImporter.maxTextureSize=2048;
    textureImporter.SaveAndReimport();
    string materialPath=directory+"/"+name+".mat";
    var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
    if(material==null)
    {
        material=new Material(Shader.Find("Standard"));
        material.name=name;
        AssetDatabase.CreateAsset(material,materialPath);
    }
    material.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
    material.SetFloat("_Glossiness",0.08f);
    material.SetFloat("_Metallic",0f);
    importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),name),material);
    importer.SaveAndReimport();
    string controllerPath=directory+"/"+name+".controller";
    var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
    if(controller==null)controller=AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
    var machine=controller.layers[0].stateMachine;
    foreach(var old in machine.states)machine.RemoveState(old.state);
    string clipA=(string)row["clipA"],clipB=(string)row["clipB"];
    var motionA=AssetDatabase.LoadAllAssetsAtPath(library+clipA+".fbx").OfType<AnimationClip>().First(c=>!c.name.StartsWith("__"));
    var motionB=AssetDatabase.LoadAllAssetsAtPath(library+clipB+".fbx").OfType<AnimationClip>().First(c=>!c.name.StartsWith("__"));
    if(!motionA.humanMotion||!motionB.humanMotion)throw new Exception(name+" non-humanoid clip");
    var idle=machine.AddState("Repouso");
    idle.motion=motionA;
    idle.cycleOffset=(index*0.137f)%1f;
    idle.speed=0.95f+(index%4)*0.035f;
    idle.iKOnFeet=true;
    machine.defaultState=idle;
    var gesture=machine.AddState("Variacao");
    gesture.motion=motionB;
    gesture.cycleOffset=(index*0.21f)%1f;
    gesture.speed=1.0f;
    gesture.iKOnFeet=true;
    var outward=idle.AddTransition(gesture);
    outward.hasExitTime=true;
    outward.exitTime=2f;
    outward.hasFixedDuration=true;
    outward.duration=0.4f;
    var inward=gesture.AddTransition(idle);
    inward.hasExitTime=true;
    inward.exitTime=1f;
    inward.hasFixedDuration=true;
    inward.duration=0.4f;
    var instance=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(modelPath));
    var animator=instance.GetComponent<Animator>();
    animator.runtimeAnimatorController=controller;
    animator.applyRootMotion=false;
    var bounds=new Bounds();
    bool hasBounds=false;
    foreach(var renderer in instance.GetComponentsInChildren<SkinnedMeshRenderer>())
    {
        var mesh=new Mesh();
        renderer.BakeMesh(mesh,true);
        foreach(var vertex in mesh.vertices)
        {
            var point=renderer.transform.TransformPoint(vertex);
            if(!hasBounds){bounds=new Bounds(point,Vector3.zero);hasBounds=true;}
            else bounds.Encapsulate(point);
        }
        UnityEngine.Object.DestroyImmediate(mesh);
    }
    int triangles=instance.GetComponentsInChildren<SkinnedMeshRenderer>().Sum(r=>r.sharedMesh.triangles.Length/3);
    reports.Add(new{name,height=bounds.size.y,minY=bounds.min.y,triangles,valid=avatar.isValid,human=avatar.isHuman,clips=new[]{motionA.name,motionB.name}});
    UnityEngine.Object.DestroyImmediate(instance);
    EditorUtility.SetDirty(material);
    EditorUtility.SetDirty(controller);
}
AssetDatabase.SaveAssets();
Directory.CreateDirectory(feature+"/evidence");
var json=Newtonsoft.Json.JsonConvert.SerializeObject(reports,Newtonsoft.Json.Formatting.Indented);
File.WriteAllText(feature+"/evidence/imports.json",json);
return json;
