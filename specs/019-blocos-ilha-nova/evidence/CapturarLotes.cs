using System.IO;
using UnityEngine;

public static class CapturarLotes019
{
    const string Folder="/Users/bruno.duartec/Documents/Projects/Freyja/specs/019-blocos-ilha-nova/evidence/";
    static void Capture(string name,Vector3 focus,Vector3 offset,float size,int width,int height)
    {
        var go=new GameObject("Camera de inspeção temporária"){hideFlags=HideFlags.HideAndDontSave};
        var camera=go.AddComponent<Camera>();
        camera.orthographic=true;camera.orthographicSize=size;
        camera.transform.position=focus+offset;camera.transform.LookAt(focus);
        if(Mathf.Abs(offset.x)<.01f && Mathf.Abs(offset.z)<.01f)camera.transform.rotation=Quaternion.Euler(90,0,0);
        camera.nearClipPlane=.1f;camera.farClipPlane=400;
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.55f,.78f,.80f);
        var target=new RenderTexture(width,height,24);var pixels=new Texture2D(width,height,TextureFormat.RGB24,false);
        var previous=RenderTexture.active;
        try{
            camera.targetTexture=target;camera.Render();RenderTexture.active=target;
            pixels.ReadPixels(new Rect(0,0,width,height),0,0);pixels.Apply();
            File.WriteAllBytes(Folder+name+".png",pixels.EncodeToPNG());
        }
        finally{
            RenderTexture.active=previous;camera.targetTexture=null;
            Object.DestroyImmediate(pixels);Object.DestroyImmediate(target);Object.DestroyImmediate(go);
        }
    }
    public static string Main()
    {
        Capture("implantacao-lotes",new Vector3(0,5,0),new Vector3(100,110,100),45,1920,1320);
        Capture("planta-blocos",Vector3.zero,new Vector3(0,100,.001f),55,2048,2048);
        var root=GameObject.Find("Blocos 019 - Implantacao").transform;
        Capture("detalhe-igreja",root.Find("Igreja").position+Vector3.up*2,new Vector3(45,55,45),17,1400,1000);
        Capture("detalhe-centro",new Vector3(0,6,0),new Vector3(45,55,45),23,1400,1000);
        Capture("detalhe-cais",new Vector3(23,5,1),new Vector3(45,55,45),19,1400,1000);
        return "Cinco vistas de inspeção salvas.";
    }
}
