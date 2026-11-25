using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Compact030
{
    const float Factor = .83f;
    const float PlazaZ = -8f;
    const string Project = "/Users/bruno.duartec/Documents/Projects/Freyja/";
    const string Evidence = Project + "Artifacts/IlhaCompacta030/";
    const string Original = "Assets/_Project/Art/Praca027/TerrenoDaPraca.asset";
    const string Compact = "Assets/_Project/Art/IlhaCompacta030/TerrenoCompacto.asset";
    const string LotsGroup = "Construções — 12 casas e 5 serviços";
    static readonly Vector3 ReviewEye = new(0, 98.139f, -170.526f);

    class Route
    {
        public string id;
        public Vector3[] points;
        public Vector3 door;
        public float width;
        public JObject Json() => new() { ["id"] = id, ["points"] = new JArray(points.Select(Point)),
            ["road"] = Point(points[0]), ["door"] = Point(door) };
    }

    sealed class Grid
    {
        public readonly Vector3 origin, size;
        public readonly int resolution;
        public Grid(Vector3 origin, Vector3 size, int resolution)
        { this.origin = origin; this.size = size; this.resolution = resolution; }
        float Step => size.x / (resolution - 1);
        public int Index(float coordinate, float start, bool roundUp) => Mathf.Clamp(roundUp
            ? Mathf.CeilToInt((coordinate - start) / Step) : Mathf.FloorToInt((coordinate - start) / Step), 0, resolution - 1);
        public Vector2 At(int x, int z) => new(origin.x + x*Step, origin.z + z*Step);
        public void Within(Vector2 a, Vector2 b, float padding, Action<int, int, Vector2> visit)
        {
            int x0 = Index(Mathf.Min(a.x,b.x)-padding,origin.x,false), x1 = Index(Mathf.Max(a.x,b.x)+padding,origin.x,true);
            int z0 = Index(Mathf.Min(a.y,b.y)-padding,origin.z,false), z1 = Index(Mathf.Max(a.y,b.y)+padding,origin.z,true);
            for(int z=z0; z<=z1; z++) for(int x=x0; x<=x1; x++) visit(x,z,At(x,z));
        }
    }

    static GameObject Island()
    {
        var scene = SceneManager.GetActiveScene();
        var island = scene.GetRootGameObjects().Single(g => g.name == "Island023");
        if (scene.name != "Ilha_Tripo_Caminhada_011" || EditorApplication.isPlaying || !island.activeInHierarchy)
            throw new InvalidOperationException("Abra a ilha na cena principal fora de Play Mode.");
        return island;
    }
    static Vector3 Map(Vector3 p) => new(p.x*Factor,p.y,PlazaZ+(p.z-PlazaZ)*Factor);
    static Vector3 Read(JToken p) => new((float)p["x"],(float)p["y"],(float)p["z"]);
    static JObject Point(Vector3 p) => new() { ["x"]=p.x,["y"]=p.y,["z"]=p.z };
    static Vector2 Flat(Vector3 p) => new(p.x,p.z);
    static float Smooth(float a,float b,float x) => Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,x));
    static JArray ReadArray(string file) => JArray.Parse(File.ReadAllText(Project+"Artifacts/"+file));
    static float Sample(TerrainData d,Vector3 o,Vector3 p) => o.y + d.GetInterpolatedHeight((p.x-o.x)/d.size.x,(p.z-o.z)/d.size.z);
    static float Length(IEnumerable<Vector3> list) => list.Zip(list.Skip(1),(a,b)=>Vector2.Distance(Flat(a),Flat(b))).Sum();

    public static object Before()
    {
        var island=Island();
        var t=island.GetComponentInChildren<Terrain>();
        if(AssetDatabase.GetAssetPath(t.terrainData)!=Original) throw new InvalidOperationException("Terreno de origem inesperado.");
        Directory.CreateDirectory(Evidence);
        var backup=Evidence+"Cena-antes-compactacao.unity";
        if(!File.Exists(backup)) File.Copy(island.scene.path,backup);
        return Capture("antes",ReviewEye,34);
    }

    public static object Apply()
    {
        var island=Island();
        var terrain=island.GetComponentInChildren<Terrain>();
        if(AssetDatabase.GetAssetPath(terrain.terrainData)!=Original) throw new InvalidOperationException("A compactação já foi aplicada ou o terreno foi trocado.");
        var old=terrain.terrainData;
        var origin=terrain.transform.position;
        var plots=island.transform.Find(LotsGroup).Cast<Transform>().ToArray();
        var props=island.transform.Find("Pedras e árvores editáveis").Cast<Transform>().ToArray();
        var dock=island.transform.Find("Cais e píer");
        var spawn=island.transform.Find("Spawn023");
        var dockAnchor=dock.Find("Cais de pedra").position;
        var dockDelta=Map(dockAnchor)-dockAnchor;
        var originalRoads=ReadArray("Island023/Tripo/roads-layout.json");
        var originalDoors=ReadArray("Island023/Tripo/doors-layout.json").ToDictionary(p=>(string)p["id"]);
        foreach(var file in new[]{"Island023/Tripo/remaining-layout.json","Praca027/praca-layout.json"})
            foreach(var route in ReadArray(file)) if(originalDoors.ContainsKey((string)route["id"])) originalDoors[(string)route["id"]]=route;
        var roads=originalRoads.Select(r=>Road(r,old,origin)).ToArray();
        var landBefore=Measure(old,origin);
        var lotBefore=plots.ToDictionary(p=>p.name,p=>p.position);
        var natureLift=props.ToDictionary(p=>p,p=>p.position.y-Sample(old,origin,p.position));

        var next=AssetDatabase.LoadAssetAtPath<TerrainData>(Compact);
        if(!next)
        {
            if(!AssetDatabase.CopyAsset(Original,Compact))throw new IOException("Não foi possível copiar o TerrainData.");
            next=AssetDatabase.LoadAssetAtPath<TerrainData>(Compact);
        }
        var size=old.size;
        next.size=new Vector3(size.x*Factor,size.y,size.z*Factor);
        var newOrigin=Map(origin);
        var heights=old.GetHeights(0,0,old.heightmapResolution,old.heightmapResolution);
        var grid=new Grid(newOrigin,next.size,next.heightmapResolution);
        foreach(var lot in plots) SeatPlot(lot,Map(lot.position),heights,grid);
        WidenRoads(roads,heights,grid);
        SmoothPlazaApproach(heights,grid);
        next.SetHeights(0,0,heights);
        PaintRoads(next,old,newOrigin,roads);
        terrain.transform.position=newOrigin;
        terrain.terrainData=next;
        terrain.GetComponent<TerrainCollider>().terrainData=next;
        foreach(var plot in plots) plot.position=Map(plot.position);
        foreach(var prop in props)
        {
            var moved=Map(prop.position);
            moved.y=Sample(next,newOrigin,moved)+natureLift[prop];
            prop.position=moved;
        }
        dock.position+=dockDelta;
        spawn.position+=dockDelta;
        var player=GameObject.Find("PlayerPrototype");
        var body=player.GetComponent<CharacterController>();
        bool enabled=body.enabled;
        body.enabled=false;player.transform.position=spawn.position;body.enabled=enabled;
        GameObject.Find("Cais Gameplay Camera").transform.position+=dockDelta;
        terrain.Flush();Physics.SyncTransforms();

        var accesses=plots.Select(p=>Access(p,lotBefore[p.name],originalDoors[p.name],next,newOrigin)).ToArray();
        var dockRoute=DockWalk(dockDelta,next,newOrigin);
        var main=new Route {id="Píer à igreja",points=Connect(dockRoute,roads,plots.Single(p=>p.name=="Igreja")).ToArray()};
        main.door=main.points[main.points.Length-1];
        var validation=accesses.Concat(new[]{main}).ToArray();
        var offsets = KeepCorridorsClear(props,validation);
        var report=new {
            factor=Factor, landBefore, landAfter=Measure(next,newOrigin), dockDelta=Point(dockDelta),
            routes=roads.Select((r,i)=>new {r.id, oldMeters=Length(originalRoads[i]["points"].Select(Read)),newMeters=Length(r.points),width=r.width}).ToArray(),
            plots=plots.Select(p=> new{lot=p.name,before=Point(lotBefore[p.name]),after=Point(p.position),scale=p.lossyScale.ToString("F3")}).ToArray(),
            obstaclesAdjusted=offsets, terrain=Compact
        };
        Save(island,next);
        File.WriteAllText(Evidence+"compacta-layout.json",new JArray(validation.Select(v=>v.Json())).ToString());
        File.WriteAllText(Evidence+"implantacao.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));
        Capture("depois",ReviewEye,34);
        Capture("depois-enquadrado",ReviewEye,28.8f);
        Capture("miolo",new Vector3(-22,36,-51),43,new Vector3(0,8,3));
        return new {report.landBefore,report.landAfter,moved=plots.Length,report.obstaclesAdjusted,sceneSaved=!island.scene.isDirty};
    }

    public static object PolishPlazaApproach()
    {
        var island = Island();
        var terrain = island.GetComponentInChildren<Terrain>();
        var data = terrain.terrainData;
        if (AssetDatabase.GetAssetPath(data) != Compact) throw new InvalidOperationException("A ilha compacta não está ativa.");
        int n = data.heightmapResolution;
        var heights = data.GetHeights(0,0,n,n);
        var grid = new Grid(terrain.transform.position,data.size,n);
        SmoothPlazaApproach(heights,grid);
        data.SetHeights(0,0,heights);
        terrain.Flush();
        Save(island,data);
        Capture("depois",ReviewEye,34);
        Capture("depois-enquadrado",ReviewEye,28.8f);
        return Capture("miolo",new Vector3(-22,36,-51),43,new Vector3(0,8,3));
    }

    static void SmoothPlazaApproach(float[,] heights, Grid grid)
    {
        // The east street runs just below this entrance. Soften the connection within its small envelope.
        var center = Flat(Map(new Vector3(-8,0,-17)));
        for (int iteration=0; iteration<4; iteration++)
        {
            var before = (float[,])heights.Clone();
            grid.Within(center,center,9,(x,z,p)=>{
                var delta = p-center;
                float radius = new Vector2(delta.x/8.7f,delta.y/4.7f).magnitude;
                float weight = .94f*(1-Smooth(.52f,1,radius));
                if (weight <= 0) return;
                const int sample = 4;
                float mean = (4*before[z,x]+before[z-sample,x]+before[z+sample,x]+before[z,x-sample]+before[z,x+sample])/8;
                heights[z,x] = Mathf.Lerp(before[z,x],mean,weight);
            });
        }
    }

    static Route Road(JToken r,TerrainData baseline,Vector3 origin)
    {
        string id=(string)r["id"];
        var points=r["points"].Select(Read).Select(p=>Map(new Vector3(p.x,Sample(baseline,origin,p),p.z))).ToArray();
        return new Route{id=id,points=points,door=points[points.Length-1],width=id switch {"Casa isolada"=>3,"Rua leste"=>3.8f,"Caminho da colina"=>3.5f,_=>4}};
    }

    static void SeatPlot(Transform plot,Vector3 center,float[,] data,Grid grid)
    {
        var c=plot.GetComponent<BoxCollider>();
        var half=new Vector2(c.size.x,c.size.z)*.5f;
        var colCenter=Flat(center+plot.rotation*new Vector3(c.center.x,0,c.center.z));
        var inverse=Quaternion.Inverse(plot.rotation);
        grid.Within(colCenter,colCenter,half.magnitude+1.25f,(x,z,p)=>{
            var q=inverse*new Vector3(p.x-colCenter.x,0,p.y-colCenter.y);
            float d=Mathf.Max(Mathf.Abs(q.x)-half.x,Mathf.Abs(q.z)-half.y);
            float weight=1-Smooth(.015f,1.15f,d);
            data[z,x]=Mathf.Lerp(data[z,x],(center.y-grid.origin.y)/grid.size.y,weight);
        });
    }

    static (float distance,float ratio) Nearest(Vector2 p,Vector2 a,Vector2 b)
    {
        var ab=b-a;
        float t=Mathf.Clamp01(Vector2.Dot(p-a,ab)/Mathf.Max(.001f,ab.sqrMagnitude));
        return (Vector2.Distance(a+t*ab,p),t);
    }

    static void WidenRoads(Route[] roads,float[,] heights,Grid grid)
    {
        // Use the nearest road sample rather than adding stamps of different elevations.
        var touched=new Dictionary<int,(float distance,float height,float width)>();
        foreach(var r in roads)
        for(int i=1;i<r.points.Length;i++)
        {
            var a=r.points[i-1];var b=r.points[i];
            grid.Within(Flat(a),Flat(b),r.width/2+1.4f,(x,z,p)=>{
                var q=Nearest(p,Flat(a),Flat(b));
                if(q.distance>r.width/2+1.4f)return;
                int key=z*grid.resolution+x;
                if(!touched.TryGetValue(key,out var existing)||q.distance<existing.distance)
                    touched[key]=(q.distance,Mathf.Lerp(a.y,b.y,q.ratio),r.width);
            });
        }
        foreach(var item in touched)
        {
            int x=item.Key%grid.resolution,z=item.Key/grid.resolution;
            var r=item.Value;
            float weight=1-Smooth(r.width*.31f,r.width*.5f+1.45f,r.distance);
            heights[z,x]=Mathf.Lerp(heights[z,x],(r.height-grid.origin.y)/grid.size.y,weight);
        }
    }

    static void PaintRoads(TerrainData target,TerrainData source,Vector3 origin,Route[] roads)
    {
        var layers=source.terrainLayers;
        int sand=Array.FindIndex(layers,l=>l.name=="PathSand"),stone=Array.FindIndex(layers,l=>l.diffuseTexture&&l.diffuseTexture.name=="PedraDoLargo");
        if(sand<0||stone<0)throw new InvalidOperationException("Camadas do largo e caminho são necessárias.");
        int n=source.alphamapResolution;
        var weights=source.GetAlphamaps(0,0,n,n);
        var grid=new Grid(origin,target.size,n);
        foreach(var r in roads)for(int i=1;i<r.points.Length;i++)
        {
            var a=Flat(r.points[i-1]);var b=Flat(r.points[i]);
            grid.Within(a,b,r.width/2+.52f,(x,z,p)=>{
                float d=Nearest(p,a,b).distance+(Mathf.PerlinNoise(p.x*1.2f,p.y*1.2f)-.5f)*.24f;
                float coverage=(1-Smooth(r.width/2-.25f,r.width/2+.38f,d))*(1-weights[z,x,stone]);
                float xfer=Mathf.Min(weights[z,x,0],Mathf.Max(0,coverage-weights[z,x,sand]));
                weights[z,x,0]-=xfer;weights[z,x,sand]+=xfer;
            });
        }
        target.SetAlphamaps(0,0,weights);target.SetBaseMapDirty();
        foreach(var map in target.alphamapTextures)EditorUtility.SetDirty(map);
    }

    static Route Access(Transform plot,Vector3 oldLot,JToken source,TerrainData terrain,Vector3 origin)
    {
        var oldPoints=source["points"].Select(Read).ToArray();
        var door=plot.Find("Door023").position;
        var mapped=oldPoints.Select((p,i)=>{
            float close=1-Smooth(4.1f,10f,Vector2.Distance(Flat(p),Flat(oldLot)));
            if(i==0)close=0;
            var moved=Vector3.Lerp(Map(p),p+plot.position-oldLot,close);
            moved.y=Sample(terrain,origin,moved);return moved;
        }).ToArray();
        mapped[mapped.Length-1]=door;
        return new Route{id=plot.name,points=SamplePath(mapped,.53f),door=door,width=1.7f};
    }

    static Vector3[] DockWalk(Vector3 delta,TerrainData terrain,Vector3 origin)
    {
        var old=ReadArray("Praca027/cais-layout.json")[0]["points"].Select(Read);
        return old.Select(p=>{
            float blend=Smooth(-51f,-45.4f,p.z);
            var v=Vector3.Lerp(p+delta,Map(p),blend);
            if(blend>.32f)v.y=Sample(terrain,origin,v);
            return v;
        }).ToArray();
    }
    static IEnumerable<Vector3> Connect(Vector3[] start,Route[] roads,Transform church)
    {
        var path=new List<Vector3>(start);
        var south=roads.Single(r=>r.id=="Cais — praça").points;
        var north=roads.Single(r=>r.id=="Praça — igreja").points;
        int i=Enumerable.Range(0,south.Length).OrderBy(k=>Vector2.Distance(Flat(start.Last()),Flat(south[k]))).First();
        path.AddRange(south.Skip(i+1));path.AddRange(north.Skip(1));path.Add(church.Find("Door023").position);
        return SamplePath(path,.56f);
    }
    static Vector3[] SamplePath(IList<Vector3> points,float step)
    {
        var path=new List<Vector3>{points[0]};
        for(int i=1;i<points.Count;i++)
        {
            int n=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(Flat(points[i-1]),Flat(points[i]))/step));
            for(int k=1;k<=n;k++)path.Add(Vector3.Lerp(points[i-1],points[i],k/(float)n));
        }
        return path.ToArray();
    }
    static string[] KeepCorridorsClear(IEnumerable<Transform> props,Route[] paths)
    {
        var moved=new List<string>();
        foreach(var p in props)
        {
            if(!p.gameObject.activeInHierarchy) continue;
            var c=p.GetComponent<Collider>();
            if(!c)continue;
            var samples=paths.SelectMany(r=>r.points).Where(v=>c.bounds.SqrDistance(v+Vector3.up*.75f)<.42f*.42f);
            if(samples.Any(v=>(c.ClosestPoint(v+Vector3.up*.75f)-(v+Vector3.up*.75f)).sqrMagnitude<.42f*.42f))
            {p.gameObject.SetActive(false);moved.Add(p.name);}
        }
        return moved.ToArray();
    }
    static float[] Measure(TerrainData t,Vector3 origin)
    {
        float loX=float.MaxValue,loZ=float.MaxValue,hiX=float.MinValue,hiZ=float.MinValue;
        for(float z=origin.z;z<origin.z+t.size.z;z+=.5f)for(float x=origin.x;x<origin.x+t.size.x;x+=.5f)
        {
            if(Sample(t,origin,new Vector3(x,0,z))<=.08f)continue;
            loX=Mathf.Min(loX,x);loZ=Mathf.Min(loZ,z);hiX=Mathf.Max(hiX,x);hiZ=Mathf.Max(hiZ,z);
        }
        return new[]{hiX-loX,hiZ-loZ};
    }
    static void Save(GameObject island,TerrainData terrain)
    {
        EditorUtility.SetDirty(terrain);AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(island.scene);EditorSceneManager.SaveScene(island.scene);
    }
    static string Capture(string name,Vector3 eye,float fov,Vector3? focus=null)
    {
        var go=new GameObject("Captura da ilha compacta"){hideFlags=HideFlags.HideAndDontSave};
        var camera=go.AddComponent<Camera>();var target=new RenderTexture(1536,1024,24);
        var image=new Texture2D(1536,1024,TextureFormat.RGB24,false);var previous=RenderTexture.active;
        try
        {
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.13f,.46f,.53f);
            camera.farClipPlane=800;camera.allowHDR=false;camera.allowMSAA=false;camera.fieldOfView=fov;
            camera.transform.position=eye;camera.transform.LookAt(focus??new Vector3(0,8,-1));
            camera.targetTexture=target;camera.Render();RenderTexture.active=target;
            image.ReadPixels(new Rect(0,0,1536,1024),0,0);image.Apply();
            var path=Evidence+"ilha-"+name+".png";File.WriteAllBytes(path,image.EncodeToPNG());return path;
        }
        finally
        {
            RenderTexture.active=previous;camera.targetTexture=null;
            UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
