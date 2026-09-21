/**
 * AresVoiceDirector.ts
 * Neuro-linked AI Tactical Voice Director (A.R.E.S. - Advanced Reconnaissance & ECLSS Specialist)
 * Uses Web Speech API with procedural Quindar audio cues and broadcast listeners for HUD waveforms.
 */

import { SoundDirector } from './SoundDirector';

export interface AresTransmission {
  sender: 'A.R.E.S.' | 'FLIGHT DIRECTOR' | 'MISSION CONTROL' | 'SYSTEM';
  callsign: string;
  message: string;
  priority: 'ROUTINE' | 'TACTICAL' | 'CRITICAL' | 'ACHIEVEMENT';
  timestamp?: string;
}

export type AresMessageCallback = (transmission: AresTransmission | null) => void;
export type AresWaveformCallback = (frequencyValues: number[]) => void;

export class AresVoiceDirector {
  private static instance: AresVoiceDirector;
  private messageListeners: AresMessageCallback[] = [];
  private waveformListeners: AresWaveformCallback[] = [];
  private isSpeaking: boolean = false;
  private waveformInterval: number | null = null;
  private currentVoice: SpeechSynthesisVoice | null = null;

  private constructor() {
    if (typeof window !== 'undefined' && 'speechSynthesis' in window) {
      window.speechSynthesis.onvoiceschanged = () => {
        this.selectIdealVoice();
      };
      this.selectIdealVoice();
    }
  }

  public static getInstance(): AresVoiceDirector {
    if (!this.instance) {
      this.instance = new AresVoiceDirector();
    }
    return this.instance;
  }

  private selectIdealVoice(): void {
    if (typeof window === 'undefined' || !('speechSynthesis' in window)) return;
    const voices = window.speechSynthesis.getVoices();
    // Prefer English robotic or deep clear voices like Google US English, Samantha, Daniel, or en-US
    const preferred = voices.find(
      (v) =>
        v.lang.startsWith('en') &&
        (v.name.includes('Google') || v.name.includes('Natural') || v.name.includes('David') || v.name.includes('Daniel') || v.name.includes('Alex'))
    );
    this.currentVoice = preferred || voices.find((v) => v.lang.startsWith('en')) || voices[0] || null;
  }

  public onMessage(callback: AresMessageCallback): () => void {
    this.messageListeners.push(callback);
    return () => {
      this.messageListeners = this.messageListeners.filter((cb) => cb !== callback);
    };
  }

  public onWaveform(callback: AresWaveformCallback): () => void {
    this.waveformListeners.push(callback);
    return () => {
      this.waveformListeners = this.waveformListeners.filter((cb) => cb !== callback);
    };
  }

  public transmit(transmission: AresTransmission): void {
    transmission.timestamp = new Date().toISOString().substring(11, 19);

    // Notify HUD UI
    this.messageListeners.forEach((cb) => cb(transmission));

    // Audio cue
    SoundDirector.playQuindarBeep(true);

    // Synthesize Speech
    if (typeof window !== 'undefined' && 'speechSynthesis' in window) {
      window.speechSynthesis.cancel();

      const utterance = new SpeechSynthesisUtterance(transmission.message);
      if (this.currentVoice) {
        utterance.voice = this.currentVoice;
      }
      utterance.pitch = transmission.sender === 'A.R.E.S.' ? 0.92 : 1.05;
      utterance.rate = 1.08;
      utterance.volume = 0.9;

      utterance.onstart = () => {
        this.isSpeaking = true;
        this.startWaveformSimulation();
      };

      utterance.onend = () => {
        this.isSpeaking = false;
        this.stopWaveformSimulation();
        SoundDirector.playQuindarBeep(false);
        // Clear message card after delay
        setTimeout(() => {
          if (!this.isSpeaking) {
            this.messageListeners.forEach((cb) => cb(null));
          }
        }, 3200);
      };

      utterance.onerror = () => {
        this.isSpeaking = false;
        this.stopWaveformSimulation();
      };

      window.speechSynthesis.speak(utterance);
    } else {
      // Fallback if SpeechSynthesis is unavailable
      this.startWaveformSimulation();
      setTimeout(() => {
        this.stopWaveformSimulation();
        this.messageListeners.forEach((cb) => cb(null));
      }, 4000);
    }
  }

  private startWaveformSimulation(): void {
    if (this.waveformInterval) clearInterval(this.waveformInterval);
    this.waveformInterval = window.setInterval(() => {
      const bars = 16;
      const values: number[] = [];
      for (let i = 0; i < bars; i++) {
        values.push(Math.random() * 0.8 + 0.2);
      }
      this.waveformListeners.forEach((cb) => cb(values));
    }, 80);
  }

  private stopWaveformSimulation(): void {
    if (this.waveformInterval) {
      clearInterval(this.waveformInterval);
      this.waveformInterval = null;
    }
    const zeroBars = new Array(16).fill(0.08);
    this.waveformListeners.forEach((cb) => cb(zeroBars));
  }
}
