"""Export readable UV island silhouettes without 580k internal triangle edges."""
from pathlib import Path
import numpy as np
from PIL import Image,ImageDraw

OUT=Path(__file__).resolve().parents[1]
d=np.load(OUT/'scripts/uv_geometry.npz')
uv=d['uv'];edges=d['edges'].reshape(-1)
starts=uv.reshape(-1,2)
ends=np.roll(uv,-1,axis=1).reshape(-1,2)
order=np.argsort(edges,kind='stable')
sorted_edges=edges[order]
first=np.r_[0,np.flatnonzero(np.diff(sorted_edges))+1]
counts=np.diff(np.r_[first,len(edges)])
seam=np.zeros(len(edges),bool)
singles=order[first[counts==1]];seam[singles]=True
p=first[counts==2];a=order[p];b=order[p+1]
same=(np.max(abs(starts[a]-ends[b]),axis=1)<1e-7)&(np.max(abs(ends[a]-starts[b]),axis=1)<1e-7)
seam[a[~same]]=True;seam[b[~same]]=True
for p,c in zip(first[counts>2],counts[counts>2]):seam[order[p:p+c]]=True
size=2048
im=Image.new('RGB',(size,size),(30,38,44));draw=ImageDraw.Draw(im)
pixels=uv.copy();pixels[:,:,0]=16+pixels[:,:,0]*(size-32);pixels[:,:,1]=16+(1-pixels[:,:,1])*(size-32)
for tri in pixels:draw.polygon([tuple(v) for v in tri],fill=(158,185,159))
for a,b in zip(starts[seam],ends[seam]):
    xy=[(16+float(v[0])*(size-32),16+(1-float(v[1]))*(size-32)) for v in [a,b]]
    draw.line(xy,fill=(43,74,67),width=1)
im.save(OUT/'previews/UV_Layout.png')
print({'boundary_loop_edges':int(seam.sum()),'layout_size':size})
