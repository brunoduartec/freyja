using System;
using System.Linq;
using UnityEngine;
namespace Freyja.WorldBuilder
{
    public static class GridMath
    {
        public static float Snap(float value,float step)
        {
            if(step<=0)throw new ArgumentOutOfRangeException(nameof(step));
            return (float)Math.Round(value/step,MidpointRounding.AwayFromZero)*step;
        }
        public static Vector2 Snap(Vector2 p,float s)=>new Vector2(Snap(p.x,s),Snap(p.y,s));
    }
    public static class TerrainSpace
    {
        public static Vector2 UV(WorldDefinition w,Vector2 p)=>new Vector2(p.x/w.size.x+.5f,p.y/w.size.y+.5f);
        public static Vector2 World(WorldDefinition w,int x,int z)=>new Vector2((float)x/(w.resolution-1)*w.size.x-w.size.x/2,(float)z/(w.resolution-1)*w.size.y-w.size.y/2);
        public static float Normalized(WorldDefinition w,float y)=>(y-w.originY)/w.terrainHeight;
        public static float WorldY(WorldDefinition w,float n)=>w.originY+n*w.terrainHeight;
    }
    public struct OrientedRectXZ
    {
        public Vector2 center,size;
        public float yaw;
        public OrientedRectXZ(Vector2 c,Vector2 s,float y)
        {
            center=c;
            size=s;
            yaw=y;
        }
        public Vector2 X=>new Vector2(Mathf.Cos(yaw*Mathf.Deg2Rad),-Mathf.Sin(yaw*Mathf.Deg2Rad));
        public Vector2 Z=>new Vector2(Mathf.Sin(yaw*Mathf.Deg2Rad),Mathf.Cos(yaw*Mathf.Deg2Rad));
        public Vector2 Local(Vector2 p)=>new Vector2(Vector2.Dot(p-center,X),Vector2.Dot(p-center,Z));
        public Vector2 LocalToWorld(Vector2 p)=>center+X*p.x+Z*p.y;
        public Vector2[] Corners()=>new[]
        {
            LocalToWorld(size/2),LocalToWorld(new Vector2(-size.x,size.y)/2),LocalToWorld(-size/2),LocalToWorld(new Vector2(size.x,-size.y)/2)
        };
        public float Distance(Vector2 p)
        {
            var q=Local(p);
            return new Vector2(Mathf.Max(0,Mathf.Abs(q.x)-size.x/2),Mathf.Max(0,Mathf.Abs(q.y)-size.y/2)).magnitude;
        }
        public OrientedRectXZ Expanded(float m)=>new OrientedRectXZ(center,size+Vector2.one*2*m,yaw);
        public bool Overlaps(OrientedRectXZ b)
        {
            foreach(var a in new[]
            {
                X,Z,b.X,b.Z
            }
            )
            {
                float r=Mathf.Abs(Vector2.Dot(X,a))*size.x/2+Mathf.Abs(Vector2.Dot(Z,a))*size.y/2+Mathf.Abs(Vector2.Dot(b.X,a))*b.size.x/2+Mathf.Abs(Vector2.Dot(b.Z,a))*b.size.y/2;
                if(Mathf.Abs(Vector2.Dot(b.center-center,a))>=r-.001f)return false;
            }
            return true;
        }
    }
    public static class TerrainMath
    {
        public static float BaseHeight(WorldDefinition w,Vector2 p)
        {
            float hill=Mathf.Exp(-((p.x-22)*(p.x-22)+(p.y-20)*(p.y-20))/1500);
            float noise=Mathf.PerlinNoise(p.x*.035f+100+w.seed*.13f,p.y*.035f+100+w.seed*.07f);
            return w.amplitude*(hill*.7f+noise*.3f);
        }
        public static float Foundation(WorldDefinition w,LotDefinition l)
        {
            if(l.manualHeight)return l.foundationY;
            var samples=new float[25];
            for(int z=0;z<5;z++)for(int x=0;x<5;x++)samples[z*5+x]=BaseHeight(w,l.Support.LocalToWorld(new Vector2((x/4f-.5f)*l.footprint.x,(z/4f-.5f)*l.footprint.y)));
            Array.Sort(samples);
            return samples[12];
        }
        public static float[,] Generate(WorldDefinition w)
        {
            int n=w.resolution;
            var h=new float[n,n];
            var lots=w.lots.OrderBy(l=>l.id,StringComparer.Ordinal).ToArray();
            var y=lots.Select(l=>Foundation(w,l)).ToArray();
            for(int z=0;z<n;z++)for(int x=0;x<n;x++)
            {
                var p=TerrainSpace.World(w,x,z);
                float a=BaseHeight(w,p);
                foreach(var r in w.roads)
                {
                    float d=r.Rect.Distance(p);
                    a=Mathf.Lerp(r.Height(p),a,Mathf.SmoothStep(0,1,Mathf.Clamp01(d/2)));
                }
                for(int i=0;i<lots.Length;i++)
                {
                    float d=lots[i].Support.Distance(p);
                    if(d<=lots[i].blend)a=Mathf.Lerp(y[i],a,Mathf.SmoothStep(0,1,Mathf.Clamp01(d/Mathf.Max(.001f,lots[i].blend))));
                }
                h[z,x]=TerrainSpace.Normalized(w,a);
            }
            return h;
        }
        public static float Sample(WorldDefinition w,float[,] h,Vector2 p)
        {
            var uv=TerrainSpace.UV(w,p)*(w.resolution-1);
            int x=Mathf.Clamp(Mathf.FloorToInt(uv.x),0,w.resolution-2),z=Mathf.Clamp(Mathf.FloorToInt(uv.y),0,w.resolution-2);
            return TerrainSpace.WorldY(w,Mathf.Lerp(Mathf.Lerp(h[z,x],h[z,x+1],uv.x-x),Mathf.Lerp(h[z+1,x],h[z+1,x+1],uv.x-x),uv.y-z));
        }
    }
}
