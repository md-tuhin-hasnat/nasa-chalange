import os, re

target_html = "Build/WebGL/index.html"
template_dir = "Assets/WebGLTemplates/HollywoodNASA"
os.makedirs(template_dir, exist_ok=True)

html_content = """<!DOCTYPE html>
<html lang="en-us">
  <head>
    <meta charset="utf-8">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no">
    <title>NASA ODYSSEY // Junior Astronaut: From Zero-G to the ISS [Hollywood 3D]</title>
    <link rel="shortcut icon" href="TemplateData/favicon.ico">
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link href="https://fonts.googleapis.com/css2?family=Chakra+Petch:ital,wght@0,400;0,600;0,700;1,700&family=Share+Tech+Mono&display=swap" rel="stylesheet">
    <style>
      :root {
        --color-bg: #03060a;
        --color-nasa-blue: #0b3d91;
        --color-cyan-hud: #00f0ff;
        --color-amber-alert: #ffb800;
        --color-red-danger: #ff2a4b;
        --color-terminal-green: #00ffaa;
      }

      * {
        box-sizing: border-box;
        margin: 0;
        padding: 0;
        user-select: none;
      }

      html, body {
        width: 100vw;
        height: 100vh;
        overflow: hidden;
        background-color: var(--color-bg);
        font-family: 'Share Tech Mono', monospace;
        color: #e0e8f0;
      }

      .scanlines {
        position: fixed;
        top: 0;
        left: 0;
        width: 100vw;
        height: 100vh;
        pointer-events: none;
        background: linear-gradient(
          rgba(18, 16, 16, 0) 50%, 
          rgba(0, 0, 0, 0.22) 50%
        ), linear-gradient(
          90deg,
          rgba(0, 150, 255, 0.02),
          rgba(0, 255, 170, 0.01),
          rgba(0, 100, 255, 0.02)
        );
        background-size: 100% 3px, 6px 100%;
        z-index: 99;
        opacity: 0.55;
      }

      .vignette {
        position: fixed;
        top: 0;
        left: 0;
        width: 100vw;
        height: 100vh;
        pointer-events: none;
        box-shadow: inset 0 0 120px rgba(0, 5, 15, 0.85);
        z-index: 98;
      }

      #tactical-console {
        position: absolute;
        top: 0;
        left: 0;
        width: 100vw;
        height: 100vh;
        display: flex;
        flex-direction: column;
        justify-content: space-between;
        pointer-events: none;
        z-index: 50;
      }

      #top-hud-bar {
        background: linear-gradient(180deg, rgba(3, 8, 16, 0.94) 0%, rgba(3, 8, 16, 0.6) 80%, transparent 100%);
        padding: 12px 24px;
        display: flex;
        justify-content: space-between;
        align-items: center;
        border-bottom: 1px solid rgba(0, 240, 255, 0.35);
        box-shadow: 0 4px 20px rgba(0, 240, 255, 0.08);
      }

      .hud-badge {
        display: flex;
        align-items: center;
        gap: 12px;
        font-family: 'Chakra Petch', sans-serif;
      }

      .hud-insignia {
        color: var(--color-cyan-hud);
        font-size: 18px;
        font-weight: 700;
        letter-spacing: 2px;
        text-shadow: 0 0 10px rgba(0, 240, 255, 0.6);
      }

      .status-pill {
        background: rgba(0, 240, 255, 0.15);
        border: 1px solid var(--color-cyan-hud);
        color: var(--color-cyan-hud);
        font-size: 11px;
        padding: 2px 8px;
        border-radius: 2px;
        letter-spacing: 1px;
      }

      .status-alert {
        background: rgba(0, 255, 170, 0.15);
        border: 1px solid var(--color-terminal-green);
        color: var(--color-terminal-green);
        font-size: 11px;
        padding: 2px 8px;
        border-radius: 2px;
        letter-spacing: 1px;
        animation: pulse-alert 2s infinite ease-in-out;
      }

      @keyframes pulse-alert {
        0%, 100% { opacity: 0.8; }
        50% { opacity: 1; box-shadow: 0 0 12px rgba(0, 255, 170, 0.6); }
      }

      .telemetry-ticker {
        display: flex;
        gap: 24px;
        font-size: 12px;
        letter-spacing: 1px;
      }

      .telemetry-item {
        display: flex;
        flex-direction: column;
        align-items: flex-end;
      }

      .telemetry-label {
        font-size: 9px;
        color: #7b9bb5;
        text-transform: uppercase;
      }

      .telemetry-val {
        color: var(--color-cyan-hud);
        font-weight: bold;
      }

      #unity-container {
        position: absolute;
        top: 0;
        left: 0;
        width: 100vw;
        height: 100vh;
        z-index: 10;
        background: #000000;
      }

      #unity-canvas {
        width: 100%;
        height: 100%;
        display: block;
      }

      #bottom-command-bar {
        background: linear-gradient(0deg, rgba(3, 8, 16, 0.95) 0%, rgba(3, 8, 16, 0.7) 80%, transparent 100%);
        padding: 12px 24px;
        display: flex;
        justify-content: space-between;
        align-items: center;
        border-top: 1px solid rgba(0, 240, 255, 0.25);
        pointer-events: auto;
      }

      .control-hints {
        display: flex;
        gap: 16px;
        font-size: 11px;
        color: #8da4bc;
        align-items: center;
      }

      .key-cap {
        background: rgba(255, 255, 255, 0.08);
        border: 1px solid rgba(0, 240, 255, 0.3);
        color: var(--color-cyan-hud);
        padding: 1px 6px;
        border-radius: 3px;
        font-weight: bold;
      }

      .action-buttons {
        display: flex;
        gap: 12px;
      }

      .btn-tactical {
        background: rgba(0, 240, 255, 0.08);
        border: 1px solid rgba(0, 240, 255, 0.4);
        color: var(--color-cyan-hud);
        padding: 5px 16px;
        font-family: 'Chakra Petch', sans-serif;
        font-size: 12px;
        letter-spacing: 1px;
        cursor: pointer;
        transition: all 0.2s ease;
        border-radius: 2px;
      }

      .btn-tactical:hover {
        background: rgba(0, 240, 255, 0.25);
        box-shadow: 0 0 10px rgba(0, 240, 255, 0.4);
        color: #ffffff;
      }

      #loading-overlay {
        position: absolute;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background: radial-gradient(circle at center, #051428 0%, #020408 85%);
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        z-index: 100;
        transition: opacity 0.8s ease;
      }

      .mission-badge-logo {
        width: 90px;
        height: 90px;
        margin-bottom: 24px;
        border: 2px solid rgba(0, 240, 255, 0.6);
        border-radius: 50%;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 38px;
        box-shadow: 0 0 35px rgba(0, 240, 255, 0.4);
        animation: spin-slow 25s infinite linear;
      }

      @keyframes spin-slow {
        from { transform: rotate(0deg); }
        to { transform: rotate(360deg); }
      }

      .loading-title {
        font-family: 'Chakra Petch', sans-serif;
        font-size: 26px;
        letter-spacing: 4px;
        color: #ffffff;
        margin-bottom: 8px;
        text-shadow: 0 0 15px rgba(0, 240, 255, 0.6);
      }

      .loading-subtitle {
        font-size: 13px;
        color: var(--color-cyan-hud);
        letter-spacing: 2px;
        margin-bottom: 30px;
      }

      .loading-progress-container {
        width: 420px;
        height: 6px;
        background: rgba(255, 255, 255, 0.1);
        border-radius: 3px;
        overflow: hidden;
        margin-bottom: 14px;
        border: 1px solid rgba(0, 240, 255, 0.3);
      }

      #loading-progress-bar {
        width: 0%;
        height: 100%;
        background: linear-gradient(90deg, #0b3d91, #00f0ff);
        box-shadow: 0 0 12px #00f0ff;
        transition: width 0.3s ease;
      }

      .loading-status-text {
        font-size: 11px;
        color: #7b9bb5;
        letter-spacing: 1px;
      }

      #briefing-modal {
        position: absolute;
        top: 50%;
        left: 50%;
        transform: translate(-50%, -50%);
        width: 620px;
        max-width: 90vw;
        background: rgba(4, 10, 20, 0.96);
        border: 1px solid var(--color-cyan-hud);
        box-shadow: 0 0 45px rgba(0, 240, 255, 0.3), 0 0 80px rgba(0, 0, 0, 0.95);
        padding: 28px 32px;
        z-index: 120;
        backdrop-filter: blur(10px);
        display: none;
      }

      .modal-header {
        font-family: 'Chakra Petch', sans-serif;
        font-size: 18px;
        letter-spacing: 2px;
        color: var(--color-cyan-hud);
        border-bottom: 1px solid rgba(0, 240, 255, 0.3);
        padding-bottom: 12px;
        margin-bottom: 16px;
        display: flex;
        justify-content: space-between;
        align-items: center;
      }

      .modal-body {
        font-size: 13px;
        line-height: 1.7;
        color: #c4d7e8;
        margin-bottom: 24px;
      }

      .modal-body strong {
        color: var(--color-amber-alert);
      }

      .modal-actions {
        display: flex;
        justify-content: flex-end;
        gap: 12px;
      }
    </style>
  </head>
  <body>
    <div class="scanlines"></div>
    <div class="vignette"></div>

    <!-- NASA AUDIO ENGINE -->
    <audio id="audio-launch-comms" preload="auto">
      <source src="StreamingAssets/Audio/NASA_Apollo_Launch_Comms.ogg" type="audio/ogg">
    </audio>
    <audio id="audio-cupola-symphony" loop preload="auto">
      <source src="StreamingAssets/Audio/ISS_Cupola_Earth_Symphony.ogg" type="audio/ogg">
    </audio>

    <div id="tactical-console">
      <!-- TOP HUD TELEMETRY BAR -->
      <div id="top-hud-bar">
        <div class="hud-badge">
          <span class="hud-insignia">NASA // EXPEDITION ODYSSEY</span>
          <span class="status-pill">JUNIOR ASTRONAUT</span>
          <span class="status-alert">SIMULATION ACTIVE</span>
        </div>
        <div class="telemetry-ticker">
          <div class="telemetry-item">
            <span class="telemetry-label">STATION ORBIT</span>
            <span class="telemetry-val">408 KM LEO</span>
          </div>
          <div class="telemetry-item">
            <span class="telemetry-label">INCLINATION</span>
            <span class="telemetry-val">51.6°</span>
          </div>
          <div class="telemetry-item">
            <span class="telemetry-label">VELOCITY</span>
            <span class="telemetry-val">7.66 KM/S</span>
          </div>
        </div>
      </div>

      <!-- BOTTOM COMMAND & CONTROL BAR -->
      <div id="bottom-command-bar">
        <div class="control-hints">
          <span><span class="key-cap">W</span><span class="key-cap">S</span> 6-DOF FWD/REV</span>
          <span><span class="key-cap">A</span><span class="key-cap">D</span> STRAFE</span>
          <span><span class="key-cap">SPACE</span><span class="key-cap">SHIFT</span> UP/DN</span>
          <span><span class="key-cap">Q</span><span class="key-cap">E</span> ROLL</span>
          <span><span class="key-cap">X</span> COUNTER-BRAKE</span>
          <span><span class="key-cap">MOUSE</span> PITCH/YAW</span>
        </div>
        <div class="action-buttons">
          <button class="btn-tactical" id="btn-audio-toggle">AUDIO: ON</button>
          <button class="btn-tactical" id="btn-briefing">MISSION BRIEFING</button>
          <button class="btn-tactical" id="btn-fullscreen">FULLSCREEN</button>
        </div>
      </div>
    </div>

    <!-- BRIEIFNG MODAL -->
    <div id="briefing-modal">
      <div class="modal-header">
        <span>★ NASA FLIGHT DIRECTOR BRIEFING ★</span>
        <button id="btn-close-briefing" style="background:none; border:none; color:var(--color-cyan-hud); cursor:pointer; font-size:18px;">&times;</button>
      </div>
      <div class="modal-body">
        <p><strong>RECRUITMENT & INDUCTION:</strong> Welcome to the NASA Astronaut Candidate Program, Junior Astronaut. Before you venture beyond Earth's gravity well, you must master microgravity physics.</p>
        <br>
        <p><strong>PHASE 1 - ZERO-G PHYSICS:</strong> Navigate through 4 scientific calibration rings in the simulation chamber. Learn Newton's 1st Law (inertia without friction) and 3rd Law (every thruster action has an equal and opposite reaction).</p>
        <br>
        <p><strong>PHASE 2 - ROCKET LAUNCH:</strong> Say farewell and ascend through the atmosphere aboard the Saturn V rocket into low Earth orbit.</p>
        <br>
        <p><strong>PHASE 3 - MANUAL ISS DOCKING:</strong> Take control of the crew capsule. Use 6-DOF RCS thrusters to align with the ISS PMA-2 docking port with closure speed &lt; 0.35 m/s.</p>
        <br>
        <p><strong>PHASE 4 - CUPOLA EARTH VIEW:</strong> Float inside the ISS Cupola observation deck and gaze upon the majesty of planet Earth to a stirring cosmic symphony.</p>
      </div>
      <div class="modal-actions">
        <button class="btn-tactical" id="btn-dismiss-briefing">ACKNOWLEDGE & RESUME</button>
      </div>
    </div>

    <!-- HOLLYWOOD CINEMATIC LOADING SCREEN -->
    <div id="loading-overlay">
      <div class="mission-badge-logo">🚀</div>
      <div class="loading-title">NASA ODYSSEY</div>
      <div class="loading-subtitle">FROM ZERO-G TO THE INTERNATIONAL SPACE STATION</div>
      <div class="loading-progress-container">
        <div id="loading-progress-bar"></div>
      </div>
      <div class="loading-status-text" id="loading-status">INITIALIZING COSMIC SIMULATION ENGINE...</div>
    </div>

    <div id="unity-container">
      <canvas id="unity-canvas" tabindex="-1"></canvas>
    </div>

    <script>
      var canvas = document.querySelector("#unity-canvas");
      var loadingOverlay = document.querySelector("#loading-overlay");
      var progressBar = document.querySelector("#loading-progress-bar");
      var statusText = document.querySelector("#loading-status");
      var briefingModal = document.querySelector("#briefing-modal");
      var audioToggleBtn = document.querySelector("#btn-audio-toggle");

      var audioSymphony = document.querySelector("#audio-cupola-symphony");
      var audioLaunch = document.querySelector("#audio-launch-comms");
      var audioEnabled = true;

      function startAudio() {
        if (!audioEnabled) return;
        if (audioSymphony && audioSymphony.paused) {
          audioSymphony.volume = 0.65;
          audioSymphony.play().catch(function(e) { console.log("Audio play postponed:", e); });
        }
      }

      window.addEventListener("click", function() { startAudio(); }, { once: true });
      window.addEventListener("keydown", function() { startAudio(); }, { once: true });

      audioToggleBtn.addEventListener("click", function() {
        audioEnabled = !audioEnabled;
        if (audioEnabled) {
          audioToggleBtn.textContent = "AUDIO: ON";
          audioToggleBtn.style.color = "var(--color-cyan-hud)";
          startAudio();
        } else {
          audioToggleBtn.textContent = "AUDIO: MUTED";
          audioToggleBtn.style.color = "var(--color-amber-alert)";
          if (audioSymphony) audioSymphony.pause();
          if (audioLaunch) audioLaunch.pause();
        }
      });

      document.querySelector("#btn-briefing").addEventListener("click", function() {
        briefingModal.style.display = "block";
      });
      document.querySelector("#btn-close-briefing").addEventListener("click", function() {
        briefingModal.style.display = "none";
      });
      document.querySelector("#btn-dismiss-briefing").addEventListener("click", function() {
        briefingModal.style.display = "none";
      });

      document.querySelector("#btn-fullscreen").addEventListener("click", function() {
        if (!document.fullscreenElement) {
          document.documentElement.requestFullscreen().catch(function(err){});
        } else {
          document.exitFullscreen().catch(function(err){});
        }
      });

      var buildUrl = "Build";
      var loaderUrl = buildUrl + "/WebGL.loader.js";
      var config = {
        arguments: [],
        dataUrl: buildUrl + "/WebGL.data",
        frameworkUrl: buildUrl + "/WebGL.framework.js",
        codeUrl: buildUrl + "/WebGL.wasm",
        streamingAssetsUrl: "StreamingAssets",
        companyName: "NASA Challenge",
        productName: "NASA Odyssey: From Zero-G to the ISS",
        productVersion: "1.0",
        showBanner: function() {}
      };

      var script = document.createElement("script");
      script.src = loaderUrl;
      script.onload = function() {
        createUnityInstance(canvas, config, function(progress) {
          progressBar.style.width = (progress * 100) + "%";
          if (progress < 0.4) {
            statusText.textContent = "CALIBRATING ZERO-G PHYSICS SIMULATOR (" + Math.round(progress * 100) + "%)...";
          } else if (progress < 0.8) {
            statusText.textContent = "MOUNTING NASA 3D ORBITAL ASSETS & TEXTURES (" + Math.round(progress * 100) + "%)...";
          } else {
            statusText.textContent = "ESTABLISHING TELEMETRY LINK TO HOUSTON (" + Math.round(progress * 100) + "%)...";
          }
        }).then(function(unityInstance) {
          statusText.textContent = "ALL SYSTEMS GO. WELCOME CANDIDATE.";
          setTimeout(function() {
            loadingOverlay.style.opacity = "0";
            setTimeout(function() {
              loadingOverlay.style.display = "none";
            }, 800);
          }, 600);
        }).catch(function(message) {
          alert("Unity WebGL Initialization Error: " + message);
        });
      };
      document.body.appendChild(script);
    </script>
  </body>
</html>
"""

with open(target_html, "w") as f:
    f.write(html_content)

with open(os.path.join(template_dir, "index.html"), "w") as f:
    f.write(html_content)

print(f"Applied tactical shell to {target_html} and {template_dir}/index.html")
