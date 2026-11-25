import bpy, math, random, json
from pathlib import Path
from mathutils import Vector
ROOT=Path('/Users/bruno.duartec/Documents/Projects/Freyja/.utmp/022-kit-cenario-ps1')
SRC=ROOT/'Source/Blender/KitPS1_022';ART=ROOT/'Freyja/Assets/_Project/Art/KitPS1_022';OUT=ROOT/'Artifacts/KitPS1_022'
for p in [SRC,ART,OUT]:p.mkdir(parents=True,exist_ok=True)
scene=bpy.data.scenes.new('KitPS1_022');bpy.context.window.scene=scene
scene.unit_settings.system='METRIC'
colors={'Grass':[(54,69,34),(66,82,39),(79,94,46),(92,105,53)],'Dirt':[(91,65,42),(113,83,53),(133,100,65),(148,113,75)],'PathSand':[(142,115,73),(160,133,86),(177,149,100),(190,163,113)],'BeachSand':[(182,157,109),(201,177,126),(218,195,143),(230,208,159)],'Stone':[(91,94,86),(113,116,105),(136,137,121),(158,157,137)],'Brick':[(103,63,47),(132,76,52),(155,92,61),(175,113,77)],'Bark':[(62,47,29),(82,61,36),(106,79,43),(125,96,54)],'Leaves':[(38,55,30),(50,70,34),(65,85,40),(84,101,48)]}
colors['Rock']=[(110,111,99),(123,124,111),(137,137,122),(148,148,132)]
materials={}
for name,palette in colors.items():
 n=32 if name in ['Bark','Leaves'] else 64;rng=random.Random(22+list(colors).index(name));cells=[[rng.random() for x in range(8)] for y in range(8)];pix=[]
 for y in range(n):
  for x in range(n):
   cell=cells[(y//(n//8))%8][(x//(n//8))%8];idx=min(3,int(cell*3.6))
   if name=='Grass':
    idx=1+int(cell>.45);idx=max(0,idx-1) if (x+2*y)%13==0 else idx
   elif name in ['BeachSand','PathSand']:idx=1+int(math.sin(2*math.pi*(y/n*4+.16*math.sin(x/n*2*math.pi)))>.35);idx=0 if rng.random()<.025 else idx
   elif name in ['Brick','Stone']:
    row=y//(8 if name=='Brick' else 16);width=16 if name=='Brick' else 32;xx=(x+(row%2)*width//2)%width;yy=y%(8 if name=='Brick' else 16);idx=0 if xx==0 or yy==0 else min(3,1+int(cells[row%8][((x+row*width//2)//width)%8]*3));idx=max(0,idx-1) if xx==1 or yy==1 else idx
   elif name=='Bark':idx=int((math.sin(x*.8+math.sin(y*.3))+.999)*1.5)
   elif name=='Leaves':idx=0 if (x*3+y*5)%19<3 else idx
   c=palette[max(0,min(3,idx))];pix.extend([c[0]/255,c[1]/255,c[2]/255,1])
 image=bpy.data.images.new('PS1_'+name,n,n);image.pixels.foreach_set(pix);image.filepath_raw=str(ART/(name+'.png'));image.file_format='PNG';image.save();image.pack()
 mat=bpy.data.materials.new('PS1_'+name);mat.use_nodes=True;nodes=mat.node_tree.nodes;bs=nodes.get('Principled BSDF');bs.inputs['Roughness'].default_value=1;tex=nodes.new('ShaderNodeTexImage');tex.image=image;tex.interpolation='Closest';mat.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color']);materials[name]=mat

def finish(obj,name,mat):
 obj.name=name;bpy.context.view_layer.objects.active=obj;obj.select_set(True);bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);obj.data.materials.append(materials[mat]);bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project(island_margin=.03);bpy.ops.object.mode_set(mode='OBJECT');return obj
assets=[]
for i in range(3):
 bpy.ops.object.select_all(action='DESELECT');bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1)
 o=bpy.context.object;rng=random.Random(30+i)
 for v in o.data.vertices:v.co*=rng.uniform(.8,1.15)
 o.scale=[(1.1,.8,.7),(1.5,.65,.5),(.7,.7,1.05)][i];finish(o,'Rock_'+str(i+1),'Rock');low=min(v.co.z for v in o.data.vertices)
 for v in o.data.vertices:v.co.z-=low
 assets.append(o)
for style in [0,1]:
 parts=[];bpy.ops.object.select_all(action='DESELECT');height=4.6 if style==0 else 6.2
 bpy.ops.mesh.primitive_cone_add(vertices=6,radius1=.24,radius2=.13,depth=height*.7,location=(0,0,height*.35));trunk=finish(bpy.context.object,'trunk','Bark');parts.append(trunk)
 if style==0:
  for x,y,z,s in [(-1,0,3.5,1.6),(.8,.2,4.1,1.7),(0,-.7,3.9,1.5)]:
   bpy.ops.object.select_all(action='DESELECT');bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1,location=(x,y,z));o=bpy.context.object;o.scale=(s,s*.8,s*.55);parts.append(finish(o,'canopy','Leaves'))
 else:
  for z,r,d in [(2.8,1.15,2.5),(4.1,.9,2.8),(5.3,.55,2.1)]:
   bpy.ops.object.select_all(action='DESELECT');bpy.ops.mesh.primitive_cone_add(vertices=7,radius1=r,radius2=.05,depth=d,location=(0,0,z));parts.append(finish(bpy.context.object,'canopy','Leaves'))
 bpy.ops.object.select_all(action='DESELECT')
 for o in parts:o.select_set(True)
 bpy.context.view_layer.objects.active=trunk;bpy.ops.object.join();o=bpy.context.object;o.name=['Tree_Pine','Tree_Cypress'][style];scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR');assets.append(o)
manifest=[]
for i,o in enumerate(assets):
 bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o;bpy.context.view_layer.update()
 bpy.ops.export_scene.fbx(filepath=str(ART/(o.name+'.fbx')),use_selection=True,object_types={'MESH'},apply_unit_scale=True,axis_forward='-Z',axis_up='Y',bake_anim=False,add_leaf_bones=False)
 o.data.calc_loop_triangles();manifest.append({'name':o.name,'triangles':len(o.data.loop_triangles),'dimensions':list(o.dimensions),'uv':len(o.data.uv_layers)})
 o.location=(i*4-8,0,0)
bpy.ops.mesh.primitive_plane_add(size=30);floor=bpy.context.object;floor.name='PreviewGround';floor.data.materials.append(materials['BeachSand'])
bpy.ops.object.camera_add(location=(12,-19,12));cam=bpy.context.object;cam.rotation_euler=(Vector((0,0,2))-cam.location).to_track_quat('-Z','Y').to_euler();scene.camera=cam;cam.data.type='ORTHO';cam.data.ortho_scale=24
bpy.ops.object.light_add(type='SUN',location=(0,-5,10));bpy.context.object.rotation_euler=(.4,-.5,-.4);bpy.context.object.data.energy=2
scene.world=bpy.data.worlds.new('KitWorld');scene.world.use_nodes=True;scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.55,.65,.75,1);scene.world.node_tree.nodes['Background'].inputs[1].default_value=.7
scene.render.engine='BLENDER_EEVEE';scene.render.resolution_x=1200;scene.render.resolution_y=700;scene.render.resolution_percentage=100;scene.render.filepath=str(OUT/'kit-blender.png');scene.view_settings.view_transform='Standard'
(SRC/'manifest.json').write_text(json.dumps({'models':manifest,'textures':{k:32 if k in ['Bark','Leaves'] else 64 for k in colors}},indent=2))
bpy.data.libraries.write(str(SRC/'KitPS1_022.blend'),{scene},fake_user=True)
bpy.ops.render.render(write_still=True)
result={'source':str(SRC/'KitPS1_022.blend'),'manifest':manifest}
