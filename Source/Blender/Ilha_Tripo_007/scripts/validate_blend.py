"""Read-only final verification, run after reopening the saved .blend."""
import bpy,numpy as np,hashlib,json
from pathlib import Path

OUT=Path(__file__).resolve().parents[1]
ROOT=OUT.parents[2]
obj=bpy.data.objects['Island_Tripo'];mesh=obj.data
baseline=json.loads((OUT/'baseline.json').read_text())
v=np.empty(len(mesh.vertices)*3,dtype=np.float32);mesh.vertices.foreach_get('co',v)
idx=np.empty(len(mesh.loops),dtype=np.int32);mesh.loops.foreach_get('vertex_index',idx)
geometry_hash=hashlib.sha256(v.tobytes()+idx.tobytes()).hexdigest()
uv=np.empty(len(mesh.loops)*2,dtype=np.float32);mesh.uv_layers.active.data.foreach_get('uv',uv)
tri=uv.reshape(-1,3,2).astype(np.float64)
a=tri[:,1]-tri[:,0];b=tri[:,2]-tri[:,0]
areas=abs(a[:,0]*b[:,1]-a[:,1]*b[:,0])/2
images=[]
for im in bpy.data.images:
    if im.source not in {'FILE','GENERATED'} or im.name in {'Render Result','Viewer Node'}:continue
    path=Path(bpy.path.abspath(im.filepath)) if im.filepath else None
    images.append({'name':im.name,'size':list(im.size),'packed':bool(im.packed_file),'external_exists':bool(path and path.exists()),'path':im.filepath,'loaded':bool(im.has_data)})
original_hash=hashlib.sha256((ROOT/'TripoAi/island.glb').read_bytes()).hexdigest()
uvcheck=json.loads((OUT/'uv_check.json').read_text())
result={'blender_version':bpy.app.version_string,'filepath':bpy.data.filepath,'scene':bpy.context.scene.name,'active_camera':bpy.context.scene.camera.name,
        'original_glb_sha256':original_hash,'original_unchanged':original_hash==baseline['file_sha256'],
        'geometry_sha256':geometry_hash,'geometry_unchanged':geometry_hash==baseline['geometry_sha256'],
        'transform_unchanged':[list(row) for row in obj.matrix_world]==baseline['matrix'],
        'vertices':len(mesh.vertices),'triangles':len(mesh.polygons),
        'uv':{'name':mesh.uv_layers.active.name,'mapped_faces':len(tri),'finite':bool(np.isfinite(tri).all()),'zero_area_faces':int(np.count_nonzero(areas<1e-15)),
              'min':tri.min((0,1)).tolist(),'max':tri.max((0,1)).tolist(),'area_sum':float(areas.sum()),'overlap_check':uvcheck},
        'material':obj.data.materials[0].name,'images':images,
        'image_interpolation':{mat.name:{n.name:n.interpolation for n in mat.node_tree.nodes if n.type=='TEX_IMAGE'} for mat in [bpy.data.materials['Island_Terrain_Atlas'],bpy.data.materials['Terrain_Source_Editable']]},
        'source_material_retained':bool(bpy.data.materials.get('Terrain_Source_Editable')),
        'previews':[p.name for p in (OUT/'previews').glob('*.png')],
        'limits':['Original dense mesh preserved; no retopology or game performance validation.','No Unity integration or build in this delivery.']}
(OUT/'validation.json').write_text(json.dumps(result,indent=2))
assert result['original_unchanged'] and result['geometry_unchanged'] and result['transform_unchanged']
assert result['uv']['finite'] and result['uv']['zero_area_faces']==0
assert result['uv']['mapped_faces']==580632
assert all(im['packed'] and im['external_exists'] and im['loaded'] for im in images)
print(json.dumps(result))
