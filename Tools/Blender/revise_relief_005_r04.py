"""Spec 005, revision 04. Run through Blender MCP against the 005 source.

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
OUT = ROOT / 'specs/005-relevo-fiel-referencia/evidence/revisao-04'
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

# Direct landmark calibration against the authoritative aerial, 1672 x 941.
# This is a geometric reconstruction. Reference colors never determine altitude.
CALIBRATION_CAMERA=Vector((-48,-290,200))
CALIBRATION_TARGET=Vector((0,78,11))
Q=(CALIBRATION_TARGET-CALIBRATION_CAMERA).to_track_quat('-Z','Y')
RIGHT=Q@Vector((1,0,0)); UP=Q@Vector((0,1,0)); FORWARD=Q@Vector((0,0,-1))

def from_oracle(u,v,z):
    origin=CALIBRATION_CAMERA+RIGHT*((u/1672-.5)*204)+UP*((.5-v/941)*204*941/1672)
    p=origin+FORWARD*((z-origin.z)/FORWARD.z)
    return (float(p.x),float(p.y))

def oracle_pixel(u,v,z): return pixel(*from_oracle(u,v,z))

# Front edge of the main grassy crown, hand traced in the aerial oracle.
CREST=[(205,214,19),(302,194,20),(350,182,21),(422,172,22),(500,152,24),
 (570,126,27),(641,101,29),(683,86,30),(713,105,29),(790,111,29),
 (852,106,30),(916,97,30),(966,84,31),(1010,104,30),(1083,121,29),
 (1147,106,31),(1220,104,31),(1290,99,31),(1372,102,31),(1400,153,25)]
CREST_WORLD=sorted([(*from_oracle(u,v,z),z) for u,v,z in CREST])
SKY=[(205,214,19),(260,175,22),(350,133,25),(450,100,28),(570,78,31),
 (650,48,33),(740,49,33),(820,25,35),(870,27,35),(940,44,34),
 (1030,30,35),(1095,32,35),(1160,48,34),(1250,52,34),(1310,65,33),
 (1370,85,32),(1440,160,25)]
SKY_WORLD=sorted([(*from_oracle(u,v,z),z) for u,v,z in SKY])
SHORE=[(90,345),(60,385),(104,425),(75,491),(106,535),(90,558),
 (152,608),(165,650),(225,678),(245,732),(320,795),(377,808),(431,770),
 (479,742),(501,716),(548,697),(600,683),(663,679),(727,681),(791,694),
 (852,714),(918,738),(977,761),(1040,765),(1100,791),(1175,797),(1245,774),
 (1320,755),(1360,724),(1410,695),(1440,660),(1460,622),(1475,575),
 (1450,549),(1494,512),(1490,470),(1530,428),(1570,365),(1535,322),
 (1500,292),(1480,245),(1490,210),(1450,175),(1400,120),(1300,80),
 (1180,65),(1050,55),(900,50),(750,60),(600,90),(450,130),(320,170),
 (220,225),(160,280)]
outline=[oracle_pixel(u,v,0) for u,v in SHORE]
BEACH=[from_oracle(u,v,1.0) for u,v in [(505,682),(560,664),(630,651),
 (700,650),(780,666),(864,692),(944,726),(966,750),(917,742),
 (847,718),(780,692),(700,679),(630,679),(560,693),(516,714)]]

LANDMARKS={
 'B1':(560,336,15),'B2':(675,280,17),'A1':(755,394,10),
 'A2':(900,370,10),'A3':(1060,402,10),'L1':(1175,305,16),
 'B3':(550,505,8),'B4':(670,550,7),'L3':(1200,480,9),
 'L4':(1100,560,7),'Casa_Recanto_004':(900,200,13),
 'Plaza':(905,433,8.3),'Ruin':(290,320,16),
 'Garden1':(280,385,13),'Garden2':(325,445,10.5),'Garden3':(390,515,8),
 'Warehouse':(1160,724,2.8),'Pier':(1055,726,1.4)
}
# Ground platforms follow the shapes in the oracle instead of circular pads.
PLATFORMS=[
 ('Ruin',16,[(135,285),(225,257),(318,263),(377,292),(375,325),(331,352),(215,355),(146,336)],3.0),
 ('UpperWest',16.3,[(449,286),(526,272),(665,260),(714,281),(698,311),(635,340),(524,351),(452,328)],2.5),
 ('MiddleButtress',21,[(712,206),(785,190),(832,205),(869,227),(850,253),(780,267),(717,246)],4.0),
 ('UpperEast',16,[(1045,259),(1145,247),(1232,272),(1274,302),(1267,327),(1182,343),(1083,325)],3.0),
 ('RightButtress',21,[(1350,231),(1407,186),(1440,188),(1460,234),(1473,278),(1444,316),(1385,306)],4.0),
 ('Garden1',13,[(139,375),(218,349),(293,348),(360,370),(368,395),(295,415),(194,421),(141,402)],1.6),
 ('Garden2',10.5,[(194,438),(264,412),(349,414),(395,440),(413,466),(344,486),(255,487),(205,469)],1.6),
 ('Garden3',8,[(270,507),(342,480),(406,486),(450,516),(460,543),(414,562),(339,552),(284,532)],1.6),
 ('LowerWest',7.2,[(483,493),(578,490),(650,520),(737,527),(791,538),(761,550),(677,565),(566,554),(496,538)],2.0),
 ('LowerEast',7.3,[(990,485),(1084,495),(1162,523),(1229,524),(1266,537),(1249,558),(1174,568),(1085,563),(1013,541)],2.0),
]
PLATFORM_WORLD=[(name,level,[from_oracle(u,v,level) for u,v in poly],width) for name,level,poly,width in PLATFORMS]

def height(P):
    P=np.atleast_2d(np.asarray(P,dtype=float)); px=P[:,0]; py=P[:,1]
    x=(px/1312-.5)*200; y=162-py*.14512; XY=np.column_stack((x,y))
    # The village climbs gradually from the low southern foreland.
    z=np.maximum(.55, .60+.132*(y-40))
    # Main escarpment: broad crown with an irregular, traced front edge.
    line=np.array(CREST_WORLD)
    front=np.interp(x,line[:,0],line[:,1]); crown=np.interp(x,line[:,0],line[:,2])
    behind=y-front
    shoulder=(.75*np.sin(x*.48)+.28*np.sin(x*1.13+y*.25))
    face=clamp((behind+5.3+shoulder*smooth(-behind/3))/5.3)
    sky=np.array(SKY_WORLD)
    skyline=np.interp(x,sky[:,0],sky[:,1]);skyheight=np.interp(x,sky[:,0],sky[:,2])
    across=clamp(behind/np.maximum(4,skyline-front))
    cap=crown+(skyheight-crown)*across
    cap+=.3*np.sin(x*.4)*smooth(behind/8)
    # Descend behind the visible ridge; the reverse slope is inferred.
    back=y-skyline
    reverse=skyheight-13*smooth(back/4.5)-9*smooth((back-18)/4.5)-9*smooth((back-31)/5.5)-4*smooth((back-44)/5)
    cap=np.where(back>0,reverse,cap)
    cap=np.maximum(.5,cap)
    # The rear still has slopes and shelves, without a uniform wall extrusion.
    z=z*(1-face)+cap*face
    for name,level,poly,width in PLATFORM_WORLD:
        dist=signed_distance(XY,poly)
        influence=clamp((dist+width)/width)
        if name in ['MiddleButtress','RightButtress']:
            z=np.maximum(z, level*influence)
        else:
            z=z*(1-influence)+level*influence
    # Flat supported land under the reference building footprints.
    for name,(u,v,level) in LANDMARKS.items():
        if name in ['Plaza','Ruin','Garden1','Garden2','Garden3','Pier','Warehouse','Casa_Recanto_004']:continue
        cx,cy=from_oracle(u,v,level)
        d=np.maximum(np.abs((x-cx)/8.6),np.abs((y-cy)/6.8))
        a=1-smooth((d-.9)/.40);z=z*(1-a)+level*a
    cx,cy=from_oracle(*LANDMARKS['Plaza'])
    d=np.sqrt(((x-cx)/22.0)**2+((y-cy)/17.8)**2)
    a=1-smooth((d-.94)/.19);z=z*(1-a)+8.3*a
    # Keep beach changes local to the concave southern cove.
    coast=signed_distance(P,outline)
    coastal=clamp(coast/32)
    rear=smooth((y-180)/15)
    tiered=.19*clamp(coast/18)+.36*smooth((coast-25)/20)+.45*smooth((coast-61)/29)
    coastal=coastal*(1-rear)+tiered*rear
    z=z*coastal-.8*(1-coastal)
    beach_distance=signed_distance(XY,BEACH)
    a=smooth((beach_distance+1.0)/1.3)
    bz=-.15+np.maximum(coast,0)*.045
    z=z*(1-a)+bz*a
    # The secluded house sits on the inferred reverse slope, behind the skyline.
    cx,cy=10,173
    d=np.sqrt(((x-cx)/7.5)**2+((y-cy)/5.8)**2)
    a=1-smooth((d-1)/.6);z=z*(1-a)+13*a
    z+=.12*np.sin(x*.74+y*.29)*np.sin(x*.31-y*.61)*smooth((y-112)/8)*smooth(coast/20)
    return z

# Triangulate a traced boundary and interior points: no staircase silhouette.
pts=[]; edges=[]
for i,(a,b) in enumerate(zip(outline,outline[1:]+outline[:1])):
    n=max(1,math.ceil(math.dist(a,b)/9))
    for j in range(n): pts.append((a[0]+(b[0]-a[0])*j/n,a[1]+(b[1]-a[1])*j/n))
boundary_count=len(pts)
edges=[(i,(i+1)%boundary_count) for i in range(boundary_count)]
grid=[(x+rng.uniform(-1.8,1.8),y+rng.uniform(-1.8,1.8)) for y in range(int(min(p[1] for p in outline))-2,int(max(p[1] for p in outline))+2,7) for x in range(int(min(p[0] for p in outline))-2,int(max(p[0] for p in outline))+2,7)]
inside=signed_distance(np.array(grid),outline)
pts.extend(p for p,d in zip(grid,inside) if d>3.5)
v,e,f,_,_,_=delaunay_2d_cdt([Vector(p) for p in pts],edges,[list(range(boundary_count))],1,1e-5)
P=np.array([tuple(p) for p in v]); Z=height(P)
vertices=[(*world(p),float(z)) for p,z in zip(P,Z)]
faces=[tuple(reversed(face)) for face in f if len(face)==3]
mesh=bpy.data.meshes.new('Terrain005_Revision04_Surface')
mesh.from_pydata(vertices,[],faces); mesh.update()
terrain=bpy.data.objects.get('Terrain_005_ReferenceRelief')
terrain.data=mesh
terrain['revision']='04; explicit landforms, aerial oracle; no color-to-height mapping'
terrain['walkability']='Visual maquette only; 70–80 degree faces must be blocked in Unity.'

def mat(name,color):
    m=bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.diffuse_color=(*color,1); m.use_nodes=True
    bs=m.node_tree.nodes.get('Principled BSDF'); bs.inputs['Base Color'].default_value=(*color,1)
    bs.inputs['Roughness'].default_value=.9
    return m

grasses=[mat('005 Grass '+str(i),c) for i,c in enumerate([(.29,.33,.055),(.32,.35,.060),(.35,.37,.065),(.26,.30,.045)])]
rocks=[mat('005 Rock '+str(i),c) for i,c in enumerate([(.20,.19,.21),(.31,.23,.20),(.38,.28,.22),(.24,.22,.24),(.43,.30,.22)])]
sand=mat('005 Beach',(.68,.48,.25))
for m in grasses+rocks+[sand]:mesh.materials.append(m)
angles=[]
for face in mesh.polygons:
    c=face.center; px,py=pixel(c.x,c.y)
    a=math.degrees(math.acos(max(-1,min(1,face.normal.z)))); angles.append(a)
    if signed_distance([(c.x,c.y)],BEACH)[0]>-.6 and c.z<3: face.material_index=9
    elif a>43: face.material_index=4+int((math.sin(px*.11+py*.018)+math.sin(px*.037-py*.032)+2)*1.22)%5
    else: face.material_index= int((math.sin(px*.035+py*.027)+math.sin(px*.014-py*.019)+2)*.9)%4

# Keep reused proxies grounded; roof meshes authored in world coordinates need
# their evaluated bounds aligned rather than a second translation.
def ground_object(obj,px,py):
    w=world((px,py)); corners=[obj.matrix_world@Vector(c) for c in obj.bound_box]
    cx=(min(p.x for p in corners)+max(p.x for p in corners))/2
    cy=(min(p.y for p in corners)+max(p.y for p in corners))/2
    bottom=min(p.z for p in corners)
    obj.location+=Vector((w[0]-cx,w[1]-cy,float(height([(px,py)])[0])-bottom+.05))

houses={name:oracle_pixel(*landmark) for name,landmark in LANDMARKS.items() if name in ['B1','B2','B3','B4','A1','A2','A3','L1','L3','L4','Casa_Recanto_004']}
houses['Casa_Recanto_004']=pixel(10,173)
for name,p in houses.items():
    o=bpy.data.objects[name]
    o.scale*=2.0
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
houses['Casa_Recanto_004']=pixel(10,173)
for name,p in houses.items():
    o=bpy.data.objects[name]; corners=[o.matrix_world@Vector(c) for c in o.bound_box]
    x0=min(c.x for c in corners)-.45;x1=max(c.x for c in corners)+.45
    y0=min(c.y for c in corners)-.45;y1=max(c.y for c in corners)+.45
    z=max(c.z for c in corners); xm=(x0+x1)/2
    verts=[(x0,y0,z),(x1,y0,z),(x0,y1,z),(x1,y1,z),(xm,y0,z+3.0),(xm,y1,z+3.0)]
    me=bpy.data.meshes.new('005 proxy roof '+name);me.from_pydata(verts,[],[(0,2,5,4),(1,4,5,3),(0,4,1),(2,3,5)])
    ob=bpy.data.objects.new('005_ProxyRoof_'+name,me);proxies.objects.link(ob);me.materials.append(roofmat)
    ob['purpose']='Reused building massing landmark, not finished architecture.'

# Landmark proxies make the relief comparison legible; no detailed architecture.
plaster=bpy.data.materials.get('04 House Plaster')
def proxy_box(name,location,scale,material):
    bpy.ops.mesh.primitive_cube_add(size=1,location=location)
    ob=bpy.context.object;ob.name=name;ob.scale=scale
    for c in list(ob.users_collection):c.objects.unlink(ob)
    proxies.objects.link(ob);ob.data.materials.append(material)
    ob['purpose']='Provisional massing landmark for relief comparison.'
    return ob
church=bpy.data.objects['A2']; cx,cy=from_oracle(*LANDMARKS['A2']); base=LANDMARKS['A2'][2]
proxy_box('005_ChurchTower_Proxy',(cx,cy-3.7,base+8.0),(4,4,16),plaster)
wx,wy=from_oracle(*LANDMARKS['Plaza']); wz=8.3
bpy.ops.mesh.primitive_cylinder_add(vertices=16,radius=1.8,depth=1.3,location=(wx,wy,wz+.65))
well=bpy.context.object;well.name='005_Well_Proxy';well.data.materials.append(bpy.data.materials['005 Rock 2'])
for c in list(well.users_collection):c.objects.unlink(well)
proxies.objects.link(well)
wx,wy=from_oracle(*LANDMARKS['Warehouse']); wz=LANDMARKS['Warehouse'][2]
proxy_box('005_DockWarehouse_Proxy',(wx,wy,wz+3.0),(12,9,6),plaster)
proxy_box('005_DockWarehouse_Roof_Proxy',(wx,wy,wz+6.0),(13,10,.6),roofmat)

# Plaza is circular in both primary references.
plaza=bpy.data.objects['Largo_004']; px,py=oracle_pixel(*LANDMARKS['Plaza']); wx,wy=world((px,py)); zz=8.38
verts=[(wx+21.7*math.cos(i*math.tau/64),wy+17.5*math.sin(i*math.tau/64),zz) for i in range(64)]
me=bpy.data.meshes.new('005_PlazaCircular');me.from_pydata(verts,[],[tuple(range(64))]);me.materials.append(mat('005 Plaza',(.44,.34,.24)))
plaza.data=me;plaza.matrix_world.identity()
# Place the reused garden markers across the three calibrated terraces.
for i,o in enumerate(sorted([o for o in bpy.context.scene.objects if o.name.startswith('Horta_Canteiro')],key=lambda o:o.name)):
    u,v,z=LANDMARKS['Garden'+str(min(3,i//2+1))]
    ground_object(o,*oracle_pixel(u+(i%2-.5)*44,v,z))
ruins=sorted([o for o in bpy.context.scene.objects if o.name.startswith('Ruina_Murete')],key=lambda o:o.name)
for i,o in enumerate(ruins):
    u,v,z=LANDMARKS['Ruin']; ground_object(o,*oracle_pixel(u+(i-1.5)*27,v+abs(i-1.5)*10,z))
# Preserve the original pier assembly, calibrated at its landward end.
bpy.context.view_layer.update()
from mathutils import Matrix
anchor=Vector((*from_oracle(*LANDMARKS['Pier']),1.4))
turn=Matrix.Rotation(math.radians(-34),4,'Z')
for o in bpy.context.scene.objects:
    if o.name.startswith('Cais_'):
        original=o.matrix_world.copy()
        original.translation=Vector((original.translation.x*2.4,original.translation.y,original.translation.z))
        o.matrix_world=Matrix.Translation(anchor)@turn@Matrix.Translation(Vector((0,-21.6,-1.0)))@original
        o.scale.x*=2.4
boat=bpy.data.objects.get('Barco_Cais_004');boat.location=(*from_oracle(860,859,0),0)

# Replace tubular paths with narrow surface ribbons sampled from the same field.
REF_ROUTES={
 'Pier_to_Plaza':[(1055,726),(1034,687),(985,654),(941,625),(918,601),(926,568),(957,536),(958,507),(932,477)],
 'West_Ruins':[(758,435),(705,402),(655,378),(599,373),(543,357),(494,350),(459,332),(420,314),(365,310)],
 'West_Gardens':[(686,461),(620,462),(565,475),(500,488),(450,483),(419,456),(395,427),(378,399),(360,375)],
 'East_Homes':[(1080,449),(1142,472),(1202,494),(1245,522),(1239,554),(1191,566)],
}
routes={name:[oracle_pixel(u,v,.60+.132*(from_oracle(u,v,8)[1]-40)) for u,v in points] for name,points in REF_ROUTES.items()}
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
    if 14<=i<=22:continue
    x,y=world(p);radius=rng.uniform(2.2,4.4)
    if y>168:radius*=.35
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
 'aerea-005':camera('Camera_005_Aerial',(-48,-290,200),(0,78,11),204,(1672,941)),
 'topdown-match-005':camera('Camera_005_Top',(0,-180,500),(0,112,9),270,(1312,1199)),
 'planta-005':camera('Camera_005_Overhead',(0,112,300),(0,112,0),270,(1312,1199)),
 'perfil-norte-005':camera('Camera_005_North',(0,414,48),(0,112,18),240,(1800,600)),
 'perfil-sul-005':camera('Camera_005_South',(0,-190,48),(0,112,18),240,(1800,600)),
 'perfil-leste-005':camera('Camera_005_East',(302,112,48),(0,112,18),240,(1800,600)),
 'perfil-oeste-005':camera('Camera_005_West',(-302,112,48),(0,112,18),240,(1800,600)),
}
scene.camera=CAMERAS['aerea-005'][0]
scene.render.resolution_x,scene.render.resolution_y=CAMERAS['aerea-005'][1]
scene.render.filepath=str(OUT/'aerea-005.png')
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'Source/Blender/Ilha_Relevo_005.blend'))
stats={'revision':4,'vertices':len(mesh.vertices),'faces':len(mesh.polygons),'min_z':float(min(Z)),'max_z':float(max(Z)),
 'faces_70_to_80':sum(70<=a<=80 for a in angles),'faces_over_80':sum(a>80 for a in angles),
 'oracle':'Referencias/modelagem-vista-aerea.png','unity_validation':False,
 'note':'Face slopes describe visual geometry, not certified traversability.'}
(OUT/'oracle-landmarks.json').write_text(json.dumps({'camera':list(CALIBRATION_CAMERA),'target':list(CALIBRATION_TARGET),'ortho_scale':204,'landmarks':LANDMARKS,'crest':CREST,'skyline':SKY,'shoreline':SHORE,'platforms':PLATFORMS},indent=2))
(OUT/'geometry.json').write_text(json.dumps(stats,indent=2))
bpy.ops.render.render(write_still=True)
result=stats
