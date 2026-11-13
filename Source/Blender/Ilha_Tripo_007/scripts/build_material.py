"""Run inside the connected Blender session after UV creation."""
import bpy
from pathlib import Path
from mathutils import Vector

OUT=Path(__file__).resolve().parents[1]
obj=bpy.data.objects['Island_Tripo']
scene=bpy.context.scene
mat=bpy.data.materials.get('Terrain_Source_Editable') or bpy.data.materials.new('Terrain_Source_Editable')
mat.use_nodes=True
nodes=mat.node_tree.nodes
links=mat.node_tree.links
nodes.clear()


def node(kind,name,x,y):
    n=nodes.new(kind);n.name=name;n.label=name;n.location=(x,y);return n


def link(a,b):links.new(a,b)


def math(op,a,b=None,name='Math'):
    n=node('ShaderNodeMath',name,-50,-600);n.operation=op
    for i,v in enumerate([a,b]):
        if v is None:continue
        if isinstance(v,(int,float)):n.inputs[i].default_value=v
        else:link(v,n.inputs[i])
    return n.outputs[0]


def mix(a,b,f,name,x,y):
    n=node('ShaderNodeMixRGB',name,x,y)
    link(a,n.inputs[1]);link(b,n.inputs[2]);link(f,n.inputs[0]);return n.outputs[0]


coord=node('ShaderNodeTexCoord','Object Coordinates',-1400,500)
maskvec=node('ShaderNodeVectorMath','Mask Extent 1.08',-1200,800);maskvec.operation='SCALE';maskvec.inputs[3].default_value=1/1.08
link(coord.outputs['Object'],maskvec.inputs[0])
offset=node('ShaderNodeVectorMath','Mask Center',-1000,800);offset.operation='ADD';offset.inputs[1].default_value=(.5,.5,0)
link(maskvec.outputs[0],offset.inputs[0])
mask=node('ShaderNodeTexImage','Terrain Distribution RGB',-800,800)
mask.image=bpy.data.images.load(str(OUT/'textures/Terrain_Distribution.png'),check_existing=True)
mask.image.colorspace_settings.name='Non-Color';mask.interpolation='Closest';mask.extension='CLIP'
link(offset.outputs[0],mask.inputs['Vector'])
channels=node('ShaderNodeSeparateColor','R Paths / G Beach / B Rock',-550,800);link(mask.outputs['Color'],channels.inputs[0])

textures={}
for i,(name,scale) in enumerate([('Grass',8),('Rock',5),('Dirt',8),('Sand',8)]):
    mapping=node('ShaderNodeVectorMath',name+' Tiling',-1200,400-i*240);mapping.operation='SCALE';mapping.inputs[3].default_value=scale
    link(coord.outputs['Object'],mapping.inputs[0])
    t=node('ShaderNodeTexImage',name+' Pixel Source',-1000,400-i*240)
    t.image=bpy.data.images.load(str(OUT/f'textures/{name}_Pixel.png'),check_existing=True)
    t.interpolation='Closest';t.extension='REPEAT'
    if name=='Rock':t.projection='BOX';t.projection_blend=.15
    link(mapping.outputs[0],t.inputs['Vector']);textures[name]=t.outputs['Color']

geo=node('ShaderNodeNewGeometry','Surface Slope',-780,-650)
normal=node('ShaderNodeSeparateXYZ','Normal Z',-560,-650);link(geo.outputs['Normal'],normal.inputs[0])
position=node('ShaderNodeSeparateXYZ','Height',-560,-950);link(coord.outputs['Object'],position.inputs[0])
slope=node('ShaderNodeMapRange','Grass on Upper Surfaces',-350,50)
link(normal.outputs['Z'],slope.inputs['Value']);slope.inputs['From Min'].default_value=.43;slope.inputs['From Max'].default_value=.91
slope.clamp=True
grassfactor=math('MULTIPLY',slope.outputs[0],math('SUBTRACT',1.0,channels.outputs['Blue'],'Exclude Coastal Outcrops'),'Grass Coverage')
ground=mix(textures['Rock'],textures['Grass'],grassfactor,'Stone / Grass',20,400)
pathSlope=node('ShaderNodeMapRange','Paths Keep Stone Risers',-330,-200);link(normal.outputs['Z'],pathSlope.inputs['Value'])
pathSlope.inputs['From Min'].default_value=.25;pathSlope.inputs['From Max'].default_value=.7
pathfactor=math('MULTIPLY',channels.outputs['Red'],pathSlope.outputs[0],'Path Coverage')
ground=mix(ground,textures['Dirt'],pathfactor,'Paths and Plaza',250,400)
beachheight=node('ShaderNodeMapRange','Low Beach Only',-330,-400);link(position.outputs['Z'],beachheight.inputs['Value'])
beachheight.inputs['From Min'].default_value=.016;beachheight.inputs['From Max'].default_value=.032
beachheight.inputs['To Min'].default_value=1;beachheight.inputs['To Max'].default_value=0
beachslope=node('ShaderNodeMapRange','Sand on Beach Floor',-330,-650);link(normal.outputs['Z'],beachslope.inputs['Value'])
beachslope.inputs['From Min'].default_value=.65;beachslope.inputs['From Max'].default_value=.92
beachfactor=math('MULTIPLY',channels.outputs['Green'],beachheight.outputs[0],'Beach Height Coverage')
beachfactor=math('MULTIPLY',beachfactor,beachslope.outputs[0],'Beach Slope Coverage')
beachfactor=math('MULTIPLY',beachfactor,math('SUBTRACT',1.0,channels.outputs['Blue'],'Beach Excludes Outcrops'),'Beach Coverage')
ground=mix(ground,textures['Sand'],beachfactor,'Beach Sand',480,400)
principled=node('ShaderNodeBsdfPrincipled','Matte Terrain Preview',730,400)
principled.inputs['Roughness'].default_value=.93
principled.inputs['Specular IOR Level'].default_value=.16
link(ground,principled.inputs['Base Color'])
output=node('ShaderNodeOutputMaterial','Material Output',1030,400);link(principled.outputs['BSDF'],output.inputs['Surface'])
emission=node('ShaderNodeEmission','Bake Color Without Lighting',730,50);link(ground,emission.inputs['Color'])
mat['bake_color_node']='Bake Color Without Lighting'
mat['note']='Editable source; final material uses Island_BaseColor atlas.'
obj.data.materials.clear();obj.data.materials.append(mat)

# Preserve the initial session but keep default objects out of renders.
for name in ['Cube','Camera','Light']:
    if name in bpy.data.objects:bpy.data.objects[name].hide_render=True


def camera(name,loc,target,scale):
    c=bpy.data.objects.get(name)
    if c is None:
        data=bpy.data.cameras.new(name);c=bpy.data.objects.new(name,data);scene.collection.objects.link(c)
    c.data.type='ORTHO';c.data.ortho_scale=scale
    c.location=loc;c.rotation_euler=(Vector(target)-c.location).to_track_quat('-Z','Y').to_euler()
    return c


camera('Camera_General',(0,-1.15,.90),(0,0,.055),1.17)
camera('Camera_Path_Detail',(-.29,-.48,.36),(-.19,-.035,.055),.37)
camera('Camera_Cliff_Detail',(.4,-.3,.34),(.16,.17,.095),.40)
camera('Camera_Beach_Detail',(.2,-.65,.30),(-.035,-.235,.025),.39)
scene.camera=bpy.data.objects['Camera_General']
scene.render.resolution_x=1440;scene.render.resolution_y=1080;scene.render.resolution_percentage=100
scene.cycles.samples=32
scene.world.use_nodes=True
scene.world.node_tree.nodes['Background'].inputs['Color'].default_value=(.36,.40,.44,1)
scene.world.node_tree.nodes['Background'].inputs['Strength'].default_value=.65
sun=bpy.data.lights['Key_Sun'];sun.energy=3.0;sun.color=(1,.9,.75)
scene.render.film_transparent=True
scene.view_settings.view_transform='AgX'
scene.render.filepath=str(OUT/'previews/Source_General.png')
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'Ilha_Tripo_Texturizada.blend'))
print('Editable source material and presentation cameras prepared.')
