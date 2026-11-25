using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
namespace Freyja.WorldBuilder.Editor
{
    public static class WorldReport
    {
        [Serializable] public class Check
        {
            public string id,status,details;
            public Check(string i,string s,string d)
            {
                id=i;
                status=s;
                details=d;
            }
        }
        [Serializable] public class Shot
        {
            public string file;
            public Vector3 position,euler;
            public float fov,orthographicSize;
            public int width=1280,height=720;
        }
        [Serializable] public class Report
        {
            public string worldId,unityVersion,scene,sceneHash;
            public int seed,lots,externalGenerationJobs;
            public Vector2 mapMeters;
            public double milliseconds;
            public string buildStatus="not_run",testStatus="not_run";
            public List<Check> checks=new List<Check>();
            public List<Shot> screenshots=new List<Shot>();
        }
        public static string Capture(WorldDefinition w)
        {
            var timer=System.Diagnostics.Stopwatch.StartNew();
            var root=WorldGenerator.FindRoot(w);
            if(!root)throw new InvalidOperationException("Aplique o mundo primeiro.");
            string folder=Path.GetFullPath("../Artifacts/WorldBuilder/"+DateTime.Now.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(folder);
            var r=new Report
            {
                worldId=w.worldId,unityVersion=Application.unityVersion,scene=SceneManager.GetActiveScene().path,seed=w.seed,lots=w.lots.Count,mapMeters=w.size
            };
            r.sceneHash=AssetDatabase.GetAssetDependencyHash(r.scene).ToString();
            var errors=WorldValidation.Issues(w);
            r.checks.Add(new Check("layout",errors.Count==0?"passed":"failed",string.Join("; ",errors)));
            var terrain=root.GetComponentInChildren<Terrain>();
            float max=0;
            foreach(var l in w.lots)for(int z=0;z<4;z++)for(int x=0;x<4;x++)
            {
                var q=l.Rect.LocalToWorld(new Vector2((x/3f-.5f)*l.footprint.x,(z/3f-.5f)*l.footprint.y));
                var uv=TerrainSpace.UV(w,q);
                float y=terrain.transform.position.y+terrain.terrainData.GetInterpolatedHeight(uv.x,uv.y);
                max=Mathf.Max(max,Mathf.Abs(y-TerrainMath.Foundation(w,l)));
            }
            r.checks.Add(new Check("foundation-16-samples",max<=.05f?"passed":"failed","maxDelta="+max));
            var houses=root.GetComponentsInChildren<WorldInstanceId>(true).Where(t=>t.kind=="house").ToArray();
            r.checks.Add(new Check("prefabs",houses.Length==w.lots.Count&&houses.All(t=>PrefabUtility.IsPartOfPrefabInstance(t)&&t.transform.localScale==Vector3.one)?"passed":"failed","houses="+houses.Length));
            if(SystemInfo.graphicsDeviceType!=UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                foreach(var shot in new[]
                {
                    new Shot
                    {
                        file="top_down.png",position=new Vector3(0,95,0),euler=new Vector3(90,0,0),orthographicSize=48
                    }
                    ,new Shot
                    {
                        file="isometric_overview.png",position=new Vector3(65,55,-65),orthographicSize=33
                    }
                    ,new Shot
                    {
                        file="street_level.png",position=new Vector3(2,3,-28),fov=65
                    }
                }
                )
                {
                    CaptureShot(shot,folder);
                    r.screenshots.Add(shot);
                }
                r.checks.Add(new Check("visual","needs_review","Capturas reais; sem baseline automático."));
            }
            else r.checks.Add(new Check("visual","not_run","Graphics device ausente."));
            timer.Stop();
            r.milliseconds=timer.Elapsed.TotalMilliseconds;
            File.WriteAllText(folder+"/report.json",JsonUtility.ToJson(r,true));
            File.WriteAllText(folder+"/report.md","# World Builder\n\n"+string.Join("\n",r.checks.Select(c=>"- "+c.id+": "+c.status+" — "+c.details))+"\n\nExternal 3D generation jobs: 0\nBuild: not_run. Consulte os XMLs do Test Runner separadamente.\n");
            return folder;
        }
        static void CaptureShot(Shot shot,string folder)
        {
            var go=new GameObject("WB capture")
            {
                hideFlags=HideFlags.HideAndDontSave
            };
            var cam=go.AddComponent<Camera>();
            cam.transform.position=shot.position;
            if(shot.file=="top_down.png")cam.transform.eulerAngles=shot.euler;
            else cam.transform.LookAt(shot.file=="street_level.png"?new Vector3(0,3,12):new Vector3(0,2,0));
            shot.euler=cam.transform.eulerAngles;
            cam.orthographic=shot.orthographicSize>0;
            cam.orthographicSize=shot.orthographicSize;
            cam.fieldOfView=shot.fov>0?shot.fov:60;
            cam.farClipPlane=400;
            cam.clearFlags=CameraClearFlags.SolidColor;
            cam.backgroundColor=new Color(.53f,.69f,.75f);
            var rt=new RenderTexture(1280,720,24);
            var prev=RenderTexture.active;
            Texture2D tex=null;
            try
            {
                cam.targetTexture=rt;
                cam.Render();
                RenderTexture.active=rt;
                tex=new Texture2D(1280,720,TextureFormat.RGB24,false);
                tex.ReadPixels(new Rect(0,0,1280,720),0,0);
                tex.Apply();
                File.WriteAllBytes(folder+"/"+shot.file,tex.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active=prev;
                cam.targetTexture=null;
                if(tex)UnityEngine.Object.DestroyImmediate(tex);
                rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);
                UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}
