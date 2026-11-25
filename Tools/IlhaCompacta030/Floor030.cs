using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Floor030
{
    const string Base = "Assets/_Project/Art/IlhaCompacta030/";
    const string Floor = Base + "Pisos/";
    const string Current = Base + "TerrenoCompacto.asset";
    const string Revised = Floor + "TerrenoPisos.asset";
    const string Evidence = "/Users/bruno.duartec/Documents/Projects/Freyja/Artifacts/IlhaCompacta030/Pisos/";

    public static object Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.name != "Ilha_Tripo_Caminhada_011" || EditorApplication.isPlaying)
            throw new InvalidOperationException("Abra a cena da ilha fora do Play Mode.");
        var island = scene.GetRootGameObjects().Single(x => x.name == "Island023");
        var terrain = island.GetComponentInChildren<Terrain>();
        string assetPath = AssetDatabase.GetAssetPath(terrain.terrainData);
        if (assetPath != Current && assetPath != Revised)
            throw new InvalidOperationException("A implantação compacta não está ativa.");
        Directory.CreateDirectory(Evidence);
        if (assetPath == Current)
        {
            string backup = Evidence + "Cena-antes-pisos.unity";
            if (!File.Exists(backup)) File.Copy(scene.path,backup);
            Capture("antes");
        }
        AssetDatabase.Refresh();
        var source = AssetDatabase.LoadAssetAtPath<TerrainData>(Current);
        var revised = AssetDatabase.LoadAssetAtPath<TerrainData>(Revised);
        if (!revised)
        {
            if (!AssetDatabase.CopyAsset(Current,Revised)) throw new IOException("Falha ao duplicar o terreno.");
            revised = AssetDatabase.LoadAssetAtPath<TerrainData>(Revised);
        }
        var stone = MakeLayer("LajesDaPraca");
        var earth = MakeLayer("TerraBatida");
        var sparse = MakeLayer("LajesNaTerra");
        var report = Paint(source,revised,stone,earth,sparse);
        // Painting must leave the walking surface identical down to the saved height samples.
        if (!SameSurface(source,revised)) throw new InvalidOperationException("A altura do terreno mudou.");
        var collider = terrain.GetComponent<TerrainCollider>();
        Undo.RecordObjects(new UnityEngine.Object[]{terrain,collider}, "Suavizar transição da praça");
        terrain.terrainData = revised;
        collider.terrainData = revised;
        terrain.Flush();
        EditorUtility.SetDirty(revised);
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Falha ao salvar a cena da ilha.");
        var captures = Capture("depois");
        return new {report, heightUnchanged=true, source=Current,revised=Revised,captures};
    }

    static TerrainLayer MakeLayer(string name)
    {
        var image = Floor+name+".png";
        var importer = AssetImporter.GetAtPath(image) as TextureImporter;
        if (!importer) throw new InvalidOperationException("Textura ausente: "+image);
        importer.textureType = TextureImporterType.Default;
        importer.filterMode = FilterMode.Point;
        importer.wrapMode = TextureWrapMode.Repeat;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 64;
        importer.SaveAndReimport();
        var path = Floor+name+".terrainlayer";
        var layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(path);
        if (!layer)
        {
            layer = new TerrainLayer {name=name};
            AssetDatabase.CreateAsset(layer,path);
        }
        layer.diffuseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(image);
        layer.tileSize = new Vector2(3,3);
        layer.smoothness = 0;
        EditorUtility.SetDirty(layer);
        return layer;
    }

    static object Paint(TerrainData source, TerrainData target, TerrainLayer stone, TerrainLayer earth, TerrainLayer sparse)
    {
        int n = source.alphamapResolution;
        var old = source.GetAlphamaps(0,0,n,n);
        var layers = source.terrainLayers;
        int grass = Array.FindIndex(layers,l=>l.name=="Grass");
        int path = Array.FindIndex(layers,l=>l.name=="PathSand");
        int plaza = Array.FindIndex(layers,l=>l.diffuseTexture && l.diffuseTexture.name=="PedraDoLargo");
        if (grass<0 || path<0 || plaza<0) throw new InvalidOperationException("As camadas originais estão ausentes.");
        int worn = layers.Length;
        layers[plaza]=stone;
        layers[path]=earth;
        target.terrainLayers=layers.Append(sparse).ToArray();
        float step=source.size.x/(n-1);
        var distance=DistanceFromPaving(old,plaza,step);
        var mix=new float[n,n,layers.Length+1];
        int changed=0;
        float peak=0,maxDrift=0;
        for(int z=0;z<n;z++) for(int x=0;x<n;x++)
        {
            for(int k=0;k<layers.Length;k++) mix[z,x,k]=old[z,x,k];
            float pathWeight=old[z,x,path], stoneWeight=old[z,x,plaza], grassWeight=old[z,x,grass];
            float irregularity=(Mathf.PerlinNoise(x*.125f,z*.125f)-.5f)*1.05f;
            float d=Mathf.Max(0,distance[z,x]+irregularity);
            float toStone=1-Fade(.15f,1.9f,d);
            float toSparse=(1-toStone)*(1-Fade(2.5f,7.1f,d));
            // Stay on the existing path, with a longer band of scattered stones at every exit.
            mix[z,x,path]-=pathWeight*(toStone+toSparse);
            mix[z,x,plaza]+=pathWeight*toStone;
            mix[z,x,worn]+=pathWeight*toSparse;
            // Erode the edge softly where the plaza already meets grass.
            float verge=stoneWeight*grassWeight*.43f;
            mix[z,x,plaza]-=verge;
            mix[z,x,worn]+=verge;
            if (mix[z,x,worn]>.025f) changed++;
            peak=Mathf.Max(peak,mix[z,x,worn]);
            float total=0;
            for(int k=0;k<=worn;k++)total+=mix[z,x,k];
            maxDrift=Mathf.Max(maxDrift,Mathf.Abs(total-1));
        }
        target.SetAlphamaps(0,0,mix);
        target.SetBaseMapDirty();
        foreach(var map in target.alphamapTextures) EditorUtility.SetDirty(map);
        return new {transitionSamples=changed,maxTransitionWeight=peak,weightDrift=maxDrift,extentMeters=7.1f};
    }

    static float Fade(float a,float b,float p) => Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,p));

    static float[,] DistanceFromPaving(float[,,] map,int layer,float step)
    {
        int n=map.GetLength(0);
        var result=new float[n,n];
        float diagonal=step*1.41421356f;
        for(int z=0;z<n;z++)for(int x=0;x<n;x++)result[z,x]=map[z,x,layer]>=.58f?0:float.PositiveInfinity;
        for(int z=0;z<n;z++)for(int x=0;x<n;x++)
        {
            if(x>0)Improve(x,z,x-1,z,step);
            if(z>0)
            {
                Improve(x,z,x,z-1,step);
                if(x>0)Improve(x,z,x-1,z-1,diagonal);
                if(x+1<n)Improve(x,z,x+1,z-1,diagonal);
            }
        }
        for(int z=n-1;z>=0;z--)for(int x=n-1;x>=0;x--)
        {
            if(x+1<n)Improve(x,z,x+1,z,step);
            if(z+1<n)
            {
                Improve(x,z,x,z+1,step);
                if(x>0)Improve(x,z,x-1,z+1,diagonal);
                if(x+1<n)Improve(x,z,x+1,z+1,diagonal);
            }
        }
        return result;
        void Improve(int x,int z,int u,int v,float cost) => result[z,x]=Mathf.Min(result[z,x],result[v,u]+cost);
    }

    static bool SameSurface(TerrainData a,TerrainData b)
    {
        if (a.size!=b.size || a.heightmapResolution!=b.heightmapResolution) return false;
        int n=a.heightmapResolution;
        var x=a.GetHeights(0,0,n,n);var y=b.GetHeights(0,0,n,n);
        for(int i=0;i<n;i++) for(int j=0;j<n;j++) if(x[i,j]!=y[i,j])return false;
        return true;
    }

    static string[] Capture(string phase)
    {
        var holder=new GameObject("Captura temporária dos pisos"){hideFlags=HideFlags.HideAndDontSave};
        var c=holder.AddComponent<Camera>();
        var rt=new RenderTexture(1400,960,24);
        var image=new Texture2D(1400,960,TextureFormat.RGB24,false);
        var prior=RenderTexture.active;
        try
        {
            c.targetTexture=rt;c.clearFlags=CameraClearFlags.SolidColor;
            c.backgroundColor=new Color(.13f,.46f,.53f);c.allowHDR=false;c.allowMSAA=false;
            c.farClipPlane=800;c.fieldOfView=46;
            return new[]{
                Shot("geral",new Vector3(-17,33,-39),new Vector3(0,6,-6)),
                Shot("chegada",new Vector3(-14,8.1f,-20.6f),new Vector3(-3,6.7f,-5)),
                Shot("subida",new Vector3(-4,8.3f,-8.9f),new Vector3(5,7,2))};
        }
        finally
        {
            RenderTexture.active=prior;c.targetTexture=null;
            UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(holder);
        }
        string Shot(string file,Vector3 eye,Vector3 focus)
        {
            c.transform.position=eye;c.transform.LookAt(focus);c.Render();RenderTexture.active=rt;
            image.ReadPixels(new Rect(0,0,1400,960),0,0);image.Apply();
            var path=Evidence+file+"-"+phase+".png";File.WriteAllBytes(path,image.EncodeToPNG());return path;
        }
    }
}
