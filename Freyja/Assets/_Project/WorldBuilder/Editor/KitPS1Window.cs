using UnityEngine;using UnityEditor;
namespace Freyja.WorldBuilder.Editor {
 public sealed class KitPS1Window:EditorWindow {
  string placing,message;string[] labels={"Pedra arredondada","Pedra baixa","Pedra vertical","Pinheiro","Árvore de copa larga"};
  [MenuItem("Tools/Freyja/Kit PS1 — praia")]
  public static void Open()=>GetWindow<KitPS1Window>("Kit PS1");
  void OnEnable()=>SceneView.duringSceneGui+=Scene;
  void OnDisable()=>SceneView.duringSceneGui-=Scene;
  void Run(System.Action action){try{action();message="Pronto.";}catch(System.Exception e){message=e.Message;}}
  void OnGUI(){EditorGUILayout.LabelField("Freyja • kit PS1",EditorStyles.boldLabel);if(GUILayout.Button("Abrir trecho de praia"))Run(()=>KitPS1.Activate());if(!KitPS1.World)return;EditorGUILayout.HelpBox("Texturas 32/64 px. Escolha um objeto e clique no terreno. Depois use mover, girar e escala do Unity. Esc cancela.",MessageType.Info);
   if(GUILayout.Button("Pintar terreno (pincéis do Unity)"))Run(()=>Selection.activeGameObject=WorldGenerator.FindRoot(KitPS1.World).GetComponentInChildren<Terrain>().gameObject);
   EditorGUILayout.LabelField("Acabamento do caminho");foreach(var pair in new[]{("Dirt","Terra"),("Stone","Pedra"),("Brick","Tijolo")})if(GUILayout.Button(pair.Item2))Run(()=>{Undo.RecordObject(KitPS1.World,"Material do caminho");KitPS1.World.roadMaterial=KitPS1.Material(pair.Item1);EditorUtility.SetDirty(KitPS1.World);WorldGenerator.Apply(KitPS1.World);});
   EditorGUILayout.LabelField("Colocar objeto");for(int i=0;i<labels.Length;i++)if(GUILayout.Button(labels[i])){placing=KitPS1.Objects[i];SceneView.RepaintAll();}
   if(GUILayout.Button("Editar lotes e relevo")){WorldBuilderWindow.OpenWorld(KitPS1.World);Selection.activeObject=KitPS1.World;}
   if(GUILayout.Button("Reiniciar pintura da praia")&&EditorUtility.DisplayDialog("Reiniciar pintura","Substituir a pintura manual pela distribuição inicial? É possível desfazer.","Reiniciar","Cancelar"))Run(()=>KitPS1.ResetPaint(KitPS1.World));
   if(GUILayout.Button("Voltar à demonstração anterior"))Run(()=>{WorldGenerator.FindRoot(KitPS1.World).SetActive(false);WorldDemo.Activate(AssetDatabase.LoadAssetAtPath<WorldDefinition>(WorldDemo.Data));});
   EditorGUILayout.HelpBox(!string.IsNullOrEmpty(placing)?"Clique no terreno para colocar: "+placing:message??"",MessageType.None);
  }
  void Scene(SceneView view){if(string.IsNullOrEmpty(placing)||!KitPS1.World)return;var e=Event.current;if(e.type==EventType.KeyDown&&e.keyCode==KeyCode.Escape){placing=null;e.Use();return;}var root=WorldGenerator.FindRoot(KitPS1.World);if(!root)return;var col=root.GetComponentInChildren<TerrainCollider>();var ray=HandleUtility.GUIPointToWorldRay(e.mousePosition);if(!col.Raycast(ray,out var hit,2000))return;Handles.color=Color.yellow;Handles.DrawWireDisc(hit.point,Vector3.up,.8f);if(e.type==EventType.Layout)HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));if(e.type==EventType.MouseDown&&e.button==0&&!e.alt){KitPS1.Place(placing,hit.point);placing=null;e.Use();Repaint();}view.Repaint();}
 }
}
