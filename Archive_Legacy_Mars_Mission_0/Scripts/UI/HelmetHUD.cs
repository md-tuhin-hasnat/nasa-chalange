using System;
using System.Collections;
using UnityEngine;

namespace AresResurgence.UI
{
    /// <summary>
    /// Diegetic first-person astronaut helmet visor HUD.
    /// Renders real-time telemetry, compass heading, objective distance markers,
    /// biometric readouts, interaction prompts, and radio communication transmissions.
    /// </summary>
    public class HelmetHUD : MonoBehaviour
    {
        public static HelmetHUD Instance { get; private set; }

        [Header("Telemetry Data")]
        public float oxygenLevel = 98.4f;
        public float suitPressureBar = 0.29f; // Standard Apollo/Artemis EVA suit pressure (psi ~ 4.3 -> ~0.29 bar)
        public float heartRateBPM = 74f;
        public float radiationLevel = 0.08f; // mSv/h

        [Header("Objective & Navigation")]
        public string activeObjective = "DIAGNOSE AIRLOCK STATUS & RESTORE LIFE SUPPORT";
        public Transform objectiveTarget;

        [Header("Comms / Dialogue")]
        private string currentSpeaker = "";
        private string currentDialogue = "";
        private float dialogueTimer = 0f;
        private bool isDialogueActive = false;

        [Header("Interaction")]
        private string currentPrompt = "";
        private bool hasInteractable = false;

        private GUIStyle compassStyle;
        private GUIStyle objectiveStyle;
        private GUIStyle telemetryLabelStyle;
        private GUIStyle telemetryValueStyle;
        private GUIStyle promptStyle;
        private GUIStyle radioSpeakerStyle;
        private GUIStyle radioTextStyle;
        private Texture2D boxTexture;
        private Texture2D visorVignetteTexture;

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
            CreateProceduralTextures();
        }

        private void Update()
        {
            if (isDialogueActive)
            {
                dialogueTimer -= Time.deltaTime;
                if (dialogueTimer <= 0f)
                {
                    isDialogueActive = false;
                }
            }
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

        private void CreateProceduralTextures()
        {
            boxTexture = new Texture2D(1, 1);
            boxTexture.SetPixel(0, 0, new Color(0.02f, 0.05f, 0.08f, 0.65f));
            boxTexture.Apply();

            // Create circular vignette for helmet visor rim
            const int vSize = 128;
            visorVignetteTexture = new Texture2D(vSize, vSize);
            for (int y = 0; y < vSize; y++)
            {
                for (int x = 0; x < vSize; x++)
                {
                    float u = (x / (float)vSize) * 2f - 1f;
                    float v = (y / (float)vSize) * 2f - 1f;
                    float dist = Mathf.Sqrt(u * u + v * v);
                    float alpha = Mathf.SmoothStep(0.75f, 1.0f, dist) * 0.7f;
                    visorVignetteTexture.SetPixel(x, y, new Color(0.01f, 0.02f, 0.04f, alpha));
                }
            }
            visorVignetteTexture.Apply();
        }

        private void InitStyles()
        {
            if (compassStyle != null) return;

            compassStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };
            compassStyle.normal.textColor = new Color(0.2f, 0.85f, 1f, 0.9f);

            objectiveStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 13,
                fontStyle = FontStyle.Bold
            };
            objectiveStyle.normal.textColor = new Color(1f, 0.75f, 0.2f, 0.95f);

            telemetryLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 11,
                fontStyle = FontStyle.Normal
            };
            telemetryLabelStyle.normal.textColor = new Color(0.4f, 0.7f, 0.9f, 0.8f);

            telemetryValueStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };
            telemetryValueStyle.normal.textColor = new Color(0.3f, 0.95f, 1f, 1f);

            promptStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15,
                fontStyle = FontStyle.Bold
            };
            promptStyle.normal.textColor = new Color(1f, 0.95f, 0.3f, 1f);

            radioSpeakerStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 13,
                fontStyle = FontStyle.Bold
            };
            radioSpeakerStyle.normal.textColor = new Color(0.95f, 0.45f, 0.2f, 1f);

            radioTextStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 14,
                fontStyle = FontStyle.Normal,
                wordWrap = true
            };
            radioTextStyle.normal.textColor = new Color(0.9f, 0.95f, 1f, 0.95f);
        }

        private void OnGUI()
        {
            InitStyles();

            float sw = Screen.width;
            float sh = Screen.height;

            // 1. Visor subtle curved vignette
            if (visorVignetteTexture != null)
            {
                GUI.DrawTexture(new Rect(0, 0, sw, sh), visorVignetteTexture, ScaleMode.StretchToFill);
            }

            // 2. Top Compass Ribbon & Navigation
            DrawCompassRibbon(sw);

            // 3. Top-Left: Active Mission Banner
            DrawMissionBanner(sw);

            // 4. Bottom-Left: Life Support & Biometrics (O2, Heartrate)
            DrawLifeSupportTelemetry(sh);

            // 5. Bottom-Right: Environmental Telemetry (Pressure, Radiation)
            DrawSuitEnvironmentTelemetry(sw, sh);

            // 6. Center Reticle & Interaction Prompt
            DrawCenterReticle(sw, sh);

            // 7. Radio Transmission / Subtitles
            if (isDialogueActive)
            {
                DrawRadioTransmission(sw, sh);
            }
        }

        private void DrawCompassRibbon(float sw)
        {
            float compassWidth = Mathf.Min(sw * 0.45f, 500f);
            float cx = (sw - compassWidth) * 0.5f;
            float cy = 18f;

            GUI.DrawTexture(new Rect(cx, cy, compassWidth, 32), boxTexture);

            float currentYaw = Camera.main != null ? Camera.main.transform.eulerAngles.y : 0f;
            string headingStr = GetCardinalDirection(currentYaw);

            string distanceText = "";
            if (objectiveTarget != null && Camera.main != null)
            {
                float dist = Vector3.Distance(Camera.main.transform.position, objectiveTarget.position);
                Vector3 toTarget = (objectiveTarget.position - Camera.main.transform.position).normalized;
                float angle = Vector3.SignedAngle(Camera.main.transform.forward, toTarget, Vector3.up);

                string arrow = Mathf.Abs(angle) < 15f ? "▲ TARGET" : (angle > 0 ? "TARGET ▶" : "◀ TARGET");
                distanceText = $" | {arrow} {dist:F0}m";
            }

            GUI.Label(new Rect(cx, cy + 4, compassWidth, 24), $"HDG {currentYaw:000}° [{headingStr}]{distanceText}", compassStyle);
        }

        private void DrawMissionBanner(float sw)
        {
            float bx = 30f;
            float by = 20f;
            float bw = Mathf.Min(sw * 0.35f, 420f);
            float bh = 54f;

            GUI.DrawTexture(new Rect(bx, by, bw, bh), boxTexture);

            GUI.Label(new Rect(bx + 12, by + 6, bw - 24, 18), "MISSION 0: THE JEZERO ANOMALY", telemetryLabelStyle);
            GUI.Label(new Rect(bx + 12, by + 24, bw - 24, 26), activeObjective, objectiveStyle);
        }

        private void DrawLifeSupportTelemetry(float sh)
        {
            float bx = 30f;
            float by = sh - 110f;
            float bw = 170f;
            float bh = 85f;

            GUI.DrawTexture(new Rect(bx, by, bw, bh), boxTexture);

            Color o2Color = oxygenLevel > 50f ? new Color(0.2f, 0.95f, 1f) : (oxygenLevel > 25f ? new Color(1f, 0.7f, 0.2f) : new Color(1f, 0.25f, 0.2f));
            telemetryValueStyle.normal.textColor = o2Color;

            GUI.Label(new Rect(bx + 10, by + 6, bw - 20, 16), "O2 LIFE SUPPORT", telemetryLabelStyle);
            GUI.Label(new Rect(bx + 10, by + 22, bw - 20, 24), $"{oxygenLevel:F1}%", telemetryValueStyle);

            telemetryValueStyle.normal.textColor = new Color(0.3f, 0.95f, 1f);
            GUI.Label(new Rect(bx + 10, by + 46, bw - 20, 16), "HEART RATE", telemetryLabelStyle);
            GUI.Label(new Rect(bx + 10, by + 60, bw - 20, 24), $"{heartRateBPM:F0} BPM", telemetryValueStyle);
        }

        private void DrawSuitEnvironmentTelemetry(float sw, float sh)
        {
            float bw = 170f;
            float bh = 85f;
            float bx = sw - bw - 30f;
            float by = sh - 110f;

            GUI.DrawTexture(new Rect(bx, by, bw, bh), boxTexture);

            GUI.Label(new Rect(bx + 10, by + 6, bw - 20, 16), "SUIT PRESSURE", telemetryLabelStyle);
            GUI.Label(new Rect(bx + 10, by + 22, bw - 20, 24), $"{suitPressureBar:F2} BAR", telemetryValueStyle);

            GUI.Label(new Rect(bx + 10, by + 46, bw - 20, 16), "SURFACE DOSIMETER", telemetryLabelStyle);
            GUI.Label(new Rect(bx + 10, by + 60, bw - 20, 24), $"{radiationLevel:F2} mSv/h", telemetryValueStyle);
        }

        private void DrawCenterReticle(float sw, float sh)
        {
            float cx = sw * 0.5f;
            float cy = sh * 0.5f;

            // Small reticle crosshair
            Color reticleColor = hasInteractable ? new Color(1f, 0.85f, 0.2f, 0.9f) : new Color(0.3f, 0.9f, 1f, 0.45f);
            Texture2D tex = boxTexture;

            GUI.color = reticleColor;
            GUI.DrawTexture(new Rect(cx - 5, cy - 1, 10, 2), tex);
            GUI.DrawTexture(new Rect(cx - 1, cy - 5, 2, 10), tex);
            GUI.color = Color.white;

            if (hasInteractable && !string.IsNullOrEmpty(currentPrompt))
            {
                float pw = 360f;
                float ph = 30f;
                GUI.DrawTexture(new Rect(cx - pw * 0.5f, cy + 20, pw, ph), boxTexture);
                GUI.Label(new Rect(cx - pw * 0.5f, cy + 20, pw, ph), $"[E] {currentPrompt}", promptStyle);
            }
        }

        private void DrawRadioTransmission(float sw, float sh)
        {
            float bw = Mathf.Min(sw * 0.65f, 750f);
            float bh = 75f;
            float bx = (sw - bw) * 0.5f;
            float by = sh - 180f;

            GUI.DrawTexture(new Rect(bx, by, bw, bh), boxTexture);

            GUI.Label(new Rect(bx + 16, by + 8, bw - 32, 18), $"▶ COMMS LINK: {currentSpeaker}", radioSpeakerStyle);
            GUI.Label(new Rect(bx + 16, by + 28, bw - 32, 42), currentDialogue, radioTextStyle);
        }

        private string GetCardinalDirection(float degrees)
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
    }
}
