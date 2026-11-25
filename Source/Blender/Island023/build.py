import bpy, math, random, os, json
from pathlib import Path
base=Path('/Users/bruno.duartec/Documents/Projects/Freyja')
out=base/'Freyja/Assets/_Project/Art/Island023'
source=base/'.utmp/023-ilha-referencia-editor/Source/Blender/Island023'
scene=bpy.data.scenes.new('Island023 — construções provisórias')
bpy.context.window.scene=scene
scene.unit_settings.system='METRIC'
# Texturas pequenas, paleta limitada e amostragem nearest.
materials={}
for name,col in [('Plaster',(.79,.71,.51)),('Roof',(.64,.24,.09)),('Wood',(.22,.34,.24)),('Dark',(.09,.14,.13)),('Trim',(.91,.81,.61)),('Awning',(.32,.52,.59))]:
    im=bpy.data.images.new('I023_'+name,width=32,height=32)
    rng=random.Random(23); pixels=[]
    for y in range(32):
        for x in range(32):
            k=rng.choice([.93,1,1,1.04])
            if name=='Roof' and (y%8==0 or (x+(y//8%2)*4)%8==0):k=.72
            if name=='Wood' and x%8==0:k=.75
            if name=='Awning' and x//4%2==0:k=1.35
            pixels.extend([min(1,c*k) for c in col]+[1])
    im.pixels[:]=pixels; im.filepath_raw=str(out/(name+'.png'));im.file_format='PNG';im.save();im.pack()
    m=bpy.data.materials.new('I023_'+name);m.diffuse_color=(*col,1);m.use_nodes=True
    tex=m.node_tree.nodes.new('ShaderNodeTexImage');tex.image=im;tex.interpolation='Closest'
    bs=m.node_tree.nodes.get('Principled BSDF');m.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color']);bs.inputs['Roughness'].default_value=1
    materials[name]=m
objects=[]
def box(name,loc,scale,mat):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc);o=bpy.context.object;o.name=name;o.dimensions=scale
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(materials[mat]);objects.append(o);return o
def roof(w,d,h):
    verts=[(-w/2,-d/2,h),(w/2,-d/2,h),(0,-d/2,h+1.6),(-w/2,d/2,h),(w/2,d/2,h),(0,d/2,h+1.6)]
    mesh=bpy.data.meshes.new('Gable');mesh.from_pydata(verts,[],[(2,5,3,0),(1,4,5,2),(1,2,0),(5,4,3)]);mesh.update()
    o=bpy.data.objects.new('Telhado duas águas',mesh);scene.collection.objects.link(o);o.data.materials.append(materials['Roof']);objects.append(o)
    bpy.context.view_layer.objects.active=o;o.select_set(True);bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project();bpy.ops.object.mode_set(mode='OBJECT');o.select_set(False)
def facade(w,d,h,shop=False):
    box('Porta', (0,-d/2-.045,1.05),(1.1,.1,2.1),'Wood')
    for z in ([2,4.6] if h>5 else [2]):
        for x in [-w*.31,w*.31]:
            box('Moldura',(x,-d/2-.07,z),(1.18,.12,1.35),'Trim');box('Janela',(x,-d/2-.15,z),(1,.08,1.17),'Dark')
            for side in [-1,1]:box('Veneziana',(x+side*.58,-d/2-.18,z),(.23,.1,1.2),'Wood')
    for side in [-1,1]:
        for y in [-d*.22,d*.22]:box('Janela lateral',(side*(w/2+.04),y,2),(.1,1,1.3),'Wood')
    if shop:
        a=box('Toldo',(0,-d/2-.9,2.8),(w*.85,1.8,.12),'Awning');a.rotation_euler.x=.15
        for x in [-w*.4,w*.4]:box('Pilar toldo',(x,-d/2-1.65,1.35),(.12,.12,2.7),'Wood')
layoutIndex=0
for name,w,d,h in [('House',6,7,3.5),('TallHouse',6,7,6),('OldHouse',4.2,5,2.8),('Shop',6,7,5.5),('Hall',9,6,3.8),('Warehouse',7,6,3.5),('Church',7,10,5.3)]:
    objects=[]
    box('Paredes',(0,0,h/2),(w,d,h),'Plaster');roof(w+.6,d+.6,h);facade(w,d,3.5 if name=='Church' else h,name=='Shop')
    if name=='Church':
        bpy.ops.mesh.primitive_cylinder_add(vertices=12,radius=.65,depth=.12,location=(0,-5.1,3.9),rotation=(math.pi/2,0,0));o=bpy.context.object;o.name='Rosácea';o.data.materials.append(materials['Dark']);objects.append(o)
        box('Campanário',(1.8,2,7),(2.2,2.6,4.5),'Plaster')
        box('Sino escuro',(1.8,.66,8),(1.1,.08,1.6),'Dark')
        for side in [-1,1]:box('Vão torre lateral',(1.8+side*1.14,2,8),(.08,1.4,1.6),'Dark')
        # Telhado piramidal e cruz, sem silos cilíndricos.
        bpy.ops.mesh.primitive_cone_add(vertices=4,radius1=2,depth=1.6,location=(1.8,2,10));o=bpy.context.object;o.rotation_euler.z=math.pi/4;o.data.materials.append(materials['Roof']);objects.append(o)
        box('Cruz vertical',(1.8,2,11.4),(.12,.12,1.2),'Trim');box('Cruz horizontal',(1.8,2,11.6),(.7,.12,.12),'Trim')
    else:box('Chaminé',(-w*.25,d*.15,h+1.4),(.55,.6,1.5),'Trim')
    bpy.ops.object.select_all(action='DESELECT')
    for o in objects:o.select_set(True)
    bpy.ops.export_scene.fbx(filepath=str(out/(name+'.fbx')),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_anim=False)
    collection=bpy.data.collections.new(name);scene.collection.children.link(collection)
    for o in objects:
        collection.objects.link(o);o.location.x+=layoutIndex*14;o.hide_set(False)
    layoutIndex+=1
scene['provisional']=True
bpy.ops.wm.save_as_mainfile(filepath=str(source/'Island023.blend'))
result={'models':7,'textures':6,'source':str(source/'Island023.blend')}
