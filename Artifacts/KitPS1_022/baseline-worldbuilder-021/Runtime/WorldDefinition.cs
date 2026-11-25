using System;
using System.Collections.Generic;
using UnityEngine;
namespace Freyja.WorldBuilder
{
    [CreateAssetMenu(menuName="Freyja/World Definition")]  public sealed class WorldDefinition:ScriptableObject
    {
        public string worldId=Guid.NewGuid().ToString("N");
        public int seed=1138;
        public Vector2 size=new Vector2(160,160);
        public float terrainHeight=32,originY=-4;
        public int resolution=257;
        public float amplitude=6,gridStep=1;
        public List<LotDefinition> lots=new List<LotDefinition>();
        public List<RoadDefinition> roads=new List<RoadDefinition>();
        public TerrainLayer grass;
        public Material roadMaterial,foundationMaterial;
    }
    [Serializable] public sealed class LotDefinition
    {
        public string id=Guid.NewGuid().ToString("N");
        public Vector2 center;
        public Vector2 size=new Vector2(12,14),footprint=new Vector2(9,11);
        public float yaw,setback=.5f,blend=2;
        public bool manualHeight;
        public float foundationY;
        public GameObject prefab;
        public OrientedRectXZ Rect=>new OrientedRectXZ(center,size,yaw);
        public OrientedRectXZ Support=>new OrientedRectXZ(center,footprint+Vector2.one*1.5f,yaw);
    }
    [Serializable] public sealed class RoadDefinition
    {
        public string id=Guid.NewGuid().ToString("N");
        public Vector2 start=new Vector2(0,-28),end=new Vector2(0,28);
        public float width=5,startY=.6f,endY=3.5f;
        public OrientedRectXZ Rect=>new OrientedRectXZ((start+end)/2,new Vector2(width,Vector2.Distance(start,end)),Mathf.Atan2(end.x-start.x,end.y-start.y)*Mathf.Rad2Deg);
        public float Height(Vector2 p)=>Mathf.Lerp(startY,endY,Mathf.Clamp01(Vector2.Dot(p-start,end-start)/(end-start).sqrMagnitude));
    }
}
