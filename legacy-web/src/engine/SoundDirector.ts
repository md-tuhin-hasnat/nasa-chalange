/**
 * SoundDirector.ts
 * Procedural Web Audio API sound synthesizer for high-impact space flight & FPS gaming.
 * Zero external mp3 dependencies — works instantly offline and in any browser.
 */

export class SoundDirector {
  private static ctx: AudioContext | null = null;
  private static isMuted: boolean = false;

  private static getContext(): AudioContext {
    if (!this.ctx) {
      const AudioCtxClass = window.AudioContext || (window as unknown as { webkitAudioContext: typeof AudioContext }).webkitAudioContext;
      this.ctx = new AudioCtxClass();
    }
    if (this.ctx.state === 'suspended') {
      this.ctx.resume();
    }
    return this.ctx;
  }

  /**
   * Quindar Beep (Apollo / NASA radio comms chirp)
   */
  public static playQuindarBeep(isIntro = true): void {
    if (this.isMuted) return;
    try {
      const ctx = this.getContext();
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();

      osc.type = 'sine';
      osc.frequency.setValueAtTime(isIntro ? 2524 : 2475, ctx.currentTime);

      gain.gain.setValueAtTime(0.08, ctx.currentTime);
      gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.25);

      osc.connect(gain);
      gain.connect(ctx.destination);

      osc.start();
      osc.stop(ctx.currentTime + 0.25);
    } catch {
      // Audio context might need user gesture
    }
  }

  /**
   * 6-DOF RCS Cold-Gas Thruster Hiss
   */
  public static playRCSBurst(duration = 0.2): void {
    if (this.isMuted) return;
    try {
      const ctx = this.getContext();
      const bufferSize = ctx.sampleRate * duration;
      const buffer = ctx.createBuffer(1, bufferSize, ctx.sampleRate);
      const output = buffer.getChannelData(0);

      for (let i = 0; i < bufferSize; i++) {
        output[i] = Math.random() * 2 - 1;
      }

      const whiteNoise = ctx.createBufferSource();
      whiteNoise.buffer = buffer;

      const filter = ctx.createBiquadFilter();
      filter.type = 'bandpass';
      filter.frequency.setValueAtTime(1400, ctx.currentTime);
      filter.Q.setValueAtTime(3.0, ctx.currentTime);

      const gain = ctx.createGain();
      gain.gain.setValueAtTime(0.15, ctx.currentTime);
      gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + duration);

      whiteNoise.connect(filter);
      filter.connect(gain);
      gain.connect(ctx.destination);

      whiteNoise.start();
    } catch {
      // Audio fallback
    }
  }

  /**
   * Heavy Rocket Engine Deep Rumble
   */
  public static startRocketRumble(): { stop: () => void } {
    if (this.isMuted) return { stop: () => {} };
    try {
      const ctx = this.getContext();
      const bufferSize = ctx.sampleRate * 2;
      const buffer = ctx.createBuffer(1, bufferSize, ctx.sampleRate);
      const data = buffer.getChannelData(0);
      let lastOut = 0.0;
      for (let i = 0; i < bufferSize; i++) {
        const white = Math.random() * 2 - 1;
        data[i] = (lastOut + 0.02 * white) / 1.02; // Brown noise for deep bass
        lastOut = data[i];
        data[i] *= 3.5;
      }

      const noise = ctx.createBufferSource();
      noise.buffer = buffer;
      noise.loop = true;

      const filter = ctx.createBiquadFilter();
      filter.type = 'lowpass';
      filter.frequency.setValueAtTime(120, ctx.currentTime);

      const gain = ctx.createGain();
      gain.gain.setValueAtTime(0.01, ctx.currentTime);
      gain.gain.linearRampToValueAtTime(0.35, ctx.currentTime + 3.0);

      noise.connect(filter);
      filter.connect(gain);
      gain.connect(ctx.destination);

      noise.start();

      return {
        stop: () => {
          try {
            gain.gain.linearRampToValueAtTime(0.001, ctx.currentTime + 1.5);
            setTimeout(() => noise.stop(), 1600);
          } catch {
            // Ignore
          }
        },
      };
    } catch {
      return { stop: () => {} };
    }
  }

  /**
   * Heavy Metallic Hydraulic Docking Clamp Lock
   */
  public static playDockingClamp(): void {
    if (this.isMuted) return;
    try {
      const ctx = this.getContext();
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();

      osc.type = 'triangle';
      osc.frequency.setValueAtTime(80, ctx.currentTime);
      osc.frequency.exponentialRampToValueAtTime(40, ctx.currentTime + 0.4);

      gain.gain.setValueAtTime(0.4, ctx.currentTime);
      gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.5);

      osc.connect(gain);
      gain.connect(ctx.destination);

      osc.start();
      osc.stop(ctx.currentTime + 0.5);

      // Clank resonance
      setTimeout(() => {
        const highOsc = ctx.createOscillator();
        const highGain = ctx.createGain();
        highOsc.type = 'sine';
        highOsc.frequency.setValueAtTime(840, ctx.currentTime);
        highOsc.frequency.exponentialRampToValueAtTime(220, ctx.currentTime + 0.3);
        highGain.gain.setValueAtTime(0.2, ctx.currentTime);
        highGain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.3);
        highOsc.connect(highGain);
        highGain.connect(ctx.destination);
        highOsc.start();
        highOsc.stop(ctx.currentTime + 0.3);
      }, 80);
    } catch {
      // Audio fallback
    }
  }

  /**
   * High-Tech UI Tactical Beep
   */
  public static playTacticalClick(): void {
    if (this.isMuted) return;
    try {
      const ctx = this.getContext();
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();

      osc.type = 'sine';
      osc.frequency.setValueAtTime(1200, ctx.currentTime);
      osc.frequency.exponentialRampToValueAtTime(600, ctx.currentTime + 0.05);

      gain.gain.setValueAtTime(0.12, ctx.currentTime);
      gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.05);

      osc.connect(gain);
      gain.connect(ctx.destination);

      osc.start();
      osc.stop(ctx.currentTime + 0.05);
    } catch {
      // Audio fallback
    }
  }

  /**
   * Mission Level Up / Ring Pass Fanfare Chime
   */
  public static playSuccessChime(): void {
    if (this.isMuted) return;
    try {
      const ctx = this.getContext();
      const freqs = [523.25, 659.25, 783.99, 1046.5]; // C5, E5, G5, C6
      freqs.forEach((freq, idx) => {
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();
        osc.type = 'triangle';
        osc.frequency.setValueAtTime(freq, ctx.currentTime + idx * 0.08);

        gain.gain.setValueAtTime(0, ctx.currentTime + idx * 0.08);
        gain.gain.linearRampToValueAtTime(0.15, ctx.currentTime + idx * 0.08 + 0.02);
        gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + idx * 0.08 + 0.6);

        osc.connect(gain);
        gain.connect(ctx.destination);

        osc.start(ctx.currentTime + idx * 0.08);
        osc.stop(ctx.currentTime + idx * 0.08 + 0.6);
      });
    } catch {
      // Audio fallback
    }
  }

  /**
   * Emergency Valve Turn Steam Hiss
   */
  public static playValveHiss(): void {
    if (this.isMuted) return;
    try {
      const ctx = this.getContext();
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();

      osc.type = 'sawtooth';
      osc.frequency.setValueAtTime(440, ctx.currentTime);
      osc.frequency.linearRampToValueAtTime(880, ctx.currentTime + 0.8);

      gain.gain.setValueAtTime(0.1, ctx.currentTime);
      gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.8);

      osc.connect(gain);
      gain.connect(ctx.destination);

      osc.start();
      osc.stop(ctx.currentTime + 0.8);
      this.playRCSBurst(0.7);
    } catch {
      // Audio fallback
    }
  }
}
