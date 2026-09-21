/**
 * ISSDockingScene.ts
 * Scene 4: ISS Orbital Approach & Precision Docking Maneuver
 * Approaching the 3.5MB authentic NASA International Space Station, PMA-2 target alignment,
 * 6-DOF RCS micro-thrusters, and hard capture magnetic clamp lock.
 */

import {
  Scene,
  UniversalCamera,
  Vector3,
  Color3,
  Color4,
  SceneLoader,
  MeshBuilder,
  StandardMaterial,
  AbstractMesh,
  PointLight,
} from '@babylonjs/core';
import { Button, Control, Rectangle, TextBlock } from '@babylonjs/gui';
import { GameEngine } from '../engine/GameEngine';
import { TacticalFpsHUD } from '../ui/TacticalFpsHUD';
import { AresVoiceDirector } from '../engine/AresVoiceDirector';
import { SoundDirector } from '../engine/SoundDirector';

export class ISSDockingScene {
  public scene: Scene;
  public hud: TacticalFpsHUD;
  private camera!: UniversalCamera;
  private issRoot: AbstractMesh | null = null;
  private velocity: Vector3 = Vector3.Zero();
  private keysPressed: Record<string, boolean> = {};
  private isDocked = false;
  private rangeIndicator: TextBlock | null = null;

  constructor() {
    const game = GameEngine.getInstance();
    this.scene = game.createTacticalScene();
    this.hud = new TacticalFpsHUD(this.scene);

    this.setupCamera();
    this.setupOrbitalEnvironment();
    this.loadISSModel();
    this.setupKeyListeners();
    this.setupDockingLoop();
    this.playDockingBriefing();
  }

  private setupCamera(): void {
    // Start 45 meters back along the docking corridor
    this.camera = new UniversalCamera('DockingCamera', new Vector3(0, 0, -45), this.scene);
    this.camera.setTarget(new Vector3(0, 0, 15));
    this.camera.attachControl(GameEngine.getInstance().canvas, true);
    this.camera.speed = 0.35;
    this.scene.activeCamera = this.camera;
    try {
      this.scene.postProcessRenderPipelineManager.attachCamerasToRenderPipeline('TacticalPostProcess', this.camera);
    } catch {
      // Ignore if pipeline not initialized
    }
  }

  private setupOrbitalEnvironment(): void {
    // Deep Space Sky with subtle Earth Blue Horizon
    this.scene.clearColor = new Color4(0.002, 0.004, 0.012, 1.0);

    // Earth Curve Horizon Sphere below the station
    const earth = MeshBuilder.CreateSphere(
      'EarthHorizon',
      { diameter: 400, segments: 48 },
      this.scene
    );
    earth.position = new Vector3(0, -220, 40);

    const earthMat = new StandardMaterial('EarthMat', this.scene);
    earthMat.diffuseColor = new Color3(0.05, 0.25, 0.55);
    earthMat.emissiveColor = new Color3(0.02, 0.12, 0.35);
    earthMat.specularColor = new Color3(0.1, 0.3, 0.6);
    earth.material = earthMat;

    // Direct harsh sunlight of orbital space
    const sunLight = new PointLight('SunOrbital', new Vector3(60, 40, -50), this.scene);
    sunLight.intensity = 2.2;
    sunLight.diffuse = new Color3(1.0, 0.98, 0.92);
  }

  private loadISSModel(): void {
    SceneLoader.ImportMesh(
      '',
      '/assets/models/',
      'iss_orbital_station.glb',
      this.scene,
      (meshes) => {
        if (meshes.length > 0) {
          this.issRoot = meshes[0];
          this.issRoot.position = new Vector3(0, 0, 0);
          this.hud.addTickerEvent('TELEMETRY', 'AUTHENTIC NASA ISS ORBITAL MODEL ONLINE', 150);
        }
      },
      undefined,
      (err) => {
        console.warn('Fallback loading ISS model:', err);
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

      // Micro-RCS sound bursts
      if (['KeyW', 'KeyS', 'KeyA', 'KeyD', 'Space', 'ControlLeft', 'KeyC'].includes(e.code)) {
        SoundDirector.playRCSBurst(0.08);
      }

      // Loadout keys
      if (e.code === 'Digit1') this.hud.selectLoadoutSlot(0);
      if (e.code === 'Digit2') this.hud.selectLoadoutSlot(1);
      if (e.code === 'Digit3') this.hud.selectLoadoutSlot(2);
      if (e.code === 'Digit4') this.hud.selectLoadoutSlot(3);
    });

    window.addEventListener('keyup', (e) => {
      this.keysPressed[e.code] = false;
    });
  }

  private setupDockingLoop(): void {
    // Docking Alignment Telemetry HUD Card
    const dockCard = new Rectangle('DockCard');
    dockCard.width = '320px';
    dockCard.height = '64px';
    dockCard.top = '72px';
    dockCard.horizontalAlignment = Control.HORIZONTAL_ALIGNMENT_CENTER;
    dockCard.verticalAlignment = Control.VERTICAL_ALIGNMENT_TOP;
    dockCard.background = 'rgba(2, 10, 22, 0.85)';
    dockCard.color = '#00f0ff';
    dockCard.thickness = 1;
    dockCard.cornerRadius = 4;
    this.hud.ui.addControl(dockCard);

    this.rangeIndicator = new TextBlock('RangeText');
    this.rangeIndicator.text = 'PORT: PMA-2 // RANGE: 45.0 M\nALIGNMENT: [OK] // SPEED: 0.8 M/S';
    this.rangeIndicator.color = '#e0f7fa';
    this.rangeIndicator.fontSize = 11;
    this.rangeIndicator.fontFamily = 'Courier New, monospace';
    this.rangeIndicator.fontWeight = 'bold';
    dockCard.addControl(this.rangeIndicator);

    this.scene.registerBeforeRender(() => {
      if (this.isDocked) return;

      const forward = this.camera.getDirection(Vector3.Forward());
      const right = this.camera.getDirection(Vector3.Right());
      const up = this.camera.getDirection(Vector3.Up());

      const thrust = 0.012;
      if (this.keysPressed['KeyW']) this.velocity.addInPlace(forward.scale(thrust));
      if (this.keysPressed['KeyS']) this.velocity.subtractInPlace(forward.scale(thrust));
      if (this.keysPressed['KeyD']) this.velocity.addInPlace(right.scale(thrust));
      if (this.keysPressed['KeyA']) this.velocity.subtractInPlace(right.scale(thrust));
      if (this.keysPressed['Space']) this.velocity.addInPlace(up.scale(thrust));
      if (this.keysPressed['KeyC'] || this.keysPressed['ControlLeft']) this.velocity.subtractInPlace(up.scale(thrust));

      // Damping
      this.velocity.scaleInPlace(0.98);
      this.camera.position.addInPlace(this.velocity);

      // Compass Update
      this.hud.updateCompass((this.camera.rotation.y * 180) / Math.PI);

      // Distance to PMA-2 Docking Port (Target position around x=0, y=0, z=14)
      const targetPos = new Vector3(0, 0, 14.2);
      const dist = Vector3.Distance(this.camera.position, targetPos);
      const speed = this.velocity.length() * 60; // m/s estimate

      if (this.rangeIndicator) {
        this.rangeIndicator.text = `PORT: PMA-2 // RANGE: ${dist.toFixed(1)} M\nALIGNMENT: [LOCKED] // SPEED: ${speed.toFixed(2)} M/S`;
      }

      // Hard Capture threshold
      if (dist <= 3.2 && !this.isDocked) {
        this.triggerHardCapture();
      }
    });
  }

  private triggerHardCapture(): void {
    this.isDocked = true;
    this.velocity = Vector3.Zero();

    // Sound effects
    SoundDirector.playDockingClamp();
    SoundDirector.playSuccessChime();

    this.hud.addTickerEvent('DOCKING MECHANISM', 'PMA-2 MAGNETIC CAPTURE ENGAGED', 500);
    this.hud.addTickerEvent('MISSION CONTROL', 'HARD DOCK CONFIRMED // ECLSS SEAL 100%', 500);

    AresVoiceDirector.getInstance().transmit({
      sender: 'FLIGHT DIRECTOR',
      callsign: 'HOUSTON',
      message: 'Contact and capture confirmed! Welcome aboard the International Space Station, Cadet Cooper. Mission 0 qualification: 100% complete! You are officially an Astronaut Operative.',
      priority: 'ACHIEVEMENT',
    });

    // Victory Banner UI
    setTimeout(() => {
      this.showVictoryBanner();
    }, 2500);
  }

  private showVictoryBanner(): void {
    const banner = new Rectangle('VictoryBanner');
    banner.width = '620px';
    banner.height = '240px';
    banner.horizontalAlignment = Control.HORIZONTAL_ALIGNMENT_CENTER;
    banner.verticalAlignment = Control.VERTICAL_ALIGNMENT_CENTER;
    banner.background = 'rgba(2, 10, 26, 0.95)';
    banner.color = '#ffd54f';
    banner.thickness = 2;
    banner.cornerRadius = 8;
    this.hud.ui.addControl(banner);

    const title = new TextBlock('VictoryTitle');
    title.text = '★ MISSION 0: QUALIFICATION COMPLETE ★';
    title.color = '#ffd54f';
    title.fontSize = 20;
    title.fontWeight = 'bold';
    title.fontFamily = 'Courier New, monospace';
    title.top = '-70px';
    banner.addControl(title);

    const sub = new TextBlock('VictorySub');
    sub.text = 'RANK PROMOTED: JUNIOR ASTRONAUT OPERATIVE [TIER 1]\nTOTAL XP EARNED: +2,500 PTS // S-RANK FLIGHT RECORD';
    sub.color = '#e0f7fa';
    sub.fontSize = 13;
    sub.fontFamily = 'Courier New, monospace';
    sub.top = '-20px';
    banner.addControl(sub);

    const replayBtn = Button.CreateSimpleButton('ReplayBtn', 'RE-ENTER SIMULATION [ENTER]');
    replayBtn.width = '360px';
    replayBtn.height = '46px';
    replayBtn.top = '60px';
    replayBtn.background = 'rgba(0, 240, 255, 0.3)';
    replayBtn.color = '#00f0ff';
    replayBtn.fontSize = 14;
    replayBtn.fontFamily = 'Courier New, monospace';
    replayBtn.fontWeight = 'bold';
    replayBtn.cornerRadius = 4;
    replayBtn.thickness = 1.5;
    replayBtn.onPointerClickObservable.add(() => {
      window.location.reload();
    });
    banner.addControl(replayBtn);
  }

  private playDockingBriefing(): void {
    AresVoiceDirector.getInstance().transmit({
      sender: 'A.R.E.S.',
      callsign: 'AI-CORE',
      message: 'Approaching International Space Station at 400 km altitude. Target is PMA-2 forward docking port. Align crosshairs and control descent speed.',
      priority: 'TACTICAL',
    });
  }
}
