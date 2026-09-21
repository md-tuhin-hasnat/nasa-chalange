/**
 * ZeroGTrainingScene.ts
 * Scene 2: Neutral Buoyancy Lab / Orbital Zero-G Simulation Tank
 * 6-DOF RCS physics floating mechanics, target navigation rings, and emergency valve actuation.
 */

import {
  Scene,
  UniversalCamera,
  Vector3,
  SceneLoader,
  MeshBuilder,
  StandardMaterial,
  Color3,
  AbstractMesh,
} from '@babylonjs/core';
import { Button, Control, Rectangle, TextBlock } from '@babylonjs/gui';
import { GameEngine } from '../engine/GameEngine';
import { TacticalFpsHUD } from '../ui/TacticalFpsHUD';
import { AresVoiceDirector } from '../engine/AresVoiceDirector';
import { SoundDirector } from '../engine/SoundDirector';

export class ZeroGTrainingScene {
  public scene: Scene;
  public hud: TacticalFpsHUD;
  private camera!: UniversalCamera;
  private onComplete: () => void;

  // 6-DOF Motion vectors
  private velocity: Vector3 = Vector3.Zero();
  private keysPressed: Record<string, boolean> = {};

  // Mission objectives
  private ring1Passed = false;
  private ring2Passed = false;
  private ring3Passed = false;
  private valveSealed = false;
  private valveMesh: AbstractMesh | null = null;
  private valvePrompt: Rectangle | null = null;

  constructor(onComplete: () => void) {
    this.onComplete = onComplete;
    const game = GameEngine.getInstance();
    this.scene = game.createTacticalScene();
    this.hud = new TacticalFpsHUD(this.scene);

    this.setupCamera();
    this.loadArenaModel();
    this.setupKeyListeners();
    this.setupGameLoop();
    this.playIntroBriefing();
  }

  private setupCamera(): void {
    this.camera = new UniversalCamera('PlayerZeroGCamera', new Vector3(0, 0, -4), this.scene);
    this.camera.setTarget(new Vector3(0, 0, 10));
    this.camera.attachControl(GameEngine.getInstance().canvas, true);
    this.camera.speed = 0.4;
    this.camera.fov = 1.1; // 63 degrees broad FPS FOV
    this.scene.activeCamera = this.camera;
    try {
      this.scene.postProcessRenderPipelineManager.attachCamerasToRenderPipeline('TacticalPostProcess', this.camera);
    } catch {
      // Ignore if pipeline not initialized
    }
  }

  private loadArenaModel(): void {
    SceneLoader.ImportMesh(
      '',
      '/assets/models/',
      'zero_g_tactical_arena.glb',
      this.scene,
      (meshes) => {
        if (meshes.length > 0) {
          const root = meshes[0];
          root.position = new Vector3(0, 0, 0);

          // Find emergency valve mesh
          this.valveMesh = this.scene.getMeshByName('Emergency_Valve') || null;
          this.hud.addTickerEvent('SIMULATION', 'TACTICAL TRAINING ARENA LOADED', 100);
        }
      },
      undefined,
      (err) => {
        console.warn('Fallback loading arena model:', err);
      }
    );
  }

  private setupKeyListeners(): void {
    const canvas = GameEngine.getInstance().canvas;
    canvas.addEventListener('click', () => {
      canvas.requestPointerLock();
    });

    window.addEventListener('keydown', (e) => {
      this.keysPressed[e.code] = true;

      // RCS Sound bursts on impulse
      if (['KeyW', 'KeyS', 'KeyA', 'KeyD', 'Space', 'ControlLeft', 'KeyC'].includes(e.code)) {
        SoundDirector.playRCSBurst(0.12);
      }

      // Tactical Loadout keybinds
      if (e.code === 'Digit1') this.hud.selectLoadoutSlot(0);
      if (e.code === 'Digit2') this.hud.selectLoadoutSlot(1);
      if (e.code === 'Digit3') this.hud.selectLoadoutSlot(2);
      if (e.code === 'Digit4') this.hud.selectLoadoutSlot(3);

      // Valve interaction key
      if (e.code === 'KeyE' && this.isNearValve() && !this.valveSealed) {
        this.sealValve();
      }
    });

    window.addEventListener('keyup', (e) => {
      this.keysPressed[e.code] = false;
    });
  }

  private setupGameLoop(): void {
    this.scene.registerBeforeRender(() => {
      // 6-DOF Thruster Force accumulation
      const forward = this.camera.getDirection(Vector3.Forward());
      const right = this.camera.getDirection(Vector3.Right());
      const up = this.camera.getDirection(Vector3.Up());

      const thrust = 0.015;
      if (this.keysPressed['KeyW']) this.velocity.addInPlace(forward.scale(thrust));
      if (this.keysPressed['KeyS']) this.velocity.subtractInPlace(forward.scale(thrust));
      if (this.keysPressed['KeyD']) this.velocity.addInPlace(right.scale(thrust));
      if (this.keysPressed['KeyA']) this.velocity.subtractInPlace(right.scale(thrust));
      if (this.keysPressed['Space']) this.velocity.addInPlace(up.scale(thrust * 1.2));
      if (this.keysPressed['KeyC'] || this.keysPressed['ControlLeft']) this.velocity.subtractInPlace(up.scale(thrust * 1.2));

      // Inertial Zero-G damping (coasting physics)
      this.velocity.scaleInPlace(0.975);
      this.camera.position.addInPlace(this.velocity);

      // Update Compass angle from camera rotation
      const heading = (this.camera.rotation.y * 180) / Math.PI;
      this.hud.updateCompass(heading);

      // Check Ring passing triggers
      const pos = this.camera.position;
      if (!this.ring1Passed && pos.z >= 5.0 && pos.z <= 7.5 && Math.hypot(pos.x, pos.y) < 3.2) {
        this.ring1Passed = true;
        this.hud.addTickerEvent('COOPER', 'TARGET VECTOR RING 01 CLEARED', 150);
        AresVoiceDirector.getInstance().transmit({
          sender: 'A.R.E.S.',
          callsign: 'AI-CORE',
          message: 'Ring 01 cleared. Momentum vector stable. Next gate bearing 030 degrees.',
          priority: 'TACTICAL',
        });
      }

      if (!this.ring2Passed && pos.z >= 17.0 && pos.z <= 19.5 && Math.hypot(pos.x + 1.5, pos.y + 1.0) < 3.0) {
        this.ring2Passed = true;
        this.hud.addTickerEvent('COOPER', 'TARGET VECTOR RING 02 CLEARED', 200);
        AresVoiceDirector.getInstance().transmit({
          sender: 'A.R.E.S.',
          callsign: 'AI-CORE',
          message: 'Gate 02 validated. Velocity 4.2 m/s. Final gate aligned.',
          priority: 'TACTICAL',
        });
      }

      if (!this.ring3Passed && pos.z >= 29.0 && pos.z <= 31.5 && Math.hypot(pos.x - 1.2, pos.y - 1.2) < 2.5) {
        this.ring3Passed = true;
        this.hud.addTickerEvent('COOPER', 'TARGET VECTOR RING 03 CLEARED', 250);
        AresVoiceDirector.getInstance().transmit({
          sender: 'A.R.E.S.',
          callsign: 'AI-CORE',
          message: 'Warning: Emergency decompression detected at terminal bulkhead. Approach valve and press [E] to seal!',
          priority: 'CRITICAL',
        });
      }

      // Check proximity to Emergency Valve Station (around z=38 to 44)
      if (this.isNearValve() && !this.valveSealed) {
        this.showValvePrompt();
      } else {
        this.hideValvePrompt();
      }
    });
  }

  private isNearValve(): boolean {
    const p = this.camera.position;
    return p.z >= 37.0 && p.z <= 44.0 && Math.abs(p.x) < 3.5;
  }

  private showValvePrompt(): void {
    if (this.valvePrompt) return;
    this.valvePrompt = new Rectangle('ValvePrompt');
    this.valvePrompt.width = '380px';
    this.valvePrompt.height = '64px';
    this.valvePrompt.top = '120px';
    this.valvePrompt.horizontalAlignment = Control.HORIZONTAL_ALIGNMENT_CENTER;
    this.valvePrompt.verticalAlignment = Control.VERTICAL_ALIGNMENT_CENTER;
    this.valvePrompt.background = 'rgba(230, 40, 20, 0.85)';
    this.valvePrompt.color = '#ffffff';
    this.valvePrompt.thickness = 2;
    this.valvePrompt.cornerRadius = 6;
    this.hud.ui.addControl(this.valvePrompt);

    const txt = new TextBlock('ValvePromptText');
    txt.text = '[E] SEAL EMERGENCY PRESSURE VALVE';
    txt.color = '#ffffff';
    txt.fontSize = 15;
    txt.fontWeight = 'bold';
    txt.fontFamily = 'Courier New, monospace';
    this.valvePrompt.addControl(txt);

    this.valvePrompt.onPointerClickObservable.add(() => {
      this.sealValve();
    });
  }

  private hideValvePrompt(): void {
    if (this.valvePrompt) {
      this.hud.ui.removeControl(this.valvePrompt);
      this.valvePrompt.dispose();
      this.valvePrompt = null;
    }
  }

  private sealValve(): void {
    this.valveSealed = true;
    this.hideValvePrompt();
    SoundDirector.playValveHiss();

    // Rotate valve mesh if found
    if (this.valveMesh) {
      this.scene.registerBeforeRender(() => {
        this.valveMesh!.rotation.y += 0.15;
      });
    }

    this.hud.addTickerEvent('COOPER', 'EMERGENCY PRESSURE VALVE SEALED', 500);
    this.hud.addTickerEvent('SIMULATION', 'QUALIFICATION TEST COMPLETED [GRADE: S-TIER]', 350);

    AresVoiceDirector.getInstance().transmit({
      sender: 'FLIGHT DIRECTOR',
      callsign: 'CAPCOM-1',
      message: 'Outstanding work, Cadet Cooper! Cabin pressure restored to 101.3 kPa. Zero-G combat evaluation passed with S-Tier honors. Report immediately to Launch Complex 39A for liftoff to the ISS!',
      priority: 'ACHIEVEMENT',
    });

    setTimeout(() => {
      this.onComplete();
    }, 6000);
  }

  private playIntroBriefing(): void {
    AresVoiceDirector.getInstance().transmit({
      sender: 'A.R.E.S.',
      callsign: 'AI-CORE',
      message: 'Zero-G simulation online. Use WASD for lateral RCS translation, Space and C for vertical thrusters. Navigate through the 3 holographic guidance gates.',
      priority: 'TACTICAL',
    });
  }
}
