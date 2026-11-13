"""UV creation recipe; run on Island_Tripo inside Blender, before texturing.

The original GLB remains the source of truth. This script changes UVs only.
"""
import bpy

obj=bpy.data.objects['Island_Tripo']
if bpy.context.object and bpy.context.object.mode!='OBJECT':
    bpy.ops.object.mode_set(mode='OBJECT')
bpy.ops.object.select_all(action='DESELECT')
obj.hide_set(False)
obj.select_set(True)
bpy.context.view_layer.objects.active=obj
bpy.ops.object.mode_set(mode='EDIT')
bpy.ops.mesh.select_all(action='SELECT')
bpy.ops.uv.smart_project(angle_limit=1.1519173063162575,
                         island_margin=.003,area_weight=0.0,
                         correct_aspect=True,scale_to_bounds=False)
bpy.ops.object.mode_set(mode='OBJECT')
obj.data.uv_layers.active.name='Island_UV'
