import math
import os
import bpy
from mathutils import Vector

# SHOHIN CITY DRIVE - procedural low-poly / mobile-ready sedan.
# Blender axes: +Y is the front of the car, +Z is up.
ROOT_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUTPUT_PATH = os.path.join(ROOT_DIR, "exports", "shohin_car.glb")


def clear_scene():
    bpy.ops.wm.read_factory_settings(use_empty=True)


def make_material(name, color, metallic=0.0, roughness=0.5,
                  emission=None, emission_strength=0.0):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*color[:3], color[3] if len(color) > 3 else 1.0)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = mat.diffuse_color
    bsdf.inputs["Metallic"].default_value = metallic
    bsdf.inputs["Roughness"].default_value = roughness
    if emission is not None:
        socket = bsdf.inputs.get("Emission Color") or bsdf.inputs.get("Emission")
        if socket:
            socket.default_value = (*emission[:3], 1.0)
        strength = bsdf.inputs.get("Emission Strength")
        if strength:
            strength.default_value = emission_strength
    return mat


def attach(obj, root, name, material, bevel=0.0):
    obj.name = name
    if material:
        obj.data.materials.clear()
        obj.data.materials.append(material)
    obj.parent = root
    obj.matrix_parent_inverse = root.matrix_world.inverted()
    if bevel > 0 and obj.type == "MESH":
        mod = obj.modifiers.new("Soft_Edge_Bevel", "BEVEL")
        mod.width = bevel
        mod.segments = 2
        mod.limit_method = "ANGLE"
        obj.modifiers.new("Weighted_Corner_Normals", "WEIGHTED_NORMAL")
    return obj


def cube(name, location, scale, material, root, bevel=0.04, rotation=None):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=location)
    obj = bpy.context.object
    obj.dimensions = scale
    if rotation:
        obj.rotation_euler = rotation
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return attach(obj, root, name, material, bevel)


def cylinder(name, location, radius, depth, material, root, rotation=None,
             vertices=32, bevel=0.012):
    bpy.ops.mesh.primitive_cylinder_add(
        vertices=vertices, radius=radius, depth=depth,
        end_fill_type="NGON", location=location
    )
    obj = bpy.context.object
    if rotation:
        obj.rotation_euler = rotation
    return attach(obj, root, name, material, bevel)


def build_car():
    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"

    paint = make_material("SHOHIN_Forest_Green_Metallic", (0.018, 0.19, 0.105, 1), 0.78, 0.24)
    dark = make_material("Trim_Satin_Black", (0.012, 0.018, 0.018, 1), 0.12, 0.48)
    glass = make_material("Glass_Deep_Tinted", (0.025, 0.075, 0.085, 1), 0.35, 0.16)
    rubber = make_material("Tire_Rubber", (0.012, 0.014, 0.016, 1), 0.0, 0.82)
    gold = make_material("SHOHIN_Warm_Gold_Alloy", (0.72, 0.48, 0.12, 1), 0.88, 0.23)
    chrome = make_material("Chrome", (0.58, 0.65, 0.66, 1), 0.92, 0.18)
    white_led = make_material("LED_White", (0.8, 0.92, 1, 1), 0.1, 0.18, (0.72, 0.88, 1), 2.0)
    red_led = make_material("LED_Red", (0.55, 0.012, 0.018, 1), 0.15, 0.2, (1, 0.015, 0.02), 1.4)

    root = bpy.data.objects.new("SHOHIN_CAR_ROOT", None)
    root.empty_display_type = "PLAIN_AXES"
    root.empty_display_size = 0.45
    bpy.context.scene.collection.objects.link(root)

    # Main lower body, bonnet and boot. Overall length is about 4.4 m.
    cube("Body_Main", (0, 0, 0.68), (1.82, 4.05, 0.72), paint, root, 0.16)
    cube("Bonnet", (0, 1.42, 0.99), (1.72, 1.20, 0.25), paint, root, 0.10)
    cube("Rear_Deck", (0, -1.58, 0.98), (1.70, 0.88, 0.25), paint, root, 0.09)
    cube("Front_Bumper", (0, 2.08, 0.49), (1.78, 0.20, 0.34), dark, root, 0.07)
    cube("Rear_Bumper", (0, -2.08, 0.49), (1.78, 0.20, 0.34), dark, root, 0.07)
    cube("Front_Grille", (0, 2.115, 0.67), (0.72, 0.035, 0.20), dark, root, 0.025)
    cube("Grille_Chrome_Bar", (0, 2.14, 0.68), (0.58, 0.018, 0.025), chrome, root, 0.008)

    # Cabin and roof.
    cube("Cabin_Shell", (0, -0.10, 1.34), (1.48, 2.16, 0.86), paint, root, 0.16)
    cube("Roof", (0, -0.10, 1.79), (1.34, 1.30, 0.12), paint, root, 0.07)

    # Dark window panels, kept opaque for reliable mobile rendering.
    cube("Windshield", (0, 0.91, 1.48), (1.27, 0.035, 0.54), glass, root, 0.025,
         (math.radians(-18), 0, 0))
    cube("Rear_Window", (0, -1.12, 1.48), (1.24, 0.035, 0.52), glass, root, 0.025,
         (math.radians(18), 0, 0))
    for side in (-1, 1):
        x = side * 0.755
        cube("Side_Window_Front_L" if side < 0 else "Side_Window_Front_R",
             (x, 0.42, 1.48), (0.025, 0.78, 0.48), glass, root, 0.018)
        cube("Side_Window_Rear_L" if side < 0 else "Side_Window_Rear_R",
             (x, -0.57, 1.48), (0.025, 0.78, 0.48), glass, root, 0.018)
        cube("Door_Handle_L" if side < 0 else "Door_Handle_R",
             (side * 0.925, -0.02, 0.91), (0.025, 0.22, 0.045), chrome, root, 0.012)
        cube("Side_Skirt_L" if side < 0 else "Side_Skirt_R",
             (side * 0.89, 0, 0.36), (0.09, 2.55, 0.13), dark, root, 0.035)
        # Mirrors and short mounting arms.
        cube("Mirror_Arm_L" if side < 0 else "Mirror_Arm_R",
             (side * 0.91, 0.77, 1.18), (0.18, 0.07, 0.055), dark, root, 0.025)
        cube("Mirror_Cap_L" if side < 0 else "Mirror_Cap_R",
             (side * 1.02, 0.77, 1.22), (0.20, 0.27, 0.14), paint, root, 0.055)
        cube("Mirror_Glass_L" if side < 0 else "Mirror_Glass_R",
             (side * 1.126, 0.77, 1.22), (0.012, 0.19, 0.085), chrome, root, 0.012)

    # Lights and small exterior details.
    for side in (-1, 1):
        cube("Headlight_L" if side < 0 else "Headlight_R",
             (side * 0.59, 2.105, 0.79), (0.42, 0.055, 0.19), white_led, root, 0.045)
        cube("Tail_Light_L" if side < 0 else "Tail_Light_R",
             (side * 0.62, -2.105, 0.78), (0.40, 0.055, 0.18), red_led, root, 0.035)
        cube("Fog_Lamp_L" if side < 0 else "Fog_Lamp_R",
             (side * 0.66, 2.115, 0.43), (0.16, 0.035, 0.07), white_led, root, 0.02)

    cube("Front_Number_Plate", (0, 2.19, 0.47), (0.48, 0.018, 0.12), chrome, root, 0.012)
    cube("Rear_Number_Plate", (0, -2.19, 0.47), (0.48, 0.018, 0.12), chrome, root, 0.012)
    cube("Underbody", (0, 0, 0.25), (1.55, 3.55, 0.16), dark, root, 0.035)

    # Four independent wheel pivots. Local X is the axle; pivot Z can steer,
    # and local X rotation can spin the wheel in an engine.
    wheel_specs = [
        ("FL", -0.91, 1.34), ("FR", 0.91, 1.34),
        ("RL", -0.91, -1.34), ("RR", 0.91, -1.34),
    ]
    for tag, x, y in wheel_specs:
        pivot = bpy.data.objects.new("Wheel_" + tag + "_Pivot", None)
        pivot.empty_display_type = "CIRCLE"
        pivot.empty_display_size = 0.22
        pivot.location = (x, y, 0.39)
        bpy.context.scene.collection.objects.link(pivot)
        pivot.parent = root
        tire = cylinder("Wheel_" + tag + "_Tire", (x, y, 0.39), 0.355, 0.24,
                        rubber, root, (0, math.radians(90), 0), 32, 0.018)
        tire.parent = pivot
        tire.location = (0, 0, 0)
        tire.matrix_parent_inverse.identity()
        # Two gold rim faces and a central hub, all parented to the pivot.
        for face_side in (-1, 1):
            rim = cylinder("Wheel_" + tag + "_Gold_Rim_" + str(face_side),
                           (x + face_side * 0.105, y, 0.39), 0.245, 0.025,
                           gold, root, (0, math.radians(90), 0), 24, 0.008)
            rim.parent = pivot
            rim.location = (face_side * 0.105, 0, 0)
            rim.matrix_parent_inverse.identity()
            hub = cylinder("Wheel_" + tag + "_Hub_" + str(face_side),
                           (x + face_side * 0.125, y, 0.39), 0.075, 0.025,
                           chrome, root, (0, math.radians(90), 0), 16, 0.006)
            hub.parent = pivot
            hub.location = (face_side * 0.125, 0, 0)
            hub.matrix_parent_inverse.identity()
        # Six simple gold spokes on each visible outer face.
        for face_side in (-1, 1):
            for spoke in range(6):
                angle = spoke * math.tau / 6.0
                spoke_obj = cube(
                    "Wheel_" + tag + "_Spoke_" + str(face_side) + "_" + str(spoke),
                    (x + face_side * 0.126, y + math.sin(angle) * 0.15,
                     0.39 + math.cos(angle) * 0.15),
                    (0.018, 0.055, 0.18), gold, root, 0.012,
                    (angle, 0, 0)
                )
                spoke_obj.parent = pivot
                spoke_obj.location = (face_side * 0.126, math.sin(angle) * 0.15,
                                      math.cos(angle) * 0.15)
                spoke_obj.matrix_parent_inverse.identity()

    # Make the root the only selected object for a clean, predictable export.
    bpy.ops.object.select_all(action="DESELECT")
    root.select_set(True)
    bpy.context.view_layer.objects.active = root

    os.makedirs(os.path.dirname(OUTPUT_PATH), exist_ok=True)
    bpy.ops.export_scene.gltf(
        filepath=OUTPUT_PATH,
        export_format="GLB",
        use_selection=False,
        export_apply=True,
        export_yup=True,
        export_materials="EXPORT",
        export_normals=True,
        export_cameras=False,
        export_lights=False,
    )
    if not os.path.isfile(OUTPUT_PATH) or os.path.getsize(OUTPUT_PATH) < 1000:
        raise RuntimeError("GLB export failed or produced an unexpectedly small file")
    print("SHOHIN CAR GLB exported:", OUTPUT_PATH)
    print("File size:", os.path.getsize(OUTPUT_PATH), "bytes")


if __name__ == "__main__":
    clear_scene()
    build_car()
