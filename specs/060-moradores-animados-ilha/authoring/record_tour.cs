using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Media;
using Freyja.Prototype;

public static class MoradoresVideo060Capture
{
    public sealed class Shot
    {
        public Vector3 camera;
        public Vector3 target;
        public string title;
        public float hold;
        public float travel;
        public Shot(Vector3 cameraPosition,Vector3 lookTarget,string label,float holdSeconds,float travelSeconds)
        {
            camera=cameraPosition;target=lookTarget;title=label;hold=holdSeconds;travel=travelSeconds;
        }
    }
    public sealed class Segment
    {
        public Shot from;
        public Shot to;
        public int frames;
        public bool hold;
    }

    private static readonly string output="/Users/bruno.duartec/Documents/Projects/Freyja/Artifacts/Videos/Freyja-Moradores-A1-A2-A3-2026-11-19.mp4";
    private static readonly string evidence="/Users/bruno.duartec/Documents/Projects/Freyja/.utmp/060-moradores-animados-ilha/specs/060-moradores-animados-ilha/evidence";
    private static List<Segment> segments;
    private static List<object> chapters;
    private static Animator[] animators;
    private static Camera camera;
    private static CaisCameraRig cameraRig;
    private static RenderTexture target;
    private static RenderTexture oldTarget;
    private static Texture2D pixels;
    private static GameObject titleRoot;
    private static Text title;
    private static MediaEncoder encoder;
    private static int frame;
    private static int total;
    private static int segmentIndex;
    private static int segmentFrame;
    private static float oldTimeScale;
    private static bool oldRigEnabled;
    private static bool finished;

    public static string Begin()
    {
        if(!EditorApplication.isPlaying)throw new InvalidOperationException("Start Play Mode.");
        var root=GameObject.Find("Island023/Moradores060");
        if(root==null||root.transform.childCount!=10)throw new InvalidOperationException("Expected ten residents.");
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        camera=Camera.main;
        cameraRig=UnityEngine.Object.FindFirstObjectByType<CaisCameraRig>();
        oldRigEnabled=cameraRig.enabled;
        cameraRig.enabled=false;
        oldTimeScale=Time.timeScale;
        Time.timeScale=0;
        var clock=UnityEngine.Object.FindFirstObjectByType<Freyja.Environment.CoastalDayNight>();
        var serialized=new SerializedObject(clock);
        serialized.FindProperty("hour").floatValue=11;
        serialized.FindProperty("advanceClock").boolValue=false;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        animators=UnityEngine.Object.FindObjectsByType<Animator>(FindObjectsInactive.Exclude,FindObjectsSortMode.None)
            .Where(a=>a.enabled&&a.runtimeAnimatorController!=null).ToArray();

        var shots=new[]
        {
            new Shot(new Vector3(0,15,-22),new Vector3(0,7,-5),"",2,0),
            new Shot(new Vector3(-9.4f,8.4f,-4.3f),new Vector3(-6.4f,7.9f,-2.9f),"Elisa Neves · Venda",4,3),
            new Shot(new Vector3(5.8f,7.3f,-14.6f),new Vector3(8,6.8f,-12),"Tomás Faria · Largo",4,4),
            new Shot(new Vector3(8.2f,7.6f,-8.3f),new Vector3(11,6.95f,-10.6f),"Amélia Faria · Salão comunitário",4,3),
            new Shot(new Vector3(17.1f,7.6f,-10),new Vector3(15,7.05f,-12.8f),"Luana Batista · Salão comunitário",4,3),
            new Shot(new Vector3(16,7.4f,-14.9f),new Vector3(17.5f,6.95f,-11.5f),"Sara Faria · Salão comunitário",4,3),
            new Shot(new Vector3(13,15,-1),new Vector3(14,9,3),"",0,3),
            new Shot(new Vector3(7.7f,9.3f,1.6f),new Vector3(11,8.9f,2.8f),"Ivo Batista · Acesso à A3",5,3),
            new Shot(new Vector3(15.8f,9.4f,1.1f),new Vector3(18.3f,9,3.2f),"Helena Batista · Casa A3",5,3),
            new Shot(new Vector3(3,21,13),new Vector3(3,11,17),"",0,4),
            new Shot(new Vector3(-5,17,16.8f),new Vector3(-8.5f,14.6f,21.5f),"",2,4),
            new Shot(new Vector3(-7.3f,15.2f,18.6f),new Vector3(-10,14.8f,20.2f),"Mauro Neves · Área da A1",4,3),
            new Shot(new Vector3(-10.2f,14.85f,18.9f),new Vector3(-7,14.35f,20),"Duda Neves · Área da A1",4,3),
            new Shot(new Vector3(-7.8f,15.6f,19.4f),new Vector3(-8.8f,15,22.2f),"Joaquim Neves · Área da A1",5,3),
            new Shot(new Vector3(-3.5f,20,16),new Vector3(-10,14.8f,22),"",2,4)
        };
        segments=new List<Segment>();
        chapters=new List<object>();
        total=0;
        for(int i=0;i<shots.Length;i++)
        {
            if(i>0&&shots[i].travel>0)
            {
                int length=Mathf.RoundToInt(shots[i].travel*30);
                segments.Add(new Segment{from=shots[i-1],to=shots[i],frames=length,hold=false});
                total+=length;
            }
            if(shots[i].hold>0)
            {
                int length=Mathf.RoundToInt(shots[i].hold*30);
                if(!string.IsNullOrEmpty(shots[i].title))
                    chapters.Add(new{title=shots[i].title,start=total/30f,duration=shots[i].hold});
                segments.Add(new Segment{from=shots[i],to=shots[i],frames=length,hold=true});
                total+=length;
            }
        }

        oldTarget=camera.targetTexture;
        target=new RenderTexture(1280,720,24,RenderTextureFormat.ARGB32);
        target.Create();
        camera.targetTexture=target;
        pixels=new Texture2D(1280,720,TextureFormat.RGBA32,false);
        titleRoot=new GameObject("MoradoresVideo060Caption",typeof(RectTransform),typeof(Canvas));
        var canvas=titleRoot.GetComponent<Canvas>();
        canvas.renderMode=RenderMode.ScreenSpaceCamera;
        canvas.worldCamera=camera;
        canvas.planeDistance=0.5f;
        canvas.overrideSorting=true;
        canvas.sortingOrder=1000;
        var textObject=new GameObject("Name",typeof(RectTransform),typeof(CanvasRenderer),typeof(Text),typeof(Shadow));
        textObject.transform.SetParent(titleRoot.transform,false);
        title=textObject.GetComponent<Text>();
        title.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        title.fontSize=25;
        title.color=Color.white;
        title.alignment=TextAnchor.LowerLeft;
        var rect=textObject.GetComponent<RectTransform>();
        rect.anchorMin=Vector2.zero;
        rect.anchorMax=Vector2.zero;
        rect.pivot=Vector2.zero;
        rect.anchoredPosition=new Vector2(32,24);
        rect.sizeDelta=new Vector2(1100,52);
        var shadow=textObject.GetComponent<Shadow>();
        shadow.effectColor=new Color(0,0,0,0.9f);
        shadow.effectDistance=new Vector2(2,-2);
        encoder=new MediaEncoder(output,new VideoTrackAttributes
        {
            frameRate=new MediaRational(30),
            width=1280,
            height=720,
            includeAlpha=false,
            bitRateMode=VideoBitrateMode.High
        });
        frame=0;segmentIndex=0;segmentFrame=0;finished=false;
        return Status();
    }

    public static string Step(int count)
    {
        if(finished)return Status();
        try
        {
            int until=Math.Min(frame+count,total);
            while(frame<until)
            {
                var segment=segments[segmentIndex];
                float fraction=segment.frames<2?1:segmentFrame/(float)(segment.frames-1);
                float blend=Mathf.SmoothStep(0,1,fraction);
                Vector3 eye=Vector3.Lerp(segment.from.camera,segment.to.camera,blend);
                if(!segment.hold)eye.y+=Mathf.Sin(fraction*Mathf.PI)*0.9f;
                Vector3 focus=Vector3.Lerp(segment.from.target,segment.to.target,blend);
                camera.transform.SetPositionAndRotation(eye,Quaternion.LookRotation(focus-eye,Vector3.up));
                title.text=segment.hold?segment.to.title:"";
                foreach(var animator in animators)animator.Update(1f/30f);
                Canvas.ForceUpdateCanvases();
                camera.Render();
                var previous=RenderTexture.active;
                RenderTexture.active=target;
                pixels.ReadPixels(new Rect(0,0,1280,720),0,0,false);
                pixels.Apply(false,false);
                RenderTexture.active=previous;
                if(!encoder.AddFrame(pixels))throw new InvalidOperationException("Video encoder rejected frame "+frame);
                if(segment.hold&&!string.IsNullOrEmpty(segment.to.title)&&segmentFrame==segment.frames/2)
                    File.WriteAllBytes(Path.Combine(evidence,"video-frame-"+frame.ToString("D4")+".png"),pixels.EncodeToPNG());
                frame++;
                segmentFrame++;
                if(segmentFrame>=segment.frames)
                {
                    segmentIndex++;
                    segmentFrame=0;
                }
            }
            if(frame>=total)Finish();
            return Status();
        }
        catch
        {
            Finish();
            throw;
        }
    }

    public static string Status()
    {
        return Newtonsoft.Json.JsonConvert.SerializeObject(new{frame,total,seconds=frame/30f,duration=total/30f,finished,path=output});
    }

    public static void Finish()
    {
        if(finished)return;
        if(encoder!=null){encoder.Dispose();encoder=null;}
        camera.targetTexture=oldTarget;
        cameraRig.enabled=oldRigEnabled;
        Time.timeScale=oldTimeScale;
        if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}
        if(pixels!=null)UnityEngine.Object.Destroy(pixels);
        if(titleRoot!=null)UnityEngine.Object.Destroy(titleRoot);
        finished=true;
        File.WriteAllText(Path.Combine(evidence,"video-chapters.json"),Newtonsoft.Json.JsonConvert.SerializeObject(chapters,Newtonsoft.Json.Formatting.Indented));
        File.WriteAllText(Path.Combine(evidence,"video-recording.json"),Status());
    }
}
return MoradoresVideo060Capture.Begin();
