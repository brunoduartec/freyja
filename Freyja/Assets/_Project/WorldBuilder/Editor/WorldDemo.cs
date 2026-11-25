using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
namespace Freyja.WorldBuilder.Editor
{
    public static class WorldDemo
    {
        public const string Data=WorldGenerator.Folder+"/DemoWorld.asset";
        const string StatePath=WorldGenerator.Folder+"/PreviousScene.json";
        [Serializable] public class SceneState
        {
            public string scenePath,spawnId,title;
            public string[] roots;
            public bool[] active;
            public Vector3 playerPosition,cameraPosition;
            public Quaternion playerRotation,cameraRotation;
            public Bounds bounds;
            public float fallHeight;
        }
        static Component Session()=>UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(c=>c.GetType().Name=="CaisSessionController");
        static void Check()
        {
            if(EditorApplication.isPlaying||SceneManager.GetActiveScene().name!="Ilha_Tripo_Caminhada_011")throw new InvalidOperationException("A demonstração requer a cena histórica Ilha_Tripo_Caminhada_011 aberta em Edit Mode.");
        }
        static Material Mat(string name,Color color)
        {
            string path=WorldGenerator.Folder+"/"+name+".mat";
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m)
            {
                m=new Material(Shader.Find("Standard"));
                m.color=color;
                m.SetFloat("_Glossiness",0);
                AssetDatabase.CreateAsset(m,path);
            }
            return m;
        }
        static GameObject Box(Transform parent,string name,Vector3 position,Vector3 size,Material mat)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name=name;
            go.transform.SetParent(parent,false);
            go.transform.localPosition=position;
            go.transform.localScale=size;
            go.GetComponent<Renderer>().sharedMaterial=mat;
            go.layer=8;
            return go;
        }
        public static WorldDefinition Create()
        {
            Check();
            Directory.CreateDirectory(WorldGenerator.Folder);
            AssetDatabase.Refresh();
            var w=AssetDatabase.LoadAssetAtPath<WorldDefinition>(Data);
            if(w)return w;
            w=ScriptableObject.CreateInstance<WorldDefinition>();
            w.name="Primeira rua";
            w.worldId="freyja-demo-021";
            w.roadMaterial=Mat("Rua",new Color(.56f,.5f,.39f));
            w.foundationMaterial=Mat("Fundacao",new Color(.49f,.46f,.39f));
            var grass=new TerrainLayer
            {
                tileSize=new Vector2(4,4)
            };
            var texture=new Texture2D(64,64,TextureFormat.RGB24,false);
            texture.filterMode=FilterMode.Point;
            var random=new System.Random(1138);
            for(int y=0;y<64;y++)for(int x=0;x<64;x++)
            {
                float k=(float)random.NextDouble()*.10f;
                texture.SetPixel(x,y,new Color(.31f+k,.43f+k,.20f+k));
            }
            texture.Apply();
            File.WriteAllBytes(WorldGenerator.Folder+"/Grass.png",texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(WorldGenerator.Folder+"/Grass.png");
            var importer=(TextureImporter)AssetImporter.GetAtPath(WorldGenerator.Folder+"/Grass.png");
            importer.filterMode=FilterMode.Point;
            importer.wrapMode=TextureWrapMode.Repeat;
            importer.SaveAndReimport();
            grass.diffuseTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(WorldGenerator.Folder+"/Grass.png");
            AssetDatabase.CreateAsset(grass,WorldGenerator.Folder+"/Grass.terrainlayer");
            w.grass=grass;
            var roof=Mat("Telha",new Color(.52f,.21f,.13f));
            var dark=Mat("Porta",new Color(.17f,.24f,.24f));
            var window=Mat("Janela",new Color(.37f,.58f,.61f));
            var trim=Mat("Moldura",new Color(.87f,.83f,.69f));
            var colors=new[]
            {
                new Color(.79f,.58f,.33f),new Color(.72f,.36f,.27f),new Color(.67f,.74f,.64f)
            };
            var centers=new[]
            {
                new Vector2(-16,-15),new Vector2(-16,12),new Vector2(16,12)
            };
            var road=new RoadDefinition
            {
                id="rua-principal"
            };
            w.roads.Add(road);
            for(int i=0;i<3;i++)
            {
                var go=new GameObject("Casa "+(i+1));
                try
                {
                    float height=i==1?6:4.2f;
                    Box(go.transform,"Paredes",new Vector3(0,height/2,0),new Vector3(7,height,8),Mat("Reboco"+i,colors[i]));
                    for(int side=-1;side<=1;side+=2)
                    {
                        var slope=Box(go.transform,"Telhado",new Vector3(side*2,height+.9f,0),new Vector3(4.55f,.22f,9.2f),roof);
                        slope.transform.localRotation=Quaternion.Euler(0,0,-side*23);
                    }
                    Box(go.transform,"Porta",new Vector3(0,1.05f,4.04f),new Vector3(1.2f,2.1f,.12f),dark);
                    for(int side=-1;side<=1;side+=2)
                    {
                        Box(go.transform,"Moldura",new Vector3(side*2.3f,2.4f,4.04f),new Vector3(1.3f,1.5f,.15f),trim);
                        Box(go.transform,"Janela",new Vector3(side*2.3f,2.4f,4.14f),new Vector3(1.05f,1.25f,.08f),window);
                        if(i==1)Box(go.transform,"Janela superior",new Vector3(side*2.3f,4.6f,4.06f),new Vector3(1.05f,1.25f,.1f),window);
                    }
                    var prefab=PrefabUtility.SaveAsPrefabAsset(go,WorldGenerator.Folder+"/Casa"+(i+1)+".prefab");
                    w.lots.Add(new LotDefinition
                    {
                        id="casa-"+(i+1),center=centers[i],yaw=i==2?-90:90,prefab=prefab,manualHeight=true,foundationY=road.Height(centers[i])
                    }
                    );
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(go);
                }
            }
            AssetDatabase.CreateAsset(w,Data);
            AssetDatabase.SaveAssets();
            return w;
        }
        public static GameObject Activate(WorldDefinition w)
        {
            Check();
            var root=WorldGenerator.Apply(w);
            var scene=SceneManager.GetActiveScene();
            var session=Session();
            var so=session?new SerializedObject(session):null;
            var player=GameObject.Find("PlayerPrototype");
            var cam=GameObject.Find("Cais Gameplay Camera");
            string[] names=
            {
                "Cenario","Cenario_Colisao","Galpão do cais e píer","Ilha Tripo 018 - Teste","Blocos 019 - Implantacao","Terreno modular 020 - Teste"
            };
            if(!File.Exists(StatePath))
            {
                var all=scene.GetRootGameObjects();
                var state=new SceneState
                {
                    scenePath=scene.path,roots=names,active=names.Select(n=>all.FirstOrDefault(g=>g.name==n)?.activeSelf??false).ToArray(),playerPosition=player?player.transform.position:Vector3.zero,playerRotation=player?player.transform.rotation:Quaternion.identity,cameraPosition=cam?cam.transform.position:Vector3.zero,cameraRotation=cam?cam.transform.rotation:Quaternion.identity
                }
                ;
                if(so!=null)
                {
                    state.spawnId=GlobalObjectId.GetGlobalObjectIdSlow(so.FindProperty("spawnPoint").objectReferenceValue).ToString();
                    state.bounds=so.FindProperty("recoveryBounds").boundsValue;
                    state.fallHeight=so.FindProperty("fallHeight").floatValue;
                    state.title=so.FindProperty("controlsTitle").stringValue;
                }
                File.WriteAllText(StatePath,JsonUtility.ToJson(state,true));
                AssetDatabase.ImportAsset(StatePath);
            }
            foreach(var old in scene.GetRootGameObjects().Where(g=>names.Contains(g.name)))old.SetActive(false);
            root.SetActive(true);
            var spawn=root.transform.Find("Spawn021");
            if(!spawn)
            {
                spawn=new GameObject("Spawn021").transform;
                spawn.SetParent(root.transform,false);
            }
            spawn.position=new Vector3(0,w.roads[0].startY+.15f,-25);
            spawn.rotation=Quaternion.identity;
            if(so!=null)
            {
                so.FindProperty("spawnPoint").objectReferenceValue=spawn;
                so.FindProperty("recoveryBounds").boundsValue=new Bounds(new Vector3(0,8,0),new Vector3(160,50,160));
                so.FindProperty("fallHeight").floatValue=-5;
                so.FindProperty("controlsTitle").stringValue="WORLD BUILDER — PRIMEIRA RUA";
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            if(player)Move(player,spawn.position,spawn.rotation);
            if(cam)cam.transform.SetPositionAndRotation(spawn.position+new Vector3(0,4,-10),Quaternion.Euler(18,0,0));
            EditorSceneManager.SaveScene(scene);
            Selection.activeGameObject=root;
            if(SceneView.lastActiveSceneView)SceneView.lastActiveSceneView.LookAt(new Vector3(0,2,0),Quaternion.Euler(45,-35,0),65);
            return root;
        }
        static void Move(GameObject go,Vector3 p,Quaternion r)
        {
            var c=go.GetComponent<CharacterController>();
            bool enabled=c&&c.enabled;
            if(c)c.enabled=false;
            go.transform.SetPositionAndRotation(p,r);
            if(c)c.enabled=enabled;
        }
        public static void Restore(WorldDefinition w)
        {
            Check();
            if(!File.Exists(StatePath))return;
            var state=JsonUtility.FromJson<SceneState>(File.ReadAllText(StatePath));
            var scene=SceneManager.GetActiveScene();
            if(scene.path!=state.scenePath)throw new InvalidOperationException("Backup pertence a outra cena.");
            var root=WorldGenerator.FindRoot(w);
            if(root)root.SetActive(false);
            var all=scene.GetRootGameObjects();
            for(int i=0;i<state.roots.Length;i++)
            {
                var go=all.FirstOrDefault(g=>g.name==state.roots[i]);
                if(go)go.SetActive(state.active[i]);
            }
            var session=Session();
            if(session)
            {
                var so=new SerializedObject(session);
                GlobalObjectId.TryParse(state.spawnId,out var id);
                so.FindProperty("spawnPoint").objectReferenceValue=GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);
                so.FindProperty("recoveryBounds").boundsValue=state.bounds;
                so.FindProperty("fallHeight").floatValue=state.fallHeight;
                so.FindProperty("controlsTitle").stringValue=state.title;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            var p=GameObject.Find("PlayerPrototype");
            if(p)Move(p,state.playerPosition,state.playerRotation);
            var cam=GameObject.Find("Cais Gameplay Camera");
            if(cam)cam.transform.SetPositionAndRotation(state.cameraPosition,state.cameraRotation);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
