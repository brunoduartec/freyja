using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class AjustarLotes019
{
    const string Folder = "/Users/bruno.duartec/Documents/Projects/Freyja/specs/019-blocos-ilha-nova/evidence/";
    [Serializable] public class Lot { public string id; public int pad; public float x,z,width,depth,yaw; }
    [Serializable] public class Lots { public Lot[] items; }

    public static string Main()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlaying || scene.path != "Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity")
            throw new Exception("Projeto deve estar em Edit Mode na cena principal.");
        var root = GameObject.Find("Blocos 019 - Implantacao").transform;
        var lots = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Lot>>(File.ReadAllText(Folder+"lotes-medidos.json"));
        var extra = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Lot>>(File.ReadAllText(Folder+"locais-sem-marcacao.json"));
        foreach(var lot in extra)
        {
            if(lot.id=="Casa_03"){lot.width=5;lot.depth=5.6f;}
            if(lot.id=="Casa_09"){lot.width=5.8f;lot.depth=6;}
            if(lot.id=="Casa_11"){lot.width=4.5f;lot.depth=5.2f;}
            if(lot.id=="Casa_12_Afastada"){lot.width=3.6f;lot.depth=4.2f;}
            lot.yaw=35; lots.Add(lot);
        }
        var heights=new Dictionary<string,float>{
            {"Casa_01",4},{"Casa_02",4},{"Casa_03",6},{"Casa_04",6},
            {"Casa_05",6},{"Casa_06",4},{"Casa_07",4},{"Casa_08",3.8f},
            {"Casa_09",6},{"Casa_10",4},{"Casa_11",4},{"Casa_12_Afastada",3.2f},
            {"Venda",5.8f},{"Loja",5.6f},{"Sala_Comunitaria",4.5f},{"Igreja",5.2f},{"Galpao_Cais",4.3f}
        };
        // 90/270 degrees swaps axes without changing the measured footprint.
        var fronts=new Dictionary<string,int>{
            {"Casa_08",180},{"Casa_05",0},{"Sala_Comunitaria",90},{"Casa_07",180},
            {"Loja",0},{"Casa_01",0},{"Galpao_Cais",0},{"Casa_04",0},{"Venda",270},
            {"Casa_02",270},{"Casa_06",90},{"Igreja",90},{"Casa_10",0}
        };
        var previous=root.Cast<Transform>().Select(t=>new {id=t.name,position=t.position.ToString("F6"),rotation=t.eulerAngles.ToString("F6"),scale=t.Find("Volume").localScale.ToString("F6")}).ToArray();
        if(!File.Exists(Folder+"implantacao-antes-lotes.json"))
            File.WriteAllText(Folder+"implantacao-antes-lotes.json",Newtonsoft.Json.JsonConvert.SerializeObject(previous));
        var records=new List<object>();
        var rows=new StringBuilder("# Implantação pelos limites do terreno — revisão 019\n\n13 lotes medidos na captura ortográfica `terreno-planta.png` (110 m / 2048 px). Solo exposto segmentado por cor dentro de contornos selecionados, seguido por ajuste de retângulo orientado aos percentis 1–99% da mancha. Os limites da textura são arredondados; não constituem uma planta retangular exata. `lotes-medidos.png` mostra os contornos cianos e retângulos vermelhos.\n\nAs frentes foram orientadas para os acessos próximos, preservando a projeção medida. Nos lotes sem marcação, a posição é uma estimativa pela referência e pelo espaço livre no terreno. Alturas incluem cobertura e são estimadas pela tipologia da imagem. O bloco se prolonga até a menor altura amostrada para não flutuar; a coluna H box inclui esse embasamento.\n\n| Construção | Lote | Base XYZ (m) | L × P (m) | H visual / H box (m) | Y | Desnível (m) |\n|---|---|---|---|---|---|---|\n");
        root.gameObject.SetActive(false);
        try
        {
            foreach(var lot in lots)
            {
                var t=root.Find(lot.id);
                if(!t)throw new Exception("Bloco ausente: "+lot.id);
                int turn=fronts.TryGetValue(lot.id,out var a)?a:0;
                float w=lot.width,d=lot.depth;
                if(turn%180!=0){w=lot.depth;d=lot.width;}
                float yaw=(lot.yaw+turn)%360;
                var rotation=Quaternion.Euler(0,yaw,0);
                var terrain=new List<float>();
                for(int ix=0;ix<5;ix++)for(int iz=0;iz<5;iz++)
                {
                    var p=new Vector3(lot.x,100,lot.z)+rotation*new Vector3((ix/4f-.5f)*w,0,(iz/4f-.5f)*d);
                    if(!Physics.Raycast(p,Vector3.down,out var hit,150,1<<8))throw new Exception("Apoio ausente: "+lot.id);
                    terrain.Add(hit.point.y);
                }
                terrain.Sort();
                float bottom=terrain[0]-.05f, floor=terrain[12], height=heights[lot.id]+floor-bottom;
                Undo.RecordObject(t,"Encaixar no lote medido");
                t.position=new Vector3(lot.x,bottom,lot.z);t.rotation=rotation;
                var volume=t.Find("Volume");Undo.RecordObject(volume,"Ajustar box ao lote");
                volume.localScale=new Vector3(w,height,d);volume.localPosition=new Vector3(0,height/2,0);
                var front=t.Find("Frente (+Z)");Undo.RecordObject(front,"Orientar acesso");
                front.localPosition=new Vector3(0,floor-bottom+1,d/2+.025f);
                if(lot.id=="Igreja")
                {
                    var tower=t.Find("Torre");Undo.RecordObject(tower,"Ajustar torre");
                    float th=7.8f+floor-bottom;
                    tower.localScale=new Vector3(1.8f,th,1.8f);
                    tower.localPosition=new Vector3(w/2-1.0f,th/2,-d/2+1.0f);
                }
                var corners=new List<Vector3>();
                foreach(var p in new[]{new Vector3(-w/2,0,-d/2),new Vector3(w/2,0,-d/2),new Vector3(w/2,0,d/2),new Vector3(-w/2,0,d/2)})corners.Add(t.TransformPoint(p));
                records.Add(new {id=lot.id,pad=lot.pad,x=lot.x,y=bottom,z=lot.z,width=w,depth=d,height,yaw,floor,terrainMin=terrain[0],terrainMax=terrain[24],corners=corners.Select(p=>new{x=p.x,y=p.y,z=p.z}).ToArray()});
                rows.AppendLine($"| {lot.id} | {(lot.pad==0?"sem marca":lot.pad.ToString("D2"))} | {t.position.ToString("F2")} | {w:F2} × {d:F2} | {heights[lot.id]:F2} / {height:F2} | {yaw:F0}° | {terrain[24]-terrain[0]:F2} |");
            }
        }
        finally{root.gameObject.SetActive(true);}
        Physics.SyncTransforms();
        var clashes=new List<string>();
        for(int i=0;i<root.childCount;i++)for(int j=i+1;j<root.childCount;j++)
        {
            var t=root.GetChild(i);var u=root.GetChild(j);
            var a=t.Find("Volume").GetComponent<BoxCollider>();var b=u.Find("Volume").GetComponent<BoxCollider>();
            if(Physics.ComputePenetration(a,a.transform.position,a.transform.rotation,b,b.transform.position,b.transform.rotation,out var dir,out var distance))clashes.Add(t.name+" / "+u.name+" : "+distance);
        }
        File.WriteAllText(Folder+"implantacao-medida.json",Newtonsoft.Json.JsonConvert.SerializeObject(records));
        rows.AppendLine("\nIgreja implantada no lote 12, com nave e torre separadas. Casa_10 e Casa_11 são complementares à contagem visível da referência. Nenhum lote marcado fica vazio. As construções sem marcação são Casa_03, Casa_09, Casa_11 e Casa_12_Afastada; não devem ser tratadas como encaixes determinados pelo GLB.\n\nInterpenetrações entre volumes: "+(clashes.Count==0?"nenhuma.":string.Join("; ",clashes)));
        File.WriteAllText(Folder+"implantacao.md",rows.ToString());
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        return "17 blocos; 13 lotes medidos; conflitos: "+string.Join("; ",clashes);
    }
}
