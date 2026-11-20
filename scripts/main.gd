extends Node3D

var car: CharacterBody3D
var camera: Camera3D
var speed := 0.0
var steering := 0.0
var throttle := false
var braking := false
var left_pressed := false
var right_pressed := false
var speed_label: Label

const MAX_SPEED := 24.0
const ACCEL := 13.0
const TURN_RATE := 1.65

func _ready() -> void:
	_make_world()
	_make_car()
	_make_camera()
	_make_ui()

func _make_world() -> void:
	var sun := DirectionalLight3D.new()
	sun.rotation_degrees = Vector3(-48, -28, 0)
	sun.light_energy = 1.15
	add_child(sun)
	var env := WorldEnvironment.new()
	var e := Environment.new()
	e.background_mode = Environment.BG_SKY
	e.sky = Sky.new()
	e.sky.sky_material = ProceduralSkyMaterial.new()
	e.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	e.ambient_light_color = Color(0.72, 0.78, 0.84)
	e.ambient_light_energy = 0.65
	env.environment = e
	add_child(env)
	_box(Vector3(0, -0.35, 0), Vector3(180, 0.5, 180), Color(0.25, 0.43, 0.25))
	# Main roads and side streets
	_box(Vector3(0, -0.08, 0), Vector3(13, 0.15, 180), Color(0.13, 0.14, 0.16))
	_box(Vector3(0, -0.07, 0), Vector3(180, 0.15, 13), Color(0.13, 0.14, 0.16))
	for z in range(-80, 81, 8):
		_box(Vector3(0, 0.012, z), Vector3(0.18, 0.025, 3.2), Color(0.95, 0.88, 0.55))
	for x in range(-80, 81, 8):
		_box(Vector3(x, 0.012, 0), Vector3(3.2, 0.025, 0.18), Color(0.95, 0.88, 0.55))
	# Simple city blocks with varied building heights
	var colors = [Color(0.78,0.72,0.61), Color(0.72,0.77,0.78), Color(0.83,0.79,0.68), Color(0.63,0.70,0.68)]
	for x in range(-70, 71, 20):
		for z in range(-70, 71, 20):
			if abs(x) < 12 or abs(z) < 12:
				continue
			var h := float( randi_range(7, 18) )
			_box(Vector3(x, h * 0.5, z), Vector3(10, h, 10), colors[randi_range(0, colors.size()-1)])
			# windows on street-facing side
			for wy in range(2, int(h)-1, 3):
				_box(Vector3(x, wy, z - 5.06), Vector3(1.1, 1.0, 0.08), Color(0.20,0.43,0.55))
	# Distant mountain silhouettes
	for i in range(18):
		var m := MeshInstance3D.new()
		var mesh := CylinderMesh.new()
		mesh.top_radius = 0.0
		mesh.bottom_radius = float(randi_range(9,18))
		mesh.height = float(randi_range(18,34))
		m.mesh = mesh
		m.position = Vector3(-100 + i*12, 10, -105)
		var mat := StandardMaterial3D.new()
		mat.albedo_color = Color(0.34,0.43,0.45)
		m.material_override = mat
		add_child(m)

func _box(pos: Vector3, size: Vector3, color: Color) -> void:
	var mi := MeshInstance3D.new()
	var mesh := BoxMesh.new()
	mesh.size = size
	mi.mesh = mesh
	mi.position = pos
	var mat := StandardMaterial3D.new()
	mat.albedo_color = color
	mi.material_override = mat
	add_child(mi)
	if size.y > 0.2:
		var body := StaticBody3D.new()
		var shape := CollisionShape3D.new()
		var box := BoxShape3D.new()
		box.size = size
		shape.shape = box
		body.add_child(shape)
		body.position = pos
		add_child(body)

func _make_car() -> void:
	car = CharacterBody3D.new()
	car.position = Vector3(0, 0.7, 24)
	add_child(car)
	var car_collision := CollisionShape3D.new()
	var car_shape := BoxShape3D.new()
	car_shape.size = Vector3(2.0, 1.4, 4.0)
	car_collision.shape = car_shape
	car_collision.position.y = 0.55
	car.add_child(car_collision)
	var body := MeshInstance3D.new()
	var bm := BoxMesh.new()
	bm.size = Vector3(2.0, 0.65, 4.0)
	body.mesh = bm
	body.position.y = 0.45
	var paint := StandardMaterial3D.new()
	paint.albedo_color = Color(0.04, 0.34, 0.23)
	paint.metallic = 0.55
	paint.roughness = 0.28
	body.material_override = paint
	car.add_child(body)
	var cabin := MeshInstance3D.new()
	var cm := BoxMesh.new()
	cm.size = Vector3(1.55, 0.65, 1.8)
	cabin.mesh = cm
	cabin.position = Vector3(0, 1.08, -0.25)
	var glass := StandardMaterial3D.new()
	glass.albedo_color = Color(0.12,0.22,0.27)
	cabin.material_override = glass
	car.add_child(cabin)
	for x in [-1.05, 1.05]:
		for z in [-1.35, 1.35]:
			var wheel := MeshInstance3D.new()
			var wm := CylinderMesh.new()
			wm.top_radius = 0.42
			wm.bottom_radius = 0.42
			wm.height = 0.28
			wheel.mesh = wm
			wheel.rotation_degrees.z = 90
			wheel.position = Vector3(x, 0.0, z)
			var rubber := StandardMaterial3D.new()
			rubber.albedo_color = Color(0.035,0.035,0.04)
			wheel.material_override = rubber
			car.add_child(wheel)

func _make_camera() -> void:
	camera = Camera3D.new()
	camera.current = true
	camera.fov = 72
	add_child(camera)

func _make_ui() -> void:
	var layer := CanvasLayer.new()
	add_child(layer)
	var title := Label.new()
	title.text = "SHOHIN CITY DRIVE  |  DUSHANBE"
	title.position = Vector2(24, 18)
	title.add_theme_font_size_override("font_size", 24)
	title.add_theme_color_override("font_color", Color(0.82,0.96,0.78))
	layer.add_child(title)
	speed_label = Label.new()
	speed_label.position = Vector2(24, 56)
	speed_label.add_theme_font_size_override("font_size", 22)
	layer.add_child(speed_label)
	_add_control(layer, "◀", Vector2(28,-112), "left")
	_add_control(layer, "▶", Vector2(128,-112), "right")
	_add_control(layer, "BRAKE", Vector2(-220,-112), "brake")
	_add_control(layer, "GAS", Vector2(-112,-112), "gas")

func _add_control(layer: CanvasLayer, caption: String, offset: Vector2, action: String) -> void:
	var b := Button.new()
	b.text = caption
	b.custom_minimum_size = Vector2(88, 76)
	b.anchor_left = 0.0 if offset.x >= 0 else 1.0
	b.anchor_right = 0.0 if offset.x >= 0 else 1.0
	b.anchor_top = 1.0
	b.anchor_bottom = 1.0
	b.position = offset
	b.add_theme_font_size_override("font_size", 20)
	b.modulate = Color(0.78,0.94,0.82,0.92)
	layer.add_child(b)
	b.button_down.connect(func(): _set_action(action, true))
	b.button_up.connect(func(): _set_action(action, false))

func _set_action(action: String, pressed: bool) -> void:
	match action:
		"gas": throttle = pressed
		"brake": braking = pressed
		"left": left_pressed = pressed
		"right": right_pressed = pressed

func _physics_process(delta: float) -> void:
	if throttle:
		speed = move_toward(speed, MAX_SPEED, ACCEL * delta)
	elif braking:
		speed = move_toward(speed, 0.0, 28.0 * delta)
	else:
		speed = move_toward(speed, 0.0, 3.0 * delta)
	var steer_input := float(right_pressed) - float(left_pressed)
	steering = move_toward(steering, steer_input, 3.0 * delta)
	if abs(speed) > 0.2:
		car.rotate_y(-steering * TURN_RATE * delta * clamp(abs(speed)/8.0, 0.25, 1.0))
	var forward := -car.global_transform.basis.z
	car.velocity = forward * speed
	car.move_and_slide()
	speed_label.text = "SPEED  %03d km/h" % int(abs(speed) * 3.6)
	var target := car.global_position + Vector3(0, 5.2, 10.5).rotated(Vector3.UP, car.rotation.y)
	camera.global_position = camera.global_position.lerp(target, 5.0 * delta)
	camera.look_at(car.global_position + Vector3(0, 1.0, 0), Vector3.UP)
