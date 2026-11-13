"""Deterministic pixel material sources and hand-positioned terrain masks.

Mask coordinates refer to Inspection_Top.png (1200px, ortho width 1.08).
R = paths/plaza, G = beach, B = exposed coastal rock.
"""
from pathlib import Path
import json
import numpy as np
from PIL import Image, ImageDraw, ImageFilter

OUT = Path(__file__).resolve().parents[1]
TEX = OUT / 'textures'
TEX.mkdir(exist_ok=True)
rng = np.random.default_rng(7007)


def field(size, grid):
    a = rng.integers(0, 256, (grid, grid), dtype=np.uint8)
    return np.asarray(Image.fromarray(a).resize((size, size), Image.Resampling.BILINEAR), dtype=float) / 255 - .5


def base(size, color, amplitude=14):
    f = field(size, 12) * amplitude + field(size, 48) * amplitude * .35
    return Image.fromarray(np.uint8(np.clip(np.array(color)[None, None, :] + f[..., None], 0, 255)))


def grass():
    im = base(128, (124, 134, 44), 23)
    d = ImageDraw.Draw(im)
    palette = [(143, 151, 52), (155, 156, 62), (100, 118, 38), (113, 126, 38), (137, 143, 42)]
    for _ in range(1500):
        x, y = rng.integers(0, 128, 2)
        w, h = int(rng.integers(1, 5)), int(rng.integers(1, 4))
        c = palette[int(rng.integers(len(palette)))]
        for dx in [-128, 0, 128]:
            for dy in [-128, 0, 128]:
                d.line([(x+dx, y+dy), (x+dx+w, y+dy), (x+dx+w-1, y+dy-h)], fill=c, width=1)
    return im


def rock():
    size = 256
    yy, xx = np.mgrid[:size, :size]
    seeds = rng.uniform(0, size, (105, 2))
    first = np.full((size, size), 1e9)
    second = first.copy()
    labels = np.zeros((size, size), int)
    for i, (sx, sy) in enumerate(seeds):
        dx = np.minimum(abs(xx-sx), size-abs(xx-sx))
        dy = np.minimum(abs(yy-sy), size-abs(yy-sy))
        dist = dx*dx*1.4 + dy*dy*.55
        new = dist < first
        second = np.where(new, first, np.minimum(second, dist))
        first = np.minimum(first, dist)
        labels = np.where(new, i, labels)
    palette = np.array([(122,116,111),(137,122,108),(108,107,110),(146,130,111),(119,115,116),(129,119,110),(102,105,108),(151,132,111)])
    c = palette[rng.integers(0, len(palette), len(seeds))][labels].astype(float)
    f = field(size, 28)*18 + field(size, 100)*10
    c += f[..., None]
    edge = (np.sqrt(second)-np.sqrt(first)) < .65
    # Fine mineral fissures, deliberately subtler than masonry outlines.
    c[edge] *= .78
    im = Image.fromarray(np.uint8(np.clip(c,0,255)))
    d = ImageDraw.Draw(im)
    for _ in range(160):
        x,y = rng.integers(0,size,2)
        length = int(rng.integers(2,11))
        d.line([(x,y),(x+1,y+length//2),(x-1,y+length)], fill=(87,91,88), width=1)
    return im


def dirt():
    im = base(128,(177,146,96),23)
    d=ImageDraw.Draw(im)
    for _ in range(700):
        x,y=rng.integers(0,128,2);w=int(rng.integers(1,5))
        c=[(193,164,111),(156,133,93),(185,155,103),(169,141,96)][int(rng.integers(4))]
        d.line([(x,y),(x+w,y)],fill=c,width=int(rng.integers(1,3)))
    return im


def sand():
    im=base(128,(208,180,129),17)
    d=ImageDraw.Draw(im)
    for _ in range(900):
        x,y=rng.integers(0,128,2)
        c=(224,196,144) if rng.random()>.4 else (189,163,117)
        d.point((x,y),fill=c)
    return im


def mask():
    size=1200
    channels=[Image.new('L',(size,size),0) for _ in range(3)]
    path=ImageDraw.Draw(channels[0])
    # Match the actual recessed winding route, then join the central clearing.
    route=[(303,482),(318,509),(331,541),(351,565),(371,586),(398,600),(424,615),(442,642),(474,656),(508,669),(537,673)]
    path.line(route,fill=255,width=24,joint='curve')
    for x,y in route:path.ellipse((x-12,y-12,x+12,y+12),fill=255)
    plaza=[(502,636),(535,609),(570,597),(607,604),(644,622),(675,648),(692,677),(678,703),(651,724),(612,737),(574,730),(539,715),(511,690),(497,662)]
    path.polygon(plaza,fill=255)
    coast_route=[(669,723),(687,748),(677,775),(658,800),(666,825),(689,851),(707,883),(716,914),(716,948)]
    path.line(coast_route,fill=255,width=27,joint='curve')
    for x,y in coast_route:path.ellipse((x-13,y-13,x+13,y+13),fill=255)
    stairs=[(303,641),(315,668),(331,700),(351,728),(376,747),(405,751)]
    path.line(stairs,fill=255,width=27,joint='curve')
    # Small worn caps on the left terraces and upper-right platform.
    for polygon in [[(163,657),(290,616),(305,640),(175,678)],[(205,714),(325,676),(335,698),(216,733)],[(243,764),(348,729),(360,749),(253,784)],[(698,603),(741,582),(779,594),(770,618),(731,635)]]:
        path.polygon(polygon,fill=125)
    beach=ImageDraw.Draw(channels[1])
    beach.polygon([(355,899),(388,866),(437,843),(494,832),(546,838),(593,855),(631,883),(660,916),(675,965),(647,973),(625,936),(592,908),(550,890),(506,882),(462,891),(419,917),(389,951),(367,940)],fill=255)
    rock=ImageDraw.Draw(channels[2])
    # Outer outcrops remain stone even where the generated mesh has flat caps.
    outer=[[(10,0),(1200,0),(1200,300),(1080,278),(1010,251),(930,267),(861,235),(799,256),(735,233),(667,261),(610,220),(550,249),(480,238),(416,256),(352,218),(302,253),(240,216),(170,260),(120,251),(45,310),(10,300)],[(0,270),(92,270),(133,354),(105,425),(134,504),(119,565),(149,616),(156,671),(189,731),(207,802),(239,856),(292,922),(319,1017),(0,1200)],[(1088,250),(1200,230),(1200,1200),(681,1200),(661,985),(724,984),(773,1010),(815,993),(844,1016),(894,968),(897,892),(932,835),(960,757),(981,666),(1025,614),(1040,534),(1075,463),(1060,380)]]
    for polygon in outer:rock.polygon(polygon,fill=255)
    rock.ellipse((153,231,228,307),fill=255)
    rock.ellipse((334,847,398,916),fill=255)
    # Natural pixel-scale roughness at material boundaries.
    out=[]
    for i,ch in enumerate(channels):
        ch=ch.filter(ImageFilter.GaussianBlur(2.1 if i<2 else 1.8))
        a=np.asarray(ch,dtype=float)
        rough=field(size,180)*35
        a=np.clip((a-100+rough)*3.1,0,255)
        out.append(Image.fromarray(a.astype('uint8')).resize((1024,1024),Image.Resampling.NEAREST))
    Image.merge('RGB',out).save(TEX/'Terrain_Distribution.png')
    (TEX/'Terrain_Distribution.json').write_text(json.dumps({'extent':[-.54,.54,-.54,.54],'red':'paths and plaza','green':'beach','blue':'exposed coastal rock','inspection_size':1200,'seed':7007},indent=2))


if __name__=='__main__':
    for name,fn in [('Grass',grass),('Rock',rock),('Dirt',dirt),('Sand',sand)]:fn().save(TEX/f'{name}_Pixel.png')
    mask()
    print('Created four pixel material sources and terrain distribution mask.')
