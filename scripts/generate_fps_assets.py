"""
generate_fps_assets.py
Headless Blender 4.3 Python script to generate high-fidelity PBR 3D assets for
'Outpost ESM: The Astronaut Odyssey' - Mainstream FPS & Avengers/PUBG style.

Assets generated:
1. public/assets/models/astronaut_operative.glb
2. public/assets/models/zero_g_tactical_arena.glb
3. public/assets/models/falcon_cockpit.glb
4. public/assets/models/iss_orbital_station.glb
"""

# pyrefly: ignore [missing-import]
import bpy
import math
import os

OUTPUT_DIR = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "public", "assets", "models"))
os.makedirs(OUTPUT_DIR, exist_ok=True)

def clear_scene():
    """Wipes all existing objects, meshes, materials, and collections safely without blocking."""
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    for block in bpy.data.meshes:
        bpy.data.meshes.remove(block, do_unlink=True)
    for block in bpy.data.materials:
        bpy.data.materials.remove(block, do_unlink=True)
    for block in bpy.data.armatures:
        bpy.data.armatures.remove(block, do_unlink=True)

def create_pbr_material(name, base_color, metallic=0.0, roughness=0.5, emission_color=(0,0,0,1), emission_strength=0.0):
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    nodes = mat.node_tree.nodes
    nodes.clear()
    
    node_pbr = nodes.new(type='ShaderNodeBsdfPrincipled')
    node_pbr.location = (0, 0)
    node_pbr.inputs['Base Color'].default_value = base_color
    node_pbr.inputs['Metallic'].default_value = metallic
    node_pbr.inputs['Roughness'].default_value = roughness
    if 'Emission Color' in node_pbr.inputs:
        node_pbr.inputs['Emission Color'].default_value = emission_color
        node_pbr.inputs['Emission Strength'].default_value = emission_strength
        
    node_output = nodes.new(type='ShaderNodeOutputMaterial')
    node_output.location = (300, 0)
    mat.node_tree.links.new(node_pbr.outputs['BSDF'], node_output.inputs['Surface'])
    return mat

def assign_material(obj, mat):
    if len(obj.data.materials) == 0:
        obj.data.materials.append(mat)
    else:
        obj.data.materials[0] = mat

# ==========================================
# 1. ASTRONAUT OPERATIVE (Marvel / Tactical FPS)
# ==========================================
def build_astronaut_operative():
    clear_scene()
    print("Building Astronaut Operative (Tactical FPS Style)...")

    # Materials
    mat_suit = create_pbr_material("Mat_Suit", (0.88, 0.90, 0.94, 1.0), metallic=0.15, roughness=0.35)
    mat_armor = create_pbr_material("Mat_Armor", (0.12, 0.14, 0.18, 1.0), metallic=0.7, roughness=0.25)
    mat_visor = create_pbr_material("Mat_Visor", (0.95, 0.72, 0.12, 1.0), metallic=0.95, roughness=0.05)
    mat_glow_cyan = create_pbr_material("Mat_GlowCyan", (0.0, 0.94, 1.0, 1.0), metallic=0.1, roughness=0.2, emission_color=(0.0, 0.94, 1.0, 1.0), emission_strength=5.0)
    mat_glow_orange = create_pbr_material("Mat_GlowOrange", (1.0, 0.45, 0.05, 1.0), metallic=0.1, roughness=0.2, emission_color=(1.0, 0.45, 0.05, 1.0), emission_strength=4.0)

    # 1. Torso Base Suit
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0, 1.25))
    torso = bpy.context.active_object
    torso.name = "Torso_Suit"
    torso.scale = (0.55, 0.35, 0.75)
    bpy.ops.object.transform_apply(scale=True)
    assign_material(torso, mat_suit)

    # Tactical Chest Armor Rig
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, -0.18, 1.35))
    chest_plate = bpy.context.active_object
    chest_plate.name = "Chest_Plate"
    chest_plate.scale = (0.50, 0.12, 0.45)
    bpy.ops.object.transform_apply(scale=True)
    assign_material(chest_plate, mat_armor)

    # Glowing Arc Reactor / ECLSS Heart Status Ring
    bpy.ops.mesh.primitive_torus_add(major_radius=0.12, minor_radius=0.025, location=(0, -0.25, 1.35), rotation=(math.radians(90), 0, 0))
    eclss_core = bpy.context.active_object
    eclss_core.name = "ECLSS_Core_Indicator"
    assign_material(eclss_core, mat_glow_cyan)

    # Tactical Belt & Buckle
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0, 0.90))
    belt = bpy.context.active_object
    belt.name = "Tactical_Belt"
    belt.scale = (0.58, 0.38, 0.10)
    bpy.ops.object.transform_apply(scale=True)
    assign_material(belt, mat_armor)

    # 2. Helmet
    bpy.ops.mesh.primitive_uv_sphere_add(segments=32, ring_count=24, radius=0.32, location=(0, 0, 1.85))
    helmet = bpy.context.active_object
    helmet.name = "Helmet_Shell"
    assign_material(helmet, mat_suit)

    # Gold Reflective Spherical Visor
    bpy.ops.mesh.primitive_uv_sphere_add(segments=32, ring_count=24, radius=0.28, location=(0, -0.10, 1.86))
    visor = bpy.context.active_object
    visor.name = "Visor_Gold"
    visor.scale = (0.9, 0.7, 0.75)
    bpy.ops.object.transform_apply(scale=True)
    assign_material(visor, mat_visor)

    # Helmet Tactical Flashlights (Dual Headlamps)
    for side in (-1, 1):
        bpy.ops.mesh.primitive_cylinder_add(radius=0.04, depth=0.12, location=(side * 0.32, -0.05, 1.95), rotation=(math.radians(90), 0, 0))
        lamp = bpy.context.active_object
        lamp.name = f"Headlamp_{side}"
        assign_material(lamp, mat_glow_cyan)

    # 3. Life Support Backpack (PLSS) & RCS Thruster Pods
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0.32, 1.30))
    plss = bpy.context.active_object
    plss.name = "PLSS_Backpack"
    plss.scale = (0.46, 0.26, 0.70)
    bpy.ops.object.transform_apply(scale=True)
    assign_material(plss, mat_armor)

    # High-pressure O2 Tanks
    for side in (-1, 1):
        bpy.ops.mesh.primitive_cylinder_add(radius=0.08, depth=0.65, location=(side * 0.15, 0.44, 1.30))
        tank = bpy.context.active_object
        tank.name = f"O2_Tank_{side}"
        assign_material(tank, mat_suit)
        # Thruster Nozzle at bottom of tank
        bpy.ops.mesh.primitive_cone_add(radius1=0.05, radius2=0.02, depth=0.08, location=(side * 0.15, 0.44, 0.94))
        nozzle = bpy.context.active_object
        nozzle.name = f"RCS_Nozzle_{side}"
        assign_material(nozzle, mat_glow_orange)

    # 4. Shoulders & Arms
    for side in (-1, 1):
        # Shoulder Armor Pauldron
        bpy.ops.mesh.primitive_uv_sphere_add(radius=0.18, location=(side * 0.42, 0, 1.50))
        pauldron = bpy.context.active_object
        pauldron.name = f"Pauldron_{side}"
        pauldron.scale = (1.1, 0.9, 0.9)
        bpy.ops.object.transform_apply(scale=True)
        assign_material(pauldron, mat_armor)

        # Upper Arm
        bpy.ops.mesh.primitive_cylinder_add(radius=0.10, depth=0.35, location=(side * 0.46, 0, 1.25))
        upper_arm = bpy.context.active_object
        upper_arm.name = f"UpperArm_{side}"
        assign_material(upper_arm, mat_suit)

        # Forearm with Tactical Gauntlet
        bpy.ops.mesh.primitive_cylinder_add(radius=0.09, depth=0.35, location=(side * 0.46, -0.05, 0.90))
        forearm = bpy.context.active_object
        forearm.name = f"Forearm_{side}"
        assign_material(forearm, mat_armor if side == 1 else mat_suit)

        # Holographic Wrist Display on Left Forearm
        if side == -1:
            bpy.ops.mesh.primitive_cube_add(size=1.0, location=(-0.46, -0.15, 0.90))
            wrist_hud = bpy.context.active_object
            wrist_hud.name = "Wrist_HUD"
            wrist_hud.scale = (0.12, 0.03, 0.18)
            bpy.ops.object.transform_apply(scale=True)
            assign_material(wrist_hud, mat_glow_cyan)

        # Tactical Glove
        bpy.ops.mesh.primitive_uv_sphere_add(radius=0.09, location=(side * 0.46, -0.05, 0.68))
        glove = bpy.context.active_object
        glove.name = f"Glove_{side}"
        assign_material(glove, mat_armor)

    # 5. Legs & Mag-Boots
    for side in (-1, 1):
        # Thigh
        bpy.ops.mesh.primitive_cylinder_add(radius=0.13, depth=0.45, location=(side * 0.20, 0, 0.65))
        thigh = bpy.context.active_object
        thigh.name = f"Thigh_{side}"
        assign_material(thigh, mat_suit)

        # Knee Armor Plate
        bpy.ops.mesh.primitive_cube_add(size=1.0, location=(side * 0.20, -0.12, 0.42))
        knee = bpy.context.active_object
        knee.name = f"Knee_{side}"
        knee.scale = (0.16, 0.08, 0.14)
        bpy.ops.object.transform_apply(scale=True)
        assign_material(knee, mat_armor)

        # Shin / Calf
        bpy.ops.mesh.primitive_cylinder_add(radius=0.11, depth=0.40, location=(side * 0.20, 0, 0.22))
        shin = bpy.context.active_object
        shin.name = f"Shin_{side}"
        assign_material(shin, mat_suit)

        # Heavy Magnetic Boot
        bpy.ops.mesh.primitive_cube_add(size=1.0, location=(side * 0.20, -0.08, 0.06))
        boot = bpy.context.active_object
        boot.name = f"MagBoot_{side}"
        boot.scale = (0.18, 0.32, 0.12)
        bpy.ops.object.transform_apply(scale=True)
        assign_material(boot, mat_armor)

        # Boot Sole Clamp Light
        bpy.ops.mesh.primitive_cube_add(size=1.0, location=(side * 0.20, -0.08, 0.01))
        boot_light = bpy.context.active_object
        boot_light.name = f"BootLight_{side}"
        boot_light.scale = (0.15, 0.28, 0.02)
        bpy.ops.object.transform_apply(scale=True)
        assign_material(boot_light, mat_glow_cyan)

    out_path = os.path.join(OUTPUT_DIR, "astronaut_operative.glb")
    bpy.ops.export_scene.gltf(filepath=out_path, export_format='GLB', use_selection=False)
    print(f"Exported: {out_path}")

# ==========================================
# 2. ZERO-G TACTICAL TRAINING ARENA
# ==========================================
def build_zero_g_tactical_arena():
    clear_scene()
    print("Building Zero-G Tactical Training Arena...")

    mat_wall = create_pbr_material("Mat_Wall", (0.20, 0.22, 0.26, 1.0), metallic=0.6, roughness=0.4)
    mat_floor = create_pbr_material("Mat_Floor", (0.10, 0.12, 0.14, 1.0), metallic=0.8, roughness=0.3)
    mat_metal_yellow = create_pbr_material("Mat_Hazard", (0.9, 0.7, 0.05, 1.0), metallic=0.3, roughness=0.4)
    mat_ring_cyan = create_pbr_material("Mat_RingCyan", (0.0, 0.9, 1.0, 1.0), metallic=0.2, roughness=0.1, emission_color=(0.0, 0.9, 1.0, 1.0), emission_strength=8.0)
    mat_ring_orange = create_pbr_material("Mat_RingOrange", (1.0, 0.5, 0.0, 1.0), metallic=0.2, roughness=0.1, emission_color=(1.0, 0.5, 0.0, 1.0), emission_strength=8.0)
    mat_ring_green = create_pbr_material("Mat_RingGreen", (0.1, 1.0, 0.3, 1.0), metallic=0.2, roughness=0.1, emission_color=(0.1, 1.0, 0.3, 1.0), emission_strength=8.0)
    mat_valve_red = create_pbr_material("Mat_ValveRed", (0.95, 0.15, 0.10, 1.0), metallic=0.5, roughness=0.3, emission_color=(0.95, 0.15, 0.10, 1.0), emission_strength=2.0)

    # 1. Massive Tactical Training Cylinder / Tunnel (Length: 50m, Radius: 7m)
    bpy.ops.mesh.primitive_cylinder_add(vertices=16, radius=7.0, depth=50.0, location=(0, 20.0, 0), rotation=(math.radians(90), 0, 0))
    arena_hull = bpy.context.active_object
    arena_hull.name = "Arena_Hull"
    # Flip normals to interior
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.mesh.flip_normals()
    bpy.ops.object.mode_set(mode='OBJECT')
    assign_material(arena_hull, mat_wall)

    # 2. Structural Bulkhead Ribs along tunnel
    for z_pos in [-2.0, 8.0, 18.0, 28.0, 38.0]:
        bpy.ops.mesh.primitive_torus_add(major_radius=6.8, minor_radius=0.35, location=(0, z_pos, 0), rotation=(math.radians(90), 0, 0))
        rib = bpy.context.active_object
        rib.name = f"Bulkhead_Rib_{z_pos}"
        assign_material(rib, mat_floor)

    # 3. Target Navigation Hologram Rings
    ring_configs = [
        {"name": "NavRing_01", "mat": mat_ring_cyan, "loc": (0.0, 6.0, 0.5), "radius": 3.0},
        {"name": "NavRing_02", "mat": mat_ring_orange, "loc": (-1.5, 18.0, -1.0), "radius": 2.6},
        {"name": "NavRing_03", "mat": mat_ring_green, "loc": (1.2, 30.0, 1.2), "radius": 2.2},
    ]

    for ring in ring_configs:
        bpy.ops.mesh.primitive_torus_add(major_radius=ring["radius"], minor_radius=0.18, location=ring["loc"], rotation=(math.radians(90), 0, 0))
        r_obj = bpy.context.active_object
        r_obj.name = ring["name"]
        assign_material(r_obj, ring["mat"])
        
        # Pylon anchors on 4 sides of ring
        for angle in [0, 90, 180, 270]:
            rad = math.radians(angle)
            px = ring["loc"][0] + (ring["radius"] + 0.5) * math.cos(rad)
            pz = ring["loc"][2] + (ring["radius"] + 0.5) * math.sin(rad)
            bpy.ops.mesh.primitive_cube_add(size=0.4, location=(px, ring["loc"][1], pz))
            pylon = bpy.context.active_object
            pylon.name = f"{ring['name']}_Pylon_{angle}"
            assign_material(pylon, mat_hazard_if_needed := mat_metal_yellow)

    # 4. Emergency ECLSS Manual Pressure Valve Terminal at the end of the arena (Y = 42)
    # Console Base
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 42.0, -2.5))
    console = bpy.context.active_object
    console.name = "Valve_Console_Base"
    console.scale = (2.2, 0.8, 1.8)
    bpy.ops.object.transform_apply(scale=True)
    assign_material(console, mat_floor)

    # Piping Manifold
    bpy.ops.mesh.primitive_cylinder_add(radius=0.25, depth=4.0, location=(0, 42.0, -1.2), rotation=(0, math.radians(90), 0))
    pipe = bpy.context.active_object
    pipe.name = "Manifold_Pipe"
    assign_material(pipe, mat_metal_yellow)

    # Rotary Emergency Valve Wheel
    bpy.ops.mesh.primitive_torus_add(major_radius=0.6, minor_radius=0.08, location=(0, 41.5, -1.2), rotation=(math.radians(90), 0, 0))
    valve_wheel = bpy.context.active_object
    valve_wheel.name = "Emergency_Valve_Wheel"
    assign_material(valve_wheel, mat_valve_red)

    # Valve Spokes
    for sp in [0, 60, 120]:
        bpy.ops.mesh.primitive_cylinder_add(radius=0.04, depth=1.1, location=(0, 41.5, -1.2), rotation=(0, 0, math.radians(sp)))
        spoke = bpy.context.active_object
        spoke.name = f"Valve_Spoke_{sp}"
        assign_material(spoke, mat_valve_red)

    # Digital Holographic Status Panel above Valve
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 42.2, 0.2))
    panel = bpy.context.active_object
    panel.name = "Valve_Status_HoloPanel"
    panel.scale = (1.4, 0.05, 0.8)
    bpy.ops.object.transform_apply(scale=True)
    assign_material(panel, mat_ring_cyan)

    out_path = os.path.join(OUTPUT_DIR, "zero_g_tactical_arena.glb")
    bpy.ops.export_scene.gltf(filepath=out_path, export_format='GLB', use_selection=False)
    print(f"Exported: {out_path}")

# ==========================================
# 3. FALCON TACTICAL COCKPIT
# ==========================================
def build_falcon_cockpit():
    clear_scene()
    print("Building Falcon Tactical Cockpit...")

    mat_carbon = create_pbr_material("Mat_Carbon", (0.08, 0.09, 0.11, 1.0), metallic=0.7, roughness=0.35)
    mat_seat = create_pbr_material("Mat_Seat", (0.16, 0.18, 0.22, 1.0), metallic=0.2, roughness=0.7)
    mat_hud_screen = create_pbr_material("Mat_Screen", (0.0, 0.4, 0.8, 1.0), metallic=0.1, roughness=0.1, emission_color=(0.0, 0.8, 1.0, 1.0), emission_strength=4.0)
    mat_warning_screen = create_pbr_material("Mat_WarnScreen", (0.8, 0.2, 0.0, 1.0), metallic=0.1, roughness=0.1, emission_color=(1.0, 0.3, 0.0, 1.0), emission_strength=5.0)
    mat_frame = create_pbr_material("Mat_Frame", (0.22, 0.25, 0.30, 1.0), metallic=0.8, roughness=0.2)

    # 1. Main Pilot Couch / Acceleration Seat
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0, 0.5))
    seat_back = bpy.context.active_object
    seat_back.name = "Pilot_Seat_Back"
    seat_back.scale = (0.70, 0.20, 1.1)
    seat_back.rotation_euler = (math.radians(-15), 0, 0)
    bpy.ops.object.transform_apply(scale=True, rotation=True)
    assign_material(seat_back, mat_seat)

    # Seat Cushion Base
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0.35, 0.15))
    seat_base = bpy.context.active_object
    seat_base.name = "Pilot_Seat_Base"
    seat_base.scale = (0.68, 0.65, 0.25)
    bpy.ops.object.transform_apply(scale=True)
    assign_material(seat_base, mat_seat)

    # 2. Main Flight Console Dashboard
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 1.35, 0.85))
    dashboard = bpy.context.active_object
    dashboard.name = "Dashboard_Console"
    dashboard.scale = (2.2, 0.65, 0.70)
    dashboard.rotation_euler = (math.radians(25), 0, 0)
    bpy.ops.object.transform_apply(scale=True, rotation=True)
    assign_material(dashboard, mat_carbon)

    # Multi-Function Displays (3 Screens)
    for i, offset_x in enumerate([-0.65, 0.0, 0.65]):
        bpy.ops.mesh.primitive_cube_add(size=1.0, location=(offset_x, 1.15, 0.95))
        screen = bpy.context.active_object
        screen.name = f"MFD_Screen_{i}"
        screen.scale = (0.50, 0.04, 0.35)
        screen.rotation_euler = (math.radians(25), 0, 0)
        bpy.ops.object.transform_apply(scale=True, rotation=True)
        assign_material(screen, mat_warning_screen if i == 1 else mat_hud_screen)

    # Dual Flight Control Sticks (HOTAS)
    for side in (-1, 1):
        bpy.ops.mesh.primitive_cylinder_add(radius=0.03, depth=0.35, location=(side * 0.45, 0.75, 0.45), rotation=(math.radians(10), 0, 0))
        stick = bpy.context.active_object
        stick.name = f"Flight_Stick_{side}"
        assign_material(stick, mat_frame)

    # Canopy Frame Arches
    for y_arch in [0.5, 1.8]:
        bpy.ops.mesh.primitive_torus_add(major_radius=1.6, minor_radius=0.12, location=(0, y_arch, 0.8), rotation=(0, math.radians(90), 0))
        arch = bpy.context.active_object
        arch.name = f"Canopy_Arch_{y_arch}"
        assign_material(arch, mat_frame)

    out_path = os.path.join(OUTPUT_DIR, "falcon_cockpit.glb")
    bpy.ops.export_scene.gltf(filepath=out_path, export_format='GLB', use_selection=False)
    print(f"Exported: {out_path}")

# ==========================================
# 4. ISS ORBITAL STATION
# ==========================================
def build_iss_orbital_station():
    clear_scene()
    print("Building ISS Orbital Station...")

    mat_hull = create_pbr_material("Mat_ISSHull", (0.85, 0.87, 0.90, 1.0), metallic=0.7, roughness=0.3)
    mat_gold_solar = create_pbr_material("Mat_Solar", (0.95, 0.65, 0.10, 1.0), metallic=0.9, roughness=0.15, emission_color=(0.95, 0.65, 0.10, 1.0), emission_strength=1.5)
    mat_truss = create_pbr_material("Mat_Truss", (0.35, 0.38, 0.42, 1.0), metallic=0.8, roughness=0.3)
    mat_dock_glow = create_pbr_material("Mat_DockTarget", (0.0, 1.0, 0.4, 1.0), metallic=0.2, roughness=0.2, emission_color=(0.0, 1.0, 0.4, 1.0), emission_strength=6.0)

    # 1. Main Pressurized Module Spine (Zvezda, Zarya, Unity, Destiny)
    bpy.ops.mesh.primitive_cylinder_add(radius=2.1, depth=24.0, location=(0, 0, 0), rotation=(math.radians(90), 0, 0))
    spine = bpy.context.active_object
    spine.name = "Module_Spine"
    assign_material(spine, mat_hull)

    # Transverse Laboratory (Kibo / Columbus)
    bpy.ops.mesh.primitive_cylinder_add(radius=2.0, depth=14.0, location=(0, 4.0, 0), rotation=(0, math.radians(90), 0))
    lab_wing = bpy.context.active_object
    lab_wing.name = "Module_Lab_Transverse"
    assign_material(lab_wing, mat_hull)

    # 2. Integrated Truss Structure (ITS) - 40m long main beam
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0, 3.5))
    truss = bpy.context.active_object
    truss.name = "Truss_Structure"
    truss.scale = (42.0, 1.4, 1.4)
    bpy.ops.object.transform_apply(scale=True)
    assign_material(truss, mat_truss)

    # 3. Giant Articulated Solar Array Wings (4 sets on each side of the truss)
    for side in (-1, 1):
        for array_idx, x_pos in enumerate([10.0 * side, 17.0 * side]):
            # Solar Panel Wings
            bpy.ops.mesh.primitive_cube_add(size=1.0, location=(x_pos, 0, 3.5))
            panel = bpy.context.active_object
            panel.name = f"Solar_Panel_{side}_{array_idx}"
            panel.scale = (4.5, 14.0, 0.08)
            panel.rotation_euler = (0, 0, math.radians(15 * side))
            bpy.ops.object.transform_apply(scale=True, rotation=True)
            assign_material(panel, mat_gold_solar)

    # 4. Cupola Observation Module (7 bay windows)
    bpy.ops.mesh.primitive_cone_add(vertices=7, radius1=1.8, radius2=1.2, depth=1.4, location=(0, 2.0, -2.4), rotation=(math.radians(180), 0, 0))
    cupola = bpy.context.active_object
    cupola.name = "Cupola_Module"
    assign_material(cupola, mat_hull)

    # 5. Pressurized Mating Adapter 2 (PMA-2 Docking Target Port) at forward axial port (Y = 12.0)
    bpy.ops.mesh.primitive_cylinder_add(radius=1.2, depth=2.0, location=(0, 13.0, 0), rotation=(math.radians(90), 0, 0))
    dock_port = bpy.context.active_object
    dock_port.name = "PMA2_Docking_Port"
    assign_material(dock_port, mat_hull)

    # Docking Target Cross & Illuminated Laser Guides
    bpy.ops.mesh.primitive_torus_add(major_radius=0.9, minor_radius=0.08, location=(0, 14.05, 0), rotation=(math.radians(90), 0, 0))
    dock_ring = bpy.context.active_object
    dock_ring.name = "Docking_Target_Ring"
    assign_material(dock_ring, mat_dock_glow)

    out_path = os.path.join(OUTPUT_DIR, "iss_orbital_station.glb")
    bpy.ops.export_scene.gltf(filepath=out_path, export_format='GLB', use_selection=False)
    print(f"Exported: {out_path}")

def main():
    print("=== STARTING FPS ASSET GENERATION VIA BLENDER 4.3 ===")
    build_astronaut_operative()
    build_zero_g_tactical_arena()
    build_falcon_cockpit()
    build_iss_orbital_station()
    print("=== ASSET GENERATION COMPLETE ===")

if __name__ == "__main__":
    main()
