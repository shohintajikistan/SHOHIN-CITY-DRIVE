extends Node3D

const SEGMENTS := 160
const TRACK_LENGTH_X := 150.0
const TRACK_LENGTH_Z := 82.0
const ROAD_WIDTH := 14.0
const SHOULDER_WIDTH := 3.0
const BARRIER_HEIGHT := 0.85

var road_points: Array[Vector3] = []

func _ready() -> void:
	_build_environment()
	_build_track()
	_build_camera()

func _build_environment() -> void:
	var world := WorldEnvironment.new()
	var env := Environment.new()
	env.background_mode = Environment.BG_SKY
	var sky := Sky.new()
	var sky_material := ProceduralSkyMaterial.new()
	sky_material.sky_top_color = Color(0.18, 0.31, 0.39)
	sky_material.sky_horizon_color = Color(0.72, 0.79, 0.78)
	sky_material.ground_bottom_color = Color(0.10, 0.14, 0.12)
	sky_material.ground_horizon_color = Color(0.42, 0.48, 0.42)
	sky.sky_material = sky_material
	env.sky = sky
	env.ambient_light_source = Environment.AMBIENT_SOURCE_SKY
	env.ambient_light_energy = 0.55
	env.tonemap_mode = Environment.TONE_MAPPER_FILMIC
	world.environment = env
	add_child(world)

	var sun := DirectionalLight3D.new()
	sun.rotation_degrees = Vector3(-52.0, -28.0, 0.0)
	sun.light_energy = 1.25
	sun.shadow_enabled = true
	add_child(sun)

	var ground := MeshInstance3D.new()
	var ground_mesh := PlaneMesh.new()
	ground_mesh.size = Vector2(620.0, 420.0)
	ground.mesh = ground_mesh
	ground.position.y = -0.42
	var grass := StandardMaterial3D.new()
	grass.albedo_color = Color(0.16, 0.23, 0.17)
	grass.roughness = 1.0
	ground.material_override = grass
	add_child(ground)

func _build_track() -> void:
	for i in range(SEGMENTS):
		var angle := TAU * float(i) / float(SEGMENTS)
		road_points.append(Vector3(TRACK_LENGTH_X * cos(angle), 0.0, TRACK_LENGTH_Z * sin(angle)))

	_add_ribbon("Shoulders", ROAD_WIDTH + SHOULDER_WIDTH * 2.0, -0.08, Color(0.48, 0.49, 0.45), 1.0)
	_add_ribbon("Asphalt", ROAD_WIDTH, 0.015, Color(0.105, 0.12, 0.125), 0.96)
	_add_ribbon("Outer edge line", ROAD_WIDTH - 0.45, 0.035, Color(0.88, 0.88, 0.81), 0.8, true)
	_add_ribbon("Inner edge line", ROAD_WIDTH - 0.45, 0.035, Color(0.88, 0.88, 0.81), 0.8, false)
	_add_center_dashes()
	_add_barriers()

func _add_ribbon(label: String, width: float, height: float, color: Color, roughness: float, edge_only: bool = false) -> void:
	var st := SurfaceTool.new()
	st.begin(Mesh.PRIMITIVE_TRIANGLES)
	for i in range(SEGMENTS):
		var next_i := (i + 1) % SEGMENTS
		var p0 := road_points[i]
		var p1 := road_points[next_i]
		var tangent := (p1 - p0).normalized()
		var side := Vector3(-tangent.z, 0.0, tangent.x)
		var left0 := p0 + side * width * 0.5
		var right0 := p0 - side * width * 0.5
		var left1 := p1 + side * width * 0.5
		var right1 := p1 - side * width * 0.5
		if edge_only:
			# Thin continuous edge strips are drawn separately below.
			continue
		st.set_uv(Vector2(0.0, float(i) / 8.0)); st.add_vertex(left0 + Vector3.UP * height)
		st.set_uv(Vector2(1.0, float(i) / 8.0)); st.add_vertex(right0 + Vector3.UP * height)
		st.set_uv(Vector2(1.0, float(i + 1) / 8.0)); st.add_vertex(right1 + Vector3.UP * height)
		st.set_uv(Vector2(0.0, float(i) / 8.0)); st.add_vertex(left0 + Vector3.UP * height)
		st.set_uv(Vector2(1.0, float(i + 1) / 8.0)); st.add_vertex(right1 + Vector3.UP * height)
		st.set_uv(Vector2(0.0, float(i + 1) / 8.0)); st.add_vertex(left1 + Vector3.UP * height)
	st.generate_normals()
	var mesh := st.commit()
	var surface := MeshInstance3D.new()
	surface.name = label
	surface.mesh = mesh
	var material := StandardMaterial3D.new()
	material.albedo_color = color
	material.roughness = roughness
	material.cull_mode = BaseMaterial3D.CULL_DISABLED
	surface.material_override = material
	add_child(surface)
	if label == "Asphalt":
		surface.create_trimesh_collision()

func _add_center_dashes() -> void:
	var paint := StandardMaterial3D.new()
	paint.albedo_color = Color(0.92, 0.88, 0.68)
	paint.roughness = 0.8
	for i in range(0, SEGMENTS, 8):
		var p0 := road_points[i]
		var p1 := road_points[(i + 3) % SEGMENTS]
		var mid := (p0 + p1) * 0.5
		var dash := MeshInstance3D.new()
		var box := BoxMesh.new()
		box.size = Vector3(0.16, 0.035, p0.distance_to(p1) * 0.48)
		dash.mesh = box
		dash.position = Vector3(mid.x, 0.055, mid.z)
		dash.look_at(Vector3(p1.x, 0.055, p1.z), Vector3.UP)
		dash.material_override = paint
		add_child(dash)

func _add_barriers() -> void:
	var barrier_material := StandardMaterial3D.new()
	barrier_material.albedo_color = Color(0.72, 0.75, 0.72)
	barrier_material.metallic = 0.32
	barrier_material.roughness = 0.48
	var stripe_material := StandardMaterial3D.new()
	stripe_material.albedo_color = Color(0.12, 0.17, 0.16)
	for i in range(0, SEGMENTS, 2):
		var p := road_points[i]
		var next_p := road_points[(i + 2) % SEGMENTS]
		var tangent := (next_p - p).normalized()
		var side := Vector3(-tangent.z, 0.0, tangent.x)
		for sign in [-1.0, 1.0]:
			var midpoint := (p + next_p) * 0.5 + side * sign * (ROAD_WIDTH * 0.5 + SHOULDER_WIDTH * 0.72)
			var length := p.distance_to(next_p) + 0.12
			var wall := StaticBody3D.new()
			var shape := CollisionShape3D.new()
			var box_shape := BoxShape3D.new()
			box_shape.size = Vector3(0.42, BARRIER_HEIGHT, length)
			shape.shape = box_shape
			shape.position.y = BARRIER_HEIGHT * 0.5
			wall.add_child(shape)
			wall.position = Vector3(midpoint.x, 0.0, midpoint.z)
			wall.look_at(Vector3(next_p.x + side.x * sign, 0.0, next_p.z + side.z * sign), Vector3.UP)
			add_child(wall)

			var visual := MeshInstance3D.new()
			var box_mesh := BoxMesh.new()
			box_mesh.size = box_shape.size
			visual.mesh = box_mesh
			visual.position.y = BARRIER_HEIGHT * 0.5
			visual.material_override = barrier_material
			wall.add_child(visual)

			if i % 8 == 0:
				var stripe := MeshInstance3D.new()
				var stripe_mesh := BoxMesh.new()
				stripe_mesh.size = Vector3(0.44, 0.12, maxf(0.7, length * 0.25))
				stripe.mesh = stripe_mesh
				stripe.position = Vector3(0.0, 0.0, 0.0)
				stripe.material_override = stripe_material
				wall.add_child(stripe)

func _build_camera() -> void:
	var camera := Camera3D.new()
	camera.position = Vector3(0.0, 205.0, 235.0)
	camera.look_at(Vector3.ZERO, Vector3.UP)
	camera.fov = 48.0
	camera.current = true
	add_child(camera)
