using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Freyja.Prototype;

public static class RecordIsland023
{
 const string Folder="/Users/bruno.duartec/Documents/Projects/Freyja/Artifacts/Island023/";
 static EditorApplication.CallbackFunction tick;
 public static string Main()
 {
  if(!EditorApplication.isPlaying)throw new Exception("Entre em Play Mode.");
  var session=GameObject.Find("Cais Session").GetComponent<CaisSessionController>();
  if(!session.IsInitialized)throw new Exception("Sessão não inicializada.");
  var player=GameObject.Find("PlayerPrototype").GetComponent<CaisPlayerController>();
  var camera=GameObject.Find("Cais Gameplay Camera").GetComponent<Camera>();var target=camera.targetTexture;
  var points=new[]{new Vector2(-40,-69),new Vector2(-40,-58),new Vector2(-38,-53),new Vector2(-42,-45),new Vector2(-46,-33),new Vector2(-42,-28),new Vector2(-33,-23),new Vector2(-16,-19),new Vector2(-8,-15),new Vector2(0,-11)};
  session.enabled=false;player.ControlEnabled=true;
  Directory.CreateDirectory(Folder+"frames");
  int index=0,frame=0;float start=Time.realtimeSinceStartup,next=start,holdUntil=start+1.5f;bool low=false,high=false;float endAt=0;
  var frames=new List<object>();var positions=new List<object>();var tex=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);
  tick=()=>{
   try{
    if(!EditorApplication.isPlaying){EditorApplication.update-=tick;UnityEngine.Object.DestroyImmediate(tex);return;}
    float now=Time.realtimeSinceStartup,elapsed=now-start;var p=player.transform.position;var delta=new Vector3(points[index].x-p.x,0,points[index].y-p.z);
    if(now>=holdUntil && delta.magnitude<.7f && index<points.Length-1){if(index==1){low=true;holdUntil=now+1.4f;}index++;delta=new Vector3(points[index].x-p.x,0,points[index].y-p.z);}
    if(index==points.Length-1 && delta.magnitude<.4f && !high){high=true;endAt=now+2;}
    var dir=delta.normalized;var f=Vector3.ProjectOnPlane(camera.transform.forward,Vector3.up).normalized;var r=Vector3.ProjectOnPlane(camera.transform.right,Vector3.up).normalized;
    player.SetMoveInput(high||now<holdUntil?Vector2.zero:new Vector2(Vector3.Dot(dir,r),Vector3.Dot(dir,f)));
    if(now>=next){var old=RenderTexture.active;RenderTexture.active=target;tex.ReadPixels(new Rect(0,0,target.width,target.height),0,0);tex.Apply();RenderTexture.active=old;string name=frame.ToString("D5")+".png";File.WriteAllBytes(Folder+"frames/"+name,tex.EncodeToPNG());frames.Add(new {file=name,time=elapsed});positions.Add(new {time=elapsed,x=p.x,y=p.y,z=p.z,grounded=player.Body.isGrounded});frame++;next=now+1f/12;}
    if((high&&now>=endAt)||elapsed>95){player.SetMoveInput(Vector2.zero);EditorApplication.update-=tick;UnityEngine.Object.DestroyImmediate(tex);File.WriteAllText(Folder+"frames.json",Newtonsoft.Json.JsonConvert.SerializeObject(frames));File.WriteAllText(Folder+"trajectory.json",Newtonsoft.Json.JsonConvert.SerializeObject(positions));File.WriteAllText(Folder+"walk-result.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{reachedLow=low,reachedHigh=high,seconds=elapsed,frames=frame,position=p.ToString("F3"),lastPoint=index}));}
   }catch(Exception ex){EditorApplication.update-=tick;player.SetMoveInput(Vector2.zero);File.WriteAllText(Folder+"record-error.txt",ex.ToString());}
  };
  EditorApplication.update+=tick;return "Captura real iniciada, com timestamps por quadro.";
 }
}
