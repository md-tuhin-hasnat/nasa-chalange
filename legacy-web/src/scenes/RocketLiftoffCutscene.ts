/**
 * RocketLiftoffCutscene.ts
 * Scene 3: Falcon Heavy / Crew Dragon Cinematic Liftoff Cutscene
 * Features 2.39:1 anamorphic letterbox, realistic 3D astronaut avatar in cockpit couch,
 * countdown, procedural bass rumble, camera vibration, and orbital insertion.
 */

import {
  Scene,
  UniversalCamera,
  Vector3,
  Color3,
  PointLight,
  SceneLoader,
  MeshBuilder,
  StandardMaterial,
  AbstractMesh,
} from '@babylonjs/core';
import { GameEngine } from '../engine/GameEngine';
import { TacticalFpsHUD } from '../ui/TacticalFpsHUD';
import { AresVoiceDirector } from '../engine/AresVoiceDirector';
import { SoundDirector } from '../engine/SoundDirector';

export class RocketLiftoffCutscene {
  public scene: Scene;
  public hud: TacticalFpsHUD;
  private camera!: UniversalCamera;
  private astronautRoot: AbstractMesh | null = null;
  private cockpitRoot: AbstractMesh | null = null;
  private onComplete: () => void;
  private rumbleController: { stop: () => void } | null = null;
  private isShaking = false;
  private shakeIntensity = 0.0;

  constructor(onComplete: () => void) {
    this.onComplete = onComplete;
    const game = GameEngine.getInstance();
    this.scene = game.createTacticalScene();
    this.hud = new TacticalFpsHUD(this.scene);
    this.hud.setCinematicLetterbox(true);

    this.setupCamera();
    this.setupCockpitAndAstronaut();
    this.startCountdownSequence();
  }

  private setupCamera(): void {
    // Cockpit over-the-shoulder / dashboard view
    this.camera = new UniversalCamera('CutsceneCamera', new Vector3(0.35, 1.25, -0.4), this.scene);
    this.camera.setTarget(new Vector3(0, 0.95, 1.2));
    this.scene.activeCamera = this.camera;
    try {
      this.scene.postProcessRenderPipelineManager.attachCamerasToRenderPipeline('TacticalPostProcess', this.camera);
    } catch {
      // Ignore if pipeline not initialized
    }

    // Camera shake loop during rocket ascent
    this.scene.registerBeforeRender(() => {
      if (this.isShaking) {
        const sx = (Math.random() - 0.5) * this.shakeIntensity;
        const sy = (Math.random() - 0.5) * this.shakeIntensity;
        const sz = (Math.random() - 0.5) * this.shakeIntensity;
        this.camera.position.x = 0.35 + sx;
        this.camera.position.y = 1.25 + sy;
        this.camera.position.z = -0.4 + sz;

        if (this.astronautRoot) {
          // G-force head and body reaction
          this.astronautRoot.position.y = 0.02 + sy * 0.5;
          this.astronautRoot.rotation.x = -0.05 + sx * 0.2;
        }
      }
    });
  }

  private setupCockpitAndAstronaut(): void {
    // 1. Load Cockpit
    SceneLoader.ImportMesh(
      '',
      '/assets/models/',
      'falcon_cockpit.glb',
      this.scene,
      (meshes) => {
        if (meshes.length > 0) {
          this.cockpitRoot = meshes[0];
          this.cockpitRoot.position = new Vector3(0, 0, 0);
        }
      }
    );

    // 2. Load Astronaut Operative seated in acceleration couch
    SceneLoader.ImportMesh(
      '',
      '/assets/models/',
      'astronaut_operative.glb',
      this.scene,
      (meshes) => {
        if (meshes.length > 0) {
          this.astronautRoot = meshes[0];
          // Position seated in couch
          this.astronautRoot.position = new Vector3(0, 0.05, 0.3);
          this.astronautRoot.rotation = new Vector3(-0.1, Math.PI, 0);
          this.astronautRoot.scaling = new Vector3(0.9, 0.9, 0.9);
        }
      }
    );

    // Cockpit Internal Hologram Ambient Light
    const cyanLight = new PointLight('CockpitLight', new Vector3(0, 1.3, 0.8), this.scene);
    cyanLight.diffuse = new Color3(0.0, 0.8, 1.0);
    cyanLight.intensity = 1.4;
  }

  private startCountdownSequence(): void {
    const ares = AresVoiceDirector.getInstance();

    setTimeout(() => {
      ares.transmit({
        sender: 'MISSION CONTROL',
        callsign: 'KENNEDY SPACE CENTER',
        message: 'Cape Canaveral Launch Complex 39A. Falcon 9 orbital vehicle terminal countdown started. All systems nominal.',
        priority: 'ROUTINE',
      });
      this.hud.addTickerEvent('LAUNCH CONTROL', 'TERMINAL COUNTDOWN INITIATED', 100);
    }, 1000);

    setTimeout(() => {
      ares.transmit({
        sender: 'MISSION CONTROL',
        callsign: 'KSC',
        message: 'T-minus 5, 4, 3, 2, 1... Main engine ignition... Liftoff of Falcon 9 carrying Cadet Cooper to the International Space Station!',
        priority: 'CRITICAL',
      });
      this.hud.addTickerEvent('LAUNCH CONTROL', 'MAIN ENGINES FULL THRUST // LIFTOFF', 250);

      // Start sound rumble and camera shake
      this.rumbleController = SoundDirector.startRocketRumble();
      this.isShaking = true;
      this.shakeIntensity = 0.04;
    }, 5500);

    // Max-Q / Staging
    setTimeout(() => {
      this.shakeIntensity = 0.07;
      ares.transmit({
        sender: 'A.R.E.S.',
        callsign: 'AI-CORE',
        message: 'Vehicle through Max-Q. Telemetry: 3.8 Gs. Altitude 45 km, velocity Mach 4.6. First stage MECO in 10 seconds.',
        priority: 'TACTICAL',
      });
      this.hud.addTickerEvent('TELEMETRY', 'MAX-Q CLEARED // STAGING INCOMING', 200);
    }, 11500);

    // Orbital Insertion
    setTimeout(() => {
      this.shakeIntensity = 0.01;
      if (this.rumbleController) {
        this.rumbleController.stop();
      }
      ares.transmit({
        sender: 'A.R.E.S.',
        callsign: 'AI-CORE',
        message: 'Second stage cutoff. Orbital insertion confirmed: 408 x 412 km orbit. Approaching International Space Station for rendezvous.',
        priority: 'ACHIEVEMENT',
      });
      this.hud.addTickerEvent('ORBIT', 'INSERTION SUCCESSFUL // 28,000 KM/H', 300);
    }, 16500);

    // Transition to ISS Docking
    setTimeout(() => {
      this.hud.setCinematicLetterbox(false);
      this.onComplete();
    }, 21500);
  }
}
