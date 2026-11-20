using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace Freyja.WorldBuilder.Editor
{
    public static class WorldGenerator
    {
        public const string Folder="Assets/_Project/WorldBuilder/Demo";
        public static GameObject FindRoot(WorldDefinition w)=>SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(g=>g.TryGetComponent<WorldInstanceId>(out var id)&&id.worldId==w.worldId&&id.kind=="world");
        static GameObject Managed(Transform parent,WorldDefinition w,string id,string kind)
        {
            var found=parent.GetComponentsInChildren<WorldInstanceId>(true).FirstOrDefault(m=>m.managed&&m.worldId==w.worldId&&m.recordId==id&&m.kind==kind);
            if(found)return found.gameObject;
            var go=new GameObject(kind+" "+id);
            Undo.RegisterCreatedObjectUndo(go,"Criar elemento");
            go.transform.SetParent(parent,false);
            var tag=Undo.AddComponent<WorldInstanceId>(go);
            tag.worldId=w.worldId;
            tag.recordId=id;
            tag.kind=kind;
            return go;
        }
        public static GameObject Apply(WorldDefinition w)
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Saia de Play Mode.");
            var errors=WorldValidation.Issues(w);
            if(errors.Count>0)throw new InvalidOperationException(string.Join("\n",errors));
            Undo.IncrementCurrentGroup();
            int group=Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Aplicar World Builder");
            var root=FindRoot(w);
            if(!root)
            {
                root=new GameObject("World Builder — "+w.name);
                Undo.RegisterCreatedObjectUndo(root,"Criar mundo");
                var tag=Undo.AddComponent<WorldInstanceId>(root);
                tag.worldId=w.worldId;
                tag.kind="world";
            }
            if(root.transform.position!=Vector3.zero||root.transform.rotation!=Quaternion.identity||root.transform.localScale!=Vector3.one)throw new InvalidOperationException("Raiz do mundo deve permanecer na origem, sem rotação ou escala.");
            Directory.CreateDirectory(Folder+"/Generated");
            AssetDatabase.Refresh();
            string path=Folder+"/Generated/Terrain_"+w.worldId+".asset";
            var td=AssetDatabase.LoadAssetAtPath<TerrainData>(path);
            bool creating=!td;
            if(td && (td.heightmapResolution!=w.resolution || td.size!=new Vector3(w.size.x,w.terrainHeight,w.size.y)))throw new InvalidOperationException("Dimensões do terreno existente não podem mudar durante rebuild. Crie outro mundo.");
            if(td==w.baseTerrain && td)throw new InvalidOperationException("A base deve ser separada do terreno de saída.");
            if(!td)
            {
                td=new TerrainData();
                AssetDatabase.CreateAsset(td,path);
            }
            Undo.RegisterCompleteObjectUndo(td,"Relevo");
            if(creating)td.heightmapResolution=w.resolution;
            td.size=new Vector3(w.size.x,w.terrainHeight,w.size.y);
            td.SetHeightsDelayLOD(0,0,TerrainMath.Generate(w));
            td.SyncHeightmap();
            if(creating)td.terrainLayers=w.terrainLayers!=null&&w.terrainLayers.Length>0?w.terrainLayers:(w.grass?new[]{w.grass}:new TerrainLayer[0]);
            EditorUtility.SetDirty(td);
            var terrainGO=Managed(root.transform,w,"terrain","terrain");
            var terrain=terrainGO.GetComponent<Terrain>();
            if(!terrain)terrain=Undo.AddComponent<Terrain>(terrainGO);
            var collider=terrainGO.GetComponent<TerrainCollider>();
            if(!collider)collider=Undo.AddComponent<TerrainCollider>(terrainGO);
            terrain.terrainData=td;
            collider.terrainData=td;
            terrainGO.layer=8;
            terrainGO.transform.position=new Vector3(-w.size.x/2,w.originY,-w.size.y/2);
            terrain.drawInstanced=true;
            terrain.heightmapPixelError=2;
            var terrainMaterial=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/TerrainMaterial.mat");
            if(!terrainMaterial)
            {
                terrainMaterial=new Material(Shader.Find("Nature/Terrain/Standard"));
                AssetDatabase.CreateAsset(terrainMaterial,Folder+"/TerrainMaterial.mat");
            }
            terrain.materialTemplate=terrainMaterial;
            var expected=new HashSet<string>();
            foreach(var l in w.lots)
            {
                expected.Add(l.id);
                var lot=Managed(root.transform,w,l.id,"lot");
                Undo.RecordObject(lot.transform,"Posicionar lote");
                lot.transform.SetPositionAndRotation(new Vector3(l.center.x,TerrainMath.Foundation(w,l),l.center.y),Quaternion.Euler(0,l.yaw,0));
                lot.transform.localScale=Vector3.one;
                var visual=lot.transform.GetComponentsInChildren<WorldInstanceId>(true).FirstOrDefault(t=>t.kind=="house"&&t.worldId==w.worldId);
                if(visual&&PrefabUtility.GetCorrespondingObjectFromSource(visual.gameObject)!=l.prefab)
                {
                    PreserveManualChildren(visual.transform,lot.transform,w);
                    Undo.DestroyObjectImmediate(visual.gameObject);
                    visual=null;
                }
                if(!visual)
                {
                    var house=(GameObject)PrefabUtility.InstantiatePrefab(l.prefab,lot.transform);
                    Undo.RegisterCreatedObjectUndo(house,"Instanciar casa");
                    visual=Undo.AddComponent<WorldInstanceId>(house);
                    visual.worldId=w.worldId;
                    visual.recordId=l.id;
                    visual.kind="house";
                }
                visual.transform.localPosition=Vector3.zero;
                visual.transform.localRotation=Quaternion.identity;
                visual.transform.localScale=Vector3.one;
                var foundation=Managed(lot.transform,w,l.id,"foundation");
                var verts=new List<Vector3>();
                var tris=new List<int>();
                var corners=new OrientedRectXZ(Vector2.zero,l.footprint+Vector2.one*.15f,0).Corners();
                for(int i=0;i<4;i++)
                {
                    var a=corners[i];
                    var b=corners[(i+1)%4];
                    var wa=l.Rect.LocalToWorld(a);
                    var wb=l.Rect.LocalToWorld(b);
                    float ya=Mathf.Min(-.15f,TerrainMath.BaseHeight(w,wa)-lot.transform.position.y-.2f),yb=Mathf.Min(-.15f,TerrainMath.BaseHeight(w,wb)-lot.transform.position.y-.2f);
                    int k=verts.Count;
                    verts.AddRange(new[]
                    {
                        new Vector3(a.x,0,a.y),new Vector3(b.x,0,b.y),new Vector3(b.x,yb,b.y),new Vector3(a.x,ya,a.y)
                    }
                    );
                    tris.AddRange(new[]
                    {
                        k,k+1,k+2,k,k+2,k+3
                    }
                    );
                }
                SetMesh(foundation,w,l.id+"_foundation",verts.ToArray(),tris.ToArray(),w.foundationMaterial);
            }
            foreach(var old in root.GetComponentsInChildren<WorldInstanceId>(true).Where(t=>t.kind=="lot"&&t.worldId==w.worldId&&!expected.Contains(t.recordId)).ToArray())
            {
                foreach(var t in old.GetComponentsInChildren<Transform>(true).OrderByDescending(t=>Depth(t)))if(t!=old.transform&&!t.GetComponent<WorldInstanceId>()&&!PrefabUtility.IsPartOfPrefabInstance(t.gameObject))Undo.SetTransformParent(t,root.transform,"Preservar manual");
                Undo.DestroyObjectImmediate(old.gameObject);
            }
            expected.Clear();
            foreach(var r in w.roads)
            {
                expected.Add(r.id);
                var road=Managed(root.transform,w,r.id,"road");
                var c=r.Rect.Corners();
                var v=c.Select(q=>new Vector3(q.x,r.Height(q)+.035f,q.y)).ToArray();
                SetMesh(road,w,r.id+"_road",v,new[]
                {
                    0,2,1,0,3,2
                }
                ,w.roadMaterial);
            }
            foreach(var old in root.GetComponentsInChildren<WorldInstanceId>(true).Where(t=>t.kind=="road"&&t.worldId==w.worldId&&!expected.Contains(t.recordId)).ToArray())
            {
                PreserveManualChildren(old.transform,root.transform,w);
                Undo.DestroyObjectImmediate(old.gameObject);
            }
            CreateAccessPaths(w,root,td);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            Undo.CollapseUndoOperations(group);
            Physics.SyncTransforms();
            return root;
        }
        static void CreateAccessPaths(WorldDefinition w,GameObject root,TerrainData terrain)
        {
            foreach(var l in w.lots)
            {
                if(w.roads.Count==0)continue;
                var lot=root.GetComponentsInChildren<WorldInstanceId>().First(t=>t.kind=="lot"&&t.recordId==l.id);
                var start=l.Rect.LocalToWorld(new Vector2(0,ExternalModelNormalizer.BoundsOf(l.prefab).max.z+.06f));
                var road=w.roads.OrderBy(r=>r.Rect.Distance(start)).First();
                var axis=(road.end-road.start).normalized;
                var center=road.start+axis*Mathf.Clamp(Vector2.Dot(start-road.start,axis),0,Vector2.Distance(road.start,road.end));
                var end=center+(start-center).normalized*(road.width/2+.05f);
                var direction=(end-start).normalized;
                var normal=new Vector2(-direction.y,direction.x)*.9f;
                var vertices=new Vector3[105];
                var triangles=new int[480];
                for(int i=0;i<=20;i++)for(int side=0;side<5;side++)
                {
                    var q=Vector2.Lerp(start,end,i/20f)+normal*(side/2f-1);
                    var uv=TerrainSpace.UV(w,q);
                    vertices[i*5+side]=lot.transform.InverseTransformPoint(new Vector3(q.x,w.originY+terrain.GetInterpolatedHeight(uv.x,uv.y)+.07f,q.y));
                }
                for(int i=0;i<20;i++)for(int side=0;side<4;side++)
                {
                    int k=i*5+side,t=(i*4+side)*6;
                    triangles[t]=k;
                    triangles[t+1]=k+1;
                    triangles[t+2]=k+5;
                    triangles[t+3]=k+1;
                    triangles[t+4]=k+6;
                    triangles[t+5]=k+5;
                }
                SetMesh(Managed(lot.transform,w,l.id,"access"),w,l.id+"_access",vertices,triangles,w.roadMaterial);
            }
        }
        static int Depth(Transform t)
        {
            int d=0;
            while(t.parent)
            {
                d++;
                t=t.parent;
            }
            return d;
        }
        static void PreserveManualChildren(Transform from,Transform to,WorldDefinition w)
        {
            foreach(var t in from.GetComponentsInChildren<Transform>(true).Where(t=>t!=from).ToArray())
            {
                if(!t)continue;
                if(!t.GetComponent<WorldInstanceId>()&&(!PrefabUtility.IsPartOfPrefabInstance(t.gameObject)||PrefabUtility.IsAddedGameObjectOverride(t.gameObject)))Undo.SetTransformParent(t,to,"Preservar manual");
            }
        }
        static void SetMesh(GameObject go,WorldDefinition w,string id,Vector3[] v,int[] triangles,Material material)
        {
            string path=Folder+"/Generated/"+w.worldId+"_"+id+".asset";
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(!mesh)
            {
                mesh=new Mesh();
                AssetDatabase.CreateAsset(mesh,path);
            }
            Undo.RegisterCompleteObjectUndo(mesh,"Atualizar malha");
            mesh.Clear();
            mesh.vertices=v;
            mesh.triangles=triangles;
            mesh.uv=v.Select(p=>new Vector2(p.x,p.z)*.25f).ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
            var mf=go.GetComponent<MeshFilter>();
            if(!mf)mf=Undo.AddComponent<MeshFilter>(go);
            var mr=go.GetComponent<MeshRenderer>();
            if(!mr)mr=Undo.AddComponent<MeshRenderer>(go);
            var mc=go.GetComponent<MeshCollider>();
            if(!mc)mc=Undo.AddComponent<MeshCollider>(go);
            mf.sharedMesh=mesh;
            mr.sharedMaterial=material;
            if(w.worldId=="freyja-ps1-022" && (id.EndsWith("_road")||id.EndsWith("_access")))
            {
                var bounds=mesh.bounds;
                mesh.uv2=v.Select(q=>new Vector2(q.x-bounds.min.x,q.z-bounds.min.z)).ToArray();
                string edgePath=Folder+"/Generated/"+w.worldId+"_"+id+"_edge.mat";
                var edge=AssetDatabase.LoadAssetAtPath<Material>(edgePath);
                if(!edge){edge=new Material(Shader.Find("Freyja/PS1 Path Edge"));AssetDatabase.CreateAsset(edge,edgePath);}
                edge.mainTexture=material.mainTexture;edge.color=material.color;
                edge.SetVector("_Extent",new Vector4(bounds.size.x,bounds.size.z,0,0));
                edge.SetFloat("_EdgeWidth",.6f);
                mr.sharedMaterial=edge;EditorUtility.SetDirty(edge);EditorUtility.SetDirty(mesh);
            }
            mc.sharedMesh=null;
            mc.sharedMesh=mesh;
            go.layer=8;
        }
    }
}
