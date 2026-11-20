using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class AssembleTripo023
{
    const string Folder = "Assets/_Project/Art/Island023Tripo";
    const string Evidence = "/Users/bruno.duartec/Documents/Projects/Freyja/Artifacts/Island023/Tripo/";
    readonly struct Placement
    {
        public readonly string Lot;
        public readonly int Piece;
        public readonly float Width, DoorX, DoorDepth, DoorRise;
        public Placement(string lot, int piece, float width, float doorX, float doorDepth, float doorRise)
        { Lot=lot; Piece=piece; Width=width; DoorX=doorX; DoorDepth=doorDepth; DoorRise=doorRise; }
    }
    static readonly Placement[] Plan = {
        new("B4",1,7.5f,-.14f,.90f,.04f), // Sobrado com varanda
        new("A1",2,5.8f,-.19f,.81f,.10f), // Atrás do sobrado
        new("B2",3,5.9f,-.18f,.88f,.08f), // Largo esquerdo
        new("Igreja",4,8.6f,0,.90f,.09f),
        new("A3",5,6.15f,-.20f,.91f,.04f), // Largo direito
        new("L3",6,7.6f,.24f,.67f,.06f), // Terraço superior direito, porta térrea
        new("B3",7,6.4f,-.10f,.74f,.58f), // Escada e hortas
        new("B1",8,6.5f,-.15f,.90f,.06f), // Terraço baixo esquerdo
        new("A2",9,6.7f,-.23f,.90f,.10f), // Terraço intermediário direito
        new("L1",10,6.6f,-.22f,.88f,.07f), // Terraço baixo direito
    };
    static GameObject Prepared(int piece, float yaw = 180)
    {
        var key = $"Piece{piece:00}";
        var texturePath = $"{Folder}/{key}.png";
        var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
        if (importer.filterMode != FilterMode.Point || importer.mipmapEnabled || importer.textureCompression != TextureImporterCompression.Uncompressed)
        {
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
        var materialPath = $"{Folder}/{key}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (!mat)
        {
            mat = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(mat, materialPath);
        }
        mat.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        mat.SetFloat("_Glossiness", 0);
        mat.SetFloat("_Metallic", 0);
        EditorUtility.SetDirty(mat);
        var path = $"{Folder}/{key}.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        
        var wrapper = new GameObject(key);
        try
        {
            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>($"{Folder}/{key}.fbx");
            var mesh = (GameObject)PrefabUtility.InstantiatePrefab(fbx, wrapper.transform);
            mesh.transform.localRotation = Quaternion.Euler(0,yaw,0)*fbx.transform.localRotation;
            foreach (var r in mesh.GetComponentsInChildren<MeshRenderer>())
                r.sharedMaterials = r.sharedMaterials.Select(_ => mat).ToArray();
            var bounds = BoundsOf(wrapper);
            mesh.transform.localPosition -= new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
            wrapper.layer = 8;
            return PrefabUtility.SaveAsPrefabAsset(wrapper, path);
        }
        finally { UnityEngine.Object.DestroyImmediate(wrapper); }
    }
    static Bounds BoundsOf(GameObject go)
    {
        var renderers=go.GetComponentsInChildren<Renderer>();
        var bounds=renderers[0].bounds;
        foreach(var r in renderers.Skip(1)) bounds.Encapsulate(r.bounds);
        return bounds;
    }

    public static object PlacePlaza(int piece, float width, float yaw, float doorX, float doorZ, float doorRise)
    {
        if (piece != 12 && piece != 13) throw new ArgumentOutOfRangeException(nameof(piece));
        return PlaceBuilding(piece, width, yaw, doorX, doorZ, doorRise);
    }

    public static object PlaceBuilding(int piece, float width, float yaw, float doorX, float doorZ, float doorRise)
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.name != "Ilha_Tripo_Caminhada_011" || EditorApplication.isPlaying)
            throw new InvalidOperationException("Abra a cena da ilha fora do Play Mode.");
        var (lot, placeholder) = piece switch
        {
            12 => ("Venda", "Shop"),
            13 => ("Oficina", "Shop"),
            14 => ("A4", "House"),
            15 => ("L2", "House"),
            16 => ("L4 — casa isolada", "OldHouse"),
            17 => ("Salão comunitário", "Hall"),
            18 => ("Armazém do cais (apoio em terra)", "Warehouse"),
            _ => throw new ArgumentOutOfRangeException(nameof(piece))
        };
        var island = GameObject.Find("Island023");
        var plot = island.transform.Find($"Construções — 12 casas e 5 serviços/{lot}");
        if (!plot || !plot.Find(placeholder)) throw new InvalidOperationException("Lote provisório ausente: " + lot);
        var backup = Evidence + (piece <= 13 ? "Cena-antes-praca.unity" : "Cena-antes-14-18.unity");
        if (!File.Exists(backup)) File.Copy(scene.path, backup);
        AssetDatabase.Refresh();

        var prefab = Prepared(piece, yaw);
        var size = BoundsOf(prefab).size;
        var modelName = $"Tripo{piece:00}";
        var current = plot.Find(modelName);
        var visual = current ? current.gameObject : (GameObject)PrefabUtility.InstantiatePrefab(prefab, plot);
        visual.name = modelName;
        float scale = width / size.x;
        visual.transform.localScale = Vector3.one * scale;
        visual.transform.localPosition = Vector3.up * .025f;
        plot.Find(placeholder).gameObject.SetActive(false);

        var collider = plot.GetComponent<BoxCollider>();
        float facade = -size.z * scale * doorZ;
        float back = size.z * scale * .42f;
        collider.center = new Vector3(0, size.y * scale * .35f, (back + facade) / 2);
        collider.size = new Vector3(width * .9f, size.y * scale * .70f, back - facade);
        if (piece == 12)
            Box(plot, "Colisão da banca", new Vector3(-width * .185f, .49f, facade - .28f),
                new Vector3(width * .47f, .98f, .56f));
        if (piece == 13)
            Box(plot, "Colisão da bancada", new Vector3(width * .24f, .49f, facade - .25f),
                new Vector3(width * .29f, .98f, .50f));
        var baseBlock = plot.Find("Base de pedra");
        if (baseBlock)
        {
            baseBlock.localScale = new Vector3(width * .85f, .35f, Mathf.Min(size.z * scale * .78f, 6));
            baseBlock.localPosition = new Vector3(0, -.20f, .15f);
        }

        var door = plot.Find("Door023");
        if (!door)
        {
            door = new GameObject("Door023").transform;
            door.SetParent(plot, false);
        }
        door.localPosition = new Vector3(doorX * width, doorRise, facade - .60f);
        var terrain = island.GetComponentInChildren<Terrain>();
        float ground = terrain.SampleHeight(door.position) + terrain.transform.position.y;
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = visual;
        return new { lot, piece, width, depth = size.z * scale, height = size.y * scale,
            door = door.position.ToString("F3"), terrainAtDoor = ground, threshold = door.position.y };
    }
    static void Box(Transform parent, string name, Vector3 pos, Vector3 size)
    {
        var found=parent.Find(name);
        var go=found?found.gameObject:new GameObject(name);
        go.layer = 8;
        go.transform.SetParent(parent,false);
        go.transform.localPosition=pos;
        var collider=go.GetComponent<BoxCollider>();
        if(!collider) collider=go.AddComponent<BoxCollider>();
        collider.size=size;
    }
    public static object Main()
    {
        var scene=SceneManager.GetActiveScene();
        if (scene.name!="Ilha_Tripo_Caminhada_011" || EditorApplication.isPlaying)
            throw new InvalidOperationException("Abra a cena da ilha fora do Play Mode.");
        var island=GameObject.Find("Island023").transform;
        var buildings=island.Find("Construções — 12 casas e 5 serviços");
        
        Directory.CreateDirectory(Evidence);
        if (!File.Exists(Evidence+"Cena-antes.unity")) File.Copy(scene.path,Evidence+"Cena-antes.unity");
        AssetDatabase.Refresh();
        var placements=Plan.Select(p=>new { p, prefab=Prepared(p.Piece) }).ToArray();
        var pierPrefab=Prepared(11);
        var report=new System.Collections.Generic.List<object>();
        foreach(var item in placements)
        {
            var p=item.p;
            var plot=buildings.Find(p.Lot);
            var prefabSize=BoundsOf(item.prefab).size;
            var current=plot.Find($"Tripo{p.Piece:00}");
            var visual=current?current.gameObject:(GameObject)PrefabUtility.InstantiatePrefab(item.prefab,plot);
            visual.name=$"Tripo{p.Piece:00}";
            float scale=p.Width/prefabSize.x;
            visual.transform.localScale=Vector3.one*scale;
            visual.transform.localPosition=Vector3.up*.04f;
            plot.GetChild(0).gameObject.SetActive(false);
            var oldBase=plot.Find("Base de pedra");
            if (oldBase) oldBase.gameObject.SetActive(false);
            var collider=plot.GetComponent<BoxCollider>();
            float depth=prefabSize.z*scale;
            float frontZ=-depth/2*p.DoorDepth;
            float coreBack=depth*.40f;
            collider.center=new Vector3(0,1.5f,(coreBack+frontZ)/2+.10f);
            collider.size=new Vector3(p.Width*(p.Piece==6?.77f:.88f),3,coreBack-frontZ-.20f);
            var door=plot.Find("Door023")??new GameObject("Door023").transform;
            door.SetParent(plot,false);
            door.localPosition=new Vector3(p.DoorX*p.Width,p.DoorRise+.04f,frontZ-.50f);
            report.Add(new{lot=p.Lot,piece=p.Piece,width=p.Width,depth,height=prefabSize.y*scale,door=door.position.ToString("F3"),angle=plot.eulerAngles.y});
        }
        var dock=island.Find("Cais e píer");
        var existingPier=dock.Find("11 — Cais, píer e galpão Tripo");
        var pier=existingPier?existingPier.gameObject:(GameObject)PrefabUtility.InstantiatePrefab(pierPrefab,dock);
        pier.name="11 — Cais, píer e galpão Tripo";
        float pierScale=18/BoundsOf(pierPrefab).size.x;
        pier.transform.localScale=Vector3.one*pierScale;
        pier.transform.SetPositionAndRotation(new Vector3(-40,.04f,-62),Quaternion.Euler(0,-90,0));
        var dockPiece=pier.transform;
        var mesh=dockPiece.GetComponentInChildren<MeshFilter>();
        var surface=mesh.GetComponent<MeshCollider>();
        if(!surface) surface=mesh.gameObject.AddComponent<MeshCollider>();
        surface.enabled=true;
        surface.sharedMesh=mesh.sharedMesh;
        Physics.SyncTransforms();
        var bounds=BoundsOf(pier);
        // Find the planks at the open end, then align the deck to the established quay level.
        float deck=1.8f;
        var probe=new Ray(new Vector3(-40,14,-68),Vector3.down);
        if(surface.Raycast(probe,out RaycastHit hit,20)) pier.transform.position+=Vector3.up*(deck-hit.point.y);
        surface.enabled=false;
        Box(dock,"Colisão do convés",new Vector3(-40,1.67f,-61.5f),new Vector3(5.6f,.26f,17.1f));
        Box(dock,"Colisão do galpão",new Vector3(-40,3.24f,-56.35f),new Vector3(5.4f,2.9f,5.6f));
        Box(dock,"Passagem lateral",new Vector3(-36.32f,1.67f,-56.9f),new Vector3(1.35f,.26f,7.5f));
        Box(dock,"Passagem do convés",new Vector3(-38.5f,1.67f,-61.5f),new Vector3(4,.26f,4));
        foreach(Transform child in dock)
            if(child.name=="Píer de madeira"||child.name=="Estaca") child.gameObject.SetActive(false);
        
        File.WriteAllText(Evidence+"placement.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject=island.gameObject;
        return new {models=report.Count+1,pier=pier.transform.position.ToString(),pierBounds=bounds.size.ToString()};
    }
}
