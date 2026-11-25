using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
var group=GameObject.Find("Island023/Moradores060").transform;
var records=new List<object>();
foreach(var animator in group.GetComponentsInChildren<Animator>())
{
    var npc=animator.transform.parent;
    var points=new List<Vector3>();
    foreach(var renderer in animator.GetComponentsInChildren<SkinnedMeshRenderer>())
    {
        var mesh=new Mesh();
        renderer.BakeMesh(mesh,true);
        points.AddRange(mesh.vertices.Select(v=>renderer.transform.TransformPoint(v)));
        UnityEngine.Object.Destroy(mesh);
    }
    var minimum=points.OrderBy(v=>v.y).First();
    var hit=Physics.RaycastAll(minimum+Vector3.up*0.4f,Vector3.down,1f,~0,QueryTriggerInteraction.Ignore)
        .Where(h=>h.normal.y>0.65f&&!h.collider.transform.IsChildOf(group)).OrderBy(h=>h.distance).First();
    records.Add(new{name=npc.name,valid=animator.avatar.isValid,human=animator.isHuman,clip=animator.GetCurrentAnimatorClipInfo(0).Select(c=>c.clip.name).ToArray(),time=animator.GetCurrentAnimatorStateInfo(0).normalizedTime,footClearance=minimum.y-hit.point.y,lowestPoint=new[]{minimum.x,minimum.y,minimum.z},leftHand=animator.GetBoneTransform(HumanBodyBones.LeftHand).position.ToString(),head=animator.GetBoneTransform(HumanBodyBones.Head).position.ToString()});
}
var json=Newtonsoft.Json.JsonConvert.SerializeObject(records,Newtonsoft.Json.Formatting.Indented);
File.WriteAllText("/Users/bruno.duartec/Documents/Projects/Freyja/.utmp/060-moradores-animados-ilha/specs/060-moradores-animados-ilha/evidence/animation-end.json",json);
return json;
