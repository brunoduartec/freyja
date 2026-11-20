"""Mede os lotes na captura ortográfica Unity (110 m / 2048 px).

As regiões selecionadas isolam cada marcação da rua e das pedras. A cor
identifica o solo exposto; um retângulo orientado ajusta sua extensão.
Não modifica a textura ou a malha do terreno.
"""
from pathlib import Path
from collections import deque
import json, math
import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont

OUT = Path(__file__).parent
im = Image.open(OUT / 'terreno-planta.png').convert('RGB')
rgb = np.array(im).astype(int)
soil = (rgb[:,:,0] > 230) & (rgb[:,:,0]-rgb[:,:,1] > 40) & (rgb[:,:,2] > 70) & (rgb[:,:,1]-rgb[:,:,2] > 45)
# Contornos de seleção: delimitam o lote, sem incorporar caminhos adjacentes.
regions = [
 ('Casa_08', [(500,540),(570,521),(635,542),(677,597),(679,650),(612,665),(536,633),(495,588)]),
 ('Casa_05', [(413,673),(484,690),(555,702),(629,714),(627,762),(557,795),(477,780),(435,737)]),
 ('Sala_Comunitaria',[(724,682),(794,685),(848,707),(884,753),(892,813),(843,834),(776,815),(733,782),(713,736)]),
 ('Casa_07',[(591,908),(625,868),(672,875),(718,918),(753,972),(758,1029),(730,1050),(675,1042),(632,1005),(601,964)]),
 ('Loja',[(799,861),(855,849),(895,890),(915,947),(927,1001),(894,1017),(832,1005),(801,975)]),
 ('Casa_01',[(1162,847),(1217,847),(1281,884),(1323,930),(1365,986),(1368,1041),(1307,1032),(1250,1004),(1206,962),(1178,919)]),
 ('Galpao_Cais',[(1374,955),(1437,974),(1490,1009),(1519,1057),(1521,1102),(1481,1120),(1430,1106),(1392,1072),(1365,1026)]),
 ('Casa_04',[(878,1145),(927,1129),(985,1138),(1032,1165),(1061,1197),(1083,1260),(1052,1276),(994,1265),(936,1247),(897,1220),(874,1187)]),
 ('Venda',[(1110,1128),(1167,1129),(1210,1161),(1243,1208),(1254,1255),(1216,1260),(1164,1247),(1120,1227),(1102,1184)]),
 ('Casa_02',[(1257,1129),(1328,1143),(1383,1177),(1424,1232),(1448,1278),(1435,1316),(1378,1304),(1316,1276),(1282,1243),(1256,1190)]),
 ('Casa_06',[(934,1291),(987,1277),(1042,1297),(1078,1336),(1107,1380),(1092,1411),(1027,1404),(970,1376),(938,1341)]),
 ('Igreja',[(695,1241),(758,1240),(818,1272),(871,1309),(907,1349),(913,1387),(877,1404),(816,1389),(759,1363),(719,1322),(695,1285)]),
 ('Casa_10',[(502,1175),(556,1164),(613,1182),(661,1213),(687,1256),(670,1290),(607,1274),(558,1251),(520,1223)]),
]

overlay = im.copy()
draw = ImageDraw.Draw(overlay)
try: font = ImageFont.truetype('/System/Library/Fonts/Supplemental/Arial Bold.ttf', 22)
except OSError: font = ImageFont.load_default()
results=[]
for index,(name, poly) in enumerate(regions, 1):
    selected=Image.new('L', im.size)
    ImageDraw.Draw(selected).polygon(poly, fill=255)
    allowed=(np.array(selected)>0) & soil
    # Small gaps in rendered texels do not define a lot boundary.
    allowed=np.array(Image.fromarray(allowed.astype('uint8')*255).filter(ImageFilter.MaxFilter(3)).filter(ImageFilter.MinFilter(3)))>0
    x0=min(x for x,y in poly)-12; y0=min(y for x,y in poly)-12
    x1=max(x for x,y in poly)+12; y1=max(y for x,y in poly)+12
    crop=allowed[y0:y1,x0:x1]
    # Keep the largest connected component of the selected patch.
    visited=np.zeros(crop.shape, bool); largest=[]
    for yy,xx in zip(*np.where(crop)):
        if visited[yy,xx]: continue
        visited[yy,xx]=True; q=deque([(yy,xx)]); component=[]
        while q:
            y,x=q.popleft();component.append((y,x))
            for dy,dx in ((1,0),(-1,0),(0,1),(0,-1)):
                ny,nx=y+dy,x+dx
                if 0<=ny<crop.shape[0] and 0<=nx<crop.shape[1] and crop[ny,nx] and not visited[ny,nx]:
                    visited[ny,nx]=True;q.append((ny,nx))
        if len(component)>len(largest):largest=component
    mask=np.zeros_like(crop); yy,xx=np.array(largest).T;mask[yy,xx]=True
    # Fit the extent of the visible pad rather than a rectangle far inside it.
    # Quantiles ignore the outermost 1% of texels on each side (soft/noisy edge).
    py,px=np.where(mask)
    points=np.column_stack((px+x0,py+y0))
    origin=points.mean(axis=0)
    delta=points-origin
    best=None
    for angle in range(0,90):
        theta=math.radians(angle);c=math.cos(theta);s=math.sin(theta)
        local=delta @ np.array([[c,-s],[s,c]])
        low,high=np.quantile(local,[.01,.99],axis=0)
        width,height=high-low
        area=width*height
        if best is None or area<best[0]:best=(area,*low,width,height,angle)
    area,left,top,width,height,angle=best
    theta=math.radians(angle);c=math.cos(theta);s=math.sin(theta)
    corners=[]
    for x,y in [(left,top),(left+width,top),(left+width,top+height),(left,top+height)]:
        corners.append((origin[0]+c*x-s*y,origin[1]+s*x+c*y))
    cx=sum(x for x,y in corners)/4;cy=sum(y for x,y in corners)/4
    scale=110/2048
    result=dict(id=name,pad=index,pixel_center=[cx,cy],pixel_corners=corners,
                x=cx*scale-55,z=55-cy*scale,width=width*scale,depth=height*scale,
                yaw=angle,soil_area_m2=len(largest)*scale**2,box_area_m2=width*height*scale**2,
                fit_fraction=len(largest)/area)
    results.append(result)
    # Cyan = measured patch; red = fitted base rectangle.
    edge=mask & ~(np.roll(mask,1,0)&np.roll(mask,-1,0)&np.roll(mask,1,1)&np.roll(mask,-1,1))
    for y,x in zip(*np.where(edge)):draw.point((int(x+x0),int(y+y0)),fill=(0,255,255))
    draw.line(corners+[corners[0]],fill=(255,40,30),width=4)
    label=f'{index:02} {name}'
    box=draw.textbbox((cx,cy),label,font=font,anchor='mm')
    draw.rectangle((box[0]-3,box[1]-3,box[2]+3,box[3]+3),fill=(255,255,235))
    draw.text((cx,cy),label,font=font,fill=(0,0,0),anchor='mm')
    print(f'{index:02} {name}: {result["width"]:.2f} x {result["depth"]:.2f} m, Y={angle}°, area={result["fit_fraction"]:.0%}')
(OUT/'lotes-medidos.json').write_text(json.dumps(results,indent=2))
overlay.save(OUT/'lotes-medidos.png')
