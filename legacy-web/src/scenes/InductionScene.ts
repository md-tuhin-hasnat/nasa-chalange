/**
 * InductionScene.ts
 * Scene 1: Junior Astronaut Induction & A.R.E.S. AI Pairing Ceremony
 * Features 3D Operative Astronaut Avatar in the Space Hangar, tactical voice briefing, and mission start.
 */

import {
  Scene,
  ArcRotateCamera,
  Vector3,
  Color3,
  Color4,
  HemisphericLight,
  PointLight,
  SceneLoader,
  MeshBuilder,
  StandardMaterial,
  AbstractMesh,
} from '@babylonjs/core';
import { Button, Control, Rectangle, TextBlock } from '@babylonjs/gui';
import { GameEngine } from '../engine/GameEngine';
import { TacticalFpsHUD } from '../ui/TacticalFpsHUD';
import { AresVoiceDirector } from '../engine/AresVoiceDirector';
import { SoundDirector } from '../engine/SoundDirector';

export class InductionScene {
  public scene: Scene;
  public hud: TacticalFpsHUD;
  private astronautMesh: AbstractMesh | null = null;
  private onStartTraining: () => void;

  constructor(onStartTraining: () => void) {
    this.onStartTraining = onStartTraining;
    const game = GameEngine.getInstance();
    this.scene = game.createTacticalScene();
    this.hud = new TacticalFpsHUD(this.scene);

    this.setupCamera();
    this.setupEnvironment();
    this.loadAstronautModel();
    this.setupInductionUI();
    this.playInductionBriefing();
  }

  private setupCamera(): void {
    const camera = new ArcRotateCamera(
      'InductionCamera',
      -Math.PI / 2,
      Math.PI / 2.3,
      3.2,
      new Vector3(0, 1.2, 0),
      this.scene
    );
    camera.attachControl(GameEngine.getInstance().canvas, true);
    camera.wheelPrecision = 50;
    camera.lowerRadiusLimit = 2.0;
    camera.upperRadiusLimit = 5.5;
    camera.lowerBetaLimit = 0.5;
    camera.upperBetaLimit = Math.PI / 2;
    this.scene.activeCamera = camera;
    try {
      this.scene.postProcessRenderPipelineManager.attachCamerasToRenderPipeline('TacticalPostProcess', camera);
    } catch {
      // Ignore if pipeline not initialized
    }

    // Slow automatic cinematic orbit
    this.scene.registerBeforeRender(() => {
      camera.alpha += 0.003;
      this.hud.updateCompass((camera.alpha * 180) / Math.PI);
    });
  }

  private setupEnvironment(): void {
    // Sci-Fi Turntable Platform
    const platform = MeshBuilder.CreateCylinder(
      'Turntable',
      { diameter: 3.5, height: 0.15, tessellation: 48 },
      this.scene
    );
    platform.position.y = -0.075;

    const platMat = new StandardMaterial('PlatMat', this.scene);
    platMat.diffuseColor = new Color3(0.08, 0.1, 0.14);
    platMat.specularColor = new Color3(0.3, 0.3, 0.3);
    platform.material = platMat;

    // Glowing Cyan Trim Ring on Platform
    const trim = MeshBuilder.CreateTorus(
      'TurntableTrim',
      { diameter: 3.48, thickness: 0.04, tessellation: 48 },
      this.scene
    );
    trim.position.y = 0.01;
    const trimMat = new StandardMaterial('TrimMat', this.scene);
    trimMat.emissiveColor = new Color3(0.0, 0.94, 1.0);
    trim.material = trimMat;

    // Blue Rim Spotlight
    const spot = new PointLight('SpotHangar', new Vector3(0, 3.5, 0), this.scene);
    spot.diffuse = new Color3(0.0, 0.9, 1.0);
    spot.intensity = 1.8;
  }

  private loadAstronautModel(): void {
    SceneLoader.ImportMesh(
      '',
      '/assets/models/',
      'astronaut_operative.glb',
      this.scene,
      (meshes) => {
        if (meshes.length > 0) {
          const root = meshes[0];
          root.position = new Vector3(0, 0, 0);
          this.astronautMesh = root;
          this.hud.addTickerEvent('ARMORY', 'ASTRONAUT OPERATIVE EVA SUIT ONLINE', 100);
        }
      },
      undefined,
      (err) => {
        console.warn('Fallback astronaut avatar model warning:', err);
      }
    );
  }

  private setupInductionUI(): void {
    // Center-bottom Mission Launch Action Card
    const launchCard = new Rectangle('LaunchCard');
    launchCard.width = '460px';
    launchCard.height = '140px';
    launchCard.bottom = '120px';
    launchCard.horizontalAlignment = Control.HORIZONTAL_ALIGNMENT_CENTER;
    launchCard.verticalAlignment = Control.VERTICAL_ALIGNMENT_BOTTOM;
    launchCard.background = 'rgba(2, 10, 22, 0.88)';
    launchCard.color = '#00f0ff';
    launchCard.thickness = 1.5;
    launchCard.cornerRadius = 6;
    this.hud.ui.addControl(launchCard);

    const title = new TextBlock('LaunchTitle');
    title.text = 'MISSION 0: CADET ZERO-G COMBAT QUALIFICATION';
    title.color = '#ffd54f';
    title.fontSize = 13;
    title.fontWeight = 'bold';
    title.fontFamily = 'Courier New, monospace';
    title.top = '-40px';
    launchCard.addControl(title);

    const desc = new TextBlock('LaunchDesc');
    desc.text = 'Neutral Buoyancy Sim // 6-DOF RCS Navigation // ECLSS Overhaul';
    desc.color = 'rgba(255, 255, 255, 0.8)';
    desc.fontSize = 11;
    desc.fontFamily = 'Courier New, monospace';
    desc.top = '-15px';
    launchCard.addControl(desc);

    const btn = Button.CreateSimpleButton('StartTrainingBtn', 'DEPLOY TO ZERO-G TANK [SPACE]');
    btn.width = '400px';
    btn.height = '42px';
    btn.top = '30px';
    btn.background = 'rgba(0, 240, 255, 0.25)';
    btn.color = '#00f0ff';
    btn.fontSize = 13;
    btn.fontFamily = 'Courier New, monospace';
    btn.fontWeight = 'bold';
    btn.cornerRadius = 4;
    btn.thickness = 1.5;
    btn.onPointerClickObservable.add(() => {
      SoundDirector.playTacticalClick();
      launchCard.dispose();
      this.onStartTraining();
    });
    launchCard.addControl(btn);

    // Keyboard Space shortcut
    const keyListener = (e: KeyboardEvent) => {
      if (e.code === 'Space') {
        window.removeEventListener('keydown', keyListener);
        SoundDirector.playTacticalClick();
        launchCard.dispose();
        this.onStartTraining();
      }
    };
    window.addEventListener('keydown', keyListener);
  }

  private playInductionBriefing(): void {
    const ares = AresVoiceDirector.getInstance();
    setTimeout(() => {
      ares.transmit({
        sender: 'FLIGHT DIRECTOR',
        callsign: 'CAPCOM-1',
        message: 'Attention Cadet Cooper. Welcome to the NASA Deep Space Exploration Division. Your psychological and tactical evaluations scored in the 99th percentile.',
        priority: 'ROUTINE',
      });
      this.hud.addTickerEvent('DIRECTOR', 'CADET COOPER INDUCTION VERIFIED', 250);
    }, 800);

    setTimeout(() => {
      ares.transmit({
        sender: 'A.R.E.S.',
        callsign: 'AI-CORE',
        message: 'Neural link established. I am A.R.E.S., your integrated tactical reconnaissance and life support AI. I will guide your telemetry, RCS thrust vectors, and ECLSS systems.',
        priority: 'TACTICAL',
      });
      this.hud.addTickerEvent('A.R.E.S.', 'NEURAL COMPANION SYNC COMPLETED', 200);
    }, 6500);
  }
}
