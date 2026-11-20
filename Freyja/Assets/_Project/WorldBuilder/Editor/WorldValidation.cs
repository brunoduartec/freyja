using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
namespace Freyja.WorldBuilder.Editor
{
    public static class WorldValidation
    {
        public static List<string> Issues(WorldDefinition w)
        {
            var errors=new List<string>();
            if(!w)
            {
                errors.Add("Selecione um mundo.");
                return errors;
            }
            if(w.baseTerrain && w.baseTerrain.size!=new Vector3(w.size.x,w.terrainHeight,w.size.y))errors.Add("Base de alturas incompatível com o mundo.");
            bool Fin(float v)=>!float.IsNaN(v)&&!float.IsInfinity(v);
            if(w.size.x<=0||w.size.y<=0||!Fin(w.size.x)||!Fin(w.size.y)||w.resolution<33||w.resolution>1025||((w.resolution-1)&(w.resolution-2))!=0||w.terrainHeight<=0||!Fin(w.amplitude)||w.amplitude<0||w.amplitude>w.originY+w.terrainHeight)errors.Add("Dimensões, resolução ou amplitude do terreno inválidas.");
            if(w.lots.Any(l=>l==null)||w.roads.Any(r=>r==null))
            {
                errors.Add("Registro vazio.");
                return errors;
            }
            if(w.lots.Select(l=>l.id).Concat(w.roads.Select(r=>r.id)).Any(string.IsNullOrWhiteSpace)||w.lots.Select(l=>l.id).Concat(w.roads.Select(r=>r.id)).Distinct().Count()!=w.lots.Count+w.roads.Count)errors.Add("IDs ausentes ou duplicados.");
            bool Outside(OrientedRectXZ r)=>r.Corners().Any(c=>Mathf.Abs(c.x)>w.size.x/2||Mathf.Abs(c.y)>w.size.y/2);
            foreach(var l in w.lots)
            {
                if(!Fin(l.center.x)||!Fin(l.center.y)||!Fin(l.yaw)||!Fin(l.foundationY)||l.size.x<=0||l.size.y<=0||l.footprint.x<=0||l.footprint.y<=0||l.blend<=0)
                {
                    errors.Add(l.id+": valores inválidos.");
                    continue;
                }
                if(l.footprint.x+2*l.setback>l.size.x||l.footprint.y+2*l.setback>l.size.y)errors.Add(l.id+": construção excede recuos.");
                if(Outside(l.Rect)||Outside(l.Support.Expanded(l.blend)))errors.Add(l.id+": lote ou transição fora do mapa.");
                if(l.manualHeight&&(l.foundationY<w.originY||l.foundationY>w.originY+w.terrainHeight))errors.Add(l.id+": cota fora do terreno.");
                if(!l.prefab||!PrefabUtility.IsPartOfPrefabAsset(l.prefab))errors.Add(l.id+": atribua um prefab.");
                else
                {
                    var b=ExternalModelNormalizer.BoundsOf(l.prefab);
                    if(l.prefab.transform.localScale!=Vector3.one)errors.Add(l.id+": normalize a escala da raiz.");
                    if(b.size.x>l.footprint.x+.05f||b.size.z>l.footprint.y+.05f)errors.Add(l.id+": modelo excede footprint; normalize antes de aplicar.");
                    if(Mathf.Abs(b.min.y)>.05f)errors.Add(l.id+": base do prefab precisa estar em Y=0.");
                }
                foreach(var r in w.roads)if(l.Rect.Overlaps(r.Rect)||l.Support.Expanded(l.blend).Overlaps(r.Rect.Expanded(2)))errors.Add(l.id+": lote/transição invade a via.");
            }
            for(int i=0;i<w.lots.Count;i++)for(int j=i+1;j<w.lots.Count;j++)if(w.lots[i].Rect.Overlaps(w.lots[j].Rect)||w.lots[i].Support.Expanded(w.lots[i].blend).Overlaps(w.lots[j].Support.Expanded(w.lots[j].blend)))errors.Add("Lotes/transições em conflito: "+w.lots[i].id+" / "+w.lots[j].id);
            foreach(var r in w.roads)if(r.width<=0||Vector2.Distance(r.start,r.end)<1||Outside(r.Rect.Expanded(2))||!Fin(r.startY)||!Fin(r.endY)||r.startY<w.originY||r.endY>w.originY+w.terrainHeight)errors.Add(r.id+": via inválida ou fora dos limites.");
            for(int i=0;i<w.roads.Count;i++)for(int j=i+1;j<w.roads.Count;j++)if(w.roads[i].Rect.Expanded(2).Overlaps(w.roads[j].Rect.Expanded(2)))errors.Add("Interseção de vias ainda não suportada.");
            return errors;
        }
    }
}
