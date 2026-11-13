"""Bake source color into unique UV atlas, keeping the editable material."""
import bpy,json,time,traceback,numpy as np
from pathlib import Path

OUT=Path(__file__).resolve().parents[1]
scene=bpy.context.scene
obj=bpy.data.objects['Island_Tripo']
source=bpy.data.materials['Terrain_Source_Editable']
obj.data.materials.clear();obj.data.materials.append(source)
if bpy.context.object and bpy.context.object.mode!='OBJECT':bpy.ops.object.mode_set(mode='OBJECT')
bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj
image=bpy.data.images.get('Island_BaseColor')
if image is None:image=bpy.data.images.new('Island_BaseColor',width=2048,height=2048,alpha=False)
image.colorspace_settings.name='sRGB'
target=source.node_tree.nodes.get('Atlas Bake Target') or source.node_tree.nodes.new('ShaderNodeTexImage')
target.name='Atlas Bake Target';target.label='2048 Color Atlas — Bake Target';target.image=image;target.location=(1030,50)
for n in source.node_tree.nodes:n.select=False
target.select=True;source.node_tree.nodes.active=target
output=source.node_tree.nodes['Material Output']
source.node_tree.links.new(source.node_tree.nodes['Bake Color Without Lighting'].outputs[0],output.inputs['Surface'])
scene.render.engine='CYCLES';scene.cycles.samples=1
scene.render.bake.margin=6
scene.render.bake.use_clear=True
scene.render.bake.use_selected_to_active=False
scene.render.bake.target='IMAGE_TEXTURES'
bpy.ops.object.bake(type='EMIT')
# Dense AI meshes contain subpixel islands. Extend neighboring baked color into
# unused texels so nearest sampling never reveals the image's black clear color.
rgba=np.empty(image.size[0]*image.size[1]*4,dtype=np.float32)
image.pixels.foreach_get(rgba);rgba=rgba.reshape(image.size[1],image.size[0],4)
valid=np.max(rgba[:,:,:3],axis=2)>1e-8
initial_empty=int(np.count_nonzero(~valid))
padding_passes=0
for _ in range(512):
    if valid.all():break
    padding_passes+=1
    for dy,dx in [(1,0),(-1,0),(0,1),(0,-1)]:
        neighbor=np.roll(valid,(dy,dx),(0,1));take=(~valid)&neighbor
        if take.any():
            rgba[take]=np.roll(rgba,(dy,dx),(0,1))[take];valid[take]=True
image.pixels.foreach_set(rgba.reshape(-1));image.update()
(OUT/'padding.json').write_text(json.dumps({'method':'Iterative nearest neighboring baked-color extension until all empty texels are filled','passes':padding_passes,'empty_pixels_before':initial_empty,'empty_pixels_after':int(np.count_nonzero(~valid))},indent=2))
image.filepath_raw=str(OUT/'textures/Island_BaseColor.png');image.file_format='PNG';image.save()
source.node_tree.links.new(source.node_tree.nodes['Matte Terrain Preview'].outputs[0],output.inputs['Surface'])
source.use_fake_user=True
final=bpy.data.materials.get('Island_Terrain_Atlas') or bpy.data.materials.new('Island_Terrain_Atlas')
final.use_nodes=True
nodes=final.node_tree.nodes;nodes.clear()
tex=nodes.new('ShaderNodeTexImage');tex.name='Island Base Color';tex.image=image;tex.interpolation='Closest';tex.extension='EXTEND';tex.location=(-450,200)
uv=nodes.new('ShaderNodeUVMap');uv.uv_map='Island_UV';uv.location=(-650,200)
bsdf=nodes.new('ShaderNodeBsdfPrincipled');bsdf.inputs['Roughness'].default_value=.93;bsdf.inputs['Specular IOR Level'].default_value=.16;bsdf.location=(-100,200)
out=nodes.new('ShaderNodeOutputMaterial');out.location=(220,200)
final.node_tree.links.new(uv.outputs[0],tex.inputs[0]);final.node_tree.links.new(tex.outputs['Color'],bsdf.inputs['Base Color']);final.node_tree.links.new(bsdf.outputs[0],out.inputs['Surface'])
obj.data.materials.clear();obj.data.materials.append(final)
obj['source_material']='Terrain_Source_Editable'
obj['original_glb']='TripoAi/island.glb'
obj['geometry_preserved']=True
scene.cycles.samples=40
scene.camera=bpy.data.objects['Camera_General']
scene.render.resolution_x=1440;scene.render.resolution_y=1080
for image in bpy.data.images:
    if image.source=='FILE' or image.name=='Island_BaseColor':
        if image.filepath:
            image.filepath=bpy.path.relpath(image.filepath,start=str(OUT));image.pack()
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'Ilha_Tripo_Texturizada.blend'))
print('Baked and packed Island_BaseColor; final atlas material applied.')
