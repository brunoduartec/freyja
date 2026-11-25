using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class QuayFinish027
{
    const string Folder = "Assets/_Project/Art/Praca027/";
    const string Evidence = "/Users/bruno.duartec/Documents/Projects/Freyja/Artifacts/Praca027/";

    public static object Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.name != "Ilha_Tripo_Caminhada_011" || EditorApplication.isPlaying)
            throw new InvalidOperationException("Abra a cena principal fora de Play Mode.");
        var landing = GameObject.Find("Island023").transform.Find("Cais e píer/Cais de pedra");
        var renderer = landing.GetComponent<MeshRenderer>();
        var filter = landing.GetComponent<MeshFilter>();
        var collider = landing.GetComponent<BoxCollider>();
        var before = collider.bounds;
        var oldShared = renderer.sharedMaterial;
        var assetPath = Folder + "CaisUV.asset";
        var revised = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
        if (!revised)
        {
            Shot("cais-antes.png");
            var original = filter.sharedMesh;
            revised = UnityEngine.Object.Instantiate(original);
            revised.name = "Cais de pedra — UV em metros";
            var points = original.vertices;
            var normals = original.normals;
            var uv = new Vector2[points.Length];
            Vector3 scale = landing.lossyScale;
            for (int i = 0; i < points.Length; i++)
            {
                var p = Vector3.Scale(points[i] + Vector3.one * .5f, scale);
                var n = normals[i];
                uv[i] = Mathf.Abs(n.y) > .5f ? new Vector2(p.x,p.z)/3
                    : new Vector2(Mathf.Abs(n.x) > .5f ? p.z : p.x,p.y)/2.45f;
            }
            var surface = new List<int>();
            var sides = new List<int>();
            var faces = original.triangles;
            for(int i=0;i<faces.Length;i+=3)
            {
                var group = normals[faces[i]].y > .5f ? surface : sides;
                group.Add(faces[i]); group.Add(faces[i+1]); group.Add(faces[i+2]);
            }
            revised.uv = uv;
            revised.subMeshCount = 2;
            revised.SetTriangles(surface,0);
            revised.SetTriangles(sides,1);
            AssetDatabase.CreateAsset(revised, assetPath);
        }
        var paving = AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"PedraDoLargo.png");
        if (!paving) throw new InvalidOperationException("A textura de calçamento está ausente.");
        var top = MaterialAt("CaisPiso", paving, new Color(.98f,.93f,.85f));
        var wall = AssetDatabase.LoadAssetAtPath<Material>(Folder+"CaisAlvenaria.mat");
        if (!wall) wall = MaterialAt("CaisAlvenaria", oldShared.mainTexture, new Color(1f,.86f,.69f));
        Undo.RecordObjects(new UnityEngine.Object[]{filter,renderer}, "Ajustar acabamento do cais");
        filter.sharedMesh = revised;
        renderer.sharedMaterials = new[] {top,wall};
        var after = collider.bounds;
        if (after.center != before.center || after.size != before.size)
            throw new InvalidOperationException("O apoio físico do cais foi alterado.");
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return new {landing=landing.name, dimensions=after.size.ToString("F2"), snapshot=Shot("cais-depois.png")};
    }

    static Material MaterialAt(string name, Texture texture,Color color)
    {
        var path = Folder+name+".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!material)
        {
            material = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(material,path);
        }
        material.mainTexture = texture;
        material.color = color;
        material.SetFloat("_Glossiness",0);
        EditorUtility.SetDirty(material);
        return material;
    }

    static string Shot(string name)
    {
        var holder = new GameObject("Captura temporária do desembarque");
        var camera = holder.AddComponent<Camera>();
        var buffer=new RenderTexture(1200,840,24);
        var image=new Texture2D(1200,840,TextureFormat.RGB24,false);
        var previous=RenderTexture.active;
        try
        {
            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=new Color(.13f,.46f,.53f);
            camera.fieldOfView=50;camera.allowMSAA=false;camera.allowHDR=false;
            camera.transform.position=new Vector3(-23,13,-66);
            camera.transform.LookAt(new Vector3(-39,1.7f,-55.8f));
            camera.targetTexture=buffer;camera.Render();RenderTexture.active=buffer;
            image.ReadPixels(new Rect(0,0,1200,840),0,0);image.Apply();
            var path=Evidence+name;
            File.WriteAllBytes(path,image.EncodeToPNG());return path;
        }
        finally
        {
            RenderTexture.active=previous;camera.targetTexture=null;
            UnityEngine.Object.DestroyImmediate(buffer);UnityEngine.Object.DestroyImmediate(image);
            UnityEngine.Object.DestroyImmediate(holder);
        }
    }
}
