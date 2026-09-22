using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace AresResurgence.Audio
{
    /// <summary>
    /// Hollywood cinematic audio director featuring authentic open-source NASA audio recordings
    /// (Perseverance rover Martian wind, Martian dust storm tempest, MOXIE life-support reactor),
    /// Gustav Holst's iconic orchestral space score ("Mars, the Bringer of War"),
    /// and real-time procedural biometric Foley (heartbeat, footsteps, airlock hiss, radio comms).
    /// </summary>
    public class CinematicAudioDirector : MonoBehaviour
    {
        public static CinematicAudioDirector Instance { get; private set; }

        [Header("Audio Sources")]
        private AudioSource ambientSource;
        private AudioSource stormSource;
        private AudioSource musicSource;
        private AudioSource suitSource;
        private AudioSource heartbeatSource;
        private AudioSource radioSource;
        private AudioSource sfxSource;

        [Header("Runtime State")]
        private float targetWindVolume = 0.45f;
        private float heartRateBPM = 72f;
        private float heartbeatTimer = 0f;

        private AudioClip windClip;
        private AudioClip heartbeatClip;
        private AudioClip radioBeepClip;
        private AudioClip airlockHissClip;
        private AudioClip terminalClickClip;
        private AudioClip anomalyResonanceClip;
        private AudioClip footstepClip;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
            GenerateProceduralAudioClips();
        }

        private void Start()
        {
            // Start procedural wind immediately as fallback
            if (windClip != null)
            {
                ambientSource.clip = windClip;
                ambientSource.Play();
            }

#if !UNITY_WEBGL || UNITY_EDITOR
            // Stream authentic NASA Martian audio & Hollywood score from StreamingAssets on desktop
            StartCoroutine(LoadNasaAudioAssets());
#endif
        }

        private void InitializeAudioSources()
        {
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.loop = true;
            ambientSource.spatialBlend = 0f;
            ambientSource.volume = 0.42f;

            stormSource = gameObject.AddComponent<AudioSource>();
            stormSource.loop = true;
            stormSource.spatialBlend = 0f;
            stormSource.volume = 0.25f;

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
            musicSource.volume = 0.28f;

            suitSource = gameObject.AddComponent<AudioSource>();
            suitSource.loop = true;
            suitSource.spatialBlend = 0f;
            suitSource.volume = 0.2f;

            heartbeatSource = gameObject.AddComponent<AudioSource>();
            heartbeatSource.loop = false;
            heartbeatSource.spatialBlend = 0f;
            heartbeatSource.volume = 0.35f;

            radioSource = gameObject.AddComponent<AudioSource>();
            radioSource.loop = false;
            radioSource.spatialBlend = 0f;
            radioSource.volume = 0.7f;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f;
            sfxSource.volume = 0.55f;
        }

        private IEnumerator LoadNasaAudioAssets()
        {
            string audioDir = Path.Combine(Application.streamingAssetsPath, "Audio");

            // 1. Authentic NASA Perseverance Martian Wind recording
            string windUrl = Path.Combine(audioDir, "Mars_Atmosphere_Wind.ogg");
            yield return StartCoroutine(LoadClipFromUrl(windUrl, clip =>
            {
                if (clip != null)
                {
                    ambientSource.clip = clip;
                    ambientSource.Play();
                    Debug.Log("[CinematicAudioDirector] Authentic NASA Perseverance Martian Wind audio loaded.");
                }
            }));

            // 2. Authentic NASA Perseverance Martian Dust Storm
            string stormUrl = Path.Combine(audioDir, "Mars_Dust_Storm.ogg");
            yield return StartCoroutine(LoadClipFromUrl(stormUrl, clip =>
            {
                if (clip != null)
                {
                    stormSource.clip = clip;
                    stormSource.Play();
                    Debug.Log("[CinematicAudioDirector] Authentic NASA Martian Dust Storm audio loaded.");
                }
            }));

            // 3. Cinematic Space Orchestral Theme (Gustav Holst - Mars, the Bringer of War)
            string musicUrl = Path.Combine(audioDir, "Cinematic_Mars_Theme.ogg");
            yield return StartCoroutine(LoadClipFromUrl(musicUrl, clip =>
            {
                if (clip != null)
                {
                    musicSource.clip = clip;
                    musicSource.Play();
                    Debug.Log("[CinematicAudioDirector] Hollywood Mars Symphonic Score loaded.");
                }
            }));
        }

        private IEnumerator LoadClipFromUrl(string url, Action<AudioClip> onLoaded)
        {
            using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.OGGVORBIS))
            {
                yield return req.SendWebRequest();
                if (req.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(req);
                    onLoaded?.Invoke(clip);
                }
                else
                {
                    onLoaded?.Invoke(null);
                }
            }
        }

        private void Update()
        {
            // Heartbeat pacing
            float interval = 60f / Mathf.Clamp(heartRateBPM, 40f, 180f);
            heartbeatTimer += Time.deltaTime;
            if (heartbeatTimer >= interval)
            {
                heartbeatTimer = 0f;
                PlayHeartbeat();
            }

            // Smooth ambient wind volume
            ambientSource.volume = Mathf.Lerp(ambientSource.volume, targetWindVolume, Time.deltaTime * 0.5f);
        }

        public void SetHeartRate(float bpm)
        {
            heartRateBPM = bpm;
            heartbeatSource.volume = Mathf.Lerp(0.15f, 0.45f, (bpm - 70f) / 90f);
        }

        public void SetStormIntensity(float intensity)
        {
            targetWindVolume = Mathf.Clamp(intensity * 0.7f, 0.15f, 0.85f);
            stormSource.volume = Mathf.Clamp(intensity * 0.5f, 0.05f, 0.65f);
        }

        public void PlayRadioComm(bool incoming)
        {
            if (radioBeepClip != null)
            {
                radioSource.pitch = incoming ? 1.0f : 0.85f;
                radioSource.PlayOneShot(radioBeepClip);
            }
        }

        public void PlayAirlockCycle()
        {
            if (airlockHissClip != null)
            {
                sfxSource.PlayOneShot(airlockHissClip);
            }
        }

        public void PlayTerminalInteract()
        {
            if (terminalClickClip != null)
            {
                sfxSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
                sfxSource.PlayOneShot(terminalClickClip);
            }
        }

        public void PlayFootstep()
        {
            if (footstepClip != null)
            {
                sfxSource.pitch = UnityEngine.Random.Range(0.85f, 1.15f);
                sfxSource.PlayOneShot(footstepClip, 0.35f);
            }
        }

        public void PlayAnomalyDiscovery()
        {
            if (anomalyResonanceClip != null)
            {
                radioSource.PlayOneShot(anomalyResonanceClip, 0.8f);
            }
        }

        private void PlayHeartbeat()
        {
            if (heartbeatClip != null && heartRateBPM > 75f)
            {
                heartbeatSource.PlayOneShot(heartbeatClip);
            }
        }

        #region Procedural Audio Synthesis
        private void GenerateProceduralAudioClips()
        {
            const int sampleRate = 44100;

            // 1. Martian Wind (filtered pink/brown noise with low harmonic rumble)
            int windLength = sampleRate * 4;
            float[] windData = new float[windLength];
            float b0 = 0, b1 = 0, b2 = 0;
            System.Random rand = new System.Random(42);
            for (int i = 0; i < windLength; i++)
            {
                float white = (float)(rand.NextDouble() * 2.0 - 1.0);
                b0 = 0.99f * b0 + white * 0.05f;
                b1 = 0.95f * b1 + white * 0.1f;
                b2 = 0.85f * b2 + white * 0.2f;
                float brown = (b0 + b1 + b2) * 0.33f;
                float lfo = Mathf.Sin((float)i / sampleRate * Mathf.PI * 0.4f) * 0.3f + 0.7f;
                windData[i] = brown * lfo * 0.4f;
            }
            windClip = AudioClip.Create("MartianWind", windLength, 1, sampleRate, false);
            windClip.SetData(windData, 0);

            // 2. Heartbeat (lub-dub thump)
            int hbLength = (int)(sampleRate * 0.45f);
            float[] hbData = new float[hbLength];
            for (int i = 0; i < hbLength; i++)
            {
                float t = (float)i / sampleRate;
                float env1 = Mathf.Exp(-t * 28f) * Mathf.Sin(2f * Mathf.PI * 55f * t);
                float env2 = 0f;
                if (t > 0.15f)
                {
                    float t2 = t - 0.15f;
                    env2 = Mathf.Exp(-t2 * 32f) * Mathf.Sin(2f * Mathf.PI * 48f * t2) * 0.75f;
                }
                hbData[i] = (env1 + env2) * 0.85f;
            }
            heartbeatClip = AudioClip.Create("HeartbeatThump", hbLength, 1, sampleRate, false);
            heartbeatClip.SetData(hbData, 0);

            // 3. Radio Roger Beep
            int beepLength = (int)(sampleRate * 0.12f);
            float[] beepData = new float[beepLength];
            for (int i = 0; i < beepLength; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Sin(t / 0.12f * Mathf.PI);
                beepData[i] = Mathf.Sin(2f * Mathf.PI * 1850f * t) * env * 0.4f;
            }
            radioBeepClip = AudioClip.Create("RadioBeep", beepLength, 1, sampleRate, false);
            radioBeepClip.SetData(beepData, 0);

            // 4. Airlock Pneumatic Hiss
            int hissLength = (int)(sampleRate * 1.6f);
            float[] hissData = new float[hissLength];
            for (int i = 0; i < hissLength; i++)
            {
                float t = (float)i / sampleRate;
                float white = (float)(rand.NextDouble() * 2.0 - 1.0);
                float env = Mathf.Sin(t / 1.6f * Mathf.PI);
                hissData[i] = white * env * 0.35f;
            }
            airlockHissClip = AudioClip.Create("AirlockHiss", hissLength, 1, sampleRate, false);
            airlockHissClip.SetData(hissData, 0);

            // 5. High-Tech Terminal Click
            int clickLength = (int)(sampleRate * 0.08f);
            float[] clickData = new float[clickLength];
            for (int i = 0; i < clickLength; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 80f);
                clickData[i] = Mathf.Sin(2f * Mathf.PI * 2400f * t) * env * 0.5f;
            }
            terminalClickClip = AudioClip.Create("TerminalClick", clickLength, 1, sampleRate, false);
            terminalClickClip.SetData(clickData, 0);

            // 6. Subsurface Anomaly Bio-Resonance
            int resLength = (int)(sampleRate * 3.5f);
            float[] resData = new float[resLength];
            for (int i = 0; i < resLength; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Sin(t / 3.5f * Mathf.PI);
                float f1 = Mathf.Sin(2f * Mathf.PI * 144f * t);
                float f2 = Mathf.Sin(2f * Mathf.PI * 288f * t);
                float f3 = Mathf.Sin(2f * Mathf.PI * 432f * t);
                resData[i] = (f1 + f2 * 0.5f + f3 * 0.25f) * env * 0.6f;
            }
            anomalyResonanceClip = AudioClip.Create("AnomalyResonance", resLength, 1, sampleRate, false);
            anomalyResonanceClip.SetData(resData, 0);

            // 7. Martian Regolith Footstep
            int stepLength = (int)(sampleRate * 0.22f);
            float[] stepData = new float[stepLength];
            for (int i = 0; i < stepLength; i++)
            {
                float t = (float)i / sampleRate;
                float white = (float)(rand.NextDouble() * 2.0 - 1.0);
                float env = Mathf.Exp(-t * 22f);
                stepData[i] = white * env * 0.25f;
            }
            footstepClip = AudioClip.Create("MartianFootstep", stepLength, 1, sampleRate, false);
            footstepClip.SetData(stepData, 0);
        }
        #endregion
    }
}
