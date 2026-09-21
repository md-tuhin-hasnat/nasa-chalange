using UnityEngine;
using AresResurgence.Audio;

namespace AresResurgence.Environment
{
    /// <summary>
    /// Hollywood-grade Martian environmental lighting, dust storm density,
    /// and outpost emergency beacon system.
    /// </summary>
    public class MartianAtmosphereController : MonoBehaviour
    {
        public static MartianAtmosphereController Instance { get; private set; }

        [Header("Sun & Sky")]
        [SerializeField] private Light martianSun;
        [SerializeField] private Color stormSunColor = new Color(1.0f, 0.48f, 0.22f); // Deep Martian rust amber
        [SerializeField] private float stormSunIntensity = 0.85f;

        [Header("Atmospheric Dust Fog")]
        [SerializeField] private bool enableStormFog = true;
        [SerializeField] private Color stormFogColor = new Color(0.72f, 0.32f, 0.16f); // Dusty red/orange
        [SerializeField] private float baseFogDensity = 0.035f;
        [SerializeField] private float stormGustMultiplier = 1.6f;

        [Header("Wind & Storm Simulation")]
        [SerializeField] private float stormIntensity = 0.75f;
        [SerializeField] private float windSpeed = 22f; // m/s
        private float noiseTimer = 0f;

        [Header("Emergency Beacons")]
        [SerializeField] private Transform[] emergencyBeacons;
        [SerializeField] private float beaconRotationSpeed = 160f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            ApplyAtmosphericSettings();
        }

        private void Update()
        {
            // Rotate emergency hazard beacons
            if (emergencyBeacons != null)
            {
                foreach (var beacon in emergencyBeacons)
                {
                    if (beacon != null)
                    {
                        beacon.Rotate(Vector3.up, beaconRotationSpeed * Time.deltaTime, Space.Self);
                    }
                }
            }

            // Simulate wind gusts and volumetric fog pulsation
            noiseTimer += Time.deltaTime * 0.4f;
            float gust = Mathf.PerlinNoise(noiseTimer, 0.5f);
            float dynamicDensity = baseFogDensity * Mathf.Lerp(1.0f, stormGustMultiplier, gust * stormIntensity);

            RenderSettings.fogDensity = dynamicDensity;

            if (CinematicAudioDirector.Instance != null)
            {
                CinematicAudioDirector.Instance.SetStormIntensity(stormIntensity * (0.8f + gust * 0.4f));
            }
        }

        public void SetStormIntensity(float intensity)
        {
            stormIntensity = Mathf.Clamp01(intensity);
        }

        private void ApplyAtmosphericSettings()
        {
            if (enableStormFog)
            {
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.ExponentialSquared;
                RenderSettings.fogColor = stormFogColor;
                RenderSettings.fogDensity = baseFogDensity;
            }

            if (martianSun != null)
            {
                martianSun.color = stormSunColor;
                martianSun.intensity = stormSunIntensity;
                martianSun.shadows = LightShadows.Soft;
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.35f, 0.16f, 0.10f); // Martian ambient bounce
        }
    }
}
