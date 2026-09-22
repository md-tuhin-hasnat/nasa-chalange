using System;
using System.Collections.Generic;
using UnityEngine;

namespace AresResurgence.UI
{
    /// <summary>
    /// Hollywood-grade tactical astronaut visor HUD.
    /// Features procedural vector graphics, rotating tactical radar minimap,
    /// real-time animated ECG pulse wave, segmented life-support gauges,
    /// biometric telemetry, radio waveform visualizer, and cinematic movie title card.
    /// </summary>
    public class HollywoodVisorHUD : MonoBehaviour
    {
        public static HollywoodVisorHUD Instance { get; private set; }

        [Header("Biometrics Telemetry")]
        public float oxygenLevel = 98.4f;
        public float suitPressureBar = 0.29f;
        public float heartRateBPM = 74f;
        public float radiationLevel = 0.08f;
        public float externalTempCelsius = -62.4f;
        public float surfacePressureHpa = 6.1f;

        [Header("Navigation & Radar")]
        public string activeObjective = "DIAGNOSE AIRLOCK STATUS & RESTORE LIFE SUPPORT";
        public Transform objectiveTarget;
        public List<RadarBlip> radarBlips = new List<RadarBlip>();

        [Header("Cinematic Title Card")]
        [SerializeField] private bool showTitleCard = true;
        private float titleCardAlpha = 1.0f;
        private float titleCardTimer = 0f;

        [Header("Comms & Radio Transmission")]
        private string currentSpeaker = "";
        private string currentDialogue = "";
        private float dialogueTimer = 0f;
        private bool isDialogueActive = false;
        private float[] audioWaveformBars = new float[16];

        [Header("Interaction Reticle")]
        private string currentPrompt = "";
        private bool hasInteractable = false;
        private float reticlePulse = 0f;

        [System.Serializable]
        public struct RadarBlip
        {
            public string label;
            public Vector3 worldPos;
            public Color color;
        }

        // Procedural Textures & Styles
        private Texture2D panelTex;
        private Texture2D borderTex;
        private Texture2D solidWhiteTex;
        private Texture2D visorVignetteTex;
        private Texture2D scanlineTex;
        private Texture2D radarRingTex;

        private GUIStyle titleMainStyle;
        private GUIStyle titleSubStyle;
        private GUIStyle compassStyle;
        private GUIStyle objectiveHeaderStyle;
        private GUIStyle objectiveBodyStyle;
        private GUIStyle telemetryLabelStyle;
        private GUIStyle telemetryValueStyle;
        private GUIStyle radioSpeakerStyle;
        private GUIStyle radioTextStyle;
        private GUIStyle promptStyle;
        private GUIStyle radarLabelStyle;

        // ECG simulation
        private List<float> ecgPoints = new List<float>();
        private float ecgPhase = 0f;
        private const int MaxEcgPoints = 60;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            for (int i = 0; i < MaxEcgPoints; i++) ecgPoints.Add(0.5f);
        }

        private void Start()
        {
            GenerateProceduralTextures();
        }

        private void Update()
        {
            // Dialogue timer
            if (isDialogueActive)
            {
                dialogueTimer -= Time.deltaTime;
                if (dialogueTimer <= 0f) isDialogueActive = false;

                // Animate audio waveform bars
                for (int i = 0; i < audioWaveformBars.Length; i++)
                {
                    audioWaveformBars[i] = Mathf.PerlinNoise(Time.time * 18f, i * 0.4f);
                }
            }

            // Cinematic Title Card sequence
            if (showTitleCard)
            {
                titleCardTimer += Time.deltaTime;
                if (titleCardTimer > 5.5f)
                {
                    titleCardAlpha = Mathf.Max(0f, titleCardAlpha - Time.deltaTime * 0.8f);
                    if (titleCardAlpha <= 0f) showTitleCard = false;
                }
            }

            // Reticle pulse
            reticlePulse = Mathf.Sin(Time.time * 6f) * 0.5f + 0.5f;

            // ECG Heart-rate wave simulation
            UpdateEcgGraph();
        }

        public void SetObjective(string objective, Transform target = null)
        {
            activeObjective = objective;
            objectiveTarget = target;
        }

        public void SetInteractionPrompt(string prompt, bool canInteract)
        {
            currentPrompt = prompt;
            hasInteractable = canInteract;
        }

        public void ShowTransmission(string speaker, string message, float duration = 5f)
        {
            currentSpeaker = speaker;
            currentDialogue = message;
            dialogueTimer = duration;
            isDialogueActive = true;

            if (AresResurgence.Audio.CinematicAudioDirector.Instance != null)
            {
                AresResurgence.Audio.CinematicAudioDirector.Instance.PlayRadioComm(true);
            }
        }

        public void AddRadarBlip(string label, Vector3 pos, Color color)
        {
            radarBlips.Add(new RadarBlip { label = label, worldPos = pos, color = color });
        }

        private void UpdateEcgGraph()
        {
            float hz = heartRateBPM / 60f;
            ecgPhase += Time.deltaTime * hz * 4f;
            if (ecgPhase > 1f) ecgPhase -= 1f;

            float sample = 0.5f;
            if (ecgPhase > 0.35f && ecgPhase < 0.40f) sample = 0.65f; // P wave
            else if (ecgPhase >= 0.40f && ecgPhase < 0.43f) sample = 0.35f; // Q dip
            else if (ecgPhase >= 0.43f && ecgPhase < 0.47f) sample = 0.95f; // R spike
            else if (ecgPhase >= 0.47f && ecgPhase < 0.50f) sample = 0.20f; // S dip
            else if (ecgPhase >= 0.55f && ecgPhase < 0.65f) sample = 0.62f; // T wave

            ecgPoints.Add(sample);
            if (ecgPoints.Count > MaxEcgPoints) ecgPoints.RemoveAt(0);
        }

        #region Procedural Graphics
        private void GenerateProceduralTextures()
        {
            solidWhiteTex = new Texture2D(1, 1);
            solidWhiteTex.SetPixel(0, 0, Color.white);
            solidWhiteTex.Apply();

            // Glass panel background with subtle dark-cyan tint
            panelTex = new Texture2D(1, 1);
            panelTex.SetPixel(0, 0, new Color(0.01f, 0.04f, 0.07f, 0.78f));
            panelTex.Apply();

            // Border texture
            borderTex = new Texture2D(1, 1);
            borderTex.SetPixel(0, 0, new Color(0.15f, 0.75f, 0.95f, 0.85f));
            borderTex.Apply();

            // Visor vignette texture
            const int vSize = 128;
            visorVignetteTex = new Texture2D(vSize, vSize);
            for (int y = 0; y < vSize; y++)
            {
                for (int x = 0; x < vSize; x++)
                {
                    float u = (x / (float)vSize) * 2f - 1f;
                    float v = (y / (float)vSize) * 2f - 1f;
                    float dist = Mathf.Sqrt(u * u + v * v);
                    float alpha = Mathf.SmoothStep(0.70f, 1.05f, dist) * 0.88f;
                    visorVignetteTex.SetPixel(x, y, new Color(0.02f, 0.03f, 0.05f, alpha));
                }
            }
            visorVignetteTex.Apply();

            // Radar circular background
            const int rSize = 128;
            radarRingTex = new Texture2D(rSize, rSize);
            for (int y = 0; y < rSize; y++)
            {
                for (int x = 0; x < rSize; x++)
                {
                    float u = (x / (float)rSize) * 2f - 1f;
                    float v = (y / (float)rSize) * 2f - 1f;
                    float d = Mathf.Sqrt(u * u + v * v);
                    if (d > 1.0f)
                    {
                        radarRingTex.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        bool isRing = Mathf.Abs(d - 0.98f) < 0.03f || Mathf.Abs(d - 0.65f) < 0.02f || Mathf.Abs(d - 0.33f) < 0.02f;
                        bool isCross = (Mathf.Abs(u) < 0.015f || Mathf.Abs(v) < 0.015f);
                        if (isRing || isCross)
                        {
                            radarRingTex.SetPixel(x, y, new Color(0.15f, 0.8f, 1f, 0.6f));
                        }
                        else
                        {
                            radarRingTex.SetPixel(x, y, new Color(0.01f, 0.04f, 0.08f, 0.7f));
                        }
                    }
                }
            }
            radarRingTex.Apply();
        }

        private void InitStyles()
        {
            if (titleMainStyle != null) return;

            titleMainStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 28,
                fontStyle = FontStyle.Bold
            };
            titleMainStyle.normal.textColor = new Color(1f, 0.92f, 0.80f, 1f);

            titleSubStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Normal
            };
            titleSubStyle.normal.textColor = new Color(0.95f, 0.55f, 0.20f, 0.95f);

            compassStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13,
                fontStyle = FontStyle.Bold
            };
            compassStyle.normal.textColor = new Color(0.3f, 0.92f, 1f, 1f);

            objectiveHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 11,
                fontStyle = FontStyle.Bold
            };
            objectiveHeaderStyle.normal.textColor = new Color(0.95f, 0.6f, 0.15f, 1f);

            objectiveBodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };
            objectiveBodyStyle.normal.textColor = Color.white;

            telemetryLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 10,
                fontStyle = FontStyle.Normal
            };
            telemetryLabelStyle.normal.textColor = new Color(0.45f, 0.75f, 0.95f, 0.85f);

            telemetryValueStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 15,
                fontStyle = FontStyle.Bold
            };
            telemetryValueStyle.normal.textColor = new Color(0.25f, 0.95f, 1f, 1f);

            radioSpeakerStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };
            radioSpeakerStyle.normal.textColor = new Color(1.0f, 0.45f, 0.15f, 1f);

            radioTextStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 13,
                fontStyle = FontStyle.Normal,
                wordWrap = true
            };
            radioTextStyle.normal.textColor = new Color(0.92f, 0.96f, 1f, 0.95f);

            promptStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };
            promptStyle.normal.textColor = new Color(1f, 0.92f, 0.35f, 1f);

            radarLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 9,
                fontStyle = FontStyle.Bold
            };
            radarLabelStyle.normal.textColor = Color.cyan;
        }
        #endregion

        private void OnGUI()
        {
            InitStyles();

            float sw = Screen.width;
            float sh = Screen.height;

            // 1. Visor rim curved vignette
            if (visorVignetteTex != null)
            {
                GUI.DrawTexture(new Rect(0, 0, sw, sh), visorVignetteTex, ScaleMode.StretchToFill);
            }

            // 2. Top-Center Compass & Waypoint Indicator
            DrawCompassBar(sw);

            // 3. Top-Right Tactical Radar Minimap
            DrawTacticalRadar(sw);

            // 4. Top-Left Active Objective Feed
            DrawMissionObjective(sw);

            // 5. Bottom-Left Biometrics & Animated ECG Graph
            DrawBiometricsPanel(sh);

            // 6. Bottom-Right Environmental Telemetry
            DrawEnvironmentPanel(sw, sh);

            // 7. Center Tactical Reticle
            DrawTacticalReticle(sw, sh);

            // 8. Radio Comms Transmission Box with Waveform
            if (isDialogueActive)
            {
                DrawRadioCommsBox(sw, sh);
            }

            // 9. Hollywood Cinematic Title Card Overlay
            if (showTitleCard && titleCardAlpha > 0f)
            {
                DrawCinematicTitleCard(sw, sh);
            }
        }

        #region HUD Sections
        private void DrawCompassBar(float sw)
        {
            float bw = Mathf.Min(sw * 0.48f, 520f);
            float bh = 34f;
            float bx = (sw - bw) * 0.5f;
            float by = 18f;

            DrawFuturisticBox(bx, by, bw, bh, new Color(0.15f, 0.8f, 1f, 0.85f));

            float yaw = Camera.main != null ? Camera.main.transform.eulerAngles.y : 0f;
            string cardinal = GetCardinal(yaw);

            string targetInfo = "";
            if (objectiveTarget != null && Camera.main != null)
            {
                float dist = Vector3.Distance(Camera.main.transform.position, objectiveTarget.position);
                Vector3 toTarget = (objectiveTarget.position - Camera.main.transform.position).normalized;
                float angle = Vector3.SignedAngle(Camera.main.transform.forward, toTarget, Vector3.up);
                string arrow = Mathf.Abs(angle) < 14f ? "▲ LOCKED" : (angle > 0 ? "▶ " : "◀ ");
                targetInfo = $" | {arrow} {dist:F0}m";
            }

            GUI.Label(new Rect(bx, by + 5, bw, 24), $"NAV // HDG {yaw:000}° [{cardinal}]{targetInfo}", compassStyle);
        }

        private void DrawTacticalRadar(float sw)
        {
            float size = 120f;
            float rx = sw - size - 28f;
            float ry = 20f;

            DrawFuturisticBox(rx - 6, ry - 6, size + 12, size + 28, new Color(0.2f, 0.8f, 1f, 0.7f));
            GUI.Label(new Rect(rx, ry - 4, size, 14), "TACTICAL RADAR", telemetryLabelStyle);

            if (radarRingTex != null)
            {
                GUI.DrawTexture(new Rect(rx, ry + 12, size, size), radarRingTex);
            }

            // Rotating sweep line
            float sweepAngle = Time.time * 120f % 360f;
            float rad = sweepAngle * Mathf.Deg2Rad;
            float cx = rx + size * 0.5f;
            float cy = ry + 12f + size * 0.5f;
            float lx = cx + Mathf.Cos(rad) * (size * 0.48f);
            float ly = cy + Mathf.Sin(rad) * (size * 0.48f);
            DrawLine(new Vector2(cx, cy), new Vector2(lx, ly), new Color(0.2f, 0.95f, 1f, 0.45f), 1.5f);

            // Center Player blip
            DrawCircle(cx, cy, 3f, Color.white);

            // Draw radar blips
            if (Camera.main != null)
            {
                Vector3 playerPos = Camera.main.transform.position;
                float playerYaw = Camera.main.transform.eulerAngles.y;

                foreach (var blip in radarBlips)
                {
                    Vector3 delta = blip.worldPos - playerPos;
                    float dist = delta.magnitude;
                    if (dist > 150f) continue; // Radar max range 150m

                    float angleToBlip = Mathf.Atan2(delta.x, delta.z) * Mathf.Rad2Deg;
                    float relativeAngle = (angleToBlip - playerYaw) * Mathf.Deg2Rad;

                    float normDist = (dist / 150f) * (size * 0.45f);
                    float bx = cx + Mathf.Sin(relativeAngle) * normDist;
                    float by = cy - Mathf.Cos(relativeAngle) * normDist;

                    DrawCircle(bx, by, 3.5f, blip.color);
                    GUI.Label(new Rect(bx - 20, by - 12, 40, 12), blip.label, radarLabelStyle);
                }
            }
        }

        private void DrawMissionObjective(float sw)
        {
            float bw = Mathf.Min(sw * 0.32f, 380f);
            float bh = 68f;
            float bx = 28f;
            float by = 20f;

            DrawFuturisticBox(bx, by, bw, bh, new Color(1f, 0.65f, 0.15f, 0.85f));

            GUI.Label(new Rect(bx + 12, by + 8, bw - 24, 16), "MISSION 0 // THE JEZERO ANOMALY", objectiveHeaderStyle);
            GUI.Label(new Rect(bx + 12, by + 26, bw - 24, 38), activeObjective, objectiveBodyStyle);
        }

        private void DrawBiometricsPanel(float sh)
        {
            float bw = 210f;
            float bh = 135f;
            float bx = 28f;
            float by = sh - bh - 25f;

            DrawFuturisticBox(bx, by, bw, bh, new Color(0.2f, 0.85f, 1f, 0.75f));

            // Oxygen readout & segmented bar
            GUI.Label(new Rect(bx + 10, by + 8, 120, 14), "LIFE SUPPORT: O2", telemetryLabelStyle);
            Color o2Col = oxygenLevel > 50f ? new Color(0.2f, 0.95f, 1f) : (oxygenLevel > 25f ? new Color(1f, 0.75f, 0.2f) : new Color(1f, 0.2f, 0.2f));
            telemetryValueStyle.normal.textColor = o2Col;
            GUI.Label(new Rect(bx + 135, by + 6, 65, 20), $"{oxygenLevel:F1}%", telemetryValueStyle);

            // Draw segmented O2 bar
            int segments = 10;
            int filled = Mathf.RoundToInt((oxygenLevel / 100f) * segments);
            for (int i = 0; i < segments; i++)
            {
                Color segCol = i < filled ? o2Col : new Color(0.15f, 0.25f, 0.35f, 0.4f);
                GUI.color = segCol;
                GUI.DrawTexture(new Rect(bx + 10 + (i * 19), by + 28, 16, 7), solidWhiteTex);
            }
            GUI.color = Color.white;

            // Heart Rate & Animated ECG Line
            telemetryValueStyle.normal.textColor = new Color(0.3f, 0.95f, 1f);
            GUI.Label(new Rect(bx + 10, by + 46, 120, 14), "BIOMETRICS: PULSE", telemetryLabelStyle);
            GUI.Label(new Rect(bx + 135, by + 42, 65, 20), $"{heartRateBPM:F0} BPM", telemetryValueStyle);

            // Draw ECG wave points
            float graphX = bx + 10;
            float graphY = by + 66;
            float graphW = bw - 20;
            float graphH = 34;
            GUI.color = new Color(0.05f, 0.12f, 0.18f, 0.8f);
            GUI.DrawTexture(new Rect(graphX, graphY, graphW, graphH), solidWhiteTex);

            float step = graphW / (MaxEcgPoints - 1);
            for (int i = 0; i < ecgPoints.Count - 1; i++)
            {
                float p1x = graphX + i * step;
                float p1y = graphY + graphH - (ecgPoints[i] * graphH);
                float p2x = graphX + (i + 1) * step;
                float p2y = graphY + graphH - (ecgPoints[i + 1] * graphH);
                DrawLine(new Vector2(p1x, p1y), new Vector2(p2x, p2y), new Color(0.2f, 0.95f, 0.45f, 0.9f), 1.6f);
            }
            GUI.color = Color.white;

            // Suit Temperature & Pressure
            GUI.Label(new Rect(bx + 10, by + 108, bw - 20, 18), $"SUIT: 21.4°C | 0.29 BAR | SCRUBBER: NOMINAL", telemetryLabelStyle);
        }

        private void DrawEnvironmentPanel(float sw, float sh)
        {
            float bw = 210f;
            float bh = 120f;
            float bx = sw - bw - 28f;
            float by = sh - bh - 25f;

            DrawFuturisticBox(bx, by, bw, bh, new Color(0.95f, 0.5f, 0.15f, 0.75f));

            GUI.Label(new Rect(bx + 10, by + 8, bw - 20, 14), "SURFACE TELEMETRY", telemetryLabelStyle);

            telemetryValueStyle.normal.textColor = new Color(1f, 0.65f, 0.25f);
            GUI.Label(new Rect(bx + 10, by + 26, 110, 14), "EXT TEMP", telemetryLabelStyle);
            GUI.Label(new Rect(bx + 10, by + 40, 110, 20), $"{externalTempCelsius:F1}°C", telemetryValueStyle);

            GUI.Label(new Rect(bx + 115, by + 26, 85, 14), "ATM PRESSURE", telemetryLabelStyle);
            GUI.Label(new Rect(bx + 115, by + 40, 85, 20), $"{surfacePressureHpa:F1} hPa", telemetryValueStyle);

            GUI.Label(new Rect(bx + 10, by + 68, 110, 14), "RADIATION", telemetryLabelStyle);
            GUI.Label(new Rect(bx + 10, by + 82, 110, 20), $"{radiationLevel:F2} mSv/h", telemetryValueStyle);

            GUI.Label(new Rect(bx + 115, by + 68, 85, 14), "WIND VELOCITY", telemetryLabelStyle);
            GUI.Label(new Rect(bx + 115, by + 82, 85, 20), "24.6 m/s", telemetryValueStyle);
        }

        private void DrawTacticalReticle(float sw, float sh)
        {
            float cx = sw * 0.5f;
            float cy = sh * 0.5f;

            float rSize = hasInteractable ? 18f + reticlePulse * 6f : 12f;
            Color retColor = hasInteractable ? new Color(1f, 0.85f, 0.2f, 0.95f) : new Color(0.2f, 0.85f, 1f, 0.55f);

            // Draw targeting brackets
            DrawBracket(cx, cy, rSize, retColor);

            if (hasInteractable && !string.IsNullOrEmpty(currentPrompt))
            {
                float pw = 380f;
                float ph = 32f;
                DrawFuturisticBox(cx - pw * 0.5f, cy + 28, pw, ph, new Color(1f, 0.85f, 0.2f, 0.9f));
                GUI.Label(new Rect(cx - pw * 0.5f, cy + 28, pw, ph), $"[E] {currentPrompt}", promptStyle);
            }
        }

        private void DrawRadioCommsBox(float sw, float sh)
        {
            float bw = Mathf.Min(sw * 0.65f, 740f);
            float bh = 82f;
            float bx = (sw - bw) * 0.5f;
            float by = sh - 185f;

            DrawFuturisticBox(bx, by, bw, bh, new Color(1f, 0.45f, 0.15f, 0.9f));

            GUI.Label(new Rect(bx + 16, by + 8, bw - 32, 18), $"▶ INCOMING TRANSMISSION // {currentSpeaker}", radioSpeakerStyle);
            GUI.Label(new Rect(bx + 16, by + 28, bw - 120, 48), currentDialogue, radioTextStyle);

            // Audio waveform bars on right side of transmission box
            float waveX = bx + bw - 90f;
            float waveY = by + 28f;
            for (int i = 0; i < audioWaveformBars.Length; i++)
            {
                float barH = Mathf.Max(4f, audioWaveformBars[i] * 38f);
                GUI.color = new Color(1.0f, 0.55f, 0.2f, 0.85f);
                GUI.DrawTexture(new Rect(waveX + (i * 5), waveY + 38f - barH, 3, barH), solidWhiteTex);
            }
            GUI.color = Color.white;
        }

        private void DrawCinematicTitleCard(float sw, float sh)
        {
            GUI.color = new Color(0.01f, 0.02f, 0.04f, titleCardAlpha * 0.92f);
            GUI.DrawTexture(new Rect(0, 0, sw, sh), solidWhiteTex);
            GUI.color = Color.white;

            titleMainStyle.normal.textColor = new Color(1f, 0.92f, 0.82f, titleCardAlpha);
            titleSubStyle.normal.textColor = new Color(0.95f, 0.52f, 0.20f, titleCardAlpha);

            float cy = sh * 0.42f;
            GUI.Label(new Rect(0, cy - 30, sw, 30), "NASA ARTEMIS PROGRAM // EXPEDITION VII", titleSubStyle);
            GUI.Label(new Rect(0, cy, sw, 45), "ARES RESURGENCE: MISSION 0", titleMainStyle);
            GUI.Label(new Rect(0, cy + 45, sw, 25), "LOCATION: JEZERO CRATER, MARS — SOL 42 [03:14 UTC]", titleSubStyle);
            GUI.Label(new Rect(0, cy + 72, sw, 20), "INCIDENT: GEOMAGNETIC DUST TEMPEST & REACTOR FAILURE", titleSubStyle);
        }
        #endregion

        #region Helper Drawing Utilities
        private void DrawFuturisticBox(float x, float y, float w, float h, Color accentColor)
        {
            // Dark transparent background
            GUI.color = new Color(0.01f, 0.03f, 0.06f, 0.82f);
            GUI.DrawTexture(new Rect(x, y, w, h), solidWhiteTex);

            // Border
            GUI.color = accentColor;
            float t = 1.2f;
            float cornerLen = 10f;

            // Corner brackets
            GUI.DrawTexture(new Rect(x, y, cornerLen, t), solidWhiteTex); // top-left H
            GUI.DrawTexture(new Rect(x, y, t, cornerLen), solidWhiteTex); // top-left V
            GUI.DrawTexture(new Rect(x + w - cornerLen, y, cornerLen, t), solidWhiteTex); // top-right H
            GUI.DrawTexture(new Rect(x + w - t, y, t, cornerLen), solidWhiteTex); // top-right V
            GUI.DrawTexture(new Rect(x, y + h - t, cornerLen, t), solidWhiteTex); // bot-left H
            GUI.DrawTexture(new Rect(x, y + h - cornerLen, t, cornerLen), solidWhiteTex); // bot-left V
            GUI.DrawTexture(new Rect(x + w - cornerLen, y + h - t, cornerLen, t), solidWhiteTex); // bot-right H
            GUI.DrawTexture(new Rect(x + w - t, y + h - cornerLen, t, cornerLen), solidWhiteTex); // bot-right V

            GUI.color = Color.white;
        }

        private void DrawBracket(float cx, float cy, float r, Color col)
        {
            GUI.color = col;
            float len = r * 0.45f;
            float t = 1.5f;

            // Top-left
            GUI.DrawTexture(new Rect(cx - r, cy - r, len, t), solidWhiteTex);
            GUI.DrawTexture(new Rect(cx - r, cy - r, t, len), solidWhiteTex);
            // Top-right
            GUI.DrawTexture(new Rect(cx + r - len, cy - r, len, t), solidWhiteTex);
            GUI.DrawTexture(new Rect(cx + r - t, cy - r, t, len), solidWhiteTex);
            // Bot-left
            GUI.DrawTexture(new Rect(cx - r, cy + r - t, len, t), solidWhiteTex);
            GUI.DrawTexture(new Rect(cx - r, cy + r - len, t, len), solidWhiteTex);
            // Bot-right
            GUI.DrawTexture(new Rect(cx + r - len, cy + r - t, len, t), solidWhiteTex);
            GUI.DrawTexture(new Rect(cx + r - t, cy + r - len, t, len), solidWhiteTex);

            // Center dot
            GUI.DrawTexture(new Rect(cx - 1, cy - 1, 2, 2), solidWhiteTex);

            GUI.color = Color.white;
        }

        private void DrawLine(Vector2 p1, Vector2 p2, Color col, float width)
        {
            Vector2 delta = p2 - p1;
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            float length = delta.magnitude;

            GUIUtility.RotateAroundPivot(angle, p1);
            GUI.color = col;
            GUI.DrawTexture(new Rect(p1.x, p1.y - width * 0.5f, length, width), solidWhiteTex);
            GUI.color = Color.white;
            GUIUtility.RotateAroundPivot(-angle, p1);
        }

        private void DrawCircle(float cx, float cy, float r, Color col)
        {
            GUI.color = col;
            GUI.DrawTexture(new Rect(cx - r, cy - r, r * 2f, r * 2f), solidWhiteTex);
            GUI.color = Color.white;
        }

        private string GetCardinal(float degrees)
        {
            degrees = (degrees % 360f + 360f) % 360f;
            if (degrees >= 337.5f || degrees < 22.5f) return "N";
            if (degrees >= 22.5f && degrees < 67.5f) return "NE";
            if (degrees >= 67.5f && degrees < 112.5f) return "E";
            if (degrees >= 112.5f && degrees < 157.5f) return "SE";
            if (degrees >= 157.5f && degrees < 202.5f) return "S";
            if (degrees >= 202.5f && degrees < 247.5f) return "SW";
            if (degrees >= 247.5f && degrees < 292.5f) return "W";
            return "NW";
        }
        #endregion
    }
}
