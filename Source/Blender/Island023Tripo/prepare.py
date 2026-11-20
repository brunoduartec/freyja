from pathlib import Path
from mathutils import Vector, Matrix
import bpy, bmesh, json, math

base = Path('/Users/bruno.duartec/Documents/Projects/Freyja')
source = base / 'Source/Blender/Island023Tripo'
exports = base / 'Freyja/Assets/_Project/Art/Island023Tripo'
preview = base / 'Artifacts/Island023/Tripo'
items = sorted((base/'TripoAi/cenario').glob('*.glb'), key=lambda p:int(p.name.split()[0]))
scene = bpy.data.scenes.new('Peças Tripo da ilha')
bpy.context.window.scene = scene
scene.unit_settings.system='METRIC'
scene.render.engine='BLENDER_WORKBENCH'
scene.display.shading.light='STUDIO'
scene.display.shading.color_type='TEXTURE'
scene.display.shading.show_shadows=False
scene.display.shading.show_cavity=True
scene.display.shading.background_type='WORLD'
scene.world=bpy.data.worlds.new('Fundo da prancha')
scene.world.color=(.25,.25,.25)
scene.render.resolution_x=224
scene.render.resolution_y=224
scene.render.resolution_percentage=100
cam=bpy.data.objects.new('Prévia',bpy.data.cameras.new('Prévia'))
scene.collection.objects.link(cam)
scene.camera=cam
cam.data.type='ORTHO'
cam.data.ortho_scale=1.6
assets=[]
models=[]
for file in items:
    number = int(file.name.split()[0])
    key = f'Piece{number:02}'
    before=set(bpy.data.objects)
    bpy.ops.import_scene.gltf(filepath=str(file))
    meshes=[o for o in set(bpy.data.objects)-before if o.type=='MESH']
    bpy.ops.object.select_all(action='DESELECT')
    for obj in meshes:obj.select_set(True)
    obj=meshes[0]
    bpy.context.view_layer.objects.active=obj
    if len(meshes)>1:bpy.ops.object.join()
    obj.name=key
    bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    material=obj.active_material
    bs=next(n for n in material.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
    tex=bs.inputs['Base Color'].links[0].from_node
    material.node_tree.nodes.active=tex
    original=len(obj.data.polygons)
    if number != 11:
        welded=bmesh.new()
        welded.from_mesh(obj.data)
        bmesh.ops.remove_doubles(welded,verts=list(welded.verts),dist=.000002)
        welded.to_mesh(obj.data)
        welded.free()
        modifier=obj.modifiers.new('Redução para a montagem', 'DECIMATE')
        modifier.ratio=min(1, 26000 / len(obj.data.polygons))
        bpy.ops.object.modifier_apply(modifier=modifier.name)
    img=tex.image.copy()
    img.name=key
    img.scale(256,256)
    img.filepath_raw=str(exports/(key+'.png'))
    img.file_format='PNG'
    img.save()
    img.pack()
    tex.image=img
    tex.interpolation='Closest'
    for socket in ('Metallic','Roughness','Normal'):
        for link in list(bs.inputs[socket].links):material.node_tree.links.remove(link)
    bs.inputs['Metallic'].default_value=0
    bs.inputs['Roughness'].default_value=.95
    # Export exactly the selected mesh with original axes. Unity wrapper resolves heading per piece.
    bpy.ops.export_scene.fbx(filepath=str(exports/(key+'.fbx')),use_selection=True,axis_forward='-Z',axis_up='Y',object_types={'MESH'},bake_anim=False)
    assets.append({'id':number,'source':file.name,'original_faces':original,'faces':len(obj.data.polygons),'size':list(obj.dimensions)})
    models.append(obj)
    obj.hide_render=True
for i,obj in enumerate(models):
    obj.location.x=(i%4-1.5)*1.5
    obj.location.y=(i//4)*1.5
    obj.hide_render=False
bpy.data.libraries.write(str(source/'PecasIlha.blend'),{scene},compress=True)
(source/'pieces.json').write_text(json.dumps(assets,ensure_ascii=False,indent=2))
result={'pieces':len(assets),'triangle_count':sum(a['faces'] for a in assets)}
