using System.Collections;
using System.IO;
using UnityEngine;
using GLTFast;
using AresResurgence.Player;
using AresResurgence.Story;
using AresResurgence.UI;
using AresResurgence.Interaction;

namespace AresResurgence.Environment
{
    /// <summary>
    /// Master Scene Architect for the NASA Odyssey Experience.
    /// Procedurally constructs and manages all 4 cinematic Hollywood environments:
    /// 1. NASA Neutral Buoyancy & Zero-G Physics Simulator (with JSC Mission Control & EVA Suit)
    /// 2. Cape Canaveral Launch Complex 39A (with Saturn V Rocket on launchpad)
    /// 3. Low Earth Orbit ISS Arena (with ISS Station, Docking Port, and photorealistic Earth)
    /// 4. ISS Cupola Panoramic Observation Deck
    /// </summary>
    public class NASAMasterExperience : MonoBehaviour
    {
        private Material baseLitMaterial;
        private Transform playerTransform;
        private Camera mainCamera;
        private GameObject earthSphere;

        // Rigs for each story phase
        private GameObject trainingRig;
        private GameObject launchPadRig;
        private GameObject issRig;
        private GameObject cupolaRig;
        private GameObject rocketObj;

        private void Start()
        {
            BuildEntireNASAOdyssey();
        }

        public void BuildEntireNASAOdyssey()
        {
            Debug.Log("[NASAMasterExperience] Constructing Hollywood NASA Odyssey Environments...");

            // 1. Setup Cosmic Lighting and Starfield
            SetupLightingAndSpaceAtmosphere();

            // 2. Build Phase 1: Zero-G Physics Training Chamber
            trainingRig = BuildZeroGTrainingChamber();

            // 3. Build Phase 2: Cape Canaveral Launchpad
            launchPadRig = BuildLaunchPadComplex();

            // 4. Build Phase 3: ISS Low Earth Orbit Arena & Earth Sphere
            issRig = BuildISSOralArena();

            // 5. Build Phase 4: Cupola Observation Module Interior
            cupolaRig = BuildCupolaObservationBay();

            // 6. Spawn Astronaut Player
            GameObject player = SpawnZeroGAstronaut(new Vector3(0f, 2f, -12f));
            playerTransform = player.transform;

            // 7. Setup Directors and HUD
            SetupDirectorsAndHUD(player);

            // 8. Load Official NASA GLB 3D Models asynchronously via glTFast
            StartCoroutine(LoadAllNasaGlbAssets());
        }

        private Material GetBaseMaterial()
        {
            if (baseLitMaterial == null)
            {
                var urpAsset = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
                if (urpAsset != null && urpAsset.defaultMaterial != null)
                {
                    baseLitMaterial = new Material(urpAsset.defaultMaterial);
                }
                else
                {
                    Shader s = Shader.Find("Universal Render Pipeline/Lit") 
                               ?? Shader.Find("Universal Render Pipeline/Simple Lit") 
                               ?? Shader.Find("Universal Render Pipeline/Unlit")
                               ?? Shader.Find("Unlit/Color");
                    if (s != null)
                    {
                        baseLitMaterial = new Material(s);
                    }
                    else
                    {
                        GameObject dummy = GameObject.CreatePrimitive(PrimitiveType.Quad);
                        Renderer r = dummy.GetComponent<Renderer>();
                        if (r != null && r.sharedMaterial != null)
                            baseLitMaterial = new Material(r.sharedMaterial);
                        Destroy(dummy);
                    }
                }
            }
            return baseLitMaterial;
        }

        private Material CreatePBRMaterial(Color albedo, float metallic = 0.5f, float smoothness = 0.6f, bool emissive = false, Color? emissionColor = null)
        {
            Material mat = new Material(GetBaseMaterial());
            mat.color = albedo;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", albedo);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);

            if (emissive)
            {
                mat.EnableKeyword("_EMISSION");
                Color emCol = emissionColor ?? albedo;
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emCol);
            }
            return mat;
        }

        private void SetupLightingAndSpaceAtmosphere()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.04f, 0.08f, 0.14f);

            // Primary Sunlight (High-contrast harsh vacuum sun)
            GameObject sunObj = new GameObject("Cosmic_Sun_KeyLight");
            Light sun = sunObj.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1.0f, 0.98f, 0.94f);
            sun.intensity = 1.9f;
            sun.shadows = LightShadows.Soft;
            sunObj.transform.rotation = Quaternion.Euler(42f, -35f, 0f);

            // Earthshine Fill Light (Cyan/Blue reflection from planet)
            GameObject earthshineObj = new GameObject("Earthshine_FillLight");
            Light earthshine = earthshineObj.AddComponent<Light>();
            earthshine.type = LightType.Directional;
            earthshine.color = new Color(0.12f, 0.35f, 0.65f);
            earthshine.intensity = 0.65f;
            earthshine.shadows = LightShadows.None;
            earthshineObj.transform.rotation = Quaternion.Euler(-135f, 145f, 0f);

            // Cosmic Starfield Particle System
            GameObject starsObj = new GameObject("Cosmic_DeepSpace_Starfield");
            ParticleSystem ps = starsObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.maxParticles = 3000;
            main.startLifetime = 1000f;
            main.startSpeed = 0f;
            main.startSize = 0.35f;
            main.startColor = new Color(0.95f, 0.95f, 1f, 0.95f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 450f;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 2500) });
            ps.Play();
        }

        private GameObject BuildZeroGTrainingChamber()
        {
            GameObject rig = new GameObject("ZeroG_Training_Rig");

            // Chamber Shell (High-Tech Neutral Buoyancy Tank & Microgravity Training Module)
            // Dimensions: 30m wide x 16m high x 50m long
            Material wallMat = CreatePBRMaterial(new Color(0.18f, 0.22f, 0.28f), 0.7f, 0.4f);
            Material gridMat = CreatePBRMaterial(new Color(0.08f, 0.12f, 0.18f), 0.8f, 0.3f);
            Material accentMat = CreatePBRMaterial(new Color(0.05f, 0.4f, 0.85f), 0.2f, 0.8f, true, new Color(0.1f, 0.6f, 1f) * 1.5f);

            // Floor & Ceiling
            CreateBox("Floor", rig.transform, new Vector3(0f, -5f, 5f), new Vector3(32f, 1f, 52f), gridMat);
            CreateBox("Ceiling", rig.transform, new Vector3(0f, 12f, 5f), new Vector3(32f, 1f, 52f), gridMat);

            // Left & Right Walls
            CreateBox("Wall_Left", rig.transform, new Vector3(-16f, 3.5f, 5f), new Vector3(1f, 16f, 52f), wallMat);
            CreateBox("Wall_Right", rig.transform, new Vector3(16f, 3.5f, 5f), new Vector3(1f, 16f, 52f), wallMat);

            // End Walls
            CreateBox("Wall_Back", rig.transform, new Vector3(0f, 3.5f, -21f), new Vector3(32f, 16f, 1f), wallMat);
            CreateBox("Wall_Front", rig.transform, new Vector3(0f, 3.5f, 31f), new Vector3(32f, 16f, 1f), wallMat);

            // Glowing NASA Guide Rails
            CreateBox("Rail_L", rig.transform, new Vector3(-15.3f, 3.5f, 5f), new Vector3(0.2f, 0.2f, 50f), accentMat);
            CreateBox("Rail_R", rig.transform, new Vector3(15.3f, 3.5f, 5f), new Vector3(0.2f, 0.2f, 50f), accentMat);

            // Observation Command Bridge (Where JSC Mission Control is docked)
            GameObject bridge = CreateBox("Command_Bridge", rig.transform, new Vector3(0f, 8f, -16f), new Vector3(14f, 3f, 8f), wallMat);

            // 4 Navigation Waypoint Rings for Zero-G Training Course
            Vector3[] ringPositions = new Vector3[]
            {
                new Vector3(0f, 2f, -3f),    // Ring 0: Linear forward drift
                new Vector3(4f, 3.5f, 6f),   // Ring 1: Lateral strafe
                new Vector3(-4f, 6.5f, 15f), // Ring 2: Vertical elevation
                new Vector3(0f, 4f, 25f)     // Ring 3: Final target alignment
            };

            for (int i = 0; i < ringPositions.Length; i++)
            {
                BuildWaypointRing(rig.transform, ringPositions[i], i);
            }

            return rig;
        }

        private void BuildWaypointRing(Transform parent, Vector3 pos, int index)
        {
            GameObject ringObj = new GameObject($"Waypoint_Ring_{index}");
            ringObj.transform.parent = parent;
            ringObj.transform.position = pos;

            // Torus / Ring Mesh using 8 cylinder segments forming an octagon ring
            float radius = 2.4f;
            int segments = 12;
            Material ringMat = CreatePBRMaterial(new Color(0.1f, 0.8f, 1.0f, 0.85f), 0.1f, 0.9f, true, new Color(0.15f, 0.75f, 1.0f) * 2f);

            for (int s = 0; s < segments; s++)
            {
                float a1 = (s * 360f / segments) * Mathf.Deg2Rad;
                float a2 = ((s + 1) * 360f / segments) * Mathf.Deg2Rad;
                Vector3 p1 = new Vector3(Mathf.Cos(a1) * radius, Mathf.Sin(a1) * radius, 0f);
                Vector3 p2 = new Vector3(Mathf.Cos(a2) * radius, Mathf.Sin(a2) * radius, 0f);

                Vector3 segCenter = (p1 + p2) * 0.5f;
                Vector3 dir = (p2 - p1);

                GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                bar.name = $"Segment_{s}";
                bar.transform.parent = ringObj.transform;
                bar.transform.localPosition = segCenter;
                bar.transform.localScale = new Vector3(0.18f, dir.magnitude * 0.5f, 0.18f);
                bar.transform.localRotation = Quaternion.FromToRotation(Vector3.up, dir);
                bar.GetComponent<Renderer>().sharedMaterial = ringMat;
                Destroy(bar.GetComponent<Collider>());
            }

            // Central Trigger Collider
            SphereCollider sc = ringObj.AddComponent<SphereCollider>();
            sc.radius = 2.0f;
            sc.isTrigger = true;

            ZeroGWaypointRing ringScript = ringObj.AddComponent<ZeroGWaypointRing>();
            ringScript.ringIndex = index;
            ringScript.SetHighlight(index == 0);
        }

        private GameObject BuildLaunchPadComplex()
        {
            GameObject rig = new GameObject("LaunchPad_Rig");
            rig.transform.position = new Vector3(300f, 0f, 0f); // Offset far from training

            Material concreteMat = CreatePBRMaterial(new Color(0.35f, 0.35f, 0.38f), 0.2f, 0.3f);
            Material steelMat = CreatePBRMaterial(new Color(0.65f, 0.25f, 0.15f), 0.8f, 0.5f); // Red gantry steel

            // Launch Platform (Launch Complex 39A)
            CreateBox("Pad_Foundation", rig.transform, new Vector3(0f, 2f, 0f), new Vector3(40f, 4f, 40f), concreteMat);
            CreateBox("Flame_Trench", rig.transform, new Vector3(0f, 0.5f, 0f), new Vector3(16f, 1.5f, 50f), concreteMat);

            // Gantry Service Tower
            CreateBox("Gantry_Column1", rig.transform, new Vector3(-8f, 28f, -8f), new Vector3(2f, 52f, 2f), steelMat);
            CreateBox("Gantry_Column2", rig.transform, new Vector3(8f, 28f, -8f), new Vector3(2f, 52f, 2f), steelMat);
            CreateBox("Gantry_Arm", rig.transform, new Vector3(0f, 45f, -4f), new Vector3(14f, 2f, 8f), steelMat);

            // Rocket container
            rocketObj = new GameObject("SaturnV_Rocket_Mount");
            rocketObj.transform.parent = rig.transform;
            rocketObj.transform.position = new Vector3(0f, 4f, 0f);

            // Procedural fallback cylinder if GLB is loading
            GameObject rBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rBody.name = "Rocket_Body_Fallback";
            rBody.transform.parent = rocketObj.transform;
            rBody.transform.localPosition = new Vector3(0f, 24f, 0f);
            rBody.transform.localScale = new Vector3(4f, 24f, 4f);
            rBody.GetComponent<Renderer>().sharedMaterial = CreatePBRMaterial(Color.white, 0.3f, 0.8f);

            // Launch Smoke & Fire Particle Emitter
            GameObject flameObj = new GameObject("Launch_Exhaust_Flames");
            flameObj.transform.parent = rocketObj.transform;
            flameObj.transform.localPosition = new Vector3(0f, 0f, 0f);
            ParticleSystem ps = flameObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.maxParticles = 1200;
            main.startLifetime = 1.5f;
            main.startSpeed = 25f;
            main.startSize = 3.5f;
            main.startColor = new Color(1f, 0.6f, 0.1f, 0.9f);
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 15f;
            shape.rotation = new Vector3(90f, 0f, 0f);

            rig.SetActive(false);
            return rig;
        }

        private GameObject BuildISSOralArena()
        {
            GameObject rig = new GameObject("ISS_Orbit_Rig");
            rig.transform.position = new Vector3(0f, 600f, 0f); // Placed high in orbit

            // Massive Photorealistic Earth Sphere
            earthSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            earthSphere.name = "Earth_BlueMarble_Sphere";
            earthSphere.transform.parent = rig.transform;
            earthSphere.transform.position = new Vector3(0f, -80f, 90f);
            earthSphere.transform.localScale = Vector3.one * 140f;
            Destroy(earthSphere.GetComponent<Collider>());

            // Apply Earth texture if loaded
            Texture2D earthTex = Resources.Load<Texture2D>("Textures/Earth_BlueMarble_2048");
            if (earthTex == null)
            {
                byte[] bytes = null;
                string localPath = Path.Combine(Application.dataPath, "Resources/Textures/Earth_BlueMarble_2048.jpg");
                if (File.Exists(localPath)) bytes = File.ReadAllBytes(localPath);
                if (bytes != null)
                {
                    earthTex = new Texture2D(2, 2);
                    earthTex.LoadImage(bytes);
                }
            }

            Material earthMat = CreatePBRMaterial(Color.white, 0.05f, 0.45f);
            if (earthTex != null)
            {
                earthMat.mainTexture = earthTex;
            }
            else
            {
                earthMat.color = new Color(0.12f, 0.35f, 0.75f);
            }
            earthSphere.GetComponent<Renderer>().sharedMaterial = earthMat;

            // Earth Rotation script
            earthSphere.AddComponent<EarthOrbitalRotation>();

            // ISS Docking Target Port at origin (0, 0, 0)
            GameObject portTarget = new GameObject("ISS_Docking_Port_Target");
            portTarget.transform.parent = rig.transform;
            portTarget.transform.localPosition = Vector3.zero;

            // Visual Docking Crosshairs Guide
            Material portMat = CreatePBRMaterial(new Color(0.9f, 0.9f, 0.95f), 0.8f, 0.6f);
            CreateBox("Port_Collar", portTarget.transform, Vector3.zero, new Vector3(2.2f, 2.2f, 0.4f), portMat);
            CreateBox("Port_TargetLight", portTarget.transform, new Vector3(0f, 1.3f, 0.1f), new Vector3(0.3f, 0.3f, 0.1f), 
                      CreatePBRMaterial(Color.green, 0f, 0.9f, true, Color.green * 2f));

            rig.SetActive(false);
            return rig;
        }

        private GameObject BuildCupolaObservationBay()
        {
            GameObject rig = new GameObject("Cupola_Bay_Rig");
            rig.transform.parent = issRig.transform;
            rig.transform.localPosition = new Vector3(0f, 0f, 0f);

            Material interiorMat = CreatePBRMaterial(new Color(0.12f, 0.14f, 0.18f), 0.7f, 0.3f);
            Material frameMat = CreatePBRMaterial(new Color(0.35f, 0.38f, 0.42f), 0.85f, 0.5f);
            Material glassMat = CreatePBRMaterial(new Color(0.7f, 0.85f, 1.0f, 0.25f), 0.1f, 0.95f);

            // Dome Room Shell with 7 Bay Window Frames
            // Center ceiling window
            CreateBox("Cupola_Top_Frame", rig.transform, new Vector3(0f, 2.2f, 0.5f), new Vector3(2.5f, 0.2f, 2.5f), frameMat);

            // Perimeter Windows looking down at Earth
            int windowCount = 6;
            float r = 2.0f;
            for (int w = 0; w < windowCount; w++)
            {
                float angle = w * 60f * Mathf.Deg2Rad;
                Vector3 wPos = new Vector3(Mathf.Cos(angle) * r, 0.8f, Mathf.Sin(angle) * r + 0.5f);
                CreateBox($"Cupola_Window_Pillar_{w}", rig.transform, wPos, new Vector3(0.25f, 2.0f, 0.25f), frameMat);
            }

            // Handrail inside Cupola
            CreateBox("Cupola_Handrail", rig.transform, new Vector3(0f, -0.2f, 0.5f), new Vector3(2.8f, 0.1f, 2.8f), 
                      CreatePBRMaterial(new Color(0.1f, 0.5f, 0.9f), 0.8f, 0.8f));

            rig.SetActive(false);
            return rig;
        }

        private GameObject SpawnZeroGAstronaut(Vector3 spawnPos)
        {
            GameObject player = new GameObject("ZeroG_Astronaut_Player");
            player.transform.position = spawnPos;

            Rigidbody rb = player.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.linearDamping = 0.05f;
            rb.angularDamping = 0.8f;

            SphereCollider col = player.AddComponent<SphereCollider>();
            col.radius = 0.65f;

            Camera cam = Camera.main;
            if (cam == null)
            {
                Camera[] cams = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                if (cams.Length > 0) cam = cams[0];
            }
            if (cam == null)
            {
                GameObject camObj = new GameObject("FirstPersonCamera");
                cam = camObj.AddComponent<Camera>();
            }

            cam.gameObject.name = "FirstPersonCamera";
            cam.tag = "MainCamera";
            cam.enabled = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.002f, 0.004f, 0.012f); // Deep cosmos black
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 1200f;
            cam.fieldOfView = 75f;

            cam.transform.parent = player.transform;
            cam.transform.localPosition = new Vector3(0f, 0f, 0f);
            cam.transform.localRotation = Quaternion.identity;

            if (cam.GetComponent<AudioListener>() == null)
            {
                cam.gameObject.AddComponent<AudioListener>();
            }

            var uac = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            if (uac != null)
            {
                uac.renderPostProcessing = false;
            }

            mainCamera = cam;

            ZeroGAstronautController controller = player.AddComponent<ZeroGAstronautController>();
            controller.cameraTransform = cam.transform;

            return player;
        }

        private void SetupDirectorsAndHUD(GameObject player)
        {
            GameObject directorObj = new GameObject("NASA_Story_Director");
            NASAOdysseyDirector director = directorObj.AddComponent<NASAOdysseyDirector>();

            director.trainingRig = trainingRig.transform;
            director.launchPadRig = launchPadRig.transform;
            director.issRig = issRig.transform;
            director.cupolaRig = cupolaRig.transform;
            director.rocketTransform = rocketObj.transform;
            director.mainCamera = mainCamera;
            director.astronautController = player.GetComponent<ZeroGAstronautController>();

            GameObject hudObj = new GameObject("NASA_Odyssey_HUD");
            NASAOdysseyHUD hud = hudObj.AddComponent<NASAOdysseyHUD>();
            hud.storyDirector = director;
            hud.playerController = player.GetComponent<ZeroGAstronautController>();
        }

        private IEnumerator LoadAllNasaGlbAssets()
        {
            yield return new WaitForSeconds(0.2f);

            string nasaFolder = Path.Combine(Application.dataPath, "NASA_Models");
            if (!Directory.Exists(nasaFolder))
            {
                nasaFolder = Path.Combine(Application.dataPath, "StreamingAssets/NASA_Models");
            }

            // 1. JSC Mission Control in Training Chamber Bridge
            string jscPath = Path.Combine(nasaFolder, "JSC_Mission_Control.glb");
            if (File.Exists(jscPath))
            {
                yield return StartCoroutine(LoadGlb(jscPath, trainingRig.transform, new Vector3(0f, 7.5f, -16f), Vector3.one * 1.0f, Quaternion.identity));
            }

            // 2. NASA EVA Suit mounted in training room airlock
            string suitPath = Path.Combine(nasaFolder, "EVA_Suit.glb");
            if (File.Exists(suitPath))
            {
                yield return StartCoroutine(LoadGlb(suitPath, trainingRig.transform, new Vector3(-8f, 0f, -14f), Vector3.one * 1.1f, Quaternion.Euler(0f, 90f, 0f)));
            }

            // 3. Saturn V Rocket on Launchpad
            string rocketPath = Path.Combine(nasaFolder, "Rocket_SaturnV.glb");
            if (File.Exists(rocketPath))
            {
                yield return StartCoroutine(LoadGlb(rocketPath, rocketObj.transform, Vector3.zero, Vector3.one * 1.5f, Quaternion.identity));
            }

            // 4. International Space Station (ISS) in Orbit
            string issPath = Path.Combine(nasaFolder, "ISS_Station.glb");
            if (File.Exists(issPath))
            {
                yield return StartCoroutine(LoadGlb(issPath, issRig.transform, new Vector3(0f, 0f, 35f), Vector3.one * 2.0f, Quaternion.Euler(15f, -30f, 0f)));
            }

            // 5. Docking System Mechanism
            string dockingPath = Path.Combine(nasaFolder, "Docking_System.glb");
            if (File.Exists(dockingPath))
            {
                yield return StartCoroutine(LoadGlb(dockingPath, issRig.transform, new Vector3(0f, 0f, 0f), Vector3.one * 1.2f, Quaternion.identity));
            }
        }

        private IEnumerator LoadGlb(string path, Transform parent, Vector3 localPos, Vector3 localScale, Quaternion localRot)
        {
            GameObject container = new GameObject("NASA_GLB_" + Path.GetFileNameWithoutExtension(path));
            container.transform.parent = parent;
            container.transform.localPosition = localPos;
            container.transform.localScale = localScale;
            container.transform.localRotation = localRot;

            GltfImport gltf = new GltfImport();
            string uri = path;
#if UNITY_WEBGL && !UNITY_EDITOR
            uri = Path.Combine("StreamingAssets/NASA_Models", Path.GetFileName(path));
#endif
            var task = gltf.Load(uri);
            while (!task.IsCompleted) yield return null;

            if (task.Result)
            {
                var instTask = gltf.InstantiateMainSceneAsync(container.transform);
                while (!instTask.IsCompleted) yield return null;
                Debug.Log($"[NASAMasterExperience] Official NASA 3D Model Loaded: {Path.GetFileName(path)}");
            }
        }

        private GameObject CreateBox(string name, Transform parent, Vector3 localPos, Vector3 localScale, Material mat)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.parent = parent;
            box.transform.localPosition = localPos;
            box.transform.localScale = localScale;
            if (mat != null) box.GetComponent<Renderer>().sharedMaterial = mat;
            return box;
        }
    }

    /// <summary>
    /// Smooth orbital axial rotation for the Blue Marble Earth.
    /// </summary>
    public class EarthOrbitalRotation : MonoBehaviour
    {
        public float rotationSpeed = 0.85f;

        private void Update()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
        }
    }
}
