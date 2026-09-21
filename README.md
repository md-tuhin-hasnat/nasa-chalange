# Ares Resurgence: Mission 0 — The Jezero Anomaly
### A Hollywood-Style Cinematic NASA Space Exploration Game in Unity 6

Built for Unity 6 (6000.6.2f1) using the Universal Render Pipeline (URP), featuring authentic open-source NASA 3D assets (GLB) from NASA's official 3D Resources repository.

---

## 🚀 The Story: Mission 0 — "The Dark Dawn"

### Context & Setting
- **Location**: Jezero Crater, Mars — *Elysium Base Alpha* Sub-surface Research Outpost.
- **Year**: 2038.
- **Premise**: In the wake of humanity's first permanent Martian outpost, scientists were analyzing cached core samples left by the Mars 2020 Perseverance Rover. Forty-eight hours ago, an unnatural seismic disturbance triggered a cataclysmic geomagnetic dust storm, knocking out Elysium Base's primary nuclear-solar power grid and severing high-gain communications with Houston Mission Control.
- **Protagonist**: Commander Alex Ray, Lead Systems Astrobiologist.

### Mission 0 Walkthrough
1. **Auxiliary Life Support**: Wake up in Elysium Base during a red alert blackout. Access the auxiliary power terminal to stabilize habitat life support.
2. **Suit Up & Airlock Cycle**: Equip the EVA Environmental Suit. Access the airlock console to depressurize and unseal the blast doors.
3. **The Crimson Tempest**: Step out into the howling Martian dust storm. Low-visibility amber fog, high winds, and rotating emergency beacon strobes illuminate the red dunes.
4. **Relay Point Bravo**: Use the helmet compass HUD to navigate ~60 meters across the craggy Martian landscape to the Base Station communications array.
5. **Dish Diagnostics**: Run diagnostics on the antenna dish to uncover the source of the outage: a massive subsurface electromagnetic shockwave originating from the Perseverance Rover drill cache site.
6. **The Jezero Anomaly**: Trek to the Perseverance Rover and InSight seismic station. A deep geological fissure has split the bedrock, exposing glowing alien crystalline strata pulsing on uncatalogued radio frequencies.
7. **Extraction & Cliffhanger**: Scan the anomaly, secure the telemetry, and transmit the emergency distress beacon.

---

## 🎮 Gameplay Features

- **Martian Gravity & Physics**: $0.38g$ Martian gravity ($3.72 \text{ m/s}^2$) for floatier leaps, realistic inertia, and authentic movement.
- **Diegetic Helmet Visor HUD**:
  - 360° Compass Ribbon with active waypoint distance marker in meters.
  - Life Support: Oxygen level percentage and biometric heart rate monitor (BPM).
  - Environment: Suit pressure gauge (BAR) and Martian surface radiation dosimeter (mSv/h).
  - Dynamic interaction reticle and action prompts.
  - Subtitle / Radio Transmission box with speaker tags and audio waveforms.
- **Procedural Cinematic Audio Engine**:
  - Muffled in-helmet breathing audio reacting to player movement.
  - Dynamic heartbeat audio scaling with sprint stamina.
  - Howling Martian wind with real-time Perlin noise gust modulation.
  - Airlock depressurization hiss, terminal click feedback, and resonant alien chords.
- **Authentic NASA 3D Models (`Assets/NASA_Models/`)**:
  - `Mars 2020 Perseverance Rover.glb`
  - `Ingenuity Mars Helicopter.glb`
  - `InSight Cruise Lander (panels deployed).glb`
  - `Astronaut.glb`
  - `Base Station.glb`
  - `Habitat Demonstration Unit.glb`

---

## 🛠️ How to Play / Run in Unity

1. **Open the Project in Unity 6**:
   ```bash
   unity open /home/touhidur/Code/nasa-chalange
   ```
2. **Press Play**:
   The runtime bootstrapper (`GameBootstrap.cs` / `ProceduralMartianWorld.cs`) automatically initializes the Martian crater terrain, lighting, storm particle system, NASA models, HUD, and Mission 0 story sequence.
3. **Controls**:
   - `W`, `A`, `S`, `D`: Move
   - `Mouse`: Look around
   - `Left Shift`: Sprint
   - `Space`: Jump (0.38g Martian physics)
   - `E` or `Left Click`: Interact with terminals, consoles, and anomalies
   - `Escape`: Toggle cursor lock
