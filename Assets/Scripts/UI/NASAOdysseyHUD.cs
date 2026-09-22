using UnityEngine;
using UnityEngine.InputSystem;
using AresResurgence.Story;
using AresResurgence.Player;

namespace AresResurgence.UI
{
    /// <summary>
    /// Tactical Hollywood NASA Astronaut Visor and Mission Control HUD.
    /// Adapts across all 4 narrative phases:
    /// Phase 1: Zero-G Microgravity Physics Training HUD
    /// Phase 2: Cape Canaveral Rocket Launch & Ascent Telemetry
    /// Phase 3: Manual ISS Docking Camera HUD (Reticle, Range, Closure Rate)
    /// Phase 4: Cupola Earth Observation Panoramic HUD
    /// </summary>
    public class NASAOdysseyHUD : MonoBehaviour
    {
        public static NASAOdysseyHUD Instance { get; private set; }

        public NASAOdysseyDirector storyDirector;
        public ZeroGAstronautController playerController;

        // Visual Styles & Textures
        private Texture2D hudBgTex;
        private Texture2D reticleTex;
        private Texture2D barTex;
        private Texture2D scanlineTex;

        private GUIStyle headerStyle;
        private GUIStyle telemetryLabelStyle;
        private GUIStyle telemetryValueStyle;
        private GUIStyle dialogStyle;
        private GUIStyle instructionStyle;

        private void Awake()
        {
            Instance = this;
            GenerateTextures();
        }

        private void GenerateTextures()
        {
            // Dark translucent HUD panel
            hudBgTex = new Texture2D(1, 1);
            hudBgTex.SetPixel(0, 0, new Color(0.02f, 0.06f, 0.12f, 0.78f));
            hudBgTex.Apply();

            // Solid bar texture
            barTex = new Texture2D(1, 1);
            barTex.SetPixel(0, 0, Color.white);
            barTex.Apply();

            // Scanline texture
            scanlineTex = new Texture2D(1, 2);
            scanlineTex.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.22f));
            scanlineTex.SetPixel(0, 1, new Color(1f, 1f, 1f, 0.03f));
            scanlineTex.Apply();
        }

        private void InitStyles()
        {
            if (headerStyle != null) return;

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            headerStyle.normal.textColor = new Color(0.25f, 0.85f, 1.0f);

            telemetryLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Normal
            };
            telemetryLabelStyle.normal.textColor = new Color(0.6f, 0.8f, 0.95f, 0.85f);

            telemetryValueStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };
            telemetryValueStyle.normal.textColor = Color.white;

            dialogStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 15,
                alignment = TextAnchor.UpperLeft,
                padding = new RectOffset(16, 16, 14, 14),
                wordWrap = true
            };
            dialogStyle.normal.background = hudBgTex;
            dialogStyle.normal.textColor = new Color(0.95f, 0.98f, 1.0f);

            instructionStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(10, 10, 8, 8)
            };
            instructionStyle.normal.background = hudBgTex;
            instructionStyle.normal.textColor = new Color(1f, 0.9f, 0.3f);
        }

        private void OnGUI()
        {
            InitStyles();

            if (storyDirector == null)
            {
                storyDirector = FindAnyObjectByType<NASAOdysseyDirector>();
                if (storyDirector == null) return;
            }

            // Global Scanlines
            GUI.color = new Color(1f, 1f, 1f, 0.35f);
            GUI.DrawTextureWithTexCoords(new Rect(0, 0, Screen.width, Screen.height), scanlineTex, new Rect(0, 0, 1, Screen.height / 2f));
            GUI.color = Color.white;

            switch (storyDirector.CurrentPhase)
            {
                case NASAOdysseyDirector.StoryPhase.ZeroGTraining:
                    DrawZeroGTrainingHUD();
                    break;
                case NASAOdysseyDirector.StoryPhase.RocketLaunch:
                    DrawRocketLaunchHUD();
                    break;
                case NASAOdysseyDirector.StoryPhase.ISSDocking:
                    DrawISSDockingHUD();
                    break;
                case NASAOdysseyDirector.StoryPhase.CupolaEarthView:
                    DrawCupolaEarthViewHUD();
                    break;
            }

            DrawCapcomRadioBox();
        }

        private void DrawZeroGTrainingHUD()
        {
            // Top Banner: NASA Candidate Training (Placed safely below HTML top bar)
            Rect topBar = new Rect(Screen.width * 0.25f, 58, Screen.width * 0.5f, 36);
            GUI.DrawTexture(topBar, hudBgTex);
            GUI.Label(topBar, "★ NASA ASTRONAUT SELECTION: ZERO-G PHYSICS SIMULATION ★", headerStyle);

            // Left Panel: Newton's Laws Telemetry
            Rect leftPanel = new Rect(24, 108, 290, 230);
            GUI.DrawTexture(leftPanel, hudBgTex);
            GUILayout.BeginArea(new Rect(leftPanel.x + 12, leftPanel.y + 10, leftPanel.width - 24, leftPanel.height - 20));

            GUILayout.Label("PHYSICS OF MICROGRAVITY", headerStyle);
            GUILayout.Space(6);

            float speed = playerController != null ? playerController.speedMetersPerSec : 0f;
            GUILayout.Label($"Velocity (Inertia): {speed:F2} m/s", telemetryValueStyle);
            GUILayout.Label(playerController != null && playerController.isBraking ? "DAMPING: ACTIVE [BRAKING]" : "DAMPING: ZERO [DRIFTING]", telemetryLabelStyle);

            GUILayout.Space(8);
            GUILayout.Label("NEWTON'S 3RD LAW (F = -F):", telemetryLabelStyle);
            if (playerController != null && playerController.isThrusterFiring)
            {
                GUI.color = new Color(0.2f, 1f, 0.4f);
                GUILayout.Label($"RCS Counter-Reaction: {playerController.thrustForce:F1} N", telemetryValueStyle);
            }
            else
            {
                GUI.color = new Color(0.7f, 0.7f, 0.7f);
                GUILayout.Label("RCS Thrusters Idle", telemetryLabelStyle);
            }
            GUI.color = Color.white;

            GUILayout.Space(8);
            float rcs = playerController != null ? playerController.rcsPropellant : 100f;
            GUILayout.Label($"Suit RCS Nitrogen: {rcs:F0}%", telemetryLabelStyle);
            DrawProgressBar(rcs / 100f, new Color(0.2f, 0.8f, 1f));

            GUILayout.EndArea();

            // Right Panel: Navigation Waypoint Progress
            Rect rightPanel = new Rect(Screen.width - 314, 108, 290, 190);
            GUI.DrawTexture(rightPanel, hudBgTex);
            GUILayout.BeginArea(new Rect(rightPanel.x + 12, rightPanel.y + 10, rightPanel.width - 24, rightPanel.height - 20));

            GUILayout.Label("TRAINING PROTOCOL", headerStyle);
            GUILayout.Space(6);

            int currentRing = storyDirector.CurrentRingIndex + 1;
            int totalRings = storyDirector.TotalRings;
            GUILayout.Label($"Navigation Rings: {Mathf.Min(currentRing, totalRings)} / {totalRings}", telemetryValueStyle);
            DrawProgressBar((float)storyDirector.RingsCompleted / totalRings, new Color(1f, 0.8f, 0.2f));

            GUILayout.Space(8);
            GUILayout.Label("CONTROLS:", telemetryLabelStyle);
            GUILayout.Label("W/S: Forward / Back Thruster\nA/D: Lateral Strafe\nSpace/Shift: Up / Down\nQ/E: Roll | X: Counter-Brake", telemetryLabelStyle);

            GUILayout.EndArea();

            // Center Visor Reticle
            DrawVisorReticle();
        }

        private void DrawRocketLaunchHUD()
        {
            // Top Launch Header
            Rect topBar = new Rect(Screen.width * 0.25f, 58, Screen.width * 0.5f, 36);
            GUI.DrawTexture(topBar, hudBgTex);
            GUI.Label(topBar, "🚀 CAPE CANAVERAL SLC-39A: LAUNCH TO ORBIT 🚀", headerStyle);

            // Left Launch Telemetry
            Rect panel = new Rect(24, 108, 300, 210);
            GUI.DrawTexture(panel, hudBgTex);
            GUILayout.BeginArea(new Rect(panel.x + 14, panel.y + 12, panel.width - 28, panel.height - 24));

            GUILayout.Label("FLIGHT TELEMETRY", headerStyle);
            GUILayout.Space(6);

            float t = storyDirector.LaunchTimeElapsed;
            GUILayout.Label($"MET (Elapsed): T+{t:F1}s", telemetryValueStyle);
            GUILayout.Label($"Altitude: {storyDirector.LaunchAltitudeKm:F1} km", telemetryValueStyle);
            GUILayout.Label($"Speed: Mach {storyDirector.LaunchMachSpeed:F2}", telemetryValueStyle);
            GUILayout.Label(storyDirector.LaunchStageText, telemetryLabelStyle);

            GUILayout.Space(10);
            DrawProgressBar(Mathf.Clamp01(t / 14f), new Color(1f, 0.45f, 0.1f));

            GUILayout.EndArea();
        }

        private void DrawISSDockingHUD()
        {
            // Top Docking Header
            Rect topBar = new Rect(Screen.width * 0.25f, 58, Screen.width * 0.5f, 36);
            GUI.DrawTexture(topBar, hudBgTex);
            GUI.Label(topBar, "🛰️ MANUAL SPACECRAFT DOCKING APPROACH: ISS PMA-2 🛰️", headerStyle);

            // Center Docking Crosshair / Reticle
            float cx = Screen.width * 0.5f;
            float cy = Screen.height * 0.5f;
            DrawDockingTargetReticle(cx, cy);

            // Left Telemetry Box
            Rect leftBox = new Rect(24, 108, 320, 250);
            GUI.DrawTexture(leftBox, hudBgTex);
            GUILayout.BeginArea(new Rect(leftBox.x + 14, leftBox.y + 12, leftBox.width - 28, leftBox.height - 24));

            GUILayout.Label("DOCKING TELEMETRY", headerStyle);
            GUILayout.Space(6);

            float dist = storyDirector.DockingDistanceMeters;
            float rate = storyDirector.DockingClosureRate;
            Color rateColor = rate < 0.35f ? new Color(0.2f, 1f, 0.3f) : (rate < 0.6f ? new Color(1f, 0.8f, 0.2f) : new Color(1f, 0.2f, 0.2f));

            GUILayout.Label($"Range to Port: {dist:F2} m", telemetryValueStyle);
            DrawProgressBar(Mathf.Clamp01(1f - (dist / 20f)), new Color(0.2f, 0.9f, 1f));

            GUILayout.Space(8);
            GUI.color = rateColor;
            GUILayout.Label($"Closure Rate: {rate:F2} m/s [LIMIT < 0.35]", telemetryValueStyle);
            GUI.color = Color.white;

            GUILayout.Space(6);
            GUILayout.Label($"Alignment Offset: {storyDirector.DockingAlignmentError:F1}°", telemetryLabelStyle);
            GUILayout.Label(storyDirector.DockingStatusText, telemetryLabelStyle);

            GUILayout.Space(8);
            GUILayout.Label("W/S: Fwd/Rev | A/D: Strafe\nSpace/Shift: Elevate | Q/E: Roll", telemetryLabelStyle);

            GUILayout.EndArea();
        }

        private void DrawCupolaEarthViewHUD()
        {
            // Top Banner
            Rect topBar = new Rect(Screen.width * 0.25f, 58, Screen.width * 0.5f, 36);
            GUI.DrawTexture(topBar, hudBgTex);
            GUI.Label(topBar, "🌍 ISS CUPOLA OBSERVATION MODULE: LOW EARTH ORBIT 🌍", headerStyle);

            // Right Info
            Rect infoBox = new Rect(Screen.width - 334, 108, 310, 160);
            GUI.DrawTexture(infoBox, hudBgTex);
            GUILayout.BeginArea(new Rect(infoBox.x + 14, infoBox.y + 12, infoBox.width - 28, infoBox.height - 24));

            GUILayout.Label("EXPEDITION OBSERVATION", headerStyle);
            GUILayout.Space(4);
            GUILayout.Label("Orbital Altitude: 408 km", telemetryLabelStyle);
            GUILayout.Label("Orbital Velocity: 27,600 km/h (7.66 km/s)", telemetryLabelStyle);
            GUILayout.Label("View: Home Planet Earth (Blue Marble)", telemetryValueStyle);
            GUILayout.Label("Status: MISSION 0 ACCOMPLISHED ★", new GUIStyle(telemetryValueStyle) { normal = { textColor = new Color(0.3f, 1f, 0.5f) } });

            GUILayout.EndArea();

            // Hint to look around
            Rect hint = new Rect(Screen.width * 0.35f, Screen.height - 68, Screen.width * 0.3f, 30);
            GUI.Box(hint, "Move Mouse to look around the Cupola observation bay", instructionStyle);
        }

        private void DrawCapcomRadioBox()
        {
            if (string.IsNullOrEmpty(storyDirector.CurrentCapcomMessage)) return;

            float boxWidth = Mathf.Min(780f, Screen.width * 0.85f);
            float boxHeight = 65f;
            // Placed at Screen.height - 135 to stay safely above the bottom HTML bar (height 45px)
            Rect radioRect = new Rect((Screen.width - boxWidth) * 0.5f, Screen.height - boxHeight - 65f, boxWidth, boxHeight);

            GUI.Box(radioRect, $"[A.T.L.A.S. // TACTICAL AI & CAPCOM]\n\"{storyDirector.CurrentCapcomMessage}\"", dialogStyle);
        }

        private void DrawVisorReticle()
        {
            float cx = Screen.width * 0.5f;
            float cy = Screen.height * 0.5f;
            float size = 18f;

            GUI.color = new Color(0.3f, 0.85f, 1.0f, 0.6f);
            GUI.DrawTexture(new Rect(cx - size, cy - 1, size * 2, 2), barTex);
            GUI.DrawTexture(new Rect(cx - 1, cy - size, 2, size * 2), barTex);
            GUI.color = Color.white;
        }

        private void DrawDockingTargetReticle(float cx, float cy)
        {
            float reticleSize = 50f;

            GUI.color = new Color(0.2f, 0.9f, 0.4f, 0.8f);
            // Outer brackets
            GUI.DrawTexture(new Rect(cx - reticleSize, cy - reticleSize, 14, 2), barTex);
            GUI.DrawTexture(new Rect(cx - reticleSize, cy - reticleSize, 2, 14), barTex);

            GUI.DrawTexture(new Rect(cx + reticleSize - 14, cy - reticleSize, 14, 2), barTex);
            GUI.DrawTexture(new Rect(cx + reticleSize - 2, cy - reticleSize, 2, 14), barTex);

            GUI.DrawTexture(new Rect(cx - reticleSize, cy + reticleSize - 2, 14, 2), barTex);
            GUI.DrawTexture(new Rect(cx - reticleSize, cy + reticleSize - 14, 2, 14), barTex);

            GUI.DrawTexture(new Rect(cx + reticleSize - 14, cy + reticleSize - 2, 14, 2), barTex);
            GUI.DrawTexture(new Rect(cx + reticleSize - 2, cy + reticleSize - 14, 2, 14), barTex);

            // Center pip
            GUI.DrawTexture(new Rect(cx - 3, cy - 3, 6, 6), barTex);
            GUI.color = Color.white;
        }

        private void DrawProgressBar(float fraction, Color col)
        {
            Rect barBg = GUILayoutUtility.GetRect(200, 10);
            GUI.color = new Color(0.1f, 0.15f, 0.2f, 0.8f);
            GUI.DrawTexture(barBg, barTex);

            Rect barFill = new Rect(barBg.x, barBg.y, barBg.width * Mathf.Clamp01(fraction), barBg.height);
            GUI.color = col;
            GUI.DrawTexture(barFill, barTex);
            GUI.color = Color.white;
        }
    }
}
