"""
mcp_process_realistic_assets.py
Uses live Blender MCP server to take authentic NASA high-poly 3D models and upgrade them
with AAA mainstream FPS PBR materials, tactical armor rigs, emissive nodes, and docking targets.
"""

import socket
import json
import os

OUTPUT_DIR = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "public", "assets", "models")).replace("\\", "/")

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

def process_realistic_astronaut():
    print(">>> Enhancing Official NASA Astronaut with AAA PBR & Avengers Tactical Rig...")
    in_path = f"{OUTPUT_DIR}/nasa_astronaut.glb"
    out_path = f"{OUTPUT_DIR}/astronaut_operative.glb"

    code = f'''
import bpy
import math

bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
for b in bpy.data.meshes: bpy.data.meshes.remove(b, do_unlink=True)
for b in bpy.data.materials: bpy.data.materials.remove(b, do_unlink=True)

# Import Official NASA Astronaut
bpy.ops.import_scene.gltf(filepath="{in_path}")

# Enhance PBR materials on imported meshes
gold_visor_mat = bpy.data.materials.new("AAA_VisorGold")
gold_visor_mat.use_nodes = True
nodes = gold_visor_mat.node_tree.nodes
bsdf = next(n for n in nodes if n.type == "BSDF_PRINCIPLED")
bsdf.inputs['Base Color'].default_value = (0.98, 0.76, 0.15, 1.0)
bsdf.inputs['Metallic'].default_value = 0.98
bsdf.inputs['Roughness'].default_value = 0.04

cyan_glow = bpy.data.materials.new("AAA_CyanGlow")
cyan_glow.use_nodes = True
bsdf_cyan = next(n for n in cyan_glow.node_tree.nodes if n.type == "BSDF_PRINCIPLED")
if 'Emission Color' in bsdf_cyan.inputs:
    bsdf_cyan.inputs['Emission Color'].default_value = (0.0, 0.95, 1.0, 1.0)
    bsdf_cyan.inputs['Emission Strength'].default_value = 6.0

armor_carbon = bpy.data.materials.new("AAA_CarbonArmor")
armor_carbon.use_nodes = True
bsdf_armor = next(n for n in armor_carbon.node_tree.nodes if n.type == "BSDF_PRINCIPLED")
bsdf_armor.inputs['Base Color'].default_value = (0.08, 0.10, 0.13, 1.0)
bsdf_armor.inputs['Metallic'].default_value = 0.85
bsdf_armor.inputs['Roughness'].default_value = 0.22

# Add Avengers / Marvel Tactical Arc Reactor Core on Chest
bpy.ops.mesh.primitive_torus_add(major_radius=0.10, minor_radius=0.02, location=(0, -0.22, 1.32), rotation=(math.radians(90), 0, 0))
arc_core = bpy.context.active_object
arc_core.name = "Avengers_ECLSS_Core"
arc_core.data.materials.append(cyan_glow)

# Add Tactical Shoulder Armor Guards (PUBG/Free Fire Operative style)
for side in (-1, 1):
    bpy.ops.mesh.primitive_uv_sphere_add(radius=0.16, location=(side * 0.44, 0.02, 1.48))
    pauldron = bpy.context.active_object
    pauldron.name = f"Tactical_Pauldron_{{side}}"
    pauldron.scale = (1.1, 0.85, 0.85)
    pauldron.data.materials.append(armor_carbon)

# Add Dual Headlamp Fixtures with high intensity beams
for side in (-1, 1):
    bpy.ops.mesh.primitive_cylinder_add(radius=0.035, depth=0.10, location=(side * 0.28, -0.06, 1.88), rotation=(math.radians(90), 0, 0))
    lamp = bpy.context.active_object
    lamp.name = f"Tactical_Headlamp_{{side}}"
    lamp.data.materials.append(cyan_glow)

# Export enhanced operative model
bpy.ops.object.select_all(action='SELECT')
bpy.ops.export_scene.gltf(filepath="{out_path}", export_format='GLB', use_selection=False)
result = {{"path": "{out_path}", "status": "AAA_ASTRONAUT_EXPORTED", "total_objects": len(bpy.data.objects)}}
'''
    res = execute_on_blender(code)
    print("Realistic Astronaut result:", res)

def process_realistic_iss():
    print(">>> Enhancing Official NASA ISS with High-Fidelity Solar Array PBR & PMA-2 Target...")
    in_path = f"{OUTPUT_DIR}/nasa_iss.glb"
    out_path = f"{OUTPUT_DIR}/iss_orbital_station.glb"

    code = f'''
import bpy
import math

bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
for b in bpy.data.meshes: bpy.data.meshes.remove(b, do_unlink=True)
for b in bpy.data.materials: bpy.data.materials.remove(b, do_unlink=True)

# Import Official NASA ISS
bpy.ops.import_scene.gltf(filepath="{in_path}")

dock_green = bpy.data.materials.new("AAA_DockGreen")
dock_green.use_nodes = True
bsdf_dock = next(n for n in dock_green.node_tree.nodes if n.type == "BSDF_PRINCIPLED")
if 'Emission Color' in bsdf_dock.inputs:
    bsdf_dock.inputs['Emission Color'].default_value = (0.0, 1.0, 0.4, 1.0)
    bsdf_dock.inputs['Emission Strength'].default_value = 8.0

# Add Illuminated PMA-2 Forward Docking Target Reticle Ring
bpy.ops.mesh.primitive_torus_add(major_radius=0.85, minor_radius=0.06, location=(0, 14.5, 0), rotation=(math.radians(90), 0, 0))
target_ring = bpy.context.active_object
target_ring.name = "PMA2_Docking_Target_Ring"
target_ring.data.materials.append(dock_green)

# Docking Crosshair Guides
for angle in [0, 90, 180, 270]:
    rad = math.radians(angle)
    px = 1.05 * math.cos(rad)
    pz = 1.05 * math.sin(rad)
    bpy.ops.mesh.primitive_cube_add(size=0.12, location=(px, 14.5, pz))
    guide = bpy.context.active_object
    guide.name = f"Dock_Guide_{{angle}}"
    guide.data.materials.append(dock_green)

bpy.ops.object.select_all(action='SELECT')
bpy.ops.export_scene.gltf(filepath="{out_path}", export_format='GLB', use_selection=False)
result = {{"path": "{out_path}", "status": "AAA_ISS_EXPORTED", "total_objects": len(bpy.data.objects)}}
'''
    res = execute_on_blender(code)
    print("Realistic ISS result:", res)

def process_realistic_arena():
    print(">>> Enhancing Official NASA Habitat with Hologram Gate Rings & Emergency Valve...")
    in_path = f"{OUTPUT_DIR}/nasa_habitat.glb"
    out_path = f"{OUTPUT_DIR}/zero_g_tactical_arena.glb"

    code = f'''
import bpy
import math

bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
for b in bpy.data.meshes: bpy.data.meshes.remove(b, do_unlink=True)
for b in bpy.data.materials: bpy.data.materials.remove(b, do_unlink=True)

# Import Official NASA Habitat Unit
bpy.ops.import_scene.gltf(filepath="{in_path}")

# Scale and orient habitat as massive training module
for o in bpy.data.objects:
    o.scale = (1.8, 1.8, 1.8)

def make_emissive(name, color, strength=8.0):
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    bsdf = next(n for n in mat.node_tree.nodes if n.type == "BSDF_PRINCIPLED")
    bsdf.inputs['Base Color'].default_value = color
    if 'Emission Color' in bsdf.inputs:
        bsdf.inputs['Emission Color'].default_value = color
        bsdf.inputs['Emission Strength'].default_value = strength
    return mat

mat_cyan = make_emissive("HoloCyan", (0.0, 0.94, 1.0, 1.0), 9.0)
mat_orange = make_emissive("HoloOrange", (1.0, 0.45, 0.0, 1.0), 9.0)
mat_green = make_emissive("HoloGreen", (0.1, 1.0, 0.35, 1.0), 9.0)
mat_valve = make_emissive("ValveRed", (0.95, 0.15, 0.10, 1.0), 3.0)

# Add 3 High-Tech Tactical Hologram Navigation Rings along the axis
rings = [
    ("NavRing_01", mat_cyan, (0.0, 6.0, 0.5), 3.0),
    ("NavRing_02", mat_orange, (-1.5, 18.0, -1.0), 2.6),
    ("NavRing_03", mat_green, (1.2, 30.0, 1.2), 2.2),
]
for rname, rmat, rloc, rrad in rings:
    bpy.ops.mesh.primitive_torus_add(major_radius=rrad, minor_radius=0.18, location=rloc, rotation=(math.radians(90), 0, 0))
    robj = bpy.context.active_object
    robj.name = rname
    robj.data.materials.append(rmat)

# Add High-Detail Rotary Emergency Pressure Relief Valve Station
bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 42.0, -2.5))
c_base = bpy.context.active_object
c_base.name = "Valve_Console_Base"
c_base.scale = (2.4, 0.8, 1.8)

bpy.ops.mesh.primitive_torus_add(major_radius=0.6, minor_radius=0.08, location=(0, 41.5, -1.2), rotation=(math.radians(90), 0, 0))
v_wheel = bpy.context.active_object
v_wheel.name = "Emergency_Valve"
v_wheel.data.materials.append(mat_valve)

for sp in [0, 60, 120]:
    bpy.ops.mesh.primitive_cylinder_add(radius=0.04, depth=1.1, location=(0, 41.5, -1.2), rotation=(0, 0, math.radians(sp)))
    spk = bpy.context.active_object
    spk.data.materials.append(mat_valve)

bpy.ops.object.select_all(action='SELECT')
bpy.ops.export_scene.gltf(filepath="{out_path}", export_format='GLB', use_selection=False)
result = {{"path": "{out_path}", "status": "AAA_ARENA_EXPORTED", "total_objects": len(bpy.data.objects)}}
'''
    res = execute_on_blender(code)
    print("Realistic Arena result:", res)

def main():
    print("=======================================================")
    print("UPGRADING ALL ASSETS TO REALISTIC NASA + FPS FIDELITY")
    print("=======================================================")
    process_realistic_astronaut()
    process_realistic_iss()
    process_realistic_arena()
    print("=======================================================")
    print("REALISTIC ASSET PIPELINE COMPLETE!")
    print("=======================================================")

if __name__ == "__main__":
    main()
