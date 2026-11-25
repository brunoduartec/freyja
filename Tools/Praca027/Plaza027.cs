using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Plaza027
{
    const string Folder = "Assets/_Project/Art/Praca027/";
    const string Original = "Assets/_Project/Art/Island023Tripo/TerrenoComAcessos.asset";
    const string Base = "/Users/bruno.duartec/Documents/Projects/Freyja/Artifacts/";
    const string Evidence = Base + "Praca027/";
    const string Target = Folder + "TerrenoDaPraca.asset";

    // The outline follows the three facades and opens towards the roads on the south and north.
    static readonly Vector2[] Control = {
        new(-18.6f,-13.8f), new(-16.0f,-15.7f), new(-10.5f,-16.0f), new(-5.2f,-17.4f),
        new(-0.4f,-18.7f), new(4.7f,-17.4f), new(10.6f,-15.2f), new(17.4f,-13.5f),
        new(21.7f,-10.8f), new(22.0f,-8.2f), new(20.3f,-6.8f), new(17.0f,-6.3f),
        new(13.8f,-6.6f), new(10.3f,-4.8f), new(7.9f,-1.5f), new(7.0f,3.4f),
        new(5.8f,5.6f), new(3.9f,4.9f), new(0.5f,4.65f), new(-2.7f,4.45f),
        new(-4.6f,2.5f), new(-7.0f,1.5f), new(-10.1f,1.3f), new(-13.4f,0.8f),
        new(-15.7f,-1.5f), new(-16.6f,-5.5f), new(-16.0f,-9.1f)
    };
    static readonly Vector2[] Outline = RoundedOutline();

    static Vector2[] RoundedOutline()
    {
        var output = new List<Vector2>();
        int count = Control.Length;
        for (int i = 0; i < count; i++)
        {
            Vector2 a = Control[(i+count-1)%count], b = Control[i], c = Control[(i+1)%count], d = Control[(i+2)%count];
            for (int step = 0; step < 6; step++)
            {
                float t = step / 6f, t2 = t*t, t3 = t2*t;
                output.Add(.5f * (2*b+(c-a)*t+(2*a-5*b+4*c-d)*t2+(-a+3*b-3*c+d)*t3));
            }
        }
        return output.ToArray();
    }

    static float SignedDistance(float x, float z)
    {
        var point = new Vector2(x,z);
        float edgeSquared = float.PositiveInfinity;
        bool inside = false;
        for (int i = 0; i < Outline.Length; i++)
        {
            Vector2 a = Outline[i], b = Outline[(i+1)%Outline.Length], edge = b-a;
            float t = Mathf.Clamp01(Vector2.Dot(point-a,edge) / edge.sqrMagnitude);
            edgeSquared = Mathf.Min(edgeSquared, (point-(a+t*edge)).sqrMagnitude);
            if ((a.y>z)!=(b.y>z) && x < a.x + (b.x-a.x)*(z-a.y)/(b.y-a.y)) inside = !inside;
        }
        return Mathf.Sqrt(edgeSquared) * (inside ? -1 : 1);
    }

    static float Smooth(float start,float end,float value) => Mathf.SmoothStep(0,1,Mathf.InverseLerp(start,end,value));

    static float Height(float x, float z)
    {
        // Shop approaches share the upper grade; the hall sits one metre lower to the east.
        float upper = 5.93f + 1.07f * Smooth(-16.8f, 1f, z);
        float lower = 5.98f + .045f * Smooth(-15f, -6f, z);
        float result = Mathf.Lerp(upper, lower, Smooth(3.8f, 16.4f, x));
        // Continue the central incline into the uphill road instead of ending it in a lip.
        result += .13f * Smooth(2.8f,5.6f,z) * (1-Smooth(1f,3f,Mathf.Abs(x-6f)));
        return result;
    }

    static GameObject Island()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.name != "Ilha_Tripo_Caminhada_011" || EditorApplication.isPlaying)
            throw new InvalidOperationException("Abra a cena principal fora do Play Mode.");
        return GameObject.Find("Island023") ?? throw new InvalidOperationException("Ilha ausente");
    }

    public static object Before()
    {
        var island = Island();
        var terrain = island.GetComponentInChildren<Terrain>();
        if (AssetDatabase.GetAssetPath(terrain.terrainData) != Original)
            throw new InvalidOperationException("A praça anterior já foi trocada.");
        Directory.CreateDirectory(Evidence);
        var backup = Evidence + "Cena-antes-plaza.unity";
        if (!File.Exists(backup)) File.Copy(island.scene.path, backup);
        return Snapshots("antes");
    }

    public static object Apply()
    {
        var island = Island();
        var terrain = island.GetComponentInChildren<Terrain>();
        var data = AssetDatabase.LoadAssetAtPath<TerrainData>(Target);
        if (!data)
        {
            if (AssetDatabase.GetAssetPath(terrain.terrainData) != Original || !AssetDatabase.CopyAsset(Original, Target))
                throw new InvalidOperationException("Não foi possível copiar o terreno atual da ilha.");
            data = AssetDatabase.LoadAssetAtPath<TerrainData>(Target);
        }
        var original = AssetDatabase.LoadAssetAtPath<TerrainData>(Original);
        var origin = terrain.transform.position;
        var lots = island.transform.Find("Construções — 12 casas e 5 serviços");
        var doors = new[] { "Venda", "Oficina", "Salão comunitário" }
            .ToDictionary(n => n, n => lots.Find(n+"/Door023").position);
        var originalDoors = doors.ToDictionary(p=>p.Key,p=>terrain.SampleHeight(p.Value)+origin.y);
        Grade(original, data, origin, doors.Values.ToArray());
        Paint(original, data, origin);
        Undo.RecordObject(terrain, "Aplicar terreno da praça");
        var collider = terrain.GetComponent<TerrainCollider>();
        Undo.RecordObject(collider,"Aplicar terreno da praça");
        terrain.terrainData = data;
        collider.terrainData = data;
        terrain.Flush();
        EditorUtility.SetDirty(data);
        EditorSceneManager.MarkSceneDirty(island.scene);
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(island.scene);
        WriteRoutes(doors, terrain);
        var summary = new {
            scene = island.scene.path, original = Original, current = Target,
            doors = doors.Select(p=>new { lot=p.Key, threshold=p.Value.y, before=originalDoors[p.Key], after=terrain.SampleHeight(p.Value)+origin.y }).ToArray(),
            grade = CoreMetrics(terrain, original), snapshots=Snapshots("depois"), compileFailed=EditorUtility.scriptCompilationFailed
        };
        File.WriteAllText(Evidence + "terreno.json",Newtonsoft.Json.JsonConvert.SerializeObject(summary,Newtonsoft.Json.Formatting.Indented));
        return summary;
    }

    static void Grade(TerrainData baseline, TerrainData next, Vector3 origin, Vector3[] doors)
    {
        int last = baseline.heightmapResolution - 1;
        float step = baseline.size.x / last;
        int x0=Mathf.FloorToInt((-25-origin.x)/step), z0=Mathf.FloorToInt((-25-origin.z)/step);
        int x1=Mathf.CeilToInt((28-origin.x)/step), z1=Mathf.CeilToInt((12-origin.z)/step);
        var h = baseline.GetHeights(x0,z0,x1-x0+1,z1-z0+1);
        for(int j=0;j<h.GetLength(0);j++) for(int i=0;i<h.GetLength(1);i++)
        {
            float x=origin.x+(x0+i)*step,z=origin.z+(z0+j)*step;
            float weight=1-Smooth(.14f,4.15f,SignedDistance(x,z));
            if (weight <= 0) continue;
            // Landing at each measured doorstep, including the underlying foundation.
            foreach (var door in doors)
                weight *= Smooth(.64f,2f,Vector2.Distance(new Vector2(x,z),new Vector2(door.x,door.z)));
            h[j,i]=Mathf.Lerp(h[j,i],(Height(x,z)-origin.y)/baseline.size.y,weight);
        }
        next.SetHeights(x0,z0,h);
    }

    static void Paint(TerrainData baseline, TerrainData next, Vector3 origin)
    {
        AssetDatabase.Refresh();
        var texturePath = Folder + "PedraDoLargo.png";
        var import = (TextureImporter)AssetImporter.GetAtPath(texturePath);
        if (!import) throw new InvalidOperationException("A textura do largo não foi importada.");
        import.filterMode = FilterMode.Point;
        import.wrapMode = TextureWrapMode.Repeat;
        import.mipmapEnabled = false;
        import.textureCompression = TextureImporterCompression.Uncompressed;
        import.maxTextureSize = 64;
        import.SaveAndReimport();
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        var allLayers = baseline.terrainLayers;
        int index = Array.FindIndex(allLayers,l => l.name == "Praca");
        if (index < 0) throw new InvalidOperationException("Camada anterior da praça ausente.");
        var layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(Folder + "PedraDoLargo.terrainlayer");
        if (!layer)
        {
            layer = new TerrainLayer { name="Pedra do largo" };
            AssetDatabase.CreateAsset(layer,Folder+"PedraDoLargo.terrainlayer");
        }
        layer.diffuseTexture = texture;
        layer.tileSize = new Vector2(3.0f,3.0f);
        layer.smoothness = 0;
        EditorUtility.SetDirty(layer);
        allLayers[index] = layer;
        next.terrainLayers = allLayers;
        int last = baseline.alphamapResolution - 1;
        float step = baseline.size.x / last;
        int x0=Mathf.FloorToInt((-22-origin.x)/step), z0=Mathf.FloorToInt((-22-origin.z)/step);
        int x1=Mathf.CeilToInt((25-origin.x)/step), z1=Mathf.CeilToInt((9-origin.z)/step);
        var paint = baseline.GetAlphamaps(x0,z0,x1-x0+1,z1-z0+1);
        for(int j=0;j<paint.GetLength(0);j++) for(int i=0;i<paint.GetLength(1);i++)
        {
            float x=origin.x+(x0+i)*step,z=origin.z+(z0+j)*step;
            float distance = SignedDistance(x,z) + (Mathf.PerlinNoise(x*1.35f,z*1.35f)-.5f)*.34f;
            float stone=1-Smooth(-.24f,.29f,distance);
            paint[j,i,0]+=paint[j,i,index];
            for(int k=0;k<paint.GetLength(2);k++) if(k!=index) paint[j,i,k]*=1-stone;
            paint[j,i,index]=stone;
        }
        next.SetAlphamaps(x0,z0,paint);
        next.SetBaseMapDirty();
        foreach(var map in next.alphamapTextures)EditorUtility.SetDirty(map);
    }

    static object CoreMetrics(Terrain terrain, TerrainData old)
    {
        var origin=terrain.transform.position;
        float maxBefore=0,maxAfter=0;
        for(float x=-11;x<12;x+=.5f) for(float z=-14;z<-3;z+=.5f)
        {
            if(SignedDistance(x,z)>-.7f)continue;
            float h=old.GetInterpolatedHeight((x-origin.x)/old.size.x,(z-origin.z)/old.size.z);
            float hX=old.GetInterpolatedHeight((x+.5f-origin.x)/old.size.x,(z-origin.z)/old.size.z);
            float hZ=old.GetInterpolatedHeight((x-origin.x)/old.size.x,(z+.5f-origin.z)/old.size.z);
            float now=terrain.SampleHeight(new Vector3(x,0,z));
            float nowX=terrain.SampleHeight(new Vector3(x+.5f,0,z));
            float nowZ=terrain.SampleHeight(new Vector3(x,0,z+.5f));
            maxBefore=Mathf.Max(maxBefore,new Vector2(hX-h,hZ-h).magnitude*200);
            maxAfter=Mathf.Max(maxAfter,new Vector2(nowX-now,nowZ-now).magnitude*200);
        }
        return new { maxSlopeBeforePercent=maxBefore, maxSlopeAfterPercent=maxAfter };
    }

    static void WriteRoutes(Dictionary<string,Vector3> doors,Terrain terrain)
    {
        var tests=new JArray();
        AddDoor("Venda",new Vector2(-4.8f,-6),new Vector2(-8.8f,-1.2f));
        AddDoor("Oficina",new Vector2(0,-6),new Vector2(-1.2f,1.7f));
        AddDoor("Salão comunitário",new Vector2(7,-10),new Vector2(13,-11.2f),new Vector2(18.9f,-8.7f));
        var roads=JArray.Parse(File.ReadAllText(Base+"Island023/Tripo/roads-layout.json"));
        foreach(string name in new[]{"Cais — praça","Praça — igreja"})
        {
            var entry=(JObject)roads.Single(r=>(string)r["id"]==name).DeepClone();
            entry["door"]["y"]=Ground(Read(entry["door"]));
            tests.Add(entry);
        }
        File.WriteAllText(Evidence+"praca-layout.json",tests.ToString());

        float Ground(Vector3 p) => terrain.SampleHeight(p)+terrain.transform.position.y;
        void AddDoor(string name,params Vector2[] intermediates)
        {
            var endpoint=doors[name];
            var anchors=new List<Vector2>{new(0,-10)};
            anchors.AddRange(intermediates);
            anchors.Add(new Vector2(endpoint.x,endpoint.z));
            var points=new JArray();
            for(int i=1;i<anchors.Count;i++)
            {
                var a=anchors[i-1];var b=anchors[i];
                int n=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(a,b)/.5f));
                for(int j=0;j<n;j++)
                {
                    var v=Vector2.Lerp(a,b,j/(float)n);
                    var world=new Vector3(v.x,0,v.y);world.y=Ground(world);points.Add(Point(world));
                }
            }
            points.Add(Point(endpoint));
            tests.Add(new JObject{["id"]=name,["road"]=points.First.DeepClone(),["door"]=Point(endpoint),["points"]=points});
        }
    }
    static Vector3 Read(JToken p) => new Vector3((float)p["x"],(float)p["y"],(float)p["z"]);
    static JObject Point(Vector3 p) => new JObject{["x"]=p.x,["y"]=p.y,["z"]=p.z};

    static string[] Snapshots(string suffix)
    {
        var cameraObject=new GameObject("Captura temporária da praça");
        var camera=cameraObject.AddComponent<Camera>();
        camera.clearFlags=CameraClearFlags.SolidColor;
        camera.backgroundColor=new Color(.13f,.46f,.53f);
        camera.fieldOfView=47;camera.farClipPlane=800;
        camera.allowHDR=false;camera.allowMSAA=false;
        var target=new RenderTexture(1400,960,24);
        var pixels=new Texture2D(1400,960,TextureFormat.RGB24,false);
        var previous=RenderTexture.active;
        try
        {
            camera.targetTexture=target;
            return new[]{
                Shoot("geral",new Vector3(-12,37,-39),new Vector3(2,6,-4)),
                Shoot("travessia",new Vector3(-9,9.8f,-19),new Vector3(1,7.1f,3))};
        }
        finally
        {
            RenderTexture.active=previous;camera.targetTexture=null;
            UnityEngine.Object.DestroyImmediate(pixels);UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(cameraObject);
        }
        string Shoot(string name,Vector3 eye,Vector3 focus)
        {
            camera.transform.position=eye;camera.transform.LookAt(focus);
            camera.Render();RenderTexture.active=target;
            pixels.ReadPixels(new Rect(0,0,1400,960),0,0);pixels.Apply();
            string path=Evidence+$"{name}-{suffix}.png";
            File.WriteAllBytes(path,pixels.EncodeToPNG());return path;
        }
    }
}
