using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Freyja.Prototype;
using Newtonsoft.Json.Linq;

var feature="/Users/bruno.duartec/Documents/Projects/Freyja/.utmp/060-moradores-animados-ilha/specs/060-moradores-animados-ilha";
var routes=JArray.Parse(File.ReadAllText(feature+"/authoring/routes-retry.json"));
var p=UnityEngine.Object.FindFirstObjectByType<CaisPlayerController>();
var group=GameObject.Find("Island023/Moradores060").transform;
var previousControl=p.ControlEnabled;
var results=new List<object>();
foreach(var route in routes)
{
    var points=((JArray)route["points"]).Select(a=>new Vector3((float)a[0],(float)a[1],(float)a[2])).ToArray();
    foreach(bool reverse in new[]{false,true})
    {
        var sequence=reverse?points.Reverse().ToArray():points;
        var initial=sequence[0];
        var floor=Physics.RaycastAll(initial+Vector3.up*2f,Vector3.down,8f,~0,QueryTriggerInteraction.Ignore)
            .Where(h=>h.normal.y>.65f&&!h.collider.transform.IsChildOf(group)).OrderBy(h=>Mathf.Abs(h.point.y-initial.y)).FirstOrDefault();
        if(floor.collider!=null)initial=floor.point+Vector3.up*.04f;
        p.ResetTo(initial,Quaternion.identity);
        p.ControlEnabled=true;
        Physics.SyncTransforms();
        int reached=0,totalFrames=0,stuckFrames=0;
        float maxStep=0;
        float maxVertical=0;
        float traveled=0;
        string failure=null;
        foreach(var goal in sequence)
        {
            int limit=1800;
            for(int tick=0;tick<limit;tick++)
            {
                var delta=Vector3.ProjectOnPlane(goal-p.transform.position,Vector3.up);
                if(delta.magnitude<.28f){if(Mathf.Abs(p.transform.position.y-goal.y)>.8f){failure="unexpected-ground-height";break;} reached++;break;}
                var forward=Vector3.ProjectOnPlane(p.CameraTransform.forward,Vector3.up).normalized;
                var right=Vector3.ProjectOnPlane(p.CameraTransform.right,Vector3.up).normalized;
                var direction=delta.normalized;
                p.SetMoveInput(new Vector2(Vector3.Dot(direction,right),Vector3.Dot(direction,forward)));
                var previous=p.transform.position;
                p.Tick(1f/60f);
                Physics.SyncTransforms();
                var moved=p.transform.position-previous;
                var planar=Vector3.ProjectOnPlane(moved,Vector3.up).magnitude;
                traveled+=planar;
                maxStep=Mathf.Max(maxStep,planar);
                maxVertical=Mathf.Max(maxVertical,Mathf.Abs(moved.y));
                totalFrames++;
                stuckFrames=planar<.001f?stuckFrames+1:0;
                if(stuckFrames>120){failure="movement-blocked";break;}
                if(tick==limit-1)failure="waypoint-timeout";
            }
            if(failure!=null)break;
        }
        p.SetMoveInput(Vector2.zero);
        results.Add(new{name=(string)route["name"],reverse,success=failure==null&&reached==sequence.Length,reached,total=sequence.Length,frames=totalFrames,distance=traveled,maxPlanarStep=maxStep,maxVerticalStep=maxVertical,end=new[]{p.transform.position.x,p.transform.position.y,p.transform.position.z},failure});
    }
}
p.ControlEnabled=previousControl;
var json=Newtonsoft.Json.JsonConvert.SerializeObject(results,Newtonsoft.Json.Formatting.Indented);
File.WriteAllText(feature+"/evidence/circulation-retry.json",json);
return json;
