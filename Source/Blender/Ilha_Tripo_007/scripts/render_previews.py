"""Presentation and inspection renders; run in Blender."""
import bpy,json,time
from pathlib import Path

OUT=Path(__file__).resolve().parents[1]
scene=bpy.context.scene
scene.render.film_transparent=False
wn=scene.world.node_tree.nodes;wl=scene.world.node_tree.links
env=wn.get('Background');env.inputs['Strength'].default_value=.65
bg=wn.get('Presentation Background') or wn.new('ShaderNodeBackground');bg.name='Presentation Background';bg.inputs['Color'].default_value=(.025,.032,.037,1)
ray=wn.get('Camera Background Only') or wn.new('ShaderNodeLightPath');ray.name='Camera Background Only'
mix=wn.get('Environment / Backdrop') or wn.new('ShaderNodeMixShader');mix.name='Environment / Backdrop'
wl.new(ray.outputs['Is Camera Ray'],mix.inputs[0]);wl.new(env.outputs[0],mix.inputs[1]);wl.new(bg.outputs[0],mix.inputs[2]);wl.new(mix.outputs[0],wn.get('World Output').inputs['Surface'])
scene.cycles.samples=48
views=[('General','Camera_General',1440,1080),('Top','Camera_Top',1200,1200),('Path_Detail','Camera_Path_Detail',1200,900),('Cliff_Detail','Camera_Cliff_Detail',1200,900),('Beach_Detail','Camera_Beach_Detail',1200,900)]
for label,camera,w,h in views:
    (OUT/'progress.json').write_text(json.dumps({'stage':'rendering_'+label,'time':time.time()}))
    scene.camera=bpy.data.objects[camera]
    scene.render.resolution_x=w;scene.render.resolution_y=h
    scene.render.filepath=str(OUT/f'previews/{label}.png')
    bpy.ops.render.render(write_still=True)
scene.camera=bpy.data.objects['Camera_General']
scene.render.resolution_x=1440;scene.render.resolution_y=1080
scene.render.filepath=str(OUT/'previews/General.png')
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'Ilha_Tripo_Texturizada.blend'))
(OUT/'progress.json').write_text(json.dumps({'stage':'previews_ready','time':time.time()}))
