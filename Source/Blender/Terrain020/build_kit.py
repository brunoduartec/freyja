import bpy, math, random, json
from pathlib import Path
from mathutils import Vector

ROOT=Path('/Users/bruno.duartec/Documents/Projects/Freyja/.utmp/020-teste-terreno-modular')
OUT=ROOT/'Freyja/Assets/_Project/Art/Terrain020'
SRC=ROOT/'Source/Blender/Terrain020'
OUT.mkdir(parents=True,exist_ok=True)
scene=bpy.data.scenes.new('Terrain020_Kit')
bpy.context.window.scene=scene
scene.unit_settings.system='METRIC';scene.unit_settings.scale_length=1
random.seed(20)

def material(name,color):
    m=bpy.data.materials.new('T020_'+name);m.diffuse_color=(*color,1)
    m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*color,1);p.inputs['Roughness'].default_value=.95
    return m
stone=[material('Limestone_'+str(i),c) for i,c in enumerate([(.44,.43,.34),(.56,.54,.43),(.66,.62,.48),(.48,.47,.40)])]
brick=[material('Masonry_'+str(i),c) for i,c in enumerate([(.61,.47,.32),(.69,.55,.39),(.74,.62,.45),(.58,.48,.37)])]
wood=[material('Wood_'+str(i),c) for i,c in enumerate([(.43,.28,.13),(.53,.36,.18),(.61,.42,.23)])]

def cube(name,center,size,mat,bevel=0):
    bpy.ops.mesh.primitive_cube_add(size=1,location=center)
    o=bpy.context.object;o.name=name;o.dimensions=size
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    if bevel:
        mod=o.modifiers.new('Small edge bevel','BEVEL');mod.width=bevel;mod.segments=1
        bpy.ops.object.modifier_apply(modifier=mod.name)
    o.data.materials.append(mat)
    return o

def join(objects,name):
    bpy.ops.object.select_all(action='DESELECT')
    for o in objects:o.select_set(True)
    bpy.context.view_layer.objects.active=objects[0]
    bpy.ops.object.join();o=bpy.context.object;o.name=name
    scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
    return o

assets=[]
for i in range(3):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1,location=(0,0,0))
    o=bpy.context.object;o.name='Rock_'+chr(65+i)
    for v in o.data.vertices:
        k=random.uniform(.85,1.15);v.co.x*=1.15*k;v.co.y*=.85*k;v.co.z=(v.co.z+1)*.65*random.uniform(.88,1.12)
    zmin=min(v.co.z for v in o.data.vertices)
    for v in o.data.vertices:v.co.z-=zmin
    for m in stone:o.data.materials.append(m)
    for face in o.data.polygons:face.material_index=random.randrange(4);face.use_smooth=False
    assets.append(o)

pieces=[]
for row in range(3):
    for col in range(4):
        x=-1+(col+.5)*.5
        pieces.append(cube('Block',(x,0,.16+row*.325),(.485,.5,.31),brick[(row+col)%4],.025))
pieces.append(cube('Coping',(0,0,1.035),(2.06,.61,.14),stone[2],.035))
assets.append(join(pieces,'Wall_2m'))

pieces=[cube('Curb',(x,0,.14),(.48,.32,.28),stone[(i+1)%4],.025) for i,x in enumerate([-.75,-.25,.25,.75])]
assets.append(join(pieces,'Curb_2m'))

# Deck top at Z=0, landing end at Y=0; Unity import maps Blender Y to world Z.
pieces=[]
for i in range(22):
    pieces.append(cube('Plank',(0,-.225-i*.45,-.1),(2.8,.425,.2),wood[i%3],.015))
for x in [-1.08,1.08]:
    pieces.append(cube('Beam',(x,-5,-.28),(.2,10,.23),wood[0]))
    for y in [-.5,-3.5,-6.5,-9.5]:
        pieces.append(cube('Pile',(x,y,-.60),(.28,.28,1.9),wood[0],.025))
assets.append(join(pieces,'Pier_10m'))

# Low resolution, repeatable material maps; color variation, no baked shadows.
def texture(name,base,mode):
    n=128;image=bpy.data.images.new('T020_'+name,width=n,height=n,alpha=False)
    px=[]
    rng=random.Random(20+mode)
    grid=[[rng.random()-.5 for _ in range(16)] for _ in range(16)]
    for y in range(n):
        for x in range(n):
            gx=x/8;gy=y/8;ix=int(gx);iy=int(gy);fx=gx-ix;fy=gy-iy
            fx=fx*fx*(3-2*fx);fy=fy*fy*(3-2*fy)
            noise=(grid[iy][ix]*(1-fx)+grid[iy][(ix+1)%16]*fx)*(1-fy)+(grid[(iy+1)%16][ix]*(1-fx)+grid[(iy+1)%16][(ix+1)%16]*fx)*fy
            k=noise*.06+(rng.random()-.5)*.025
            if mode==2 and ((x%32<2) or ((y+(16 if (x//32)%2 else 0))%24<2)):k=-.12
            px.extend([max(0,min(1,c+k)) for c in base]+[1])
    image.pixels.foreach_set(px);image.filepath_raw=str(OUT/(name+'.png'));image.file_format='PNG';image.save()
    return image
texture('Grass',(0.30,.42,.16),0)
texture('Sand',(.77,.61,.37),1)
texture('Paving',(.62,.48,.31),2)

manifest=[]
for i,o in enumerate(assets):
    bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o
    o.location=(0,0,0);bpy.context.view_layer.update()
    bpy.ops.export_scene.fbx(filepath=str(OUT/(o.name+'.fbx')),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_space_transform=True,add_leaf_bones=False,bake_anim=False)
    manifest.append({'id':o.name,'dimensions_blender':list(o.dimensions)})
    o.location=(i%3*5,i//3*12,0)

scene.render.engine='BLENDER_EEVEE'
bpy.context.view_layer.update()
bpy.data.libraries.write(str(SRC/'Terrain020_Kit.blend'),{scene},fake_user=True)
(SRC/'manifest.json').write_text(json.dumps(manifest,indent=2))
result={'source':str(SRC/'Terrain020_Kit.blend'),'exports':len(assets),'textures':3,'objects':[o.name for o in assets]}
