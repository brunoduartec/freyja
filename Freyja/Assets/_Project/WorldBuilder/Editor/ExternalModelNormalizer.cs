using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
namespace Freyja.WorldBuilder.Editor
{
    public static class ExternalModelNormalizer
    {
        public static Bounds BoundsOf(GameObject root)
        {
            bool first=true;
            var b=new Bounds();
            foreach(var r in root.GetComponentsInChildren<Renderer>(true))
            {
                var local=r.localBounds;
                for(int i=0;i<8;i++)
                {
                    var q=local.center+Vector3.Scale(local.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));
                    q=root.transform.InverseTransformPoint(r.transform.TransformPoint(q));
                    if(first)
                    {
                        b=new Bounds(q,Vector3.zero);
                        first=false;
                    }
                    else b.Encapsulate(q);
                }
            }
            return b;
        }
        public static AssetDescriptor Normalize(GameObject source,Vector2 target,Vector3 orientation,string output,bool collider=true)
        {
            if(!source||!PrefabUtility.IsPartOfPrefabAsset(source)||target.x<=0||target.y<=0||float.IsNaN(target.x)||float.IsInfinity(target.x)||float.IsNaN(target.y)||float.IsInfinity(target.y))throw new ArgumentException("Selecione modelo/prefab local e dimensões válidas.");
            if(output==AssetDatabase.GetAssetPath(source)||!output.StartsWith("Assets/")||!output.EndsWith(".prefab"))throw new ArgumentException("Salve um novo wrapper dentro de Assets, separado da fonte.");
            var root=new GameObject("Normalized_"+source.name);
            try
            {
                var visual=(GameObject)PrefabUtility.InstantiatePrefab(source,root.transform);
                visual.transform.localRotation=Quaternion.Euler(orientation);
                visual.transform.localPosition=Vector3.zero;
                var b=BoundsOf(root);
                if(b.size.x<.0001f||b.size.z<.0001f||b.size.y<.0001f)throw new ArgumentException("Modelo sem volume mensurável.");
                float scale=Mathf.Min(target.x/b.size.x,target.y/b.size.z);
                visual.transform.localScale*=scale;
                visual.transform.localPosition=-new Vector3(b.center.x,b.min.y,b.center.z)*scale;
                var final=BoundsOf(root);
                if(collider)
                {
                    var c=root.AddComponent<BoxCollider>();
                    c.center=final.center;
                    c.size=final.size;
                    foreach(var existing in visual.GetComponentsInChildren<Collider>(true))existing.enabled=false;
                }
                var prefab=PrefabUtility.SaveAsPrefabAsset(root,output);
                string descriptorPath=output.Replace(".prefab",".asset");
                var d=AssetDatabase.LoadAssetAtPath<AssetDescriptor>(descriptorPath);
                if(!d)
                {
                    d=ScriptableObject.CreateInstance<AssetDescriptor>();
                    AssetDatabase.CreateAsset(d,descriptorPath);
                }
                d.sourceModel=source;
                d.normalizedPrefab=prefab;
                d.targetFootprint=target;
                d.orientation=orientation;
                d.sourceBounds=b;
                d.finalBounds=final;
                d.uniformScale=scale;
                var warnings=new System.Collections.Generic.List<string>();
                if(Mathf.Abs(b.size.x/b.size.z-target.x/target.y)>.4f)warnings.Add("Proporção diferente do lote; haverá espaço livre.");
                if(final.size.y<1||final.size.y>20)warnings.Add("Altura incomum para uma casa; revise categoria e escala.");
                if(source.GetComponentsInChildren<Renderer>(true).Any(r=>r.sharedMaterials.Any(m=>!m||!m.shader||!m.shader.isSupported)))warnings.Add("Material ausente ou incompatível.");
                warnings.Add("Confira visualmente frente e eventual piso/base embutida; não são inferidos automaticamente.");
                d.warnings=warnings.ToArray();
                EditorUtility.SetDirty(d);
                AssetDatabase.SaveAssets();
                return d;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }
}
