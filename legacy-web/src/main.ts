/**
 * main.ts
 * Main Entry Point for 'Outpost ESM: The Astronaut Odyssey'
 * Babylon.js + Havok AAA Engine (Mainstream FPS / Avengers / PUBG aesthetic)
 */

import { GameEngine } from './engine/GameEngine';
import { InductionScene } from './scenes/InductionScene';
import { ZeroGTrainingScene } from './scenes/ZeroGTrainingScene';
import { RocketLiftoffCutscene } from './scenes/RocketLiftoffCutscene';
import { ISSDockingScene } from './scenes/ISSDockingScene';

async function initGame() {
  console.log('>>> Initializing Outpost ESM Engine (Babylon.js + Havok)...');
  const game = GameEngine.getInstance('renderCanvas');

  // Initialize Havok physics
  await game.initPhysics();

  // Hide initial loader
  const loader = document.getElementById('loaderScreen');
  if (loader) {
    loader.style.opacity = '0';
    setTimeout(() => {
      loader.style.display = 'none';
    }, 500);
  }

  // Scene transition functions
  const startISSDocking = () => {
    console.log('>>> Transitioning to ISS Docking Scene...');
    const issScene = new ISSDockingScene();
    game.setScene(issScene.scene);
  };

  const startRocketLiftoff = () => {
    console.log('>>> Transitioning to Rocket Liftoff Cutscene...');
    const rocketScene = new RocketLiftoffCutscene(() => {
      startISSDocking();
    });
    game.setScene(rocketScene.scene);
  };

  const startZeroGTraining = () => {
    console.log('>>> Transitioning to Zero-G Training Scene...');
    const trainingScene = new ZeroGTrainingScene(() => {
      startRocketLiftoff();
    });
    game.setScene(trainingScene.scene);
  };

  // Start with Scene 1: Induction Scene
  console.log('>>> Launching Scene 1: Induction Ceremony...');
  const inductionScene = new InductionScene(() => {
    startZeroGTraining();
  });
  game.setScene(inductionScene.scene);
}

// Boot game when DOM is ready
if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', initGame);
} else {
  initGame();
}
