using System.IO;
using UnityEngine;
public static class CaptureTerrain020
{
 const string Folder="/Users/bruno.duartec/Documents/Projects/Freyja/.utmp/020-teste-terreno-modular/specs/020-teste-terreno-modular/evidence/";
 static void Shot(string name,Vector3 focus,Vector3 offset,float size)
 {
  var go=new GameObject("Capture020"){hideFlags=HideFlags.HideAndDontSave};var camera=go.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=size;
  camera.transform.position=focus+offset;camera.transform.LookAt(focus);camera.farClipPlane=400;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.43f,.66f,.70f);
  var rt=new RenderTexture(1600,1000,24);var tex=new Texture2D(1600,1000,TextureFormat.RGB24,false);var old=RenderTexture.active;
  try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1600,1000),0,0);tex.Apply();File.WriteAllBytes(Folder+name+".png",tex.EncodeToPNG());}
  finally{RenderTexture.active=old;camera.targetTexture=null;Object.DestroyImmediate(tex);Object.DestroyImmediate(rt);Object.DestroyImmediate(go);}
 }
 public static string Main()
 {
  Shot("vista-geral",new Vector3(200,2.5f,14),new Vector3(43,42,-52),35);
  Shot("cais-praia",new Vector3(194,1.5f,2),new Vector3(28,28,-38),18);
  Shot("dois-patamares",new Vector3(201,3,22),new Vector3(30,29,-38),19);
  return "Três capturas salvas.";
 }
}
