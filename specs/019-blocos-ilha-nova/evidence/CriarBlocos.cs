using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CriarBlocos019 {
 struct D { public string id; public float x,z,w,d,h,angle; public int color; public D(string id,float x,float z,float w,float d,float h,float angle,int color){this.id=id;this.x=x;this.z=z;this.w=w;this.d=d;this.h=h;this.angle=angle;this.color=color;} }
 public static string Main() {
  if (GameObject.Find("Blocos 019 - Implantacao") != null) return "Blocos existentes: nenhuma alteração aplicada.";
  D[] defs={
   new D("Casa_01",22.9f,-.94f,5,6,3.5f,90,0),new D("Casa_02",12.6f,3.74f,6,7,3.5f,180,0),
   new D("Casa_03",17.2f,-10.3f,6,7,3.5f,90,0),new D("Casa_04",8.2f,-10.75f,5,6,5.5f,180,0),
   new D("Casa_05",1.87f,-17.2f,6,7,5.5f,0,0),new D("Casa_06",-.94f,-9.82f,5,6,3.5f,180,0),
   new D("Casa_07",-11.3f,-16.55f,6,7,5.5f,0,0),new D("Casa_08",-21.7f,-11.2f,5,6,3.5f,90,0),
   new D("Casa_09",-9,4.5f,5,6,3.5f,0,0),new D("Casa_10",-18.7f,3,6,7,5.5f,90,0),
   new D("Casa_11",-12.25f,13.3f,5,6,3.5f,0,0),new D("Casa_12_Afastada",14,-27,4,5,3.2f,180,1),
   new D("Venda",5,1,6,6,4,180,2),new D("Loja",-2,5,5,6,4,0,2),
   new D("Sala_Comunitaria",-4,-2,6,6,4.5f,180,2),new D("Igreja",-6,-29,7,8,7,180,3),
   new D("Galpao_Cais",29,-1,6,6,4,0,2)};
  string dir="Assets/_Project/Materials/Blockout019";
  if(!AssetDatabase.IsValidFolder("Assets/_Project/Materials"))AssetDatabase.CreateFolder("Assets/_Project","Materials");
  if(!AssetDatabase.IsValidFolder(dir))AssetDatabase.CreateFolder("Assets/_Project/Materials","Blockout019");
  Color[] colors={new Color(.78f,.42f,.28f),new Color(.54f,.32f,.22f),new Color(.80f,.65f,.25f),new Color(.79f,.79f,.71f),new Color(.13f,.22f,.19f)};
  string[] names={"Casa","CasaAfastada","Servico","Igreja","Frente"};var mats=new Material[5];
  for(int i=0;i<5;i++){var path=dir+"/"+names[i]+".mat";mats[i]=AssetDatabase.LoadAssetAtPath<Material>(path);if(!mats[i]){mats[i]=new Material(Shader.Find("Standard"));mats[i].color=colors[i];mats[i].SetFloat("_Glossiness",0);AssetDatabase.CreateAsset(mats[i],path);}}
  var root=new GameObject("Blocos 019 - Implantacao");Undo.RegisterCreatedObjectUndo(root,"Implantação dos blocos 019");
  var rows=new StringBuilder("# Implantação preliminar — blocos 019\n\nPosições mundiais (X, Y, Z) em metros; dimensões largura × profundidade × altura; ângulo Y em graus. A frente é o eixo local +Z, indicada pela faixa escura. Alturas e ângulos são estimativas da referência, não medições precisas. Todas as identidades residenciais permanecem sem atribuição.\n\n| ID | Base X,Y,Z | L × P × A | Rotação | Desnível amostrado |\n|---|---|---|---:|---|\n");
  foreach(var d in defs){
   RaycastHit ground;if(!Physics.Raycast(new Vector3(d.x,100,d.z),Vector3.down,out ground,150,1<<8))throw new Exception("Sem chão no centro: "+d.id);
   float baseY=ground.point.y;var q=Quaternion.Euler(0,d.angle,0);var ys=new List<float>();
   foreach(var p in new[]{new Vector3(-d.w/2,0,-d.d/2),new Vector3(d.w/2,0,-d.d/2),new Vector3(-d.w/2,0,d.d/2),new Vector3(d.w/2,0,d.d/2)}){RaycastHit h;if(Physics.Raycast(new Vector3(d.x,100,d.z)+q*p,Vector3.down,out h,150,1<<8))ys.Add(h.point.y);}
   var anchor=new GameObject(d.id);anchor.transform.SetParent(root.transform,false);anchor.transform.position=new Vector3(d.x,baseY,d.z);anchor.transform.rotation=q;
   var box=GameObject.CreatePrimitive(PrimitiveType.Cube);box.name="Volume";box.transform.SetParent(anchor.transform,false);box.transform.localPosition=new Vector3(0,d.h/2,0);box.transform.localScale=new Vector3(d.w,d.h,d.d);box.layer=8;box.GetComponent<Renderer>().sharedMaterial=mats[d.color];
   var front=GameObject.CreatePrimitive(PrimitiveType.Cube);front.name="Frente (+Z)";front.transform.SetParent(anchor.transform,false);front.transform.localPosition=new Vector3(0,1.0f,d.d/2+.025f);front.transform.localScale=new Vector3(1.2f,2f,.05f);front.GetComponent<Collider>().enabled=false;front.GetComponent<Renderer>().sharedMaterial=mats[4];
   float lo=float.PositiveInfinity,hi=float.NegativeInfinity;foreach(var y in ys){lo=Mathf.Min(lo,y);hi=Mathf.Max(hi,y);}string diff=ys.Count==4?(hi-lo).ToString("F2")+" m":"amostra incompleta";
   rows.AppendLine($"| {d.id} | {d.x:F2}, {baseY:F2}, {d.z:F2} | {d.w:F1} × {d.d:F1} × {d.h:F1} | {d.angle:F0}° | {diff} |");
  }
  rows.AppendLine("\nDiferenças de altura entre cantos são apenas amostras: não comprovam apoio nem acesso. Muitos lotes exigem fundação ou correção do terreno. A casa afastada é uma proposta e não tem morador atribuído. Identidades das demais casas e papel da loja extra não foram decididos.\n");
  string report="/Users/bruno.duartec/Documents/Projects/Freyja/specs/019-blocos-ilha-nova/evidence/implantacao.md";File.WriteAllText(report,rows.ToString());
  AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
  return "17 blocos criados; tabela salva em "+report;
 }
}
