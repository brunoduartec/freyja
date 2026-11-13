"""Spec 005, revision 02. Run through Blender MCP against the 005 source.

Coordinates are landmarks in the top reference, not colors sampled as heights.
The aerial oracle controls elevation, cliff continuity and the southern terraces.
No external Python packages: Blender's constrained Delaunay builds the surface.
"""
import bpy, math, json, random
import numpy as np
from pathlib import Path
from mathutils import Vector
from mathutils.geometry import delaunay_2d_cdt

ROOT = Path('/Users/bruno.duartec/Documents/Projects/Freyja')
OUT = ROOT / 'specs/005-relevo-fiel-referencia/evidence/revisao-02'
OUT.mkdir(exist_ok=True)
rng = random.Random(5002)

def world(p): return ((p[0]/1312-.5)*200, 162-p[1]*.14512)
def pixel(x,y): return ((x/200+.5)*1312,(162-y)/.14512)
def clamp(x,a=0.,b=1.): return np.clip(x,a,b)
def smooth(x):
    t=clamp(x); return t*t*(3-2*t)

# Continuous coastline, deliberately excluding isolated rocks, the boat and pier.
outline=[(140,245),(180,222),(235,183),(275,155),(330,112),(390,75),
 (455,92),(515,64),(575,78),(635,44),(695,53),(750,35),(805,48),
 (850,77),(925,76),(962,108),(1015,101),(1085,132),(1110,186),
 (1160,216),(1155,267),(1185,305),(1180,355),(1210,390),(1180,445),
 (1200,490),(1162,540),(1170,590),(1120,638),(1138,680),(1090,735),
 (1080,790),(1040,825),(1060,875),(1020,930),(1010,990),(960,1030),
 (916,1005),(865,1010),(816,1000),(765,982),(745,955),(717,937),
 (682,920),(645,907),(600,901),(555,902),(510,916),(472,937),
 (444,966),(410,994),(355,1020),(315,1005),(289,968),(325,922),
 (281,879),(245,840),(205,808),(192,760),(153,725),(135,671),
 (105,630),(120,580),(86,542),(96,489),(89,441),(115,398),
 (104,355),(123,310)]

def signed_distance(P,poly):
    P=np.asarray(P); x=P[:,0]; y=P[:,1]
    inside=np.zeros(len(P),dtype=bool); dist=np.full(len(P),1e9)
    for a,b in zip(poly,poly[1:]+poly[:1]):
        a=np.array(a,dtype=float); b=np.array(b,dtype=float); d=b-a
        t=clamp(((P-a)*d).sum(axis=1)/max(float(d@d),1e-9))
        dist=np.minimum(dist,np.linalg.norm(P-(a+t[:,None]*d),axis=1))
        if a[1]!=b[1]:
            inside ^= ((a[1]>y)!=(b[1]>y)) & (x<(b[0]-a[0])*(y-a[1])/(b[1]-a[1])+a[0])
    return np.where(inside,dist,-dist)

# Crests traced as separate, irregular buttresses. The front drop is deliberately
# narrow (roughly 2-4 metres); the back of each crest is broader and vegetated.
RIDGES=[
 ([(150,280,24),(240,253,30),(320,212,35),(394,145,41),(453,163,38),(520,110,43),(570,145,36)],125,45),
 ([(190,350,22),(270,315,25),(343,322,28),(412,286,33),(475,247,35),(530,272,29),(594,278,25)],95,36),
 ([(560,159,34),(631,148,37),(701,144,35),(763,156,35),(827,151,37),(911,127,43),(975,162,39),(1036,157,39),(1100,217,29)],140,45),
 ([(625,352,25),(704,325,28),(779,309,29),(842,257,35),(895,281,32),(949,325,27),(1018,354,28),(1102,400,22)],105,38),
]

def height(P):
    P=np.atleast_2d(np.asarray(P,dtype=float)); x=P[:,0]; y=P[:,1]
    # Low residential terraces, with a coherent, unbroken plaza surface.
    terrace_y=y+22*np.sin((x-200)/140)+11*np.sin(x/70)
    z=3.1 + 2.6*smooth((894-terrace_y)/16) + 2.5*smooth((796-terrace_y)/17) + 3.0*smooth((593-terrace_y)/22)
    z += 1.2*smooth((450-y)/90)
    # Ruin on the western shoulder; garden benches below it.
    west= smooth((420-x)/60)*smooth((x-120)/60)
    z += west*(2.8*smooth((743-y)/15)+2.7*smooth((647-y)/15))
    # The rear mass is continuous in the authoritative aerial image.
    front=np.interp(x,[100,200,300,400,500,600,700,800,900,1000,1120,1220],
        [300,345,325,350,363,340,365,341,349,385,423,360])
    front += 7*np.sin(x*.057)
    crown=26.0+1.8*np.sin(x*.022)+1.1*np.sin(x*.051)+3.5*smooth((285-y)/150)
    north=crown*smooth((front+20-y)/30)
    z=np.maximum(z,north)
    foundation=z.copy()
    for ridge,back,front in RIDGES:
        best=np.full(len(P),-1e9); nearest=np.full(len(P),1e9)
        for aa,bb in zip(ridge,ridge[1:]):
            a=np.array(aa[:2]); b=np.array(bb[:2]); d=b-a
            t=clamp(((P-a)*d).sum(axis=1)/(d@d))
            q=a+t[:,None]*d; delta=P-q
            distance=np.linalg.norm(delta,axis=1)
            crest=aa[2]+t*(bb[2]-aa[2])
            crest=np.where(crest>35,35+(crest-35)*.45,crest)
            width=np.where(delta[:,1]>0,front,back)
            # Small crown, then a sharp rock face. Avoid clipping all peaks flat.
            f=clamp((distance-13)/(width-13))
            candidate=foundation+(crest-foundation)*(1-f) - 1.8*clamp(distance/13)*(1-f)
            best=np.maximum(best,candidate)
        z=np.maximum(z,best)
    # A winding, supported mountain route. Cliff faces remain steep outside it.
    ramp=[(448,406,11.3),(466,378,12),(511,356,14),(555,344,16),(579,320,18),
          (600,293,20),(651,280,22),(682,261,24),(680,242,25),(647,219,26),
          (612,207,27),(608,187,28),(634,167,29),(674,157,29),(723,158,28),
          (749,140,26),(751,118,25)]
    near=np.full(len(P),1e9); roadz=np.zeros(len(P))
    for aa,bb in zip(ramp,ramp[1:]):
        a=np.array(aa[:2]); b=np.array(bb[:2]); d=b-a
        t=clamp(((P-a)*d).sum(axis=1)/(d@d)); q=a+t[:,None]*d
        dist=np.linalg.norm(P-q,axis=1); mask=dist<near
        roadz=np.where(mask,aa[2]+t*(bb[2]-aa[2]),roadz);near=np.minimum(near,dist)
    influence=1-smooth((near-9)/18)
    z=z*(1-influence)+roadz*influence
    # Coherent terrace at the remote house, not a hole inferred from roof pixels.
    d=np.sqrt(((x-744)/63)**2+((y-90)/42)**2)
    z=z*(1-smooth((1.25-d)/.45))+25.0*smooth((1.25-d)/.45)
    # Flat plaza; gradual transitions to its surrounding housing terrace.
    d=np.sqrt(((x-683)/118)**2+((y-645)/103)**2)
    a=smooth((1.3-d)/.3); z=z*(1-a)+8.2*a
    # Localized beach ONLY along the actual bay, never on paths or house roofs.
    coast=signed_distance(P,outline)
    beach=smooth((x-415)/40)*smooth((775-x)/40)*smooth((y-846)/35)
    coastal=clamp(coast/38)
    # Rear and side cliffs have intermediate shelves instead of one extruded wall.
    tiered=.18*clamp(coast/18)+.37*smooth((coast-23)/22)+.45*smooth((coast-62)/28)
    n=smooth((450-y)/110);coastal=coastal*(1-n)+tiered*n
    z=z*coastal-.8*(1-coastal)
    beach_z=-.45+coast*.06
    z=z*(1-beach*(1-smooth((coast-18)/24)))+beach_z*beach*(1-smooth((coast-18)/24))
    # Sub-metre irregularity, suppressed on the village and the beach.
    rough=(np.sin(x*.083+y*.071)*np.sin(x*.047-y*.096))
    z += .33*rough*smooth((480-y)/100)*smooth(coast/20)
    # A supported pad for the secluded house, clear of the sea-facing cliff.
    d=np.sqrt(((x-744)/42)**2+((y-140)/35)**2)
    a=1-smooth((d-1)/.7);z=z*(1-a)+25.0*a
    return z

# Triangulate a traced boundary and interior points: no staircase silhouette.
pts=[]; edges=[]
for i,(a,b) in enumerate(zip(outline,outline[1:]+outline[:1])):
    n=max(1,math.ceil(math.dist(a,b)/9))
    for j in range(n): pts.append((a[0]+(b[0]-a[0])*j/n,a[1]+(b[1]-a[1])*j/n))
boundary_count=len(pts)
edges=[(i,(i+1)%boundary_count) for i in range(boundary_count)]
grid=[(x+rng.uniform(-1.8,1.8),y+rng.uniform(-1.8,1.8)) for y in range(45,1030,7) for x in range(80,1220,7)]
inside=signed_distance(np.array(grid),outline)
pts.extend(p for p,d in zip(grid,inside) if d>3.5)
v,e,f,_,_,_=delaunay_2d_cdt([Vector(p) for p in pts],edges,[list(range(boundary_count))],1,1e-5)
P=np.array([tuple(p) for p in v]); Z=height(P)
vertices=[(*world(p),float(z)) for p,z in zip(P,Z)]
faces=[tuple(reversed(face)) for face in f if len(face)==3]
mesh=bpy.data.meshes.new('Terrain005_Revision02_Surface')
mesh.from_pydata(vertices,[],faces); mesh.update()
terrain=bpy.data.objects.get('Terrain_005_ReferenceRelief')
terrain.data=mesh
terrain['revision']='02; explicit landforms, aerial oracle; no color-to-height mapping'
terrain['walkability']='Visual maquette only; 70–80 degree faces must be blocked in Unity.'

def mat(name,color):
    m=bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.diffuse_color=(*color,1); m.use_nodes=True
    bs=m.node_tree.nodes.get('Principled BSDF'); bs.inputs['Base Color'].default_value=(*color,1)
    bs.inputs['Roughness'].default_value=.9
    return m

grasses=[mat('005 Grass '+str(i),c) for i,c in enumerate([(.26,.31,.055),(.31,.36,.065),(.37,.39,.075),(.21,.28,.045)])]
rocks=[mat('005 Rock '+str(i),c) for i,c in enumerate([(.23,.205,.18),(.32,.25,.19),(.39,.29,.21),(.26,.25,.24),(.45,.32,.23)])]
sand=mat('005 Beach',(.68,.48,.25))
for m in grasses+rocks+[sand]:mesh.materials.append(m)
angles=[]
for face in mesh.polygons:
    c=face.center; px,py=pixel(c.x,c.y)
    a=math.degrees(math.acos(max(-1,min(1,face.normal.z)))); angles.append(a)
    if 415<px<775 and py>875 and c.z<2.2: face.material_index=9
    elif a>43: face.material_index=4+rng.randrange(5)
    else: face.material_index= int((math.sin(px*.035+py*.027)+math.sin(px*.014-py*.019)+2)*.9)%4

# Keep reused proxies grounded; roof meshes authored in world coordinates need
# their evaluated bounds aligned rather than a second translation.
def ground_object(obj,px,py):
    w=world((px,py)); corners=[obj.matrix_world@Vector(c) for c in obj.bound_box]
    cx=(min(p.x for p in corners)+max(p.x for p in corners))/2
    cy=(min(p.y for p in corners)+max(p.y for p in corners))/2
    bottom=min(p.z for p in corners)
    obj.location+=Vector((w[0]-cx,w[1]-cy,float(height([(px,py)])[0])-bottom+.05))

houses={'B1':(397,455),'B2':(514,418),'B3':(425,690),'B4':(525,775),
 'A1':(550,530),'A2':(681,469),'A3':(798,534),'L1':(895,450),
 'L3':(920,674),'L4':(815,775),'Casa_Recanto_004':(744,140)}
for name,p in houses.items():
    o=bpy.data.objects[name]
    o.scale*=1.65
    bpy.context.view_layer.update()
    ground_object(o,*p)
for name in ['A4','L2']:
    bpy.data.objects[name].hide_render=True; bpy.data.objects[name].hide_set(True)
bpy.context.view_layer.update()
roof=bpy.data.objects['Casa_Recanto_Telhado_004']
roof.hide_render=True; roof.hide_set(True)

# Minimal recognizable roofs on existing massing proxies, in a separate group.
proxies=bpy.data.collections.get('005_Reused_Massing_Proxies') or bpy.data.collections.new('005_Reused_Massing_Proxies')
if proxies.name not in bpy.context.scene.collection.children: bpy.context.scene.collection.children.link(proxies)
for o in list(proxies.objects): bpy.data.objects.remove(o,do_unlink=True)
roofmat=bpy.data.materials.get('04 Roof Terracotta')
for name,p in houses.items():
    o=bpy.data.objects[name]; corners=[o.matrix_world@Vector(c) for c in o.bound_box]
    x0=min(c.x for c in corners)-.45;x1=max(c.x for c in corners)+.45
    y0=min(c.y for c in corners)-.45;y1=max(c.y for c in corners)+.45
    z=max(c.z for c in corners); xm=(x0+x1)/2
    verts=[(x0,y0,z),(x1,y0,z),(x0,y1,z),(x1,y1,z),(xm,y0,z+3.0),(xm,y1,z+3.0)]
    me=bpy.data.meshes.new('005 proxy roof '+name);me.from_pydata(verts,[],[(0,2,5,4),(1,4,5,3),(0,4,1),(2,3,5)])
    ob=bpy.data.objects.new('005_ProxyRoof_'+name,me);proxies.objects.link(ob);me.materials.append(roofmat)
    ob['purpose']='Reused building massing landmark, not finished architecture.'

# Plaza is circular in both primary references.
plaza=bpy.data.objects['Largo_004']; px,py=683,645; wx,wy=world((px,py)); zz=float(height([(px,py)])[0])+.08
verts=[(wx+17.5*math.cos(i*math.tau/64),wy+14.2*math.sin(i*math.tau/64),zz) for i in range(64)]
me=bpy.data.meshes.new('005_PlazaCircular');me.from_pydata(verts,[],[tuple(range(64))]);me.materials.append(mat('005 Plaza',(.44,.34,.24)))
plaza.data=me;plaza.matrix_world.identity()
for i,o in enumerate(sorted([o for o in bpy.context.scene.objects if o.name.startswith('Horta_Canteiro')],key=lambda o:o.name)):
    ground_object(o,230+(i%2)*50,625+(i//2)*76)
for o in [o for o in bpy.context.scene.objects if o.name.startswith('Ruina_Murete')]:
    p=pixel(o.location.x,o.location.y);ground_object(o,*p)
for o in bpy.context.scene.objects:
    if o.name.startswith('Cais_'):
        o.location.x+=world((790,0))[0];o.location.y-=3.0
boat=bpy.data.objects.get('Barco_Cais_004');boat.location.x=world((700,0))[0];boat.location.y=-1

# Replace tubular paths with narrow surface ribbons sampled from the same field.
routes={
 'Pier_to_Plaza':[(790,989),(780,945),(750,904),(714,874),(696,844),(722,798),(713,757),(690,730)],
 'West_Ruins':[(578,625),(531,598),(486,587),(445,566),(403,563),(362,547),(340,510),(323,460),(290,432)],
 'West_Gardens':[(440,567),(460,615),(450,650),(462,703),(421,739),(382,740),(350,695),(326,648),(310,622)],
 'East_Homes':[(798,646),(850,651),(884,633),(918,620),(965,659),(984,706),(960,741),(918,756),(870,740)],
 'North_Ridge':[(600,581),(590,565),(599,524),(596,485),(565,464),(482,487),(460,454),(448,406),(466,378),(511,356),(555,344),(579,320),(600,293),(651,280),(682,261),(680,242),(647,219),(612,207),(608,187),(634,167),(674,157),(723,158),(749,140),(751,118)]}
for o in list(bpy.context.scene.objects):
    if o.name.startswith('Path005_'):bpy.data.objects.remove(o,do_unlink=True)
pathmat=mat('005 Path',(.52,.38,.20))
for name,line in routes.items():
    samples=[]
    for a,b in zip(line,line[1:]):
        n=max(1,math.ceil(math.dist(a,b)/3))
        for i in range(n): samples.append(np.array(a)+(np.array(b)-a)*i/n)
    samples.append(np.array(line[-1])); vs=[]
    for i,p in enumerate(samples):
        d=samples[min(i+1,len(samples)-1)]-samples[max(0,i-1)]
        side=np.array([-d[1],d[0]])/max(np.linalg.norm(d),1e-6)*4.0
        for q in [p+side,p-side]: vs.append((*world(q),float(height([q])[0])+.10))
    fs=[(2*i,2*i+1,2*i+3,2*i+2) for i in range(len(samples)-1)]
    me=bpy.data.meshes.new('Path005_'+name);me.from_pydata(vs,[],fs);me.materials.append(pathmat)
    ob=bpy.data.objects.new('Path005_'+name,me);bpy.context.scene.collection.objects.link(ob)
    ob['purpose']='Visual route placeholder; traversability validation pending.'

# Rock skirt follows terrain and breaks the otherwise artificial perimeter band.
rocks_col=bpy.data.collections.get('005_CoastalRocks') or bpy.data.collections.new('005_CoastalRocks')
if rocks_col.name not in bpy.context.scene.collection.children:bpy.context.scene.collection.children.link(rocks_col)
for o in list(rocks_col.objects):bpy.data.objects.remove(o,do_unlink=True)
for i,(a,b) in enumerate(zip(outline,outline[1:]+outline[:1])):
    p=(np.array(a)+np.array(b))/2
    if 420<p[0]<800 and p[1]>870:continue
    x,y=world(p);radius=rng.uniform(2.2,4.4)
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1,location=(x,y,radius*.44))
    o=bpy.context.object;o.name='005_CoastalRock_%02d'%i
    for c in list(o.users_collection):c.objects.unlink(o)
    rocks_col.objects.link(o);o.scale=(radius,radius*rng.uniform(.65,1.1),radius*rng.uniform(.9,1.6))
    o.rotation_euler=(rng.uniform(-.3,.3),rng.uniform(-.3,.3),rng.uniform(0,6.28))
    for m in rocks:o.data.materials.append(m)
    for f in o.data.polygons:f.material_index=rng.randrange(5)

scene=bpy.context.scene
scene.render.engine='BLENDER_EEVEE';scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG'
scene.view_settings.view_transform='Standard';scene.view_settings.look='Medium High Contrast'
scene.view_settings.exposure=-.4;scene.view_settings.gamma=1
sun=bpy.data.objects.get('Sun_004')
if sun:
    sun.data.energy=2.0;sun.data.use_shadow=True;sun.data.angle=.15
    sun.rotation_euler=(math.radians(28),math.radians(-25),math.radians(-32))
scene.world.color=(.28,.32,.39)
water=bpy.data.objects['Mar_Preview_004']
water.scale*=4
water.data.materials.clear();water.data.materials.append(mat('005 Water',(.018,.16,.25)))

def camera(name,loc,target,scale,res):
    o=bpy.data.objects.get(name)
    if o is None:
        o=bpy.data.objects.new(name,bpy.data.cameras.new(name));scene.collection.objects.link(o)
    o.location=loc;o.rotation_euler=(Vector(target)-o.location).to_track_quat('-Z','Y').to_euler()
    o.data.type='ORTHO';o.data.ortho_scale=scale;o.data.clip_end=1000
    return o,res
CAMERAS={
 'aerea-005':camera('Camera_005_Aerial',(22,-290,190),(0,78,12),224,(1672,941)),
 'topdown-match-005':camera('Camera_005_Top',(0,-180,500),(0,78,9),213,(1312,1199)),
 'planta-005':camera('Camera_005_Overhead',(0,78,300),(0,78,0),216,(1312,1199)),
 'perfil-norte-005':camera('Camera_005_North',(0,380,48),(0,78,18),216,(1800,600)),
 'perfil-sul-005':camera('Camera_005_South',(0,-224,48),(0,78,18),216,(1800,600)),
 'perfil-leste-005':camera('Camera_005_East',(302,78,48),(0,78,18),216,(1800,600)),
 'perfil-oeste-005':camera('Camera_005_West',(-302,78,48),(0,78,18),216,(1800,600)),
}
scene.camera=CAMERAS['aerea-005'][0]
scene.render.resolution_x,scene.render.resolution_y=CAMERAS['aerea-005'][1]
scene.render.filepath=str(OUT/'aerea-005.png')
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'Source/Blender/Ilha_Relevo_005.blend'))
stats={'revision':2,'vertices':len(mesh.vertices),'faces':len(mesh.polygons),'min_z':float(min(Z)),'max_z':float(max(Z)),
 'faces_70_to_80':sum(70<=a<=80 for a in angles),'faces_over_80':sum(a>80 for a in angles),
 'oracle':'Referencias/modelagem-vista-aerea.png','unity_validation':False,
 'note':'Face slopes describe visual geometry, not certified traversability.'}
(OUT/'geometry.json').write_text(json.dumps(stats,indent=2))
bpy.ops.render.render(write_still=True)
result=stats
