"""
mcp_asset_generator.py
Sends 3D modeling and PBR texturing code directly to the live Blender MCP server
running on localhost:9876.
Generates:
1. public/assets/models/astronaut_operative.glb
2. public/assets/models/zero_g_tactical_arena.glb
3. public/assets/models/falcon_cockpit.glb
4. public/assets/models/iss_orbital_station.glb
"""

import socket
import json
import os
import sys

OUTPUT_DIR = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "public", "assets", "models"))
os.makedirs(OUTPUT_DIR, exist_ok=True)

def execute_on_blender(code_str: str) -> dict:
    req = json.dumps({
        "type": "execute",
        "code": code_str,
        "strict_json": True
    }) + "\0"

    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.settimeout(120.0)
    s.connect(("localhost", 9876))
    s.sendall(req.encode("utf-8"))

    buf = bytearray()
    while True:
        chunk = s.recv(65536)
        if not chunk:
            break
        buf.extend(chunk)
        if b"\0" in buf:
            break
    s.close()

    raw_resp = buf.split(b"\0")[0].decode("utf-8")
    resp = json.loads(raw_resp)
    if resp.get("status") != "ok":
        print(f"Error from Blender: {resp.get('message')}")
        if "stderr" in resp:
            print(f"Stderr: {resp.get('stderr')}")
        raise RuntimeError(resp.get("message"))
    return resp.get("result", {})

def generate_astronaut():
    print(">>> Generating Astronaut Operative via Blender MCP...")
    out_path = os.path.join(OUTPUT_DIR, "astronaut_operative.glb").replace("\\", "/")
    code = f'''
import bpy
import bmesh
import math

# Wipe scene
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
for b in bpy.data.meshes: bpy.data.meshes.remove(b, do_unlink=True)
for b in bpy.data.materials: bpy.data.materials.remove(b, do_unlink=True)

def make_mat(name, color, metallic=0.0, roughness=0.5, emission_color=(0,0,0,1), emission_strength=0.0):
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    nodes = mat.node_tree.nodes
    nodes.clear()
    bsdf = nodes.new(type='ShaderNodeBsdfPrincipled')
    bsdf.inputs['Base Color'].default_value = color
    bsdf.inputs['Metallic'].default_value = metallic
    bsdf.inputs['Roughness'].default_value = roughness
    if 'Emission Color' in bsdf.inputs:
        bsdf.inputs['Emission Color'].default_value = emission_color
        bsdf.inputs['Emission Strength'].default_value = emission_strength
    out = nodes.new(type='ShaderNodeOutputMaterial')
    mat.node_tree.links.new(bsdf.outputs['BSDF'], out.inputs['Surface'])
    return mat

mat_suit = make_mat("Suit", (0.88, 0.90, 0.94, 1.0), 0.1, 0.35)
mat_armor = make_mat("Armor", (0.10, 0.12, 0.16, 1.0), 0.8, 0.25)
mat_gold = make_mat("Visor", (0.95, 0.72, 0.12, 1.0), 0.98, 0.05)
mat_cyan = make_mat("CyanGlow", (0.0, 0.94, 1.0, 1.0), 0.1, 0.2, (0.0, 0.94, 1.0, 1.0), 6.0)
mat_orange = make_mat("OrangeGlow", (1.0, 0.45, 0.05, 1.0), 0.1, 0.2, (1.0, 0.45, 0.05, 1.0), 5.0)

# Torso
bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0, 1.25))
torso = bpy.context.active_object
torso.scale = (0.55, 0.35, 0.75)
torso.data.materials.append(mat_suit)

# Chest plate
bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, -0.18, 1.35))
chest = bpy.context.active_object
chest.scale = (0.50, 0.12, 0.45)
chest.data.materials.append(mat_armor)

# Arc Core
bpy.ops.mesh.primitive_torus_add(major_radius=0.12, minor_radius=0.025, location=(0, -0.25, 1.35), rotation=(math.radians(90), 0, 0))
core = bpy.context.active_object
core.data.materials.append(mat_cyan)

# Belt
bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0, 0.90))
belt = bpy.context.active_object
belt.scale = (0.58, 0.38, 0.10)
belt.data.materials.append(mat_armor)

# Helmet
bpy.ops.mesh.primitive_uv_sphere_add(segments=24, ring_count=16, radius=0.32, location=(0, 0, 1.85))
helmet = bpy.context.active_object
helmet.data.materials.append(mat_suit)

# Visor
bpy.ops.mesh.primitive_uv_sphere_add(segments=24, ring_count=16, radius=0.28, location=(0, -0.10, 1.86))
visor = bpy.context.active_object
visor.scale = (0.9, 0.7, 0.75)
visor.data.materials.append(mat_gold)

# Headlamps
for s in (-1, 1):
    bpy.ops.mesh.primitive_cylinder_add(radius=0.04, depth=0.12, location=(s * 0.32, -0.05, 1.95), rotation=(math.radians(90), 0, 0))
    lamp = bpy.context.active_object
    lamp.data.materials.append(mat_cyan)

# Backpack
bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0.32, 1.30))
plss = bpy.context.active_object
plss.scale = (0.46, 0.26, 0.70)
plss.data.materials.append(mat_armor)

for s in (-1, 1):
    bpy.ops.mesh.primitive_cylinder_add(radius=0.08, depth=0.65, location=(s * 0.15, 0.44, 1.30))
    tank = bpy.context.active_object
    tank.data.materials.append(mat_suit)
    bpy.ops.mesh.primitive_cone_add(radius1=0.05, radius2=0.02, depth=0.08, location=(s * 0.15, 0.44, 0.94))
    nozzle = bpy.context.active_object
    nozzle.data.materials.append(mat_orange)

# Shoulders & Arms
for s in (-1, 1):
    bpy.ops.mesh.primitive_uv_sphere_add(radius=0.18, location=(s * 0.42, 0, 1.50))
    p = bpy.context.active_object
    p.scale = (1.1, 0.9, 0.9)
    p.data.materials.append(mat_armor)
    
    bpy.ops.mesh.primitive_cylinder_add(radius=0.10, depth=0.35, location=(s * 0.46, 0, 1.25))
    arm = bpy.context.active_object
    arm.data.materials.append(mat_suit)

    bpy.ops.mesh.primitive_cylinder_add(radius=0.09, depth=0.35, location=(s * 0.46, -0.05, 0.90))
    fore = bpy.context.active_object
    fore.data.materials.append(mat_armor if s == 1 else mat_suit)

    bpy.ops.mesh.primitive_uv_sphere_add(radius=0.09, location=(s * 0.46, -0.05, 0.68))
    glove = bpy.context.active_object
    glove.data.materials.append(mat_armor)

# Legs & Boots
for s in (-1, 1):
    bpy.ops.mesh.primitive_cylinder_add(radius=0.13, depth=0.45, location=(s * 0.20, 0, 0.65))
    thigh = bpy.context.active_object
    thigh.data.materials.append(mat_suit)

    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(s * 0.20, -0.12, 0.42))
    knee = bpy.context.active_object
    knee.scale = (0.16, 0.08, 0.14)
    knee.data.materials.append(mat_armor)

    bpy.ops.mesh.primitive_cylinder_add(radius=0.11, depth=0.40, location=(s * 0.20, 0, 0.22))
    shin = bpy.context.active_object
    shin.data.materials.append(mat_suit)

    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(s * 0.20, -0.08, 0.06))
    boot = bpy.context.active_object
    boot.scale = (0.18, 0.32, 0.12)
    boot.data.materials.append(mat_armor)

# Apply transforms and export
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
bpy.ops.export_scene.gltf(filepath="{out_path}", export_format='GLB', use_selection=False)
result = {{"path": "{out_path}", "status": "CREATED", "object_count": len(bpy.data.objects)}}
'''
    res = execute_on_blender(code)
    print("Astronaut Operative result:", res)

def generate_arena():
    print(">>> Generating Zero-G Tactical Arena via Blender MCP...")
    out_path = os.path.join(OUTPUT_DIR, "zero_g_tactical_arena.glb").replace("\\", "/")
    code = f'''
import bpy
import bmesh
import math

bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
for b in bpy.data.meshes: bpy.data.meshes.remove(b, do_unlink=True)
for b in bpy.data.materials: bpy.data.materials.remove(b, do_unlink=True)

def make_mat(name, color, metallic=0.0, roughness=0.5, emission_color=(0,0,0,1), emission_strength=0.0):
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    nodes = mat.node_tree.nodes
    nodes.clear()
    bsdf = nodes.new(type='ShaderNodeBsdfPrincipled')
    bsdf.inputs['Base Color'].default_value = color
    bsdf.inputs['Metallic'].default_value = metallic
    bsdf.inputs['Roughness'].default_value = roughness
    if 'Emission Color' in bsdf.inputs:
        bsdf.inputs['Emission Color'].default_value = emission_color
        bsdf.inputs['Emission Strength'].default_value = emission_strength
    out = nodes.new(type='ShaderNodeOutputMaterial')
    mat.node_tree.links.new(bsdf.outputs['BSDF'], out.inputs['Surface'])
    return mat

mat_wall = make_mat("Wall", (0.18, 0.20, 0.24, 1.0), 0.7, 0.35)
mat_rib = make_mat("Rib", (0.08, 0.09, 0.12, 1.0), 0.8, 0.25)
mat_yellow = make_mat("Hazard", (0.95, 0.75, 0.05, 1.0), 0.3, 0.35)
mat_cyan = make_mat("RingCyan", (0.0, 0.92, 1.0, 1.0), 0.1, 0.1, (0.0, 0.92, 1.0, 1.0), 8.0)
mat_orange = make_mat("RingOrange", (1.0, 0.45, 0.0, 1.0), 0.1, 0.1, (1.0, 0.45, 0.0, 1.0), 8.0)
mat_green = make_mat("RingGreen", (0.1, 1.0, 0.3, 1.0), 0.1, 0.1, (0.1, 1.0, 0.3, 1.0), 8.0)
mat_valve = make_mat("ValveRed", (0.95, 0.15, 0.10, 1.0), 0.5, 0.25, (0.95, 0.15, 0.10, 1.0), 2.5)

# 1. Arena Cylinder
bpy.ops.mesh.primitive_cylinder_add(vertices=16, radius=7.0, depth=50.0, location=(0, 20.0, 0), rotation=(math.radians(90), 0, 0))
arena = bpy.context.active_object
arena.data.materials.append(mat_wall)

# Ribs
for y_pos in [-2.0, 8.0, 18.0, 28.0, 38.0]:
    bpy.ops.mesh.primitive_torus_add(major_radius=6.8, minor_radius=0.35, location=(0, y_pos, 0), rotation=(math.radians(90), 0, 0))
    rib = bpy.context.active_object
    rib.data.materials.append(mat_rib)

# 3 Nav Target Rings
rings = [
    ("Ring01", mat_cyan, (0.0, 6.0, 0.5), 3.0),
    ("Ring02", mat_orange, (-1.5, 18.0, -1.0), 2.6),
    ("Ring03", mat_green, (1.2, 30.0, 1.2), 2.2),
]

for name, rmat, loc, radius in rings:
    bpy.ops.mesh.primitive_torus_add(major_radius=radius, minor_radius=0.18, location=loc, rotation=(math.radians(90), 0, 0))
    robj = bpy.context.active_object
    robj.name = name
    robj.data.materials.append(rmat)

# Valve Console at End
bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 42.0, -2.5))
console = bpy.context.active_object
console.scale = (2.2, 0.8, 1.8)
console.data.materials.append(mat_rib)

bpy.ops.mesh.primitive_cylinder_add(radius=0.25, depth=4.0, location=(0, 42.0, -1.2), rotation=(0, math.radians(90), 0))
pipe = bpy.context.active_object
pipe.data.materials.append(mat_yellow)

bpy.ops.mesh.primitive_torus_add(major_radius=0.6, minor_radius=0.08, location=(0, 41.5, -1.2), rotation=(math.radians(90), 0, 0))
wheel = bpy.context.active_object
wheel.name = "Emergency_Valve"
wheel.data.materials.append(mat_valve)

for sp in [0, 60, 120]:
    bpy.ops.mesh.primitive_cylinder_add(radius=0.04, depth=1.1, location=(0, 41.5, -1.2), rotation=(0, 0, math.radians(sp)))
    spk = bpy.context.active_object
    spk.data.materials.append(mat_valve)

bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
bpy.ops.export_scene.gltf(filepath="{out_path}", export_format='GLB', use_selection=False)
result = {{"path": "{out_path}", "status": "CREATED", "object_count": len(bpy.data.objects)}}
'''
    res = execute_on_blender(code)
    print("Arena result:", res)

def generate_cockpit():
    print(">>> Generating Falcon Cockpit via Blender MCP...")
    out_path = os.path.join(OUTPUT_DIR, "falcon_cockpit.glb").replace("\\", "/")
    code = f'''
import bpy
import bmesh
import math

bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
for b in bpy.data.meshes: bpy.data.meshes.remove(b, do_unlink=True)
for b in bpy.data.materials: bpy.data.materials.remove(b, do_unlink=True)

def make_mat(name, color, metallic=0.0, roughness=0.5, emission_color=(0,0,0,1), emission_strength=0.0):
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    nodes = mat.node_tree.nodes
    nodes.clear()
    bsdf = nodes.new(type='ShaderNodeBsdfPrincipled')
    bsdf.inputs['Base Color'].default_value = color
    bsdf.inputs['Metallic'].default_value = metallic
    bsdf.inputs['Roughness'].default_value = roughness
    if 'Emission Color' in bsdf.inputs:
        bsdf.inputs['Emission Color'].default_value = emission_color
        bsdf.inputs['Emission Strength'].default_value = emission_strength
    out = nodes.new(type='ShaderNodeOutputMaterial')
    mat.node_tree.links.new(bsdf.outputs['BSDF'], out.inputs['Surface'])
    return mat

mat_carbon = make_mat("Carbon", (0.08, 0.09, 0.11, 1.0), 0.7, 0.35)
mat_seat = make_mat("Seat", (0.14, 0.16, 0.20, 1.0), 0.2, 0.7)
mat_hud = make_mat("HUD", (0.0, 0.4, 0.8, 1.0), 0.1, 0.1, (0.0, 0.8, 1.0, 1.0), 5.0)
mat_warn = make_mat("WarnScreen", (0.8, 0.2, 0.0, 1.0), 0.1, 0.1, (1.0, 0.3, 0.0, 1.0), 6.0)
mat_frame = make_mat("Frame", (0.22, 0.25, 0.30, 1.0), 0.8, 0.2)

# Seat
bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0, 0.5))
seat_back = bpy.context.active_object
seat_back.scale = (0.70, 0.20, 1.1)
seat_back.rotation_euler = (math.radians(-15), 0, 0)
seat_back.data.materials.append(mat_seat)

bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0.35, 0.15))
seat_base = bpy.context.active_object
seat_base.scale = (0.68, 0.65, 0.25)
seat_base.data.materials.append(mat_seat)

# Console
bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 1.35, 0.85))
dash = bpy.context.active_object
dash.scale = (2.2, 0.65, 0.70)
dash.rotation_euler = (math.radians(25), 0, 0)
dash.data.materials.append(mat_carbon)

# 3 MFD displays
for i, ox in enumerate([-0.65, 0.0, 0.65]):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(ox, 1.15, 0.95))
    screen = bpy.context.active_object
    screen.scale = (0.50, 0.04, 0.35)
    screen.rotation_euler = (math.radians(25), 0, 0)
    screen.data.materials.append(mat_warn if i == 1 else mat_hud)

for s in (-1, 1):
    bpy.ops.mesh.primitive_cylinder_add(radius=0.03, depth=0.35, location=(s * 0.45, 0.75, 0.45), rotation=(math.radians(10), 0, 0))
    stick = bpy.context.active_object
    stick.data.materials.append(mat_frame)

for ya in [0.5, 1.8]:
    bpy.ops.mesh.primitive_torus_add(major_radius=1.6, minor_radius=0.12, location=(0, ya, 0.8), rotation=(0, math.radians(90), 0))
    arch = bpy.context.active_object
    arch.data.materials.append(mat_frame)

bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
bpy.ops.export_scene.gltf(filepath="{out_path}", export_format='GLB', use_selection=False)
result = {{"path": "{out_path}", "status": "CREATED", "object_count": len(bpy.data.objects)}}
'''
    res = execute_on_blender(code)
    print("Cockpit result:", res)

def generate_iss():
    print(">>> Generating ISS Orbital Station via Blender MCP...")
    out_path = os.path.join(OUTPUT_DIR, "iss_orbital_station.glb").replace("\\", "/")
    code = f'''
import bpy
import bmesh
import math

bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
for b in bpy.data.meshes: bpy.data.meshes.remove(b, do_unlink=True)
for b in bpy.data.materials: bpy.data.materials.remove(b, do_unlink=True)

def make_mat(name, color, metallic=0.0, roughness=0.5, emission_color=(0,0,0,1), emission_strength=0.0):
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    nodes = mat.node_tree.nodes
    nodes.clear()
    bsdf = nodes.new(type='ShaderNodeBsdfPrincipled')
    bsdf.inputs['Base Color'].default_value = color
    bsdf.inputs['Metallic'].default_value = metallic
    bsdf.inputs['Roughness'].default_value = roughness
    if 'Emission Color' in bsdf.inputs:
        bsdf.inputs['Emission Color'].default_value = emission_color
        bsdf.inputs['Emission Strength'].default_value = emission_strength
    out = nodes.new(type='ShaderNodeOutputMaterial')
    mat.node_tree.links.new(bsdf.outputs['BSDF'], out.inputs['Surface'])
    return mat

mat_hull = make_mat("Hull", (0.85, 0.88, 0.92, 1.0), 0.7, 0.25)
mat_solar = make_mat("SolarGold", (0.95, 0.65, 0.10, 1.0), 0.9, 0.15, (0.95, 0.65, 0.10, 1.0), 2.0)
mat_truss = make_mat("Truss", (0.35, 0.38, 0.42, 1.0), 0.8, 0.3)
mat_dock = make_mat("DockGlow", (0.0, 1.0, 0.4, 1.0), 0.2, 0.2, (0.0, 1.0, 0.4, 1.0), 8.0)

# Main spine
bpy.ops.mesh.primitive_cylinder_add(radius=2.1, depth=24.0, location=(0, 0, 0), rotation=(math.radians(90), 0, 0))
spine = bpy.context.active_object
spine.data.materials.append(mat_hull)

# Transverse Lab
bpy.ops.mesh.primitive_cylinder_add(radius=2.0, depth=14.0, location=(0, 4.0, 0), rotation=(0, math.radians(90), 0))
lab = bpy.context.active_object
lab.data.materials.append(mat_hull)

# Integrated Truss Structure
bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0, 3.5))
truss = bpy.context.active_object
truss.scale = (42.0, 1.4, 1.4)
truss.data.materials.append(mat_truss)

# Solar Arrays
for s in (-1, 1):
    for a_idx, xp in enumerate([10.0 * s, 17.0 * s]):
        bpy.ops.mesh.primitive_cube_add(size=1.0, location=(xp, 0, 3.5))
        pan = bpy.context.active_object
        pan.scale = (4.5, 14.0, 0.08)
        pan.rotation_euler = (0, 0, math.radians(15 * s))
        pan.data.materials.append(mat_solar)

# Cupola
bpy.ops.mesh.primitive_cone_add(vertices=7, radius1=1.8, radius2=1.2, depth=1.4, location=(0, 2.0, -2.4), rotation=(math.radians(180), 0, 0))
cupola = bpy.context.active_object
cupola.data.materials.append(mat_hull)

# PMA-2 Docking Port & Target Ring
bpy.ops.mesh.primitive_cylinder_add(radius=1.2, depth=2.0, location=(0, 13.0, 0), rotation=(math.radians(90), 0, 0))
dp = bpy.context.active_object
dp.data.materials.append(mat_hull)

bpy.ops.mesh.primitive_torus_add(major_radius=0.9, minor_radius=0.08, location=(0, 14.05, 0), rotation=(math.radians(90), 0, 0))
dring = bpy.context.active_object
dring.name = "PMA2_Docking_Target"
dring.data.materials.append(mat_dock)

bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
bpy.ops.export_scene.gltf(filepath="{out_path}", export_format='GLB', use_selection=False)
result = {{"path": "{out_path}", "status": "CREATED", "object_count": len(bpy.data.objects)}}
'''
    res = execute_on_blender(code)
    print("ISS result:", res)

def main():
    print("==========================================")
    print("EXECUTING ASSET GENERATION ON BLENDER MCP")
    print("==========================================")
    generate_astronaut()
    generate_arena()
    generate_cockpit()
    generate_iss()
    print("==========================================")
    print("ALL 4 ASSETS GENERATED VIA BLENDER MCP!")
    print("==========================================")

if __name__ == "__main__":
    main()
