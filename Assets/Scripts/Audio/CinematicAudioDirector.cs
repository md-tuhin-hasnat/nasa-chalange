using System;
using System.Collections.Generic;
using UnityEngine;

namespace AresResurgence.Audio
{
    /// <summary>
    /// Hollywood cinematic audio director featuring procedural soundscape generation.
    /// Synthesizes breathing, heartbeat, radio static, airlock depressurization,
    /// Martian wind gusts, and alien anomaly resonances in real time.
    /// </summary>
    public class CinematicAudioDirector : MonoBehaviour
    {
        public static CinematicAudioDirector Instance { get; private set; }

        [Header("Audio Sources")]
        private AudioSource ambientSource;
        private AudioSource suitSource;
        private AudioSource heartbeatSource;
        private AudioSource radioSource;
        private AudioSource sfxSource;

        [Header("Runtime State")]
        private float targetWindVolume = 0.4f;
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

        private void InitializeAudioSources()
        {
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.loop = true;
            ambientSource.spatialBlend = 0f;
            ambientSource.volume = 0.35f;

            suitSource = gameObject.AddComponent<AudioSource>();
            suitSource.loop = true;
            suitSource.spatialBlend = 0f;
            suitSource.volume = 0.2f;

            heartbeatSource = gameObject.AddComponent<AudioSource>();
            heartbeatSource.loop = false;
            heartbeatSource.spatialBlend = 0f;
            heartbeatSource.volume = 0.25f;

            radioSource = gameObject.AddComponent<AudioSource>();
            radioSource.loop = false;
            radioSource.spatialBlend = 0f;
            radioSource.volume = 0.6f;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f;
            sfxSource.volume = 0.5f;
        }

        private void Start()
        {
            if (windClip != null)
            {
                ambientSource.clip = windClip;
                ambientSource.Play();
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
            targetWindVolume = Mathf.Clamp(intensity * 0.7f, 0.1f, 0.85f);
            ambientSource.pitch = Mathf.Lerp(0.85f, 1.25f, intensity);
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
                sfxSource.PlayOneShot(footstepClip, 0.25f);
            }
        }

        public void PlayAnomalyDiscovery()
        {
            if (anomalyResonanceClip != null)
            {
                radioSource.PlayOneShot(anomalyResonanceClip, 0.7f);
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
                hbData[i] = (env1 + env2) * 0.8f;
            }
            heartbeatClip = AudioClip.Create("HeartbeatThump", hbLength, 1, sampleRate, false);
            heartbeatClip.SetData(hbData, 0);

            // 3. Radio Comms Squelch / Chirp
            int radioLength = (int)(sampleRate * 0.18f);
            float[] radioData = new float[radioLength];
            for (int i = 0; i < radioLength; i++)
            {
                float t = (float)i / sampleRate;
                float freq = Mathf.Lerp(1200f, 2400f, t / 0.18f);
                float env = Mathf.Sin((float)i / radioLength * Mathf.PI);
                radioData[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.4f;
            }
            radioClipFromData(ref radioBeepClip, radioData, radioLength, sampleRate, "RadioChirp");

            // 4. Airlock Decompression Hiss
            int hissLength = (int)(sampleRate * 1.8f);
            float[] hissData = new float[hissLength];
            for (int i = 0; i < hissLength; i++)
            {
                float t = (float)i / sampleRate;
                float noise = (float)(rand.NextDouble() * 2.0 - 1.0);
                float env = Mathf.Exp(-t * 2.2f);
                hissData[i] = noise * env * 0.5f;
            }
            airlockHissClip = AudioClip.Create("AirlockHiss", hissLength, 1, sampleRate, false);
            airlockHissClip.SetData(hissData, 0);

            // 5. Terminal Beep / Click
            int clickLength = (int)(sampleRate * 0.08f);
            float[] clickData = new float[clickLength];
            for (int i = 0; i < clickLength; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 60f);
                clickData[i] = Mathf.Sin(2f * Mathf.PI * 1800f * t) * env * 0.35f;
            }
            terminalClickClip = AudioClip.Create("TerminalClick", clickLength, 1, sampleRate, false);
            terminalClickClip.SetData(clickData, 0);

            // 6. Anomaly Resonance (haunting crystalline chord)
            int anomLength = (int)(sampleRate * 3.5f);
            float[] anomData = new float[anomLength];
            float[] freqs = { 220f, 329.63f, 440f, 587.33f, 880f };
            for (int i = 0; i < anomLength; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Sin((float)i / anomLength * Mathf.PI);
                float sum = 0f;
                for (int f = 0; f < freqs.Length; f++)
                {
                    sum += Mathf.Sin(2f * Mathf.PI * freqs[f] * t) * (1f / (f + 1));
                }
                anomData[i] = sum * env * 0.35f;
            }
            anomalyResonanceClip = AudioClip.Create("AnomalyResonance", anomLength, 1, sampleRate, false);
            anomalyResonanceClip.SetData(anomData, 0);

            // 7. Regolith Footstep
            int stepLength = (int)(sampleRate * 0.12f);
            float[] stepData = new float[stepLength];
            float stepFilter = 0f;
            for (int i = 0; i < stepLength; i++)
            {
                float t = (float)i / sampleRate;
                float white = (float)(rand.NextDouble() * 2.0 - 1.0);
                stepFilter = 0.8f * stepFilter + white * 0.2f;
                float env = Mathf.Exp(-t * 40f);
                stepData[i] = stepFilter * env * 0.4f;
            }
            footstepClip = AudioClip.Create("RegolithStep", stepLength, 1, sampleRate, false);
            footstepClip.SetData(stepData, 0);
        }

        private void radioClipFromData(ref AudioClip clip, float[] data, int length, int rate, string name)
        {
            clip = AudioClip.Create(name, length, 1, rate, false);
            clip.SetData(data, 0);
        }
        #endregion
    }
}
