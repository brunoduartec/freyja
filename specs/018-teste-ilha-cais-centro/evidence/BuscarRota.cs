using System;
using System.Collections.Generic;
using UnityEngine;

public static class BuscarRota018
{
    public static string Main()
    {
        const int width=81, depth=51; const float step=1f, minX=-5f, minZ=-25f;
        float[,] heights=new float[width,depth]; bool[,] open=new bool[width,depth];
        for(int x=0;x<width;x++) for(int z=0;z<depth;z++) {
            RaycastHit hit; float px=minX+x*step, pz=minZ+z*step;
            if(!Physics.Raycast(new Vector3(px,80,pz),Vector3.down,out hit,120,1<<8) || hit.normal.y<0.72f) continue;
            heights[x,z]=hit.point.y;
            open[x,z]=Physics.OverlapCapsule(hit.point+Vector3.up*.45f,hit.point+Vector3.up*1.5f,.25f,1<<8).Length==0;
        }
        int sx=58,sz=30,tx=6,tz=24; open[sx,sz]=true;open[tx,tz]=true;
        var q=new Queue<int>(); var prev=new int[width*depth];Array.Fill(prev,-1);
        int start=sx+sz*width,end=tx+tz*width;prev[start]=start;q.Enqueue(start);
        int[] dx={1,-1,0,0},dz={0,0,1,-1};
        while(q.Count>0 && prev[end]<0){int cur=q.Dequeue(),x=cur%width,z=cur/width;
            for(int k=0;k<4;k++){int nx=x+dx[k],nz=z+dz[k];if(nx<0||nx>=width||nz<0||nz>=depth||!open[nx,nz])continue;
                int n=nx+nz*width;if(prev[n]>=0||Math.Abs(heights[nx,nz]-heights[x,z])>0.45f)continue;prev[n]=cur;q.Enqueue(n);}}
        if(prev[end]<0)return "Nenhuma rota encontrada na amostragem; não prova ausência de percurso.";
        var p=new List<string>();for(int v=end;;v=prev[v]) {p.Add($"({minX+(v%width)*step:F1}, {minZ+(v/width)*step:F1})");if(v==start)break;}p.Reverse();return string.Join(" -> ",p);
    }
}
