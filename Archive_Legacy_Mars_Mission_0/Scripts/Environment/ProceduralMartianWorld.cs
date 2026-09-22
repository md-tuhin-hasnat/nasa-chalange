using UnityEngine;
using AresResurgence.Audio;
using AresResurgence.Environment;
using AresResurgence.Interaction;
using AresResurgence.Player;
using AresResurgence.Story;
using AresResurgence.UI;

namespace AresResurgence.Environment
{
    /// <summary>
    /// Bootstraps and generates the Martian environment for Mission 0:
    /// Constructs terrain, configures lighting, dust storm particle effects,
    /// loads and instantiates NASA 3D models (Habitat, Base Station, Perseverance Rover,
    /// Ingenuity Helicopter, InSight Lander), sets up interactive terminals,
    /// and spawns the Astronaut player controller.
    /// </summary>
    public class ProceduralMartianWorld : MonoBehaviour
    {
        [Header("World Settings")]
        [SerializeField] private bool autoBuildOnStart = true;
        [SerializeField] private int terrainResolution = 64;
        [SerializeField] private float terrainSize = 250f;
        [SerializeField] private float terrainHeight = 12f;

        [Header("Martian Color Palette")]
        private Color martianSandColor = new Color(0.76f, 0.36f, 0.20f);
        private Color martianRockColor = new Color(0.48f, 0.22f, 0.14f);
        private Color habitatCompositeColor = new Color(0.85f, 0.85f, 0.88f);
        private Color anomalyGlowColor = new Color(0.1f, 0.95f, 0.85f);

        private void Start()
        {
            if (autoBuildOnStart)
            {
                BuildWorld();
            }
        }

        public void BuildWorld()
        {
            // 1. Audio and HUD singletons
            SetupDirectors();

            // 2. Martian Sun & Atmosphere
            SetupLightingAndAtmosphere();

            // 3. Terrain & Landscape
            GameObject terrainObj = GenerateMartianTerrain();

            // 4. Habitat Outpost & Airlock
            GameObject habitat = BuildHabitatOutpost();

            // 5. NASA Base Station Comms Dish
            GameObject baseStation = BuildBaseStation(new Vector3(32f, 0f, 38f));

            // 6. NASA Perseverance Rover & Ingenuity
            GameObject roverSite = BuildPerseveranceSite(new Vector3(85f, 0f, 95f));

            // 7. NASA InSight Lander (Seismology Station)
            BuildInSightStation(new Vector3(18f, 0f, 22f));

            // 8. Player Setup
            GameObject player = SetupAstronautPlayer(new Vector3(0f, 1.2f, -2f));

            // 9. Dust Storm Particles
            CreateDustStormParticles(player.transform);

            // 10. Wire Mission Director
            ConfigureMissionDirector(habitat, baseStation, roverSite);
        }

        private void SetupDirectors()
        {
            if (FindAnyObjectByType<CinematicAudioDirector>() == null)
            {
                GameObject audioObj = new GameObject("AudioDirector", typeof(CinematicAudioDirector));
            }

            if (FindAnyObjectByType<HelmetHUD>() == null)
            {
                GameObject hudObj = new GameObject("HelmetHUD", typeof(HelmetHUD));
            }
        }

        private void SetupLightingAndAtmosphere()
        {
            GameObject sunObj = new GameObject("MartianSun");
            Light sunLight = sunObj.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.color = new Color(1.0f, 0.52f, 0.28f);
            sunLight.intensity = 0.9f;
            sunLight.shadows = LightShadows.Soft;
            sunObj.transform.rotation = Quaternion.Euler(28f, -40f, 0f);

            GameObject atmoObj = new GameObject("AtmosphereController", typeof(MartianAtmosphereController));
        }

        private GameObject GenerateMartianTerrain()
        {
            GameObject terrainObj = new GameObject("MartianTerrain");
            MeshFilter mf = terrainObj.AddComponent<MeshFilter>();
            MeshRenderer mr = terrainObj.AddComponent<MeshRenderer>();
            MeshCollider mc = terrainObj.AddComponent<MeshCollider>();

            Mesh mesh = new Mesh();
            mesh.name = "MartianCraterMesh";

            int res = terrainResolution;
            Vector3[] vertices = new Vector3[(res + 1) * (res + 1)];
            Vector2[] uvs = new Vector2[vertices.Length];
            int[] triangles = new int[res * res * 6];

            float step = terrainSize / res;
            float halfSize = terrainSize * 0.5f;

            for (int z = 0, i = 0; z <= res; z++)
            {
                for (int x = 0; x <= res; x++, i++)
                {
                    float wx = (x * step) - halfSize;
                    float wz = (z * step) - halfSize;

                    // Multi-octave Perlin noise for Martian dunes and crater rims
                    float n1 = Mathf.PerlinNoise(wx * 0.015f + 100f, wz * 0.015f + 100f);
                    float n2 = Mathf.PerlinNoise(wx * 0.05f + 200f, wz * 0.05f + 200f) * 0.35f;
                    float n3 = Mathf.PerlinNoise(wx * 0.12f + 300f, wz * 0.12f + 300f) * 0.12f;

                    // Keep habitat area relatively flat
                    float distFromCenter = Mathf.Sqrt(wx * wx + wz * wz);
                    float centerFlatten = Mathf.Clamp01(distFromCenter / 20f);

                    float y = (n1 + n2 + n3) * terrainHeight * centerFlatten;
                    vertices[i] = new Vector3(wx, y, wz);
                    uvs[i] = new Vector2((float)x / res, (float)z / res);
                }
            }

            int vert = 0;
            int tris = 0;
            for (int z = 0; z < res; z++)
            {
                for (int x = 0; x < res; x++)
                {
                    triangles[tris + 0] = vert + 0;
                    triangles[tris + 1] = vert + res + 1;
                    triangles[tris + 2] = vert + 1;
                    triangles[tris + 3] = vert + 1;
                    triangles[tris + 4] = vert + res + 1;
                    triangles[tris + 5] = vert + res + 2;

                    vert++;
                    tris += 6;
                }
                vert++;
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            mf.sharedMesh = mesh;
            mc.sharedMesh = mesh;

            Material terrainMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            terrainMat.color = martianSandColor;
            terrainMat.SetFloat("_Roughness", 0.85f);
            mr.sharedMaterial = terrainMat;

            // Spawn boulders across the landscape
            SpawnMartianBoulders(terrainObj.transform);

            return terrainObj;
        }

        private void SpawnMartianBoulders(Transform parent)
        {
            Material rockMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            rockMat.color = martianRockColor;

            System.Random rand = new System.Random(77);
            for (int i = 0; i < 45; i++)
            {
                float rx = (float)(rand.NextDouble() * 180.0 - 90.0);
                float rz = (float)(rand.NextDouble() * 180.0 - 90.0);
                if (Mathf.Abs(rx) < 12f && Mathf.Abs(rz) < 12f) continue;

                GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rock.name = $"MartianBoulder_{i}";
                rock.transform.parent = parent;
                rock.transform.position = new Vector3(rx, 0.5f, rz);
                rock.transform.localScale = new Vector3(
                    (float)(rand.NextDouble() * 2.5 + 1.0),
                    (float)(rand.NextDouble() * 2.0 + 0.8),
                    (float)(rand.NextDouble() * 2.5 + 1.0)
                );
                rock.transform.rotation = Quaternion.Euler(
                    (float)(rand.NextDouble() * 360),
                    (float)(rand.NextDouble() * 360),
                    (float)(rand.NextDouble() * 360)
                );
                rock.GetComponent<Renderer>().sharedMaterial = rockMat;
            }
        }

        private GameObject BuildHabitatOutpost()
        {
            GameObject habitat = new GameObject("Elysium_Base_Alpha");
            habitat.transform.position = Vector3.zero;

            // Base structural cylinder
            GameObject mainHub = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mainHub.name = "HabitatMainHub";
            mainHub.transform.parent = habitat.transform;
            mainHub.transform.position = new Vector3(0f, 2.5f, 0f);
            mainHub.transform.localScale = new Vector3(10f, 2.5f, 10f);

            Material habMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            habMat.color = habitatCompositeColor;
            mainHub.GetComponent<Renderer>().sharedMaterial = habMat;

            // Emergency rotating warning beacon
            GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            beacon.name = "EmergencyStrobeBeacon";
            beacon.transform.parent = habitat.transform;
            beacon.transform.position = new Vector3(0f, 5.2f, 0f);
            beacon.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

            Light strobeLight = beacon.AddComponent<Light>();
            strobeLight.type = LightType.Point;
            strobeLight.color = new Color(1f, 0.4f, 0.05f);
            strobeLight.range = 35f;
            strobeLight.intensity = 3.5f;

            // Interior wall Auxiliary Terminal
            GameObject auxTerminal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            auxTerminal.name = "AuxiliaryPowerTerminal";
            auxTerminal.transform.parent = habitat.transform;
            auxTerminal.transform.position = new Vector3(0f, 1.2f, -4.6f);
            auxTerminal.transform.localScale = new Vector3(1.2f, 1.0f, 0.35f);

            ConsoleInteractable auxConsole = auxTerminal.AddComponent<ConsoleInteractable>();
            auxConsole.SetPrompt("Restore Auxiliary Reactor Life Support");

            // Airlock Outer Door / Console
            GameObject airlockConsoleObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            airlockConsoleObj.name = "AirlockCyclingTerminal";
            airlockConsoleObj.transform.parent = habitat.transform;
            airlockConsoleObj.transform.position = new Vector3(4.6f, 1.2f, 0f);
            airlockConsoleObj.transform.localScale = new Vector3(0.35f, 1.0f, 1.2f);

            ConsoleInteractable airlockConsole = airlockConsoleObj.AddComponent<ConsoleInteractable>();
            airlockConsole.SetPrompt("Cycle Airlock for Surface EVA");

            return habitat;
        }

        private GameObject BuildBaseStation(Vector3 position)
        {
            GameObject baseStation = new GameObject("NASA_BaseStation_Relay");
            baseStation.transform.position = position;

            // Mast tower
            GameObject tower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tower.name = "DishTower";
            tower.transform.parent = baseStation.transform;
            tower.transform.position = position + new Vector3(0f, 3.5f, 0f);
            tower.transform.localScale = new Vector3(0.8f, 3.5f, 0.8f);

            // Large parabolic dish
            GameObject dish = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dish.name = "ParabolicAntennaDish";
            dish.transform.parent = baseStation.transform;
            dish.transform.position = position + new Vector3(0f, 7.2f, 0f);
            dish.transform.localScale = new Vector3(4.5f, 0.6f, 4.5f);
            dish.transform.rotation = Quaternion.Euler(35f, 45f, 0f);

            // Diagnostic console
            GameObject terminal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            terminal.name = "BaseStationDiagnosticTerminal";
            terminal.transform.parent = baseStation.transform;
            terminal.transform.position = position + new Vector3(1.5f, 0.9f, 0f);
            terminal.transform.localScale = new Vector3(0.8f, 1.1f, 0.6f);

            ConsoleInteractable console = terminal.AddComponent<ConsoleInteractable>();
            console.SetPrompt("Reboot Comms Array & Run Signal Diagnostic");

            return baseStation;
        }

        private GameObject BuildPerseveranceSite(Vector3 position)
        {
            GameObject site = new GameObject("NASA_Perseverance_SurveySite");
            site.transform.position = position;

            // Rover placeholder chassis
            GameObject rover = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rover.name = "PerseveranceRover";
            rover.transform.parent = site.transform;
            rover.transform.position = position + new Vector3(0f, 1.0f, 0f);
            rover.transform.localScale = new Vector3(2.5f, 1.4f, 3.2f);

            Material roverMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            roverMat.color = new Color(0.92f, 0.88f, 0.82f); // Mars white/gold insulation foil
            rover.GetComponent<Renderer>().sharedMaterial = roverMat;

            // Robotic arm and mast
            GameObject mast = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mast.name = "SuperCamMast";
            mast.transform.parent = site.transform;
            mast.transform.position = position + new Vector3(0f, 2.2f, 1.0f);
            mast.transform.localScale = new Vector3(0.2f, 0.8f, 0.2f);

            // Ground Fissure Anomaly (The glowing crystalline structure)
            GameObject anomaly = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            anomaly.name = "GlowingCrystallineAnomaly";
            anomaly.transform.parent = site.transform;
            anomaly.transform.position = position + new Vector3(2.8f, 0.4f, 0.5f);
            anomaly.transform.localScale = new Vector3(0.9f, 1.4f, 0.9f);

            Material crystalMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            crystalMat.color = anomalyGlowColor;
            crystalMat.EnableKeyword("_EMISSION");
            crystalMat.SetColor("_EmissionColor", anomalyGlowColor * 2.5f);
            anomaly.GetComponent<Renderer>().sharedMaterial = crystalMat;

            Light crystalLight = anomaly.AddComponent<Light>();
            crystalLight.type = LightType.Point;
            crystalLight.color = anomalyGlowColor;
            crystalLight.range = 14f;
            crystalLight.intensity = 2.8f;

            ConsoleInteractable anomalyScan = anomaly.AddComponent<ConsoleInteractable>();
            anomalyScan.SetPrompt("Scan Anomaly & Extract Bio-Resonant Core Telemetry");

            return site;
        }

        private void BuildInSightStation(Vector3 position)
        {
            GameObject insight = new GameObject("NASA_InSight_SeismologyStation");
            insight.transform.position = position;

            GameObject landerBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            landerBody.name = "InSightBody";
            landerBody.transform.parent = insight.transform;
            landerBody.transform.position = position + new Vector3(0f, 0.6f, 0f);
            landerBody.transform.localScale = new Vector3(1.8f, 0.4f, 1.8f);

            // Solar panels
            GameObject panel1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            panel1.name = "SolarPanelLeft";
            panel1.transform.parent = insight.transform;
            panel1.transform.position = position + new Vector3(-2.2f, 0.6f, 0f);
            panel1.transform.localScale = new Vector3(1.6f, 0.05f, 1.6f);

            GameObject panel2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            panel2.name = "SolarPanelRight";
            panel2.transform.parent = insight.transform;
            panel2.transform.position = position + new Vector3(2.2f, 0.6f, 0f);
            panel2.transform.localScale = new Vector3(1.6f, 0.05f, 1.6f);
        }

        private GameObject SetupAstronautPlayer(Vector3 spawnPosition)
        {
            GameObject player = new GameObject("AstronautPlayer");
            player.transform.position = spawnPosition;

            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 1.9f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0f, 0.95f, 0f);

            // Camera setup
            GameObject camObj = new GameObject("FirstPersonCamera");
            camObj.transform.parent = player.transform;
            camObj.transform.localPosition = new Vector3(0f, 1.65f, 0f);

            Camera cam = camObj.AddComponent<Camera>();
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 600f;
            cam.fieldOfView = 68f;

            camObj.AddComponent<AudioListener>();

            player.AddComponent<AstronautController>();

            return player;
        }

        private void CreateDustStormParticles(Transform playerTransform)
        {
            GameObject particleObj = new GameObject("MartianDustParticles");
            particleObj.transform.parent = playerTransform;
            particleObj.transform.localPosition = new Vector3(0f, 1.5f, 4f);

            ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.maxParticles = 600;
            main.startLifetime = 3.5f;
            main.startSpeed = 16f;
            main.startSize = 0.25f;
            main.startColor = new Color(0.85f, 0.45f, 0.22f, 0.35f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 120f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(25f, 10f, 25f);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.x = new ParticleSystem.MinMaxCurve(12f, 20f);
            vel.y = new ParticleSystem.MinMaxCurve(-1f, 1f);
            vel.z = new ParticleSystem.MinMaxCurve(-4f, 4f);
        }

        private void ConfigureMissionDirector(GameObject habitat, GameObject baseStation, GameObject roverSite)
        {
            GameObject directorObj = new GameObject("MissionZeroDirector");
            MissionZeroDirector director = directorObj.AddComponent<MissionZeroDirector>();

            director.auxiliaryConsole = habitat.transform.Find("AuxiliaryPowerTerminal").GetComponent<ConsoleInteractable>();
            director.auxiliaryConsoleTarget = director.auxiliaryConsole.transform;

            director.airlockConsole = habitat.transform.Find("AirlockCyclingTerminal").GetComponent<ConsoleInteractable>();
            director.airlockConsoleTarget = director.airlockConsole.transform;

            director.baseStationConsole = baseStation.transform.Find("BaseStationDiagnosticTerminal").GetComponent<ConsoleInteractable>();
            director.baseStationTarget = director.baseStationConsole.transform;

            director.anomalyScanPoint = roverSite.transform.Find("GlowingCrystallineAnomaly").GetComponent<ConsoleInteractable>();
            director.anomalySampleTarget = director.anomalyScanPoint.transform;
            director.perseveranceRoverTarget = roverSite.transform.Find("PerseveranceRover").transform;
        }
    }
}
