using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
namespace Freyja.WorldBuilder.Editor
{
    public sealed class WorldBuilderWindow:EditorWindow
    {
        [SerializeField] WorldDefinition world;
        [SerializeField] int selected;
        bool preview=true,grid=true;
        Vector2 scroll;
        string message="";
        GameObject source;
        Vector2 target=new Vector2(9,11);
        Vector3 orientation;
        [MenuItem("Tools/Freyja/World Builder")]public static void Open()=>GetWindow<WorldBuilderWindow>("World Builder");
        void OnEnable()
        {
            SceneView.duringSceneGui+=DrawScene;
            if(!world)world=AssetDatabase.LoadAssetAtPath<WorldDefinition>(WorldDemo.Data);
        }
        void OnDisable()=>SceneView.duringSceneGui-=DrawScene;
        void Run(Action action)
        {
            try
            {
                action();
                message="Concluído.";
            }
            catch(Exception e)
            {
                message=e.Message;
                Debug.LogWarning("World Builder: "+e.Message);
            }
            Repaint();
            SceneView.RepaintAll();
        }
        void OnGUI()
        {
            scroll=EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("Freyja • World Builder",EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Edite lotes em metros. A prévia não modifica o terreno; Aplicar confirma as mudanças.",MessageType.Info);
            world=(WorldDefinition)EditorGUILayout.ObjectField("Mundo",world,typeof(WorldDefinition),false);
            if(GUILayout.Button("Criar / abrir demonstração"))Run(()=>
            {
                world=WorldDemo.Create();Selection.activeObject=world;
            }
            );
            if(world)
            {
                var so=new SerializedObject(world);
                so.Update();
                EditorGUILayout.PropertyField(so.FindProperty("seed"));
                EditorGUILayout.PropertyField(so.FindProperty("amplitude"),new GUIContent("Relevo"));
                EditorGUILayout.PropertyField(so.FindProperty("gridStep"),new GUIContent("Passo do grid"));
                grid=EditorGUILayout.Toggle("Exibir grid",grid);
                preview=EditorGUILayout.Toggle("Prévia dos volumes",preview);
                if(world.lots.Count>0)
                {
                    selected=Mathf.Clamp(selected,0,world.lots.Count-1);
                    selected=EditorGUILayout.Popup("Lote",selected,world.lots.Select(l=>l.id).ToArray());
                    EditorGUILayout.PropertyField(so.FindProperty("lots").GetArrayElementAtIndex(selected),new GUIContent("Editar lote"),true);
                }
                EditorGUILayout.PropertyField(so.FindProperty("roads"),new GUIContent("Vias"),true);
                if(so.ApplyModifiedProperties())SceneView.RepaintAll();
                EditorGUILayout.BeginHorizontal();
                if(GUILayout.Button("Adicionar lote"))
                {
                    Undo.RecordObject(world,"Novo lote");
                    world.lots.Add(new LotDefinition
                    {
                        center=new Vector2(30,-20),prefab=world.lots.FirstOrDefault()?.prefab
                    }
                    );
                    selected=world.lots.Count-1;
                    EditorUtility.SetDirty(world);
                }
                if(GUILayout.Button("Remover lote")&&world.lots.Count>0)
                {
                    Undo.RecordObject(world,"Remover lote");
                    world.lots.RemoveAt(selected);
                    selected=0;
                    EditorUtility.SetDirty(world);
                }
                EditorGUILayout.EndHorizontal();
                var issues=WorldValidation.Issues(world);
                foreach(var issue in issues)EditorGUILayout.HelpBox(issue,MessageType.Warning);
                using(new EditorGUI.DisabledScope(EditorApplication.isPlaying||issues.Count>0))
                {
                    if(GUILayout.Button("Aplicar / reconstruir gerenciados"))Run(()=>
                    {
                        WorldGenerator.Apply(world);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                    }
                    );
                    if(GUILayout.Button("Ativar demonstração na cena"))Run(()=>WorldDemo.Activate(world));
                }
                if(GUILayout.Button("Restaurar cenário anterior"))Run(()=>WorldDemo.Restore(world));
                if(GUILayout.Button("Validar e capturar"))Run(()=>message=WorldReport.Capture(world));
            }
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Preparar modelo local",EditorStyles.boldLabel);
            source=(GameObject)EditorGUILayout.ObjectField("Modelo / FBX",source,typeof(GameObject),false);
            target=EditorGUILayout.Vector2Field("Área máxima (m)",target);
            orientation=EditorGUILayout.Vector3Field("Correção de eixos",orientation);
            if(GUILayout.Button("Criar prefab normalizado"))Run(()=>
            {
                var path=EditorUtility.SaveFilePanelInProject("Prefab normalizado","Normalized","prefab","Escolha a pasta de saída");if(!string.IsNullOrEmpty(path))
                {
                    var d=ExternalModelNormalizer.Normalize(source,target,orientation,path);Selection.activeObject=d;
                }
            }
            );
            if(!string.IsNullOrEmpty(message))EditorGUILayout.HelpBox(message,MessageType.None);
            EditorGUILayout.EndScrollView();
        }
        void DrawScene(SceneView view)
        {
            if(!world)return;
            var errors=WorldValidation.Issues(world);
            if(grid)
            {
                float step=Mathf.Max(.5f,world.gridStep);
                Handles.color=new Color(.7f,.8f,.8f,.16f);
                for(float x=-world.size.x/2;x<=world.size.x/2;x+=step)Handles.DrawLine(new Vector3(x,.02f,-world.size.y/2),new Vector3(x,.02f,world.size.y/2));
                for(float z=-world.size.y/2;z<=world.size.y/2;z+=step)Handles.DrawLine(new Vector3(-world.size.x/2,.02f,z),new Vector3(world.size.x/2,.02f,z));
            }
            for(int i=0;i<world.lots.Count;i++)
            {
                var lot=world.lots[i];
                float y=TerrainMath.Foundation(world,lot);
                var pos=new Vector3(lot.center.x,y,lot.center.y);
                Handles.color=errors.Count==0?new Color(.2f,1,.7f,.8f):new Color(1,.3f,.2f,.8f);
                var c=lot.Rect.Corners().Select(q=>new Vector3(q.x,y+.1f,q.y)).ToArray();
                Handles.DrawAAPolyLine(3,c.Concat(new[]
                {
                    c[0]
                }
                ).ToArray());
                Handles.Label(pos+Vector3.up*6,lot.id+" • "+y.ToString("F2")+" m");
                if(Handles.Button(pos,Quaternion.identity,1,1,Handles.SphereHandleCap))selected=i;
                if(preview)using(new Handles.DrawingScope(Matrix4x4.TRS(pos,Quaternion.Euler(0,lot.yaw,0),Vector3.one)))
                {
                    Handles.DrawWireCube(new Vector3(0,2,0),new Vector3(lot.footprint.x,4,lot.footprint.y));
                    Handles.ArrowHandleCap(0,new Vector3(0,.2f,0),Quaternion.identity,lot.footprint.y/2,EventType.Repaint);
                }
                if(i!=selected)continue;
                EditorGUI.BeginChangeCheck();
                var moved=Handles.PositionHandle(pos,Quaternion.identity);
                var rot=Handles.RotationHandle(Quaternion.Euler(0,lot.yaw,0),pos);
                if(EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(world,"Editar lote");
                    lot.center=GridMath.Snap(new Vector2(moved.x,moved.z),Mathf.Max(.5f,world.gridStep));
                    lot.yaw=rot.eulerAngles.y;
                    EditorUtility.SetDirty(world);
                    Repaint();
                }
            }
        }
    }
}
