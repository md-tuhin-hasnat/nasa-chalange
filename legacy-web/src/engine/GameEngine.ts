/**
 * GameEngine.ts
 * Master Babylon.js Engine & Havok Physics Director
 * Drives high-performance 60-144 FPS rendering loop, ACES tone mapping, bloom, and scene sequencing.
 */

import {
  Engine,
  Scene,
  Vector3,
  Color3,
  Color4,
  HemisphericLight,
  DirectionalLight,
  ShadowGenerator,
  DefaultRenderingPipeline,
  HavokPlugin,
} from '@babylonjs/core';
import HavokPhysics from '@babylonjs/havok';
import '@babylonjs/loaders/glTF';

export class GameEngine {
  private static instance: GameEngine;
  public canvas: HTMLCanvasElement;
  public engine: Engine;
  public currentScene: Scene | null = null;
  public havokPlugin: HavokPlugin | null = null;
  private resizeHandler: () => void;

  private constructor(canvasId: string) {
    const canvas = document.getElementById(canvasId) as HTMLCanvasElement;
    if (!canvas) {
      throw new Error(`Canvas with id ${canvasId} not found.`);
    }
    this.canvas = canvas;
    this.engine = new Engine(canvas, true, {
      preserveDrawingBuffer: true,
      stencil: true,
      antialias: true,
      powerPreference: 'high-performance',
    });

    this.resizeHandler = () => {
      this.engine.resize();
    };
    window.addEventListener('resize', this.resizeHandler);

    this.engine.runRenderLoop(() => {
      if (this.currentScene && this.currentScene.activeCamera) {
        this.currentScene.render();
      }
    });
  }

  public static getInstance(canvasId = 'renderCanvas'): GameEngine {
    if (!this.instance) {
      this.instance = new GameEngine(canvasId);
    }
    return this.instance;
  }

  /**
   * Initializes Havok Physics WebAssembly
   */
  public async initPhysics(): Promise<HavokPlugin> {
    if (this.havokPlugin) return this.havokPlugin;
    try {
      const havokInstance = await HavokPhysics();
      this.havokPlugin = new HavokPlugin(true, havokInstance);
      return this.havokPlugin;
    } catch (err) {
      console.warn('Havok physics wasm initialization warning, falling back to simulated kinematics:', err);
      return new HavokPlugin(false);
    }
  }

  /**
   * Creates a pre-configured AAA scene with ACES tone mapping and HDR bloom
   */
  public createTacticalScene(): Scene {
    const scene = new Scene(this.engine);
    scene.clearColor = new Color4(0.01, 0.02, 0.04, 1.0);

    // Setup High-End Post Processing Pipeline
    const pipeline = new DefaultRenderingPipeline(
      'TacticalPostProcess',
      true,
      scene,
      scene.activeCamera ? [scene.activeCamera] : []
    );
    pipeline.bloomEnabled = true;
    pipeline.bloomThreshold = 0.8;
    pipeline.bloomWeight = 0.35;
    pipeline.bloomKernel = 64;
    pipeline.bloomScale = 0.5;

    pipeline.imageProcessingEnabled = true;
    pipeline.imageProcessing.toneMappingEnabled = true;
    pipeline.imageProcessing.toneMappingType = 1; // ACES
    pipeline.imageProcessing.contrast = 1.15;
    pipeline.imageProcessing.exposure = 1.05;
    pipeline.imageProcessing.vignetteEnabled = true;
    pipeline.imageProcessing.vignetteWeight = 1.5;
    pipeline.imageProcessing.vignetteColor = new Color4(0, 0, 0, 0);

    // Ambient Lighting
    const hemiLight = new HemisphericLight('HemiLight', new Vector3(0, 1, 0), scene);
    hemiLight.intensity = 0.45;
    hemiLight.groundColor = new Color3(0.04, 0.06, 0.1);
    hemiLight.diffuse = new Color3(0.7, 0.8, 0.95);

    // Key Directional Sunlight
    const dirLight = new DirectionalLight('SunLight', new Vector3(-0.6, -1.0, 0.4), scene);
    dirLight.position = new Vector3(30, 50, -20);
    dirLight.intensity = 1.6;
    dirLight.diffuse = new Color3(1.0, 0.98, 0.92);

    const shadowGen = new ShadowGenerator(2048, dirLight);
    shadowGen.useBlurExponentialShadowMap = true;
    shadowGen.blurKernel = 32;

    return scene;
  }

  public setScene(scene: Scene): void {
    if (this.currentScene) {
      this.currentScene.dispose();
    }
    this.currentScene = scene;
  }

  public dispose(): void {
    window.removeEventListener('resize', this.resizeHandler);
    if (this.currentScene) {
      this.currentScene.dispose();
    }
    this.engine.dispose();
  }
}
