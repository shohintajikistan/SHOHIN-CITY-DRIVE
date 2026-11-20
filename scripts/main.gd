extends Node3D

# SHOHIN BEACH RACING — lightweight Godot 4.3 mobile prototype
const ROAD_WIDTH := 15.0
const PLAYER_SPEED := 25.0
const ACCELERATION := 17.0
const TURN_SPEED := 2.0

var track_points: Array[Vector3] = []
var player: CharacterBody3D
var camera: Camera3D
var speed := 0.0
var gas := false
var brake := false
var left := false
var right := false
var next_checkpoint := 1
var lap := 1
var lap_label: Label
var speed_label: Label
var opponents: Array[Dictionary] = []

func _ready() -> void:
	randomize()
	_build_track_points()
	_make_environment()
	_make_track()
	_make_scenery()
	_make_player()
	_make_opponents()
	_make_camera()
	_make_ui()

func _build_track_points() -> void:
	# Wide, flowing island circuit with beach-side bends.
	track_points = [
		Vector3(-42,0,48), Vector3(-18,0,58), Vector3(12,0,58),
		Vector3(40,0,48), Vector3(55,0,25), Vector3(52,0,-5),
		Vector3(38,0,-30), Vector3(12,0,-43), Vector3(-18,0,-43),
		Vector3(-45,0,-30), Vector3(-58,0,-7), Vector3(-57,0,20)
	]

func _make_environment() -> void:
	var world := WorldEnvironment.new()
	var env := Environment.new()
	env.background_mode = Environment.BG_SKY
	env.sky = Sky.new()
	env.sky.sky_material = ProceduralSkyMaterial.new()
	env.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	env.ambient_light_color = Color(0.72,0.84,0.88)
	env.ambient_light_energy = 0.8
	world.environment = env
	add_child(world)
	var sun := DirectionalLight3D.new()
	sun.rotation_degrees = Vector3(-52,-32,0)
	sun.light_energy = 1.25
	add_child(sun)
	_box(Vector3(0,-2.2,0),Vector3(500,1,500),Color(0.05,0.48,0.67),false)
	_box(Vector3(0,-0.65,0),Vector3(170,1,160),Color(0.88,0.76,0.48),false)

func _make_track() -> void:
	for i in range(track_points.size()):
		var a: Vector3 = track_points[i]
		var b: Vector3 = track_points[(i+1)%track_points.size()]
		var d := b-a
		var length := d.length()
		var angle := atan2(d.x,d.z)
		_box((a+b)*0.5+Vector3(0,0.03,0),Vector3(ROAD_WIDTH,0.35,length+2.0),Color(0.72,0.56,0.34),true,angle)
		# Bright curb blocks make the racing line easy to read.
		for side in [-1.0,1.0]:
			var offset := Vector3(cos(angle)*side*(ROAD_WIDTH*0.5+0.65),0.22,-sin(angle)*side*(ROAD_WIDTH*0.5+0.65))
			_box((a+b)*0.5+offset,Vector3(1.15,0.42,length+1.0),Color(0.91,0.20,0.12) if i%2==0 else Color(0.96,0.91,0.78),false,angle)
	# Start/finish stripe across the first straight.
	_box(track_points[0]+Vector3(0,0.27,-1),Vector3(ROAD_WIDTH,0.08,1.2),Color(0.98,0.98,0.91),false)

func _make_scenery() -> void:
	# Palm trees, rocks and small tropical props around the island.
	for i in range(track_points.size()):
		var p: Vector3 = track_points[i]
		for side in [-1.0,1.0]:
			var tree_pos := p + Vector3(side*randf_range(15,24),0,randf_range(-8,8))
			_palm(tree_pos)
	for i in range(22):
		var x := randf_range(-78,78)
		var z := randf_range(-68,72)
		if abs(x)<62 and abs(z)<48:
			continue
		_box(Vector3(x,0.6,z),Vector3(randf_range(1.5,3.5),randf_range(1.0,2.5),randf_range(1.5,3.5)),Color(0.43,0.46,0.39),false)
	# Two wooden bridges across shallow lagoon sections.
	_box(Vector3(54,0.34,10),Vector3(18,0.35,7),Color(0.42,0.23,0.12),false,-0.15)
	_box(Vector3(-53,0.34,6),Vector3(18,0.35,7),Color(0.42,0.23,0.12),false,0.12)
	# Jump ramp on the back straight.
	_box(Vector3(0,0.75,-42),Vector3(8,1.0,8),Color(0.86,0.34,0.10),false,0.0)

func _palm(pos: Vector3) -> void:
	var trunk := MeshInstance3D.new()
	var cyl := CylinderMesh.new()
	cyl.top_radius=0.22
	cyl.bottom_radius=0.38
	cyl.height=5.2
	trunk.mesh=cyl
	trunk.position=pos+Vector3(0,2.6,0)
	var bark := StandardMaterial3D.new()
	bark.albedo_color=Color(0.42,0.24,0.11)
	trunk.material_override=bark
	add_child(trunk)
	for j in range(6):
		var leaf := MeshInstance3D.new()
		var leaf_mesh := SphereMesh.new()
		leaf_mesh.radius=1.9
		leaf_mesh.height=0.45
		leaf.mesh=leaf_mesh
		leaf.position=pos+Vector3(cos(j*TAU/6.0)*1.45,5.25,sin(j*TAU/6.0)*1.45)
		leaf.rotation_degrees.z=randf_range(-18,18)
		var green := StandardMaterial3D.new()
		green.albedo_color=Color(0.08,randf_range(0.34,0.52),0.18)
		leaf.material_override=green
		add_child(leaf)

func _make_player() -> void:
	player=CharacterBody3D.new()
	player.name="PlayerBuggy"
	player.position=track_points[0]+Vector3(0,0.65,5)
	var first_dir: Vector3=track_points[1]-track_points[0]
	player.rotation.y=atan2(-first_dir.x,-first_dir.z)
	add_child(player)
	var shape:=CollisionShape3D.new()
	var box:=BoxShape3D.new()
	box.size=Vector3(2.0,1.25,3.6)
	shape.shape=box
	shape.position.y=0.45
	player.add_child(shape)
	_buggy_visual(player,Color(0.04,0.38,0.25))
	next_checkpoint=1

func _make_opponents() -> void:
	var paints=[Color(0.85,0.20,0.08),Color(0.12,0.32,0.82),Color(0.92,0.68,0.08)]
	for i in range(3):
		var rival:=Node3D.new()
		rival.name="Rival_"+str(i+1)
		rival.position=track_points[0]+Vector3((i-1)*3.2,0.55,10+i*3.5)
		add_child(rival)
		_buggy_visual(rival,paints[i])
		opponents.append({"node":rival,"target":1,"speed":17.0+float(i)*1.2})

func _buggy_visual(root: Node3D, paint_color: Color) -> void:
	var body:=MeshInstance3D.new()
	var mesh:=BoxMesh.new()
	mesh.size=Vector3(2.0,0.65,3.5)
	body.mesh=mesh
	body.position.y=0.45
	var mat:=StandardMaterial3D.new()
	mat.albedo_color=paint_color
	mat.metallic=0.28
	mat.roughness=0.34
	body.material_override=mat
	root.add_child(body)
	var cabin:=MeshInstance3D.new()
	var cab:=BoxMesh.new()
	cab.size=Vector3(1.45,0.7,1.55)
	cabin.mesh=cab
	cabin.position=Vector3(0,1.05,-0.15)
	var glass:=StandardMaterial3D.new()
	glass.albedo_color=Color(0.12,0.23,0.29)
	cabin.material_override=glass
	root.add_child(cabin)
	for x in [-1.05,1.05]:
		for z in [-1.15,1.15]:
			var wheel:=MeshInstance3D.new()
			var w:=CylinderMesh.new()
			w.top_radius=0.4
			w.bottom_radius=0.4
			w.height=0.28
			wheel.mesh=w
			wheel.rotation_degrees.z=90
			wheel.position=Vector3(x,0.05,z)
			var rubber:=StandardMaterial3D.new()
			rubber.albedo_color=Color(0.025,0.025,0.03)
			wheel.material_override=rubber
			root.add_child(wheel)

func _make_camera() -> void:
	camera=Camera3D.new()
	camera.current=true
	camera.fov=76
	add_child(camera)
	camera.position=player.global_position+Vector3(0,7,12)
	camera.look_at(player.global_position,Vector3.UP)

func _make_ui() -> void:
	var layer:=CanvasLayer.new()
	add_child(layer)
	var title:=Label.new()
	title.text="SHOHIN BEACH RACING"
	title.position=Vector2(22,16)
	title.add_theme_font_size_override("font_size",25)
	title.add_theme_color_override("font_color",Color(0.88,1.0,0.78))
	layer.add_child(title)
	lap_label=Label.new()
	lap_label.position=Vector2(22,53)
	lap_label.add_theme_font_size_override("font_size",20)
	layer.add_child(lap_label)
	speed_label=Label.new()
	speed_label.position=Vector2(22,82)
	speed_label.add_theme_font_size_override("font_size",20)
	layer.add_child(speed_label)
	_add_button(layer,"◀",Vector2(25,-112),"left")
	_add_button(layer,"▶",Vector2(125,-112),"right")
	_add_button(layer,"BRAKE",Vector2(-222,-112),"brake")
	_add_button(layer,"GAS",Vector2(-112,-112),"gas")

func _add_button(layer: CanvasLayer, caption: String, offset: Vector2, action: String) -> void:
	var b:=Button.new()
	b.text=caption
	b.custom_minimum_size=Vector2(88,76)
	b.anchor_left=0.0 if offset.x>=0 else 1.0
	b.anchor_right=0.0 if offset.x>=0 else 1.0
	b.anchor_top=1.0
	b.anchor_bottom=1.0
	b.position=offset
	b.add_theme_font_size_override("font_size",19)
	layer.add_child(b)
	b.button_down.connect(func(): _set_control(action,true))
	b.button_up.connect(func(): _set_control(action,false))

func _set_control(action: String, value: bool) -> void:
	match action:
		"gas": gas=value
		"brake": brake=value
		"left": left=value
		"right": right=value

func _physics_process(delta: float) -> void:
	if gas:
		speed=move_toward(speed,PLAYER_SPEED,ACCELERATION*delta)
	elif brake:
		speed=move_toward(speed,0.0,28.0*delta)
	else:
		speed=move_toward(speed,0.0,3.0*delta)
	var steer:=float(right)-float(left)
	if abs(speed)>0.15:
		player.rotate_y(-steer*TURN_SPEED*delta*clamp(abs(speed)/8.0,0.25,1.0))
	player.velocity=(-player.global_transform.basis.z)*speed
	player.move_and_slide()
	_update_checkpoints()
	_update_opponents(delta)
	_update_camera(delta)
	speed_label.text="SPEED  %03d km/h" % int(abs(speed)*3.6)
	lap_label.text="LAP  %d / 3     CHECKPOINT  %d / %d" % [lap,next_checkpoint,track_points.size()]

func _update_checkpoints() -> void:
	var target: Vector3=track_points[next_checkpoint]
	if player.global_position.distance_to(target)<10.0:
		next_checkpoint+=1
		if next_checkpoint>=track_points.size():
			next_checkpoint=0
			lap+=1
			if lap>3:
				lap=3
				speed=0.0
				gas=false

func _update_opponents(delta: float) -> void:
	for rival_data in opponents:
		var rival: Node3D=rival_data["node"]
		var idx: int=rival_data["target"]
		var target: Vector3=track_points[idx]
		if rival.global_position.distance_to(target)<8.0:
			idx=(idx+1)%track_points.size()
			rival_data["target"]=idx
			target=track_points[idx]
		var desired:=rival.global_position.direction_to(target)
		var wanted_y:=atan2(-desired.x,-desired.z)
		rival.rotation.y=lerp_angle(rival.rotation.y,wanted_y,2.0*delta)
		rival.position+=(-rival.global_transform.basis.z)*float(rival_data["speed"])*delta

func _update_camera(delta: float) -> void:
	var desired:=player.global_position+Vector3(0,7,11).rotated(Vector3.UP,player.rotation.y)
	camera.global_position=camera.global_position.lerp(desired,5.0*delta)
	camera.look_at(player.global_position+Vector3(0,1.0,0),Vector3.UP)

func _box(pos: Vector3, size: Vector3, color: Color, collision: bool=false, yaw: float=0.0) -> void:
	var mi:=MeshInstance3D.new()
	var mesh:=BoxMesh.new()
	mesh.size=size
	mi.mesh=mesh
	mi.position=pos
	mi.rotation.y=yaw
	var mat:=StandardMaterial3D.new()
	mat.albedo_color=color
	mi.material_override=mat
	add_child(mi)
	if collision:
		var body:=StaticBody3D.new()
		var col:=CollisionShape3D.new()
		var box:=BoxShape3D.new()
		box.size=size
		col.shape=box
		body.add_child(col)
		body.position=pos
		body.rotation.y=yaw
		add_child(body)
