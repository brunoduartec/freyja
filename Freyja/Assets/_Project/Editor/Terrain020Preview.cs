using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Freyja.Prototype;

public static class Terrain020Preview
{
    const string Data="Assets/_Project/Data/Terrain020/PreviousState.json";
    const string Root="Terreno modular 020 - Teste";
    [Serializable] public class State
    {
        public string[] roots; public bool[] active;
        public string spawnId,title; public Bounds bounds; public float fallHeight;
        public Vector3 playerPosition,cameraPosition;public Quaternion playerRotation,cameraRotation;
    }
    static CaisSessionController Session()=>UnityEngine.Object.FindFirstObjectByType<CaisSessionController>();
    static GameObject FindRoot(string name)=>SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(g=>g.name==name);
    static void Check()
    {
        if (EditorApplication.isPlaying)
            throw new InvalidOperationException("Saia de Play Mode antes de alternar o terreno.");
        if (SceneManager.GetActiveScene().name != "Ilha_Tripo_Caminhada_011")
            throw new InvalidOperationException("O teste 020 requer a cena histórica Ilha_Tripo_Caminhada_011.");
    }
    static void MovePlayer(Vector3 p,Quaternion r)
    {
        var player=GameObject.Find("PlayerPrototype");var body=player.GetComponent<CharacterController>();bool enabled=body.enabled;
        body.enabled=false;player.transform.SetPositionAndRotation(p,r);body.enabled=enabled;
    }
    static void Save()=>EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    [MenuItem("Freyja/Terreno 020/Ativar teste")]
    public static void Activate()
    {
        Check();var root=FindRoot(Root);if(!root)throw new InvalidOperationException("Monte o teste 020 primeiro.");
        var so=new SerializedObject(Session());var cam=GameObject.Find("Cais Gameplay Camera").transform;
        var player=GameObject.Find("PlayerPrototype").transform;
        string[] names={"Cenario","Cenario_Colisao","Galpão do cais e píer","Ilha Tripo 018 - Teste","Blocos 019 - Implantacao"};
        if(!File.Exists(Data))
        {
            var state=new State{roots=names,active=names.Select(n=>FindRoot(n)?.activeSelf??false).ToArray(),
                spawnId=GlobalObjectId.GetGlobalObjectIdSlow(so.FindProperty("spawnPoint").objectReferenceValue).ToString(),
                title=so.FindProperty("controlsTitle").stringValue,bounds=so.FindProperty("recoveryBounds").boundsValue,fallHeight=so.FindProperty("fallHeight").floatValue,
                playerPosition=player.position,playerRotation=player.rotation,cameraPosition=cam.position,cameraRotation=cam.rotation};
            Directory.CreateDirectory(Path.GetDirectoryName(Data));File.WriteAllText(Data,JsonUtility.ToJson(state,true));AssetDatabase.ImportAsset(Data);
        }
        foreach(var name in names){var old=FindRoot(name);if(old)old.SetActive(false);}
        root.SetActive(true);var spawn=root.transform.Find("Spawn020");
        so.FindProperty("spawnPoint").objectReferenceValue=spawn;
        so.FindProperty("recoveryBounds").boundsValue=new Bounds(new Vector3(200,5,12),new Vector3(60,40,76));
        so.FindProperty("fallHeight").floatValue=-2;
        so.FindProperty("controlsTitle").stringValue="TESTE DE TERRENO — CAIS E DOIS PATAMARES";
        so.ApplyModifiedPropertiesWithoutUndo();MovePlayer(spawn.position,spawn.rotation);
        cam.SetPositionAndRotation(spawn.position+new Vector3(0,4,-10),Quaternion.Euler(18,0,0));Save();
    }
    [MenuItem("Freyja/Terreno 020/Restaurar ilha")]
    public static void Restore()
    {
        Check();var root=FindRoot(Root);if(!root || !File.Exists(Data))return;
        var state=JsonUtility.FromJson<State>(File.ReadAllText(Data));
        root.SetActive(false);
        for(int i=0;i<state.roots.Length;i++){var old=FindRoot(state.roots[i]);if(old)old.SetActive(state.active[i]);}
        var so=new SerializedObject(Session());GlobalObjectId.TryParse(state.spawnId,out var id);
        so.FindProperty("spawnPoint").objectReferenceValue=GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);
        so.FindProperty("controlsTitle").stringValue=state.title;so.FindProperty("recoveryBounds").boundsValue=state.bounds;so.FindProperty("fallHeight").floatValue=state.fallHeight;
        so.ApplyModifiedPropertiesWithoutUndo();MovePlayer(state.playerPosition,state.playerRotation);
        GameObject.Find("Cais Gameplay Camera").transform.SetPositionAndRotation(state.cameraPosition,state.cameraRotation);Save();
    }
}
