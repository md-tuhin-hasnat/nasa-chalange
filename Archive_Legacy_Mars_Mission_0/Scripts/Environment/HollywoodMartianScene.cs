using System.Collections;
using System.IO;
using UnityEngine;
using AresResurgence.Audio;
using AresResurgence.Interaction;
using AresResurgence.Player;
using AresResurgence.Story;
using AresResurgence.UI;
using GLTFast;

namespace AresResurgence.Environment
{
    /// <summary>
    /// Hollywood-grade Martian environment and set builder.
    /// Constructs towering crater canyon rims, Elysium Base Alpha with rotating emergency beacons,
    /// high-tech server consoles, airlock depressurization chamber,
    /// loads official NASA 3D models (Perseverance, Ingenuity, InSight, Base Station, Habitat) via glTFast,
    /// and generates dynamic dust storm particles and volumetric lighting.
    /// </summary>
    public class HollywoodMartianScene : MonoBehaviour
    {
        [Header("Cinematic Palette")]
        private readonly Color skyDuskColor = new Color(0.55f, 0.26f, 0.14f); // Cinematic Martian sky
        private readonly Color stormFogColor = new Color(0.70f, 0.38f, 0.20f); // Warm iron-oxide dust storm
        private readonly Color martianSunColor = new Color(1.0f, 0.88f, 0.70f); // Piercing low golden sun
        private readonly Color martianSandColor = new Color(0.82f, 0.42f, 0.22f); // Jezero regolith ochre
        private readonly Color martianBasaltColor = new Color(0.35f, 0.18f, 0.12f); // Volcanic dark rock
        private readonly Color habitatExteriorColor = new Color(0.92f, 0.93f, 0.95f); // NASA thermal insulation
        private readonly Color emergencyAlarmColor = new Color(1.0f, 0.15f, 0.05f); // Red hazard strobe
        private readonly Color anomalyCrystalColor = new Color(0.05f, 0.95f, 0.82f); // Bio-resonant turquoise

        private Transform playerTransform;
        private Camera mainCamera;
        private Material baseLitMaterial;

        private void Start()
        {
            BuildHollywoodMovieScene();
        }

        public void BuildHollywoodMovieScene()
        {
            // 1. Configure Hollywood Atmosphere & Lighting
            ConfigureCinematicAtmosphere();

            // 2. Setup HUD & Audio Directors
            SetupHollywoodDirectors();

            // 3. Generate Colossal Martian Crater Landscape & Canyon Walls
            GameObject terrain = BuildCanyonLandscape();

            // 4. Construct Elysium Base Alpha Outpost (Interior, Exterior, Airlock)
            GameObject habitat = BuildElysiumHabitat();

            // 5. Construct NASA Base Station Comms Dish Relay
            GameObject baseStation = BuildBaseStationRelay(new Vector3(45f, 0f, 50f));

            // 6. Construct NASA Perseverance Rover Site & Anomaly Fissure
            GameObject roverSite = BuildPerseveranceSurveySite(new Vector3(85f, 0f, 95f));

            // 7. Construct NASA InSight Seismology Station
            BuildInSightStation(new Vector3(24f, 0f, 26f));

            // 8. Spawn Astronaut Player outside the habitat with cinematic first view
            GameObject player = SpawnAstronautPlayer(new Vector3(0f, 0.1f, -18f));
            playerTransform = player.transform;

            // 9. Multi-Layered Martian Dust Tempest Particles
            CreateVolumetricDustTempest(player.transform);

            // 10. Load Official NASA GLB 3D Models asynchronously via glTFast
            StartCoroutine(LoadNasaGlbAssets(habitat, baseStation, roverSite));

            // 11. Register Tactical Radar Blips & Connect Mission Director
            ConfigureMissionFlow(habitat, baseStation, roverSite);
        }

        private Material GetBaseMaterial()
        {
            if (baseLitMaterial == null)
            {
                GameObject dummy = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Renderer r = dummy.GetComponent<Renderer>();
                if (r != null && r.sharedMaterial != null)
                {
                    baseLitMaterial = new Material(r.sharedMaterial);
                }
                else
                {
                    Shader s = Shader.Find("Universal Render Pipeline/Lit") 
                               ?? Shader.Find("Standard") 
                               ?? Shader.Find("Diffuse");
                    baseLitMaterial = new Material(s);
                }
                Destroy(dummy);
            }
            return baseLitMaterial;
        }

        private Material CreateMartianMaterial(Color color, bool emissive = false, Color? emissionColor = null)
        {
            Material mat = new Material(GetBaseMaterial());
            mat.color = color;
            if (emissive)
            {
                Color emCol = emissionColor ?? (color * 2.5f);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emCol);
            }
            return mat;
        }

        private void ConfigureCinematicAtmosphere()
        {
            // Global Fog Settings (Martian dust storm)
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = stormFogColor;
            RenderSettings.fogDensity = 0.008f;

            // Ambient lighting so shadows and geometry pop with warm bounce light
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.68f, 0.40f, 0.24f);

            // Directional Sun shining from behind the player onto Elysium Base
            GameObject sunObj = new GameObject("MartianCinematicSun");
            Light sun = sunObj.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = martianSunColor;
            sun.intensity = 1.95f;
            sun.shadows = LightShadows.Soft;
            sunObj.transform.rotation = Quaternion.Euler(28f, 15f, 0f);
        }

        private void SetupHollywoodDirectors()
        {
            if (FindAnyObjectByType<CinematicAudioDirector>() == null)
            {
                new GameObject("CinematicAudioDirector", typeof(CinematicAudioDirector));
            }

            if (FindAnyObjectByType<HollywoodVisorHUD>() == null)
            {
                new GameObject("HollywoodVisorHUD", typeof(HollywoodVisorHUD));
            }
        }

        private GameObject BuildCanyonLandscape()
        {
            GameObject terrainObj = new GameObject("MartianCanyonLandscape");
            MeshFilter mf = terrainObj.AddComponent<MeshFilter>();
            MeshRenderer mr = terrainObj.AddComponent<MeshRenderer>();
            MeshCollider mc = terrainObj.AddComponent<MeshCollider>();

            const int res = 80;
            const float size = 320f;
            const float half = size * 0.5f;
            const float step = size / res;

            Vector3[] verts = new Vector3[(res + 1) * (res + 1)];
            Vector2[] uvs = new Vector2[verts.Length];
            int[] tris = new int[res * res * 6];

            for (int z = 0, i = 0; z <= res; z++)
            {
                for (int x = 0; x <= res; x++, i++)
                {
                    float wx = (x * step) - half;
                    float wz = (z * step) - half;

                    // Distance from crater center
                    float dist = Mathf.Sqrt(wx * wx + wz * wz);

                    // Smooth flat basin within 20m of base, transitioning out to dunes
                    float basinFlatten = Mathf.Clamp01((dist - 18f) / 22f);

                    // Floor noise and dune ripples
                    float floorNoise = Mathf.PerlinNoise(wx * 0.02f + 50f, wz * 0.02f + 50f) * 3.5f;
                    float duneRipples = Mathf.PerlinNoise(wx * 0.08f + 120f, wz * 0.08f + 120f) * 1.2f;

                    // Towering rim walls on crater borders (Interstellar canyon scale!)
                    float rimFactor = Mathf.Clamp01((dist - 50f) / 85f);
                    float canyonWall = Mathf.Pow(rimFactor, 2.2f) * 32f;
                    float mountainSpire = Mathf.PerlinNoise(wx * 0.035f + 300f, wz * 0.035f + 300f) * canyonWall * 0.5f;

                    float y = ((floorNoise + duneRipples) * (1f - rimFactor * 0.4f) + canyonWall + mountainSpire) * basinFlatten;
                    verts[i] = new Vector3(wx, y, wz);
                    uvs[i] = new Vector2((float)x / res * 12f, (float)z / res * 12f);
                }
            }

            for (int z = 0, v = 0, t = 0; z < res; z++)
            {
                for (int x = 0; x < res; x++, v++, t += 6)
                {
                    tris[t + 0] = v;
                    tris[t + 1] = v + res + 1;
                    tris[t + 2] = v + 1;
                    tris[t + 3] = v + 1;
                    tris[t + 4] = v + res + 1;
                    tris[t + 5] = v + res + 2;
                }
                v++;
            }

            Mesh mesh = new Mesh { name = "MartianLandscapeMesh", vertices = verts, uv = uvs, triangles = tris };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            mf.sharedMesh = mesh;
            mc.sharedMesh = mesh;
            mr.sharedMaterial = CreateMartianMaterial(martianSandColor);

            // Spawn dramatic rock spires & boulders
            SpawnMartianBoulders(terrainObj.transform);

            return terrainObj;
        }

        private void SpawnMartianBoulders(Transform parent)
        {
            Material rockMat = CreateMartianMaterial(martianBasaltColor);

            System.Random rand = new System.Random(101);
            for (int i = 0; i < 60; i++)
            {
                float rx = (float)(rand.NextDouble() * 240.0 - 120.0);
                float rz = (float)(rand.NextDouble() * 240.0 - 120.0);
                if (Mathf.Abs(rx) < 20f && Mathf.Abs(rz) < 20f) continue;

                GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rock.name = $"MartianBasaltCrag_{i}";
                rock.transform.parent = parent;
                rock.transform.position = new Vector3(rx, 1.2f, rz);
                rock.transform.localScale = new Vector3(
                    (float)(rand.NextDouble() * 4.0 + 1.2),
                    (float)(rand.NextDouble() * 6.0 + 1.8),
                    (float)(rand.NextDouble() * 4.0 + 1.2)
                );
                rock.transform.rotation = Quaternion.Euler(
                    (float)(rand.NextDouble() * 360),
                    (float)(rand.NextDouble() * 360),
                    (float)(rand.NextDouble() * 360)
                );
                rock.GetComponent<Renderer>().sharedMaterial = rockMat;
            }
        }

        private GameObject BuildElysiumHabitat()
        {
            GameObject habitat = new GameObject("Elysium_Base_Alpha");
            habitat.transform.position = Vector3.zero;

            // 1. Central Geodesic Living Hub
            GameObject hub = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hub.name = "CentralResearchDome";
            hub.transform.parent = habitat.transform;
            hub.transform.position = new Vector3(0f, 2.5f, 0f);
            hub.transform.localScale = new Vector3(12f, 2.5f, 12f);
            hub.GetComponent<Renderer>().sharedMaterial = CreateMartianMaterial(habitatExteriorColor);

            // 2. Rotating Emergency Hazard Beacon (Casting sweeping red light!)
            GameObject beaconObj = new GameObject("EmergencyBeaconAssembly");
            beaconObj.transform.parent = habitat.transform;
            beaconObj.transform.position = new Vector3(0f, 5.5f, 0f);

            GameObject beaconLamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            beaconLamp.transform.parent = beaconObj.transform;
            beaconLamp.transform.localPosition = Vector3.zero;
            beaconLamp.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            beaconLamp.GetComponent<Renderer>().sharedMaterial = CreateMartianMaterial(emergencyAlarmColor, true, emergencyAlarmColor * 4f);

            Light strobeLight = beaconLamp.AddComponent<Light>();
            strobeLight.type = LightType.Spot;
            strobeLight.spotAngle = 110f;
            strobeLight.color = emergencyAlarmColor;
            strobeLight.range = 60f;
            strobeLight.intensity = 5.0f;
            strobeLight.shadows = LightShadows.Soft;

            beaconObj.AddComponent<RotatingHazardLight>();

            // 3. Exterior Front Airlock Chamber & Console
            GameObject airlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            airlock.name = "AirlockPortal";
            airlock.transform.parent = habitat.transform;
            airlock.transform.position = new Vector3(0f, 1.3f, -6.2f);
            airlock.transform.localScale = new Vector3(2.4f, 2.4f, 1.2f);
            airlock.GetComponent<Renderer>().sharedMaterial = CreateMartianMaterial(new Color(0.2f, 0.22f, 0.26f));

            // Glowing airlock console
            GameObject consoleObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            consoleObj.name = "AirlockControlConsole";
            consoleObj.transform.parent = habitat.transform;
            consoleObj.transform.position = new Vector3(1.6f, 1.2f, -6.4f);
            consoleObj.transform.localScale = new Vector3(0.6f, 1.1f, 0.4f);
            consoleObj.GetComponent<Renderer>().sharedMaterial = CreateMartianMaterial(new Color(0.1f, 0.12f, 0.15f));

            // Screen face
            GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Quad);
            screen.transform.parent = consoleObj.transform;
            screen.transform.localPosition = new Vector3(0f, 0.2f, -0.52f);
            screen.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            screen.transform.localScale = new Vector3(0.8f, 0.5f, 1f);
            screen.GetComponent<Renderer>().sharedMaterial = CreateMartianMaterial(emergencyAlarmColor, true, emergencyAlarmColor * 3f);

            ConsoleInteractable airlockConsole = consoleObj.AddComponent<ConsoleInteractable>();
            airlockConsole.SetPrompt("Depressurize Airlock & Restore Auxiliary Power");

            Light conLight = consoleObj.AddComponent<Light>();
            conLight.type = LightType.Point;
            conLight.color = new Color(0.2f, 0.8f, 1f);
            conLight.range = 8f;
            conLight.intensity = 3.0f;

            // Power conduit cables linking habitat to exterior
            CreatePowerConduits(habitat.transform);

            // Solar Array Wing
            BuildSolarArrayWing(habitat.transform, new Vector3(-10f, 0f, 0f));

            return habitat;
        }

        private void BuildSolarArrayWing(Transform parent, Vector3 localPos)
        {
            GameObject wing = new GameObject("SolarArrayWing");
            wing.transform.parent = parent;
            wing.transform.localPosition = localPos;

            Material panelMat = CreateMartianMaterial(new Color(0.05f, 0.12f, 0.22f), true, new Color(0.1f, 0.2f, 0.4f));

            for (int i = 0; i < 4; i++)
            {
                GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                panel.name = $"SolarPanel_{i}";
                panel.transform.parent = wing.transform;
                panel.transform.localPosition = new Vector3(-2f * i, 1.2f, 0f);
                panel.transform.localScale = new Vector3(1.8f, 0.08f, 3.2f);
                panel.transform.localRotation = Quaternion.Euler(30f, 0f, 0f);
                panel.GetComponent<Renderer>().sharedMaterial = panelMat;
            }
        }

        private void CreatePowerConduits(Transform parent)
        {
            Material cableMat = CreateMartianMaterial(new Color(0.08f, 0.08f, 0.08f));

            for (int i = 0; i < 3; i++)
            {
                GameObject cable = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cable.name = $"HeavyDutyConduit_{i}";
                cable.transform.parent = parent;
                cable.transform.position = new Vector3(6f + i * 1.5f, 0.1f, 10f + i * 2f);
                cable.transform.localScale = new Vector3(0.18f, 14f, 0.18f);
                cable.transform.rotation = Quaternion.Euler(90f, 35f + i * 5f, 0f);
                cable.GetComponent<Renderer>().sharedMaterial = cableMat;
            }
        }

        private GameObject BuildBaseStationRelay(Vector3 position)
        {
            GameObject relay = new GameObject("BaseStation_Relay");
            relay.transform.position = position;

            // High-Gain Satellite Dish Support Pylon
            GameObject pylon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pylon.name = "CommsTowerPylon";
            pylon.transform.parent = relay.transform;
            pylon.transform.localPosition = new Vector3(0f, 6.0f, 0f);
            pylon.transform.localScale = new Vector3(1.0f, 6.0f, 1.0f);
            pylon.GetComponent<Renderer>().sharedMaterial = CreateMartianMaterial(new Color(0.7f, 0.72f, 0.76f));

            // Parabolic High-Gain Dish
            GameObject dish = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            dish.name = "DeepSpaceHighGainDish";
            dish.transform.parent = relay.transform;
            dish.transform.localPosition = new Vector3(0f, 12.0f, 0f);
            dish.transform.localScale = new Vector3(6.0f, 0.25f, 6.0f);
            dish.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
            dish.GetComponent<Renderer>().sharedMaterial = CreateMartianMaterial(habitatExteriorColor);

            // Feed horn emitter
            GameObject feedHorn = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            feedHorn.name = "FeedHornEmitter";
            feedHorn.transform.parent = dish.transform;
            feedHorn.transform.localPosition = new Vector3(0f, 2.5f, 0f);
            feedHorn.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            feedHorn.GetComponent<Renderer>().sharedMaterial = CreateMartianMaterial(Color.yellow, true, Color.yellow * 2.5f);

            // Uplink Terminal console
            GameObject terminal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            terminal.name = "RelayFrequencyConsole";
            terminal.transform.parent = relay.transform;
            terminal.transform.localPosition = new Vector3(0f, 1.0f, -2.5f);
            terminal.transform.localScale = new Vector3(1.4f, 1.2f, 0.5f);
            terminal.GetComponent<Renderer>().sharedMaterial = CreateMartianMaterial(new Color(0.12f, 0.14f, 0.18f));

            ConsoleInteractable console = terminal.AddComponent<ConsoleInteractable>();
            console.SetPrompt("Realign High-Gain Satellite Uplink with Earth Orbital Relay");

            return relay;
        }

        private GameObject BuildPerseveranceSurveySite(Vector3 position)
        {
            GameObject site = new GameObject("Perseverance_Survey_Site");
            site.transform.position = position;

            // Fissure Trench Depression
            GameObject trench = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trench.name = "AnomalyTrench";
            trench.transform.parent = site.transform;
            trench.transform.localPosition = new Vector3(0f, -0.4f, 0f);
            trench.transform.localScale = new Vector3(24f, 1.2f, 12f);
            trench.GetComponent<Renderer>().sharedMaterial = CreateMartianMaterial(new Color(0.18f, 0.08f, 0.04f));

            // Bio-Resonant Alien Crystalline Spires (Hollywood Mystery / Contact element!)
            Material crystalMat = CreateMartianMaterial(anomalyCrystalColor, true, anomalyCrystalColor * 4.0f);

            for (int i = 0; i < 7; i++)
            {
                GameObject spire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                spire.name = $"BioCrystallineSpire_{i}";
                spire.transform.parent = site.transform;
                float angle = i * (Mathf.PI * 2f / 7f);
                float rad = 3.5f + (i % 3) * 0.8f;
                spire.transform.localPosition = new Vector3(Mathf.Cos(angle) * rad, 1.5f + (i % 2) * 1.2f, Mathf.Sin(angle) * rad);
                spire.transform.localScale = new Vector3(0.5f, 2.5f + (i % 3), 0.5f);
                spire.transform.rotation = Quaternion.Euler((i * 15) - 30, (i * 45), (i * 20) - 10);
                spire.GetComponent<Renderer>().sharedMaterial = crystalMat;
            }

            // Central Glowing Anomaly Core Light
            GameObject coreLightObj = new GameObject("AnomalyCoreGlow");
            coreLightObj.transform.parent = site.transform;
            coreLightObj.transform.localPosition = new Vector3(0f, 1.5f, 0f);

            Light coreLight = coreLightObj.AddComponent<Light>();
            coreLight.type = LightType.Point;
            coreLight.color = anomalyCrystalColor;
            coreLight.range = 35f;
            coreLight.intensity = 6.0f;

            site.AddComponent<PulseLight>().targetLight = coreLight;

            // Anomaly Scanner Terminal
            GameObject scanPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
            scanPoint.name = "PerseveranceScannerConsole";
            scanPoint.transform.parent = site.transform;
            scanPoint.transform.localPosition = new Vector3(0f, 0.8f, -6f);
            scanPoint.transform.localScale = new Vector3(1.2f, 1.0f, 0.5f);
            scanPoint.GetComponent<Renderer>().sharedMaterial = CreateMartianMaterial(new Color(0.12f, 0.14f, 0.18f));

            ConsoleInteractable console = scanPoint.AddComponent<ConsoleInteractable>();
            console.SetPrompt("Scan Bio-Resonant Subsurface Harmonic Resonance");

            return site;
        }

        private void BuildInSightStation(Vector3 position)
        {
            GameObject station = new GameObject("InSight_Seismology_Station");
            station.transform.position = position;

            GameObject pod = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pod.name = "SeismicSensorDome";
            pod.transform.parent = station.transform;
            pod.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            pod.transform.localScale = new Vector3(2.5f, 0.4f, 2.5f);
            pod.GetComponent<Renderer>().sharedMaterial = CreateMartianMaterial(new Color(0.9f, 0.85f, 0.7f));
        }

        private GameObject SpawnAstronautPlayer(Vector3 spawnPos)
        {
            GameObject player = new GameObject("AstronautPlayer");
            player.transform.position = spawnPos;

            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 1.9f;
            cc.radius = 0.45f;
            cc.center = new Vector3(0f, 0.95f, 0f);

            // Re-use or attach MainCamera to preserve UniversalAdditionalCameraData & URP pipeline
            Camera cam = Camera.main;
            if (cam == null)
            {
                Camera[] cams = FindObjectsByType<Camera>(FindObjectsSortMode.None);
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
            cam.backgroundColor = skyDuskColor;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 800f;
            cam.fieldOfView = 75f;

            cam.transform.parent = player.transform;
            cam.transform.localPosition = new Vector3(0f, 1.65f, 0f);
            cam.transform.localRotation = Quaternion.identity;

            if (cam.GetComponent<AudioListener>() == null)
            {
                cam.gameObject.AddComponent<AudioListener>();
            }

            mainCamera = cam;
            player.AddComponent<AstronautController>();

            return player;
        }

        private void CreateVolumetricDustTempest(Transform target)
        {
            GameObject particleObj = new GameObject("MartianDustTempestParticles");
            particleObj.transform.parent = target;
            particleObj.transform.localPosition = new Vector3(0f, 1.8f, 8f);

            ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.maxParticles = 1500;
            main.startLifetime = 3.5f;
            main.startSpeed = 24f;
            main.startSize = 0.45f;
            main.startColor = new Color(0.85f, 0.42f, 0.22f, 0.55f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 300f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(45f, 16f, 45f);

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.x = new ParticleSystem.MinMaxCurve(-18f, -24f);
            velocity.y = new ParticleSystem.MinMaxCurve(-0.8f, 0.5f);
            velocity.z = new ParticleSystem.MinMaxCurve(8f, 16f);
        }

        private IEnumerator LoadNasaGlbAssets(GameObject habitat, GameObject baseStation, GameObject roverSite)
        {
            string streamingAssets = Application.streamingAssetsPath;
            string nasaFolder = Path.Combine(streamingAssets, "NASA_Models");

            // 1. Habitat Demonstration Unit
            string habPath = Path.Combine(nasaFolder, "Habitat_Demonstration_Unit.glb");
            if (File.Exists(habPath))
            {
                yield return StartCoroutine(LoadGlbFile(habPath, habitat.transform, new Vector3(-8f, 0f, 14f), new Vector3(1.2f, 1.2f, 1.2f), Quaternion.Euler(0f, 60f, 0f)));
            }

            // 2. Base Station Comms Model
            string baseStationPath = Path.Combine(nasaFolder, "Base_Station.glb");
            if (File.Exists(baseStationPath))
            {
                yield return StartCoroutine(LoadGlbFile(baseStationPath, baseStation.transform, new Vector3(0f, 0f, 0f), Vector3.one * 1.5f, Quaternion.identity));
            }

            // 3. Perseverance Rover Model
            string roverPath = Path.Combine(nasaFolder, "Perseverance_Rover.glb");
            if (File.Exists(roverPath))
            {
                yield return StartCoroutine(LoadGlbFile(roverPath, roverSite.transform, new Vector3(-4f, 0f, -2f), Vector3.one * 1.0f, Quaternion.Euler(0f, 145f, 0f)));
            }

            // 4. Ingenuity Helicopter Model
            string heliPath = Path.Combine(nasaFolder, "Ingenuity_Helicopter.glb");
            if (File.Exists(heliPath))
            {
                yield return StartCoroutine(LoadGlbFile(heliPath, roverSite.transform, new Vector3(5f, 0.2f, 4f), Vector3.one * 0.8f, Quaternion.Euler(0f, -40f, 0f)));
            }

            // 5. InSight Lander Model
            string insightPath = Path.Combine(nasaFolder, "InSight_Lander.glb");
            if (File.Exists(insightPath))
            {
                GameObject insightParent = GameObject.Find("InSight_Seismology_Station");
                if (insightParent != null)
                {
                    yield return StartCoroutine(LoadGlbFile(insightPath, insightParent.transform, Vector3.zero, Vector3.one * 1.1f, Quaternion.Euler(0f, 20f, 0f)));
                }
            }
        }

        private IEnumerator LoadGlbFile(string path, Transform parent, Vector3 localPos, Vector3 localScale, Quaternion localRot)
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
                Debug.Log($"[HollywoodMartianScene] NASA GLB loaded successfully: {Path.GetFileName(path)}");
            }
            else
            {
                Debug.LogWarning($"[HollywoodMartianScene] Could not load GLB {path}. Procedural cinematic fallback geometry active.");
            }
        }

        private void ConfigureMissionFlow(GameObject habitat, GameObject baseStation, GameObject roverSite)
        {
            HollywoodVisorHUD hud = HollywoodVisorHUD.Instance;
            if (hud != null)
            {
                hud.radarBlips.Clear();
                hud.radarBlips.Add(new HollywoodVisorHUD.RadarBlip
                {
                    label = "ELYSIUM BASE",
                    worldPos = habitat.transform.position,
                    color = Color.white
                });

                hud.radarBlips.Add(new HollywoodVisorHUD.RadarBlip
                {
                    label = "COMMS RELAY",
                    worldPos = baseStation.transform.position,
                    color = Color.yellow
                });

                hud.radarBlips.Add(new HollywoodVisorHUD.RadarBlip
                {
                    label = "PERSEVERANCE / ANOMALY",
                    worldPos = roverSite.transform.position,
                    color = anomalyCrystalColor
                });

                hud.activeObjective = "APPROACH AIRLOCK & RESTORE OUTPOST AUXILIARY POWER";
                hud.objectiveTarget = habitat.transform;
            }

            MissionZeroDirector director = FindAnyObjectByType<MissionZeroDirector>();
            if (director == null)
            {
                GameObject dirObj = new GameObject("MissionZeroDirector");
                director = dirObj.AddComponent<MissionZeroDirector>();
            }

            ConsoleInteractable airlockConsole = habitat.GetComponentInChildren<ConsoleInteractable>();
            ConsoleInteractable commsConsole = baseStation.GetComponentInChildren<ConsoleInteractable>();
            ConsoleInteractable roverConsole = roverSite.GetComponentInChildren<ConsoleInteractable>();

            director.airlockConsole = airlockConsole;
            director.baseStationConsole = commsConsole;
            director.anomalyScanPoint = roverConsole;
        }
    }

    /// <summary>
    /// Animates rotating red hazard strobe light for dramatic emergency warning.
    /// </summary>
    public class RotatingHazardLight : MonoBehaviour
    {
        public float rotationSpeed = 240f;

        private void Update()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
        }
    }

    /// <summary>
    /// Pulses mystical biological crystal resonance emission light.
    /// </summary>
    public class PulseLight : MonoBehaviour
    {
        public Light targetLight;
        public float baseIntensity = 3.5f;
        public float pulseAmplitude = 2.5f;
        public float frequency = 2.2f;

        private void Update()
        {
            if (targetLight != null)
            {
                targetLight.intensity = baseIntensity + Mathf.Sin(Time.time * frequency) * pulseAmplitude;
            }
        }
    }
}
