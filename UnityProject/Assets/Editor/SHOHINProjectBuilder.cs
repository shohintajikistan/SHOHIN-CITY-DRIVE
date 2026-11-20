using System.IO;
using SHOHIN.CityDrive;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SHOHIN.Editor
{
    public static class SHOHINProjectBuilder
    {
        private const string ScenePath = "Assets/Scenes/SHOHIN_City_Test.unity";
        private const string SettingsPath = "Assets/Settings";
        private const string MaterialsPath = "Assets/Art/Materials";

        [MenuItem("SHOHIN/Generate Starter City Scene")]
        public static void GenerateScene()
        {
            EnsureFolders();
            ConfigureURP();
            CreateMaterials();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            BuildLighting();
            BuildCity();
            VehicleController vehicle = BuildVehicle();
            BuildCamera(vehicle.transform);
            BuildHUD(vehicle);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("SHOHIN CITY DRIVE starter scene generated. Vehicle visuals are temporary test geometry.");
        }

        private static void EnsureFolders()
        {
            foreach (string folder in new[] { "Assets/Scenes", "Assets/Settings", "Assets/Art", MaterialsPath })
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    string parent = Path.GetDirectoryName(folder).Replace("\\", "/");
                    string name = Path.GetFileName(folder);
                    AssetDatabase.CreateFolder(parent, name);
                }
        }

        private static void ConfigureURP()
        {
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(SettingsPath + "/SHOHIN_URP.asset");
            if (pipeline == null)
            {
                var renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, SettingsPath + "/SHOHIN_Renderer.asset");
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                pipeline.name = "SHOHIN_URP";
                AssetDatabase.CreateAsset(pipeline, SettingsPath + "/SHOHIN_URP.asset");
            }
            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
        }

        private static Material asphalt, ground, concrete, glass, bodyPaint, rubber, lanePaint;
        private static void CreateMaterials()
        {
            asphalt = MakeMaterial("Asphalt", new Color(0.075f, 0.085f, 0.09f), 0.05f);
            ground = MakeMaterial("Ground", new Color(0.13f, 0.20f, 0.15f), 0f);
            concrete = MakeMaterial("Architecture", new Color(0.36f, 0.40f, 0.39f), 0.12f);
            glass = MakeMaterial("DarkGlass", new Color(0.025f, 0.055f, 0.065f), 0.65f);
            bodyPaint = MakeMaterial("TestChassis_Green", new Color(0.025f, 0.30f, 0.18f), 0.55f);
            rubber = MakeMaterial("TireRubber", new Color(0.018f, 0.02f, 0.022f), 0.02f);
            lanePaint = MakeMaterial("RoadMarkings", new Color(0.82f, 0.78f, 0.58f), 0.1f);
        }

        private static Material MakeMaterial(string name, Color color, float metallic)
        {
            string path = MaterialsPath + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.color = color;
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            return material;
        }

        private static void BuildLighting()
        {
            Time.fixedDeltaTime = 0.02f;
            Physics.defaultSolverIterations = 12;
            Physics.defaultSolverVelocityIterations = 4;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.31f, 0.39f, 0.46f);
            RenderSettings.ambientEquatorColor = new Color(0.17f, 0.20f, 0.19f);
            RenderSettings.ambientGroundColor = new Color(0.08f, 0.09f, 0.08f);
            var lightObject = new GameObject("Sun - warm daylight");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.color = new Color(1f, 0.91f, 0.78f);
            light.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
        }

        private static void BuildCity()
        {
            CreatePrimitive("City Ground", PrimitiveType.Plane, new Vector3(0f, -0.12f, 0f), new Vector3(24f, 1f, 24f), ground);
            const float extent = 220f;
            for (int i = -2; i <= 2; i++)
            {
                float p = i * 40f;
                CreatePrimitive("Road North-South", PrimitiveType.Cube, new Vector3(p, 0f, 0f), new Vector3(9f, 0.12f, extent), asphalt);
                CreatePrimitive("Road East-West", PrimitiveType.Cube, new Vector3(0f, 0.01f, p), new Vector3(extent, 0.12f, 9f), asphalt);
                for (int z = -100; z <= 100; z += 12)
                    CreateVisual("Road Dash", PrimitiveType.Cube, new Vector3(p, 0.071f, z), new Vector3(0.12f, 3.2f, 0.55f), lanePaint);
                for (int x = -100; x <= 100; x += 12)
                    CreateVisual("Road Dash", PrimitiveType.Cube, new Vector3(x, 0.072f, p), new Vector3(3.2f, 0.12f, 0.55f), lanePaint);
            }

            var random = new System.Random(42);
            for (int gx = -2; gx < 2; gx++)
            for (int gz = -2; gz < 2; gz++)
            {
                float cx = gx * 40f + 20f;
                float cz = gz * 40f + 20f;
                for (int b = 0; b < 3; b++)
                {
                    float x = cx + (float)(random.NextDouble() * 17f - 8.5f);
                    float z = cz + (float)(random.NextDouble() * 17f - 8.5f);
                    float w = 6f + (float)random.NextDouble() * 5f;
                    float d = 6f + (float)random.NextDouble() * 5f;
                    float h = 7f + (float)random.NextDouble() * 20f;
                    var building = CreatePrimitive("City Blockout Building", PrimitiveType.Cube,
                        new Vector3(x, h * 0.5f - 0.12f, z), new Vector3(w, h, d), concrete);
                    building.transform.rotation = Quaternion.Euler(0f, random.Next(0, 4) * 90f, 0f);
                }
            }
        }

        private static VehicleController BuildVehicle()
        {
            var root = new GameObject("SHOHIN Test Vehicle - replace visuals with licensed car model");
            root.transform.position = new Vector3(0f, 0.34f, -12f);
            var rb = root.AddComponent<Rigidbody>();
            rb.mass = 1450f;
            rb.linearDamping = 0.015f;
            rb.angularDamping = 0.55f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            var chassis = root.AddComponent<BoxCollider>();
            chassis.size = new Vector3(1.55f, 0.62f, 4.1f);
            chassis.center = new Vector3(0f, 0.15f, 0f);

            CreatePrimitiveChild(root.transform, "Temporary Chassis", PrimitiveType.Cube,
                new Vector3(0f, 0.28f, 0f), new Vector3(1.82f, 0.52f, 4.15f), bodyPaint);
            CreatePrimitiveChild(root.transform, "Temporary Cabin", PrimitiveType.Cube,
                new Vector3(0f, 0.73f, -0.25f), new Vector3(1.42f, 0.55f, 1.75f), glass);

            var wheels = new WheelCollider[4];
            Transform[] visuals = new Transform[4];
            Vector3[] positions =
            {
                new Vector3(-0.88f, 0f, 1.35f), new Vector3(0.88f, 0f, 1.35f),
                new Vector3(-0.88f, 0f, -1.35f), new Vector3(0.88f, 0f, -1.35f)
            };
            for (int i = 0; i < 4; i++)
            {
                var wheelObject = new GameObject("WheelCollider " + i);
                wheelObject.transform.SetParent(root.transform, false);
                wheelObject.transform.localPosition = positions[i];
                var wheel = wheelObject.AddComponent<WheelCollider>();
                wheel.mass = 22f;
                wheel.radius = 0.34f;
                wheel.suspensionDistance = 0.19f;
                wheel.wheelDampingRate = 0.35f;
                wheel.forceAppPointDistance = 0.12f;
                JointSpring spring = wheel.suspensionSpring;
                spring.spring = 36000f;
                spring.damper = 4600f;
                spring.targetPosition = 0.52f;
                wheel.suspensionSpring = spring;
                wheel.forwardFriction = Friction(0.38f, 1f, 0.8f, 0.65f);
                wheel.sidewaysFriction = Friction(0.25f, 1f, 0.5f, 0.75f);
                wheels[i] = wheel;

                var visual = CreatePrimitiveChild(root.transform, "Wheel Visual " + i, PrimitiveType.Cylinder,
                    positions[i], new Vector3(0.34f, 0.13f, 0.34f), rubber);
                visuals[i] = visual.transform;
            }

            var controller = root.AddComponent<VehicleController>();
            SerializedObject serialized = new SerializedObject(controller);
            serialized.FindProperty("frontLeft").objectReferenceValue = wheels[0];
            serialized.FindProperty("frontRight").objectReferenceValue = wheels[1];
            serialized.FindProperty("rearLeft").objectReferenceValue = wheels[2];
            serialized.FindProperty("rearRight").objectReferenceValue = wheels[3];
            serialized.FindProperty("frontLeftVisual").objectReferenceValue = visuals[0];
            serialized.FindProperty("frontRightVisual").objectReferenceValue = visuals[1];
            serialized.FindProperty("rearLeftVisual").objectReferenceValue = visuals[2];
            serialized.FindProperty("rearRightVisual").objectReferenceValue = visuals[3];
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return controller;
        }

        private static WheelFrictionCurve Friction(float extremumSlip, float extremumValue, float asymptoteSlip, float asymptoteValue)
        {
            return new WheelFrictionCurve
            {
                extremumSlip = extremumSlip,
                extremumValue = extremumValue,
                asymptoteSlip = asymptoteSlip,
                asymptoteValue = asymptoteValue,
                stiffness = 1f
            };
        }

        private static void BuildCamera(Transform target)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 5f, -21f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 66f;
            camera.allowHDR = false;
            camera.allowMSAA = true;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<ChaseCamera>().SetTarget(target);
        }

        private static void BuildHUD(VehicleController vehicle)
        {
            var settingsObject = new GameObject("SHOHIN Mobile Runtime Settings");
            settingsObject.AddComponent<MobileGameSettings>();
            var canvasObject = new GameObject("SHOHIN Mobile HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var speedPanel = new GameObject("HUD Speed Panel", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Shadow));
            speedPanel.transform.SetParent(canvas.transform, false);
            var speedRect = speedPanel.GetComponent<RectTransform>();
            speedRect.anchorMin = new Vector2(0.5f, 1f);
            speedRect.anchorMax = new Vector2(0.5f, 1f);
            speedRect.anchoredPosition = new Vector2(0f, -82f);
            speedRect.sizeDelta = new Vector2(440f, 104f);
            speedPanel.GetComponent<Image>().color = new Color(0.012f, 0.055f, 0.04f, 0.82f);
            var speedOutline = speedPanel.GetComponent<Outline>();
            speedOutline.effectColor = new Color(0.72f, 0.83f, 0.39f, 0.72f);
            speedOutline.effectDistance = new Vector2(2f, -2f);
            var speedShadow = speedPanel.GetComponent<Shadow>();
            speedShadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
            speedShadow.effectDistance = new Vector2(0f, -7f);
            var speed = CreateText(speedPanel.transform, "000 KM/H", 54, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
            var speedTextRect = speed.GetComponent<RectTransform>();
            speedTextRect.offsetMin = Vector2.zero;
            speedTextRect.offsetMax = Vector2.zero;
            speed.color = new Color(0.82f, 1f, 0.83f);
            var title = CreateText(canvas.transform, "SHOHIN  /  CITY DRIVE", 22, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(35f, -30f), new Vector2(560f, 55f), TextAnchor.MiddleLeft);
            title.color = new Color(0.78f, 0.88f, 0.52f);
            var serialized = new SerializedObject(vehicle);
            serialized.FindProperty("speedText").objectReferenceValue = speed;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }

            AddControl(canvas.transform, vehicle, "LEFT", HoldButton.ActionType.SteerLeft, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(130f, 135f), new Vector2(180f, 150f));
            AddControl(canvas.transform, vehicle, "RIGHT", HoldButton.ActionType.SteerRight, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(330f, 135f), new Vector2(180f, 150f));
            AddControl(canvas.transform, vehicle, "BRAKE", HoldButton.ActionType.Brake, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-300f, 135f), new Vector2(190f, 150f));
            AddControl(canvas.transform, vehicle, "GAS", HoldButton.ActionType.Accelerate, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-105f, 135f), new Vector2(190f, 150f));
            AddControl(canvas.transform, vehicle, "DRIFT", HoldButton.ActionType.Handbrake, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-105f, 300f), new Vector2(145f, 100f));
        }

        private static Text CreateText(Transform parent, string value, int size, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 dimensions, TextAnchor alignment)
        {
            var go = new GameObject("Text - " + value, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin; rect.anchorMax = anchorMax;
            rect.anchoredPosition = position; rect.sizeDelta = dimensions;
            var text = go.GetComponent<Text>();
            text.text = value; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size; text.alignment = alignment; text.color = Color.white;
            return text;
        }

        private static void AddControl(Transform parent, VehicleController vehicle, string label, HoldButton.ActionType action,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 dimensions)
        {
            var go = new GameObject("Touch " + label, typeof(RectTransform), typeof(Image), typeof(HoldButton));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin; rect.anchorMax = anchorMax;
            rect.anchoredPosition = position; rect.sizeDelta = dimensions;
            var image = go.GetComponent<Image>();
            image.color = new Color(0.018f, 0.095f, 0.067f, 0.84f);
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0.08f, 0.58f, 0.36f, 0.78f);
            outline.effectDistance = new Vector2(2f, -2f);
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.42f);
            shadow.effectDistance = new Vector2(0f, -6f);
            var hold = go.GetComponent<HoldButton>();
            hold.Configure(action, vehicle);
            var text = CreateText(go.transform, label, 25, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
            text.color = new Color(0.82f, 1f, 0.9f);
            var textRect = text.GetComponent<RectTransform>();
            textRect.offsetMin = Vector2.zero; textRect.offsetMax = Vector2.zero;
        }

        private static GameObject CreateVisual(string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material)
        {
            var go = CreatePrimitive(name, type, position, scale, material);
            Collider collider = go.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);
            return go;
        }

        private static GameObject CreatePrimitive(string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name; go.transform.position = position; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        private static GameObject CreatePrimitiveChild(Transform parent, string name, PrimitiveType type, Vector3 localPosition, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name; go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            Collider collider = go.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);
            return go;
        }

        [MenuItem("SHOHIN/Build Android APK")]
        public static void BuildAndroid()
        {
            GenerateScene();
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            PlayerSettings.productName = "SHOHIN CITY DRIVE";
            PlayerSettings.companyName = "SHOHIN TAJIKISTAN";
            PlayerSettings.applicationIdentifier = "com.shohin.citydrive";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel36;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string buildFolder = Path.Combine(projectRoot, "Build", "Android");
            Directory.CreateDirectory(buildFolder);
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = Path.Combine(buildFolder, "SHOHIN-CITY-DRIVE.apk"),
                target = BuildTarget.Android,
                options = BuildOptions.None
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new System.Exception("Android build failed: " + report.summary.result);
            Debug.Log("APK built: " + options.locationPathName);
        }
    }
}
