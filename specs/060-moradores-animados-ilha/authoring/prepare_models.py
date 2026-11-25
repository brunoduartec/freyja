import bpy
import math
from pathlib import Path

project=Path('/Users/bruno.duartec/Documents/Projects/Freyja')
summaries=[]
for slug,asset_name in characters:
    bpy.ops.wm.open_mainfile(filepath=str(project/'Source/Blender/Moradores053'/f'{slug}.blend'))
    models=[o for o in bpy.context.scene.objects if o.type in ('ARMATURE','MESH')]
    rigs=[o for o in models if o.type=='ARMATURE']
    assert len(rigs)==1
    rig=rigs[0]
    rig.name=asset_name+'Rig'
    rig.rotation_euler.z-=math.pi/2
    bpy.context.view_layer.update()
    bpy.ops.object.select_all(action='DESELECT')
    for obj in models:
        obj.select_set(True)
    bpy.context.view_layer.objects.active=rig
    bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
    meshes=[o for o in models if o.type=='MESH']
    output=project/'Freyja/Assets/_Project/Art/Moradores060'/asset_name
    output.mkdir(parents=True,exist_ok=True)
    for obj in meshes:
        for mat in obj.data.materials:
            mat.name=asset_name
            principal=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
            incoming=principal.inputs['Base Color'].links
            assert incoming and incoming[0].from_node.type=='TEX_IMAGE'
            image=incoming[0].from_node.image
            image.filepath_raw=str(output/'BaseColor.png')
            image.file_format='PNG'
            image.save()
    triangles=0
    for obj in meshes:
        obj.data.calc_loop_triangles()
        triangles+=len(obj.data.loop_triangles)
    assert triangles<=30000
    bpy.ops.export_scene.fbx(filepath=str(output/f'{asset_name}.fbx'),use_selection=True,object_types={'ARMATURE','MESH'},add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y',path_mode='STRIP',use_armature_deform_only=False)
    for obj in list(bpy.context.scene.objects):
        if obj not in models:
            bpy.data.objects.remove(obj,do_unlink=True)
    source=project/'Source/Blender/Moradores060'
    source.mkdir(parents=True,exist_ok=True)
    bpy.ops.file.pack_all()
    bpy.ops.wm.save_as_mainfile(filepath=str(source/f'{asset_name}.blend'))
    summaries.append({'character':slug,'asset':asset_name,'triangles':triangles,'rig_rotation':list(rig.rotation_euler)})
result={'models':summaries}
