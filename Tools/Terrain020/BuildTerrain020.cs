using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class BuildTerrain020
{
    const string Art="Assets/_Project/Art/Terrain020/";
    const string Data="Assets/_Project/Data/Terrain020/";
    const string Prefabs="Assets/_Project/Prefabs/Terrain020/";
    const string Evidence="/Users/bruno.duartec/Documents/Projects/Freyja/.utmp/020-teste-terreno-modular/specs/020-teste-terreno-modular/evidence/";
    public static Vector3 Origin=new Vector3(200,0,0);
    public static Vector3[] Road={new Vector3(-8,1,2),new Vector3(-8,1.2f,5),new Vector3(-8,2,9),new Vector3(-8,2,10.5f),new Vector3(-4,2,11),new Vector3(0,2.4f,13),new Vector3(3,3,16),new Vector3(4.5f,4,20),new Vector3(5,5,23.2f),new Vector3(8,5,24)};
    static Transform root;
    static float S(float a,float b,float x){var t=Mathf.Clamp01((x-a)/(b-a));return t*t*(3-2*t);}
    static float Rect(float x,float z,float cx,float cz,float w,float d)=>new Vector2(Mathf.Max(0,Mathf.Abs(x-cx)-w/2),Mathf.Max(0,Mathf.Abs(z-cz)-d/2)).magnitude;
    public static (float distance,float height) RoadSample(float x,float z)
    {
        float best=float.MaxValue,y=0;
        for(int i=0;i<Road.Length-1;i++){var a=new Vector2(Road[i].x,Road[i].z);var b=new Vector2(Road[i+1].x,Road[i+1].z);var p=new Vector2(x,z);float t=Mathf.Clamp01(Vector2.Dot(p-a,b-a)/(b-a).sqrMagnitude);float dist=Vector2.Distance(p,a+(b-a)*t);if(dist<best){best=dist;y=Mathf.Lerp(Road[i].y,Road[i+1].y,t);}}
        return(best,y);
    }
    public static float Height(float x,float z)
    {
        float shore=-2+1.8f*Mathf.Sin(x*.15f);
        float h=-1.3f+2.65f*S(shore-3,shore+6,z)+4.2f*S(8,37,z);
        h+=(Mathf.PerlinNoise(x*.12f+10,z*.1f+10)-.5f)*.65f*S(2,12,z);
        h=Mathf.Lerp(h,2,1-S(0,3,Rect(x,z,-9,14,10,10)));
        h=Mathf.Lerp(h,5,1-S(0,.35f,Rect(x,z,8,29,12,12)));
        var road=RoadSample(x,z);h=Mathf.Lerp(h,road.height,1-S(1.5f,3.1f,road.distance));
        // Exact plot interiors, with an apron that keeps interpolation outside them.
        h=Mathf.Lerp(h,2,1-S(0,1.0f,Rect(x,z,-9,14,6.5f,7.5f)));
        h=Mathf.Lerp(h,5,1-S(0,1.0f,Rect(x,z,8,29,8.5f,9.5f)));
        float edge=Mathf.Max(S(19,24,Mathf.Abs(x)),S(36,40,z));
        return Mathf.Lerp(h,-1.4f,edge);
    }
    static Material Mat(string name,Color c,string shader="Standard")
    {
        string path=Data+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!m){m=new Material(Shader.Find(shader));AssetDatabase.CreateAsset(m,path);}m.color=c;if(m.HasProperty("_Glossiness"))m.SetFloat("_Glossiness",0);return m;
    }
    static GameObject Cube(string name,Vector3 position,Vector3 scale,Material mat,bool solid=true)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(root,false);go.transform.localPosition=position;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=mat;go.layer=solid?8:0;if(!solid)UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());return go;
    }
    static GameObject Piece(string id,Vector3 position,Vector3 scale,float yaw,bool solid=true)
    {
        var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Art+id+".fbx");if(!asset)throw new Exception("Modelo ausente: "+id);
        var go=(GameObject)PrefabUtility.InstantiatePrefab(asset);go.name=id;go.transform.SetParent(root,false);go.transform.localPosition=position;go.transform.localRotation=Quaternion.Euler(0,yaw,0);go.transform.localScale=scale;
        foreach(var mf in go.GetComponentsInChildren<MeshFilter>()){mf.gameObject.layer=solid?8:0;if(solid){var col=mf.gameObject.AddComponent<MeshCollider>();col.sharedMesh=mf.sharedMesh;}}
        return go;
    }
    public static string RefreshHeightmap()
    {
        var td=GameObject.Find("Base heightmap editavel").GetComponent<Terrain>().terrainData;var h=new float[513,513];var raw=new byte[513*513*2];
        for(int z=0;z<513;z++)for(int x=0;x<513;x++){h[z,x]=(Height(x/512f*48-24,z/512f*56-16)+2)/12;ushort v=(ushort)Mathf.RoundToInt(h[z,x]*65535);int k=(z*513+x)*2;raw[k]=(byte)(v&255);raw[k+1]=(byte)(v>>8);}
        td.SetHeights(0,0,h);EditorUtility.SetDirty(td);File.WriteAllBytes(Art+"heightmap-513-u16-le.raw",raw);AssetDatabase.SaveAssets();return "Patamar superior refinado.";
    }
    public static string Main()
    {
        if(EditorApplication.isPlaying || Application.dataPath!="/Users/bruno.duartec/Documents/Projects/Freyja/Freyja/Assets" || SceneManager.GetActiveScene().name!="Ilha_Tripo_Caminhada_011")throw new Exception("Projeto/cena/modo incorreto.");
        if(SceneManager.GetActiveScene().GetRootGameObjects().Any(g=>g.name=="Terreno modular 020 - Teste"))return "Conjunto existente: preservado.";
        Directory.CreateDirectory(Data);Directory.CreateDirectory(Prefabs);AssetDatabase.Refresh();
        foreach(var id in new[]{"Grass","Sand","Paving"}){var importer=(TextureImporter)AssetImporter.GetAtPath(Art+id+".png");importer.wrapMode=TextureWrapMode.Repeat;importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=true;importer.SaveAndReimport();}
        var td=new TerrainData{heightmapResolution=513,size=new Vector3(48,12,56),alphamapResolution=256,baseMapResolution=256};AssetDatabase.CreateAsset(td,Data+"Terrain020.asset");
        var heights=new float[513,513];var raw=new byte[513*513*2];
        for(int z=0;z<513;z++)for(int x=0;x<513;x++){float h=(Height(x/512f*48-24,z/512f*56-16)+2)/12;heights[z,x]=h;ushort v=(ushort)Mathf.RoundToInt(h*65535);int k=(z*513+x)*2;raw[k]=(byte)(v&255);raw[k+1]=(byte)(v>>8);}
        td.SetHeights(0,0,heights);File.WriteAllBytes(Art+"heightmap-513-u16-le.raw",raw);
        string[] texNames={"Grass","Sand","Paving"};var layers=new TerrainLayer[3];
        for(int i=0;i<3;i++){layers[i]=new TerrainLayer{diffuseTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+texNames[i]+".png"),tileSize=new Vector2(i==2?3:4,i==2?3:4),metallic=0,smoothness=0};AssetDatabase.CreateAsset(layers[i],Data+texNames[i]+".terrainlayer");}
        td.terrainLayers=layers;
        var alpha=new float[256,256,3];
        for(int z=0;z<256;z++)for(int x=0;x<256;x++){float wx=x/255f*48-24,wz=z/255f*56-16;float beach=1-S(3,9,wz-1.8f*Mathf.Sin(wx*.15f));float path=1-S(1.35f,1.75f,RoadSample(wx,wz).distance);path*=S(3,7,wz);alpha[z,x,2]=path;alpha[z,x,1]=beach*(1-path);alpha[z,x,0]=(1-beach)*(1-path);}
        td.SetAlphamaps(0,0,alpha);td.SetBaseMapDirty();EditorUtility.SetDirty(td);
        foreach(var texture in td.alphamapTextures)EditorUtility.SetDirty(texture);
        root=new GameObject("Terreno modular 020 - Teste").transform;root.position=Origin;
        var terrain=Terrain.CreateTerrainGameObject(td);terrain.name="Base heightmap editavel";terrain.transform.SetParent(root,false);terrain.transform.localPosition=new Vector3(-24,-2,-16);terrain.layer=8;var tc=terrain.GetComponent<Terrain>();tc.heightmapPixelError=2;tc.basemapDistance=160;tc.drawInstanced=true;
        var sea=Mat("Mar",new Color(.10f,.43f,.49f),"Unlit/Color");Cube("Mar visual",new Vector3(0,-.18f,10),new Vector3(220,.15f,220),sea,false);
        var pier=Piece("Pier_10m",new Vector3(-8,1,2),Vector3.one,180,false);
        var pc=pier.AddComponent<BoxCollider>();pc.center=new Vector3(0,-.12f,5);pc.size=new Vector3(2.8f,.24f,10);pier.layer=8;
        var stone=Mat("BaseCais",new Color(.59f,.52f,.39f));Cube("Plataforma do cais",new Vector3(-8,.5f,2.7f),new Vector3(4,1,2.4f),stone);
        var coral=Mat("VolumeA",new Color(.82f,.65f,.42f));var cream=Mat("VolumeB",new Color(.82f,.78f,.57f));var roof=Mat("Cobertura",new Color(.56f,.24f,.12f));var door=Mat("Portas",new Color(.12f,.29f,.28f));
        foreach(var l in new[]{("Lote A",-9f,14f,5f,6f,2f,3.4f,coral),("Lote B",8f,29f,7f,8f,5f,5.5f,cream)})
        {
            Cube(l.Item1+" - volume",new Vector3(l.Item2,l.Item6+l.Item7/2,l.Item3),new Vector3(l.Item4,l.Item7,l.Item5),l.Item8);
            Cube(l.Item1+" - cobertura",new Vector3(l.Item2,l.Item6+l.Item7+.09f,l.Item3),new Vector3(l.Item4,.18f,l.Item5),roof,false);
            Cube(l.Item1+" - frente",new Vector3(l.Item2,l.Item6+1,l.Item3-l.Item5/2-.025f),new Vector3(1.15f,2,.05f),door,false);
        }
        // Coast rock kit, repeated and individually editable.
        var rng=new System.Random(20);
        for(int side=-1;side<=1;side+=2)for(int i=0;i<17;i++)
        {
            float z=-1+i*2.15f,x=side*(20.2f+(float)rng.NextDouble()*1.5f),h=Height(x,z);
            float k=1.2f+(float)rng.NextDouble()*1.1f;
            Piece("Rock_"+(char)('A'+i%3),new Vector3(x,h-.5f,z),new Vector3(k,1.0f+k*.35f,k),rng.Next(360));
        }
        for(int i=0;i<10;i++){float x=-18+i*4,z=34+(float)rng.NextDouble()*2;Piece("Rock_"+(char)('A'+i%3),new Vector3(x,Height(x,z)-.35f,z),new Vector3(1.8f,1.5f,1.4f),rng.Next(360));}
        for(int i=0;i<3;i++)Piece("Wall_2m",new Vector3(14.2f,4.05f,25+i*2.03f),Vector3.one,90);
        for(int i=0;i<2;i++)Piece("Wall_2m",new Vector3(11.7f+i*2.04f,3.52f,22.9f),new Vector3(1,1.45f,1),0);
        for(int i=0;i<4;i++)Piece("Wall_2m",new Vector3(-14.2f,1.03f,10.5f+i*2),Vector3.one,90);
        // Low curb follows both sides; 3.6 m clear center corridor.
        for(int i=0;i<Road.Length-1;i++)
        {
            var a=Road[i];var b=Road[i+1];var delta=b-a;float length=new Vector2(delta.x,delta.z).magnitude;if(length<2)continue;
            var direction=new Vector3(delta.x,0,delta.z).normalized;var normal=new Vector3(-direction.z,0,direction.x);int n=Mathf.FloorToInt(length/2.1f);
            for(int j=0;j<n;j++)for(int side=-1;side<=1;side+=2){var p=Vector3.Lerp(a,b,(j+.5f)/n)+normal*side*1.95f;if(p.z<7)continue;p.y=Height(p.x,p.z)-.08f;Piece("Curb_2m",p,Vector3.one,Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg+90);}
        }
        var spawn=new GameObject("Spawn020");spawn.transform.SetParent(root,false);spawn.transform.localPosition=new Vector3(-8,1.08f,-6);spawn.transform.localRotation=Quaternion.identity;
        PrefabUtility.SaveAsPrefabAsset(root.gameObject,Prefabs+"Trecho020.prefab");AssetDatabase.SaveAssets();AssetDatabase.Refresh();Physics.SyncTransforms();
        Terrain020Preview.Activate();
        File.WriteAllText(Evidence+"layout.json",Newtonsoft.Json.JsonConvert.SerializeObject(new {originX=200,width=48,depth=56,low=new{width=5,depth=6,height=2},high=new{width=7,depth=8,height=5},heightDifference=3,pathWidth=3}));
        return "Trecho 020 montado e ativado na cena principal.";
    }
}
