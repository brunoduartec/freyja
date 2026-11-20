using UnityEngine;
using System.Collections.Generic;
public static class Inspecionar019 {
 public static object Main() {
  float[,] v={
   {22.9f,-.94f,5,6,90},{12.6f,3.74f,5,6,180},{17.2f,-10.3f,5,6,90},{8.2f,-10.75f,5,6,180},
   {1.87f,-17.2f,5,6,0},{-.94f,-9.82f,5,6,180},{-11.3f,-16.55f,5,6,0},{-21.7f,-11.2f,5,6,90},
   {-9,3.9f,5,6,0},{-18.7f,3f,5,6,90},{-12.25f,13.3f,5,6,0},{18.7f,-37.4f,4,5,180},
   {5,1,6,6,180},{-2,5,5,6,0},{-4,-2,6,6,180},{-6,-29,7,8,180},{29,-1,6,6,0}};
  var result=new List<object>();for(int i=0;i<v.GetLength(0);i++){
   float x=v[i,0],z=v[i,1],w=v[i,2],d=v[i,3],a=v[i,4];var q=Quaternion.Euler(0,a,0);var ys=new List<float>();
   foreach(var p in new[]{Vector3.zero,new Vector3(-w/2,0,-d/2),new Vector3(w/2,0,-d/2),new Vector3(-w/2,0,d/2),new Vector3(w/2,0,d/2)}){
    RaycastHit h;var pos=new Vector3(x,80,z)+q*p;if(Physics.Raycast(pos,Vector3.down,out h,120,1<<8))ys.Add(h.point.y);}
   result.Add(new{id=i+1,x,z,w,d,yaw=a,heights=ys});
  }return result;
 }
}
