using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Freyja.WorldBuilder.Editor;

public static class EntrancesTripo023
{
    const string Evidence = "/Users/bruno.duartec/Documents/Projects/Freyja/Artifacts/Island023/Tripo/";
    const string Folder = "Assets/_Project/Art/Island023Tripo";
    public class Route
    {
        public string id;
        public Vector3 door,road;
        public Vector3[] points;
        public Route(string name, Vector3[] path) {id=name;points=path;road=path[0];door=path[path.Length-1];}
    }
    static Vector2 Flat(Vector3 v) => new Vector2(v.x,v.z);
    static float Ease(float a,float b,float t) => Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,t));
    static Vector3[] Resample(IList<Vector3> anchors)
    {
        var path=new List<Vector3>{anchors[0]};
        for(int i=1;i<anchors.Count;i++)
        {
            int count=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(Flat(anchors[i-1]),Flat(anchors[i]))/.55f));
            for(int k=1;k<=count;k++) path.Add(Vector3.Lerp(anchors[i-1],anchors[i],k/(float)count));
        }
        return path.ToArray();
    }
    static Route Access(Transform building, Vector3[] street)
    {
        var front=building.rotation*Vector3.back;
        var right=building.right;
        var col=building.GetComponent<BoxCollider>();
        var marker=building.Find("Door023");
        var target=marker?marker.position:building.GetComponentsInChildren<Renderer>().First(r=>r.name.StartsWith("Porta")).bounds.center+front*.78f;
        if (!marker) target.y=building.position.y;
        Vector3 Nearest(IEnumerable<Vector3> options,Vector3 to) => options.OrderBy(p=>(Flat(p)-Flat(to)).sqrMagnitude).First();
        var frontOptions=street.Where(p=>Vector2.Dot(Flat(p-target),Flat(front))>=-.20f).ToArray();
        var closest=Nearest(street,target);
        var direct=frontOptions.Length>0?Nearest(frontOptions,target):closest;
        var anchors=new List<Vector3>();
        if(frontOptions.Length>0 && Vector2.Distance(Flat(direct),Flat(target))<Vector2.Distance(Flat(closest),Flat(target))+6)
            anchors.Add(direct);
        else
        {
            float side=Mathf.Sign(Vector3.Dot(closest-building.position,right));
            float clearance=col.size.x/2+1.9f;
            var flank=building.position+right*side*clearance;
            var sideStart=flank+front*Vector3.Dot(closest-building.position,front);
            var corner=flank+front*(col.size.z/2+1.6f);
            sideStart.y=closest.y;
            corner.y=target.y;
            anchors.Add(closest);
            anchors.Add(sideStart);
            anchors.Add(corner);
        }
        anchors.Add(target);
        return new Route(building.name,Resample(anchors));
    }
    static void Grade(float[,] original,float[,] result, Terrain terrain, List<(Route path,float width,float feather)> stamps)
    {
        int n=original.GetLength(0);
        var size=terrain.terrainData.size;
        float spacing=size.x/(n-1), originY=terrain.transform.position.y;
        var strength=new float[n,n];
        var weightedHeight=new float[n,n];
        var weightTotal=new float[n,n];
        foreach(var stamp in stamps)
        {
            var route=stamp.path.points;
            float extent=stamp.width+stamp.feather;
            // Find the closest point per path so denser sampling does not change the height.
            var closest=new Dictionary<int,(float d,float y)>();
            for(int j=1;j<route.Length;j++)
            {
                var a=Flat(route[j-1]); var b=Flat(route[j]); var delta=b-a;
                int minX=Mathf.Max(0,Mathf.FloorToInt((Mathf.Min(a.x,b.x)+80-extent)/spacing));
                int maxX=Mathf.Min(n-1,Mathf.CeilToInt((Mathf.Max(a.x,b.x)+80+extent)/spacing));
                int minZ=Mathf.Max(0,Mathf.FloorToInt((Mathf.Min(a.y,b.y)+80-extent)/spacing));
                int maxZ=Mathf.Min(n-1,Mathf.CeilToInt((Mathf.Max(a.y,b.y)+80+extent)/spacing));
                for(int z=minZ;z<=maxZ;z++) for(int x=minX;x<=maxX;x++)
                {
                    var p=new Vector2(x*spacing-80,z*spacing-80);
                    float u=Mathf.Clamp01(Vector2.Dot(p-a,delta)/Mathf.Max(.0001f,delta.sqrMagnitude));
                    float d=Vector2.Distance(p,a+delta*u);
                    if(d>=extent) continue;
                    int key=z*n+x;
                    if(!closest.TryGetValue(key,out var last)||d<last.d) closest[key]=(d,Mathf.Lerp(route[j-1].y,route[j].y,u));
                }
            }
            foreach(var pair in closest)
            {
                int x=pair.Key%n,z=pair.Key/n;
                float factor=1-Ease(stamp.width,extent,pair.Value.d);
                float weight=factor*factor/(1+pair.Value.d*pair.Value.d);
                strength[z,x]=Mathf.Max(strength[z,x],factor);
                weightTotal[z,x]+=weight;
                weightedHeight[z,x]+=weight*(pair.Value.y-originY)/size.y;
            }
        }
        for(int z=0;z<n;z++) for(int x=0;x<n;x++)
            result[z,x]=weightTotal[z,x]>0?Mathf.Lerp(original[z,x],weightedHeight[z,x]/weightTotal[z,x],strength[z,x]):original[z,x];
    }
    static void Paint(Terrain terrain, Route[] roads, Route[] doors)
    {
        var td=terrain.terrainData;
        var layers=td.terrainLayers.Take(4).ToList();
        var plazaLayer=AssetDatabase.LoadAssetAtPath<TerrainLayer>(Folder+"/Praca.terrainlayer");
        if(!plazaLayer)
        {
            plazaLayer=new TerrainLayer{diffuseTexture=KitPS1.Material("Stone").mainTexture as Texture2D,tileSize=new Vector2(2.8f,2.8f)};
            AssetDatabase.CreateAsset(plazaLayer,Folder+"/Praca.terrainlayer");
        }
        layers.Add(plazaLayer);td.terrainLayers=layers.ToArray();
        int n=td.alphamapResolution;
        var a=new float[n,n,5];
        float step=160f/(n-1);
        for(int z=0;z<n;z++) for(int x=0;x<n;x++)
        {
            float wx=x*step-80,wz=z*step-80;
            float sand=(1-Ease(-40,-26,wz))*(1-Ease(29,47,Mathf.Abs(wx-5)));
            float plaza=1-Ease(.79f,1.02f,Mathf.Sqrt(wx*wx/225+(wz+9)*(wz+9)/81));
            a[z,x,0]=(1-plaza)*(1-sand);a[z,x,3]=(1-plaza)*sand;a[z,x,4]=plaza;
        }
        foreach(var path in roads.Concat(doors))
        {
            float width=roads.Contains(path)?1.85f:.84f;
            foreach(var p in path.points)
            {
                int cx=Mathf.RoundToInt((p.x+80)/step),cz=Mathf.RoundToInt((p.z+80)/step),r=Mathf.CeilToInt((width+.6f)/step);
                for(int z=Mathf.Max(0,cz-r);z<=Mathf.Min(n-1,cz+r);z++) for(int x=Mathf.Max(0,cx-r);x<=Mathf.Min(n-1,cx+r);x++)
                {
                    float wx=x*step-80,wz=z*step-80;
                    float d=Vector2.Distance(new Vector2(wx,wz),Flat(p));
                    float jitter=(Mathf.PerlinNoise(wx*1.4f,wz*1.4f)-.5f)*.55f;
                    float w=(1-Ease(width-.28f,width+.40f,d+jitter))*(1-a[z,x,4]);
                    float delta=Mathf.Max(0,w-a[z,x,2]);
                    float grassSand=a[z,x,0]+a[z,x,3];
                    if(grassSand<.0001f) continue;
                    a[z,x,0]-=delta*a[z,x,0]/grassSand;
                    a[z,x,3]-=delta*a[z,x,3]/grassSand;
                    a[z,x,2]+=delta;
                }
            }
        }
        td.SetAlphamaps(0,0,a);td.SetBaseMapDirty();
        foreach(var asset in td.alphamapTextures) EditorUtility.SetDirty(asset);
    }
    public static object Main()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Saia do Play Mode.");
        var island=GameObject.Find("Island023");
        var terrain=island.GetComponentInChildren<Terrain>();var td=terrain.terrainData;
        string baseline="Assets/_Project/WorldBuilder/Island023/Terrain.asset";
        string built=Folder+"/TerrenoComAcessos.asset";
        if(!AssetDatabase.LoadAssetAtPath<TerrainData>(built)) AssetDatabase.CopyAsset(baseline,built);
        var original=AssetDatabase.LoadAssetAtPath<TerrainData>(baseline);
        var updated=AssetDatabase.LoadAssetAtPath<TerrainData>(built);
        terrain.terrainData=updated;terrain.GetComponent<TerrainCollider>().terrainData=updated;
        td=updated;
        var designed=Island023.Routes();
        var east=designed.Single(r=>r.id=="Rua leste");
        // Turn around A4 on the inland side instead of clipping its foundation.
        var eastPoints=new[]{new Vector3(-16,4.5f,-19),new Vector3(8,3,-25),new Vector3(27,3.8f,-22),
            new Vector3(38,5,-12),new Vector3(39,6.4f,-1),new Vector3(41,7.5f,7),new Vector3(46,8.4f,13),
            new Vector3(54,9,17),new Vector3(47,12,25),new Vector3(29,13,31),new Vector3(13,14,30),new Vector3(-2,15,31)};
        int eastIndex=designed.IndexOf(east);
        designed[eastIndex]=new Island023.Route(east.id,east.width,eastPoints);
        var roads=designed.Select(r=>new Route(r.id,Resample(r.points))).ToArray();
        var buildings=island.transform.Find("Construções — 12 casas e 5 serviços");
        var street=roads.SelectMany(r=>r.points).ToArray();
        var doors=buildings.Cast<Transform>().Select(b=>Access(b,street)).ToArray();
        int n=td.heightmapResolution;
        var originalHeight=original.GetHeights(0,0,n,n);
        var baselineHeight=new float[n,n];
        var final=new float[n,n];
        Grade(originalHeight,baselineHeight,terrain,doors.Select(r=>(r,.67f,2.65f)).ToList());
        // Keep the center of each through road continuous where a driveway joins it.
        Grade(baselineHeight,final,terrain,roads.Select(r=>(r,1.1f,2.4f)).ToList());
        td.SetHeights(0,0,final);
        Paint(terrain,roads,doors);
        island.transform.Find("Ruas e caminhos").gameObject.SetActive(false);
        island.transform.Find("Acessos às 17 portas").gameObject.SetActive(false);
        foreach(Transform node in island.transform.Find("Pedras e árvores editáveis"))
        {
            var p=node.position;
            if(doors.Any(d=>d.points.Any(q=>Vector2.Distance(Flat(q),Flat(p))<1.7f))) node.gameObject.SetActive(false);
            p.y=terrain.SampleHeight(p)+terrain.transform.position.y-(node.name.StartsWith("Rock")?.5f:0);
            node.position=p;
        }
        Physics.SyncTransforms();
        var samples=roads.SelectMany(r=>r.points).Select(p=>new Vector3(p.x,terrain.SampleHeight(p)+terrain.transform.position.y+.75f,p.z)).ToArray();
        foreach(Transform node in island.transform.Find("Pedras e árvores editáveis"))
        {
            if(!node.gameObject.activeInHierarchy) continue;
            var obstacle=node.GetComponent<Collider>();
            if(obstacle && samples.Any(p=>(obstacle.ClosestPoint(p)-p).sqrMagnitude<.85f*.85f)) node.gameObject.SetActive(false);
        }
        var settings=new Newtonsoft.Json.JsonSerializerSettings{ReferenceLoopHandling=Newtonsoft.Json.ReferenceLoopHandling.Ignore};
        File.WriteAllText(Evidence+"doors-layout.json",Newtonsoft.Json.JsonConvert.SerializeObject(doors,settings));
        File.WriteAllText(Evidence+"roads-layout.json",Newtonsoft.Json.JsonConvert.SerializeObject(roads,settings));
        EditorUtility.SetDirty(td);
        AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(island.scene);
        return new{doors=doors.Length,roads=roads.Length};
    }
}
