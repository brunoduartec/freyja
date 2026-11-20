"""Prepare an island building for Unity without changing the original GLB."""
import bpy
import bmesh
import json
from pathlib import Path
from mathutils import Vector


def prepare_plaza(base, number):
    base = Path(base)
    display_name = {
        12: 'venda',
        13: 'oficina',
        14: 'casa lado leste',
        15: 'casa caminho da praia',
        16: 'casa isolada do velho',
        17: 'salao comunitario',
        18: 'armazem em terra',
    }[number]
    key = f'Piece{number:02}'
    source = base / 'TripoAi' / 'cenario' / f'{number} - {display_name}.glb'
    output = base / 'Freyja/Assets/_Project/Art/Island023Tripo'
    evidence = base / 'Artifacts/Island023/Tripo'
    scene = bpy.data.scenes.new(key)
    bpy.context.window.scene = scene
    scene.unit_settings.system = 'METRIC'
    previous = set(bpy.data.objects)
    bpy.ops.import_scene.gltf(filepath=str(source))
    parts = sorted((o for o in set(bpy.data.objects) - previous if o.type == 'MESH'), key=lambda o: o.name)
    if not parts:
        raise ValueError(f'{source.name}: no mesh')
    bpy.ops.object.select_all(action='DESELECT')
    for part in parts:
        part.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    if len(parts) > 1:
        bpy.ops.object.join()
    obj = bpy.context.object
    obj.name = key
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    obj.data.calc_loop_triangles()
    source_triangles = len(obj.data.loop_triangles)
    if source_triangles > 26000:
        welded = bmesh.new()
        welded.from_mesh(obj.data)
        bmesh.ops.remove_doubles(welded, verts=list(welded.verts), dist=.000002)
        welded.to_mesh(obj.data)
        welded.free()
        modifier = obj.modifiers.new('Redução para o Unity', 'DECIMATE')
        modifier.ratio = min(1, 26000 / len(obj.data.polygons))
        bpy.ops.object.modifier_apply(modifier=modifier.name)
    if len(obj.data.materials) != 1:
        raise ValueError(f'{source.name}: expected one color atlas')
    material = obj.active_material
    bsdf = next(n for n in material.node_tree.nodes if n.type == 'BSDF_PRINCIPLED')
    base_color = bsdf.inputs['Base Color'].links[0].from_node
    image = base_color.image.copy()
    image.name = key
    image.scale(256, 256)
    image.filepath_raw = str(output / f'{key}.png')
    image.file_format = 'PNG'
    image.save()
    image.pack()
    base_color.image = image
    base_color.interpolation = 'Closest'
    material.node_tree.nodes.active = base_color
    for socket in ('Metallic', 'Roughness', 'Normal'):
        for link in list(bsdf.inputs[socket].links):
            material.node_tree.links.remove(link)
    bsdf.inputs['Metallic'].default_value = 0
    bsdf.inputs['Roughness'].default_value = .95
    bpy.ops.export_scene.fbx(filepath=str(output / f'{key}.fbx'), use_selection=True,
                             axis_forward='-Z', axis_up='Y', object_types={'MESH'}, bake_anim=False)
    bpy.data.libraries.write(str(Path(__file__).parent / f'{key}.blend'), {scene}, compress=True)

    scene.render.engine = 'BLENDER_WORKBENCH'
    scene.display.shading.light = 'STUDIO'
    scene.display.shading.color_type = 'TEXTURE'
    scene.display.shading.show_shadows = False
    scene.display.shading.show_cavity = True
    scene.world = bpy.data.worlds.new('Fundo para verificar fachada')
    scene.world.color = (.25, .25, .25)
    scene.render.resolution_x = scene.render.resolution_y = 480
    camera = bpy.data.objects.new('Enquadramento de verificação', bpy.data.cameras.new(key))
    scene.collection.objects.link(camera)
    scene.camera = camera
    center = Vector(tuple(sum(v[i] for v in obj.bound_box) / 8 for i in range(3)))
    camera.data.type = 'ORTHO'
    camera.data.ortho_scale = max(obj.dimensions) * 1.28
    for tag, direction in [('sul', (0, -2, .3)), ('norte', (0, 2, .3)), ('leste', (2, 0, .3)), ('oeste', (-2, 0, .3))]:
        camera.location = center + Vector(direction)
        camera.rotation_euler = (center - camera.location).to_track_quat('-Z', 'Y').to_euler()
        scene.render.filepath = str(evidence / f'{key}-{tag}.png')
        bpy.ops.render.render(write_still=True)
    obj.data.calc_loop_triangles()
    report = {'piece': number, 'original_triangles': source_triangles,
              'triangles': len(obj.data.loop_triangles), 'size': list(obj.dimensions), 'source': str(source)}
    (evidence / f'{key}-import.json').write_text(json.dumps(report, indent=2))
    return report
