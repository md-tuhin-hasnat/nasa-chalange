/**
 * TacticalFpsHUD.ts
 * Mainstream AAA FPS HUD (Marvel's Avengers / PUBG / Free Fire aesthetic)
 * Rendered directly in WebGL via Babylon.js AdvancedDynamicTexture.
 */

import { Scene } from '@babylonjs/core';
import {
  AdvancedDynamicTexture,
  Rectangle,
  TextBlock,
  StackPanel,
  Control,
  Line,
} from '@babylonjs/gui';
import { AresTransmission, AresVoiceDirector } from '../engine/AresVoiceDirector';
import { SoundDirector } from '../engine/SoundDirector';

export class TacticalFpsHUD {
  public ui: AdvancedDynamicTexture;
  private compassText: TextBlock;
  private healthBar: Rectangle;
  private armorBar: Rectangle;
  private o2Bar: Rectangle;
  private xpText: TextBlock;
  private tickerStack: StackPanel;
  private aresContainer: Rectangle;
  private aresSpeakerText: TextBlock;
  private aresMessageText: TextBlock;
  private waveformBars: Rectangle[] = [];
  private loadoutSlots: Rectangle[] = [];
  public activeSlot = 0;
  private currentXP = 0;

  // Cinematic Letterbox Bars
  private letterboxTop: Rectangle;
  private letterboxBottom: Rectangle;

  constructor(scene: Scene) {
    this.ui = AdvancedDynamicTexture.CreateFullscreenUI('TacticalFPS_UI', true, scene);
    this.ui.idealWidth = 1920;
    this.ui.idealHeight = 1080;

    // 1. TOP COMPASS RIBBON (PUBG / Avengers Style)
    const compassContainer = new Rectangle('CompassContainer');
    compassContainer.width = '640px';
    compassContainer.height = '48px';
    compassContainer.top = '16px';
    compassContainer.verticalAlignment = Control.VERTICAL_ALIGNMENT_TOP;
    compassContainer.horizontalAlignment = Control.HORIZONTAL_ALIGNMENT_CENTER;
    compassContainer.background = 'rgba(4, 10, 20, 0.75)';
    compassContainer.color = 'rgba(0, 240, 255, 0.4)';
    compassContainer.thickness = 1;
    compassContainer.cornerRadius = 4;
    this.ui.addControl(compassContainer);

    // Center indicator tick
    const centerTick = new Rectangle('CenterTick');
    centerTick.width = '2px';
    centerTick.height = '18px';
    centerTick.verticalAlignment = Control.VERTICAL_ALIGNMENT_BOTTOM;
    centerTick.background = '#00f0ff';
    centerTick.thickness = 0;
    compassContainer.addControl(centerTick);

    this.compassText = new TextBlock('CompassText');
    this.compassText.text = '··· 345 ··· N ··· 015 ··· 030 ··· NE ··· 060 ··· E ···';
    this.compassText.color = '#e0f7fa';
    this.compassText.fontSize = 14;
    this.compassText.fontFamily = 'Courier New, monospace';
    this.compassText.fontWeight = 'bold';
    compassContainer.addControl(this.compassText);

    // 2. TACTICAL RADAR / MINIMAP (Top-Left)
    const radarContainer = new Rectangle('RadarContainer');
    radarContainer.width = '160px';
    radarContainer.height = '160px';
    radarContainer.left = '24px';
    radarContainer.top = '24px';
    radarContainer.horizontalAlignment = Control.HORIZONTAL_ALIGNMENT_LEFT;
    radarContainer.verticalAlignment = Control.VERTICAL_ALIGNMENT_TOP;
    radarContainer.background = 'rgba(2, 8, 16, 0.8)';
    radarContainer.color = 'rgba(0, 240, 255, 0.6)';
    radarContainer.thickness = 1.5;
    radarContainer.cornerRadius = 80;
    this.ui.addControl(radarContainer);

    const radarCrossH = new Rectangle('RadarCrossH');
    radarCrossH.width = '140px';
    radarCrossH.height = '1px';
    radarCrossH.background = 'rgba(0, 240, 255, 0.2)';
    radarCrossH.thickness = 0;
    radarContainer.addControl(radarCrossH);

    const radarCrossV = new Rectangle('RadarCrossV');
    radarCrossV.width = '1px';
    radarCrossV.height = '140px';
    radarCrossV.background = 'rgba(0, 240, 255, 0.2)';
    radarCrossV.thickness = 0;
    radarContainer.addControl(radarCrossV);

    const playerBlip = new Rectangle('PlayerBlip');
    playerBlip.width = '8px';
    playerBlip.height = '8px';
    playerBlip.background = '#00f0ff';
    playerBlip.cornerRadius = 4;
    playerBlip.thickness = 0;
    radarContainer.addControl(playerBlip);

    const radarLabel = new TextBlock('RadarLabel');
    radarLabel.text = 'NAV-SAT 01 // ORBITAL SCAN';
    radarLabel.color = 'rgba(0, 240, 255, 0.7)';
    radarLabel.fontSize = 9;
    radarLabel.fontFamily = 'Courier New, monospace';
    radarLabel.top = '65px';
    radarContainer.addControl(radarLabel);

    // 3. PUBG-STYLE NOTIFICATION & KILLFEED TICKER (Top-Right)
    const tickerContainer = new Rectangle('TickerContainer');
    tickerContainer.width = '380px';
    tickerContainer.height = '200px';
    tickerContainer.right = '24px';
    tickerContainer.top = '24px';
    tickerContainer.horizontalAlignment = Control.HORIZONTAL_ALIGNMENT_RIGHT;
    tickerContainer.verticalAlignment = Control.VERTICAL_ALIGNMENT_TOP;
    tickerContainer.thickness = 0;
    this.ui.addControl(tickerContainer);

    this.tickerStack = new StackPanel('TickerStack');
    this.tickerStack.isVertical = true;
    tickerContainer.addControl(this.tickerStack);

    // XP Score Card
    const xpContainer = new Rectangle('XPContainer');
    xpContainer.width = '240px';
    xpContainer.height = '40px';
    xpContainer.right = '24px';
    xpContainer.top = '230px';
    xpContainer.horizontalAlignment = Control.HORIZONTAL_ALIGNMENT_RIGHT;
    xpContainer.verticalAlignment = Control.VERTICAL_ALIGNMENT_TOP;
    xpContainer.background = 'rgba(0, 20, 40, 0.85)';
    xpContainer.color = '#ffb300';
    xpContainer.thickness = 1.5;
    xpContainer.cornerRadius = 4;
    this.ui.addControl(xpContainer);

    this.xpText = new TextBlock('XPText');
    this.xpText.text = 'SCORE: 000000 PTS';
    this.xpText.color = '#ffd54f';
    this.xpText.fontSize = 15;
    this.xpText.fontFamily = 'Courier New, monospace';
    this.xpText.fontWeight = 'bold';
    xpContainer.addControl(this.xpText);

    // 4. PLAYER TACTICAL VITALS (Bottom-Left)
    const vitalsContainer = new Rectangle('VitalsContainer');
    vitalsContainer.width = '380px';
    vitalsContainer.height = '140px';
    vitalsContainer.left = '24px';
    vitalsContainer.bottom = '24px';
    vitalsContainer.horizontalAlignment = Control.HORIZONTAL_ALIGNMENT_LEFT;
    vitalsContainer.verticalAlignment = Control.VERTICAL_ALIGNMENT_BOTTOM;
    vitalsContainer.background = 'rgba(4, 10, 20, 0.85)';
    vitalsContainer.color = 'rgba(0, 240, 255, 0.5)';
    vitalsContainer.thickness = 1.5;
    vitalsContainer.cornerRadius = 6;
    this.ui.addControl(vitalsContainer);

    // Operative Info Header
    const opBadge = new TextBlock('OpBadge');
    opBadge.text = 'CADET COOPER // OPERATIVE CLASS [EVA-0]';
    opBadge.color = '#00f0ff';
    opBadge.fontSize = 12;
    opBadge.fontFamily = 'Courier New, monospace';
    opBadge.fontWeight = 'bold';
    opBadge.top = '-48px';
    opBadge.left = '16px';
    opBadge.textHorizontalAlignment = Control.HORIZONTAL_ALIGNMENT_LEFT;
    vitalsContainer.addControl(opBadge);

    // Shield/Armor Track
    const armorTrack = new Rectangle('ArmorTrack');
    armorTrack.width = '340px';
    armorTrack.height = '10px';
    armorTrack.top = '-20px';
    armorTrack.background = 'rgba(0, 60, 100, 0.4)';
    armorTrack.thickness = 0;
    armorTrack.cornerRadius = 2;
    vitalsContainer.addControl(armorTrack);

    this.armorBar = new Rectangle('ArmorBar');
    this.armorBar.width = '340px';
    this.armorBar.height = '10px';
    this.armorBar.horizontalAlignment = Control.HORIZONTAL_ALIGNMENT_LEFT;
    this.armorBar.background = '#00e5ff';
    this.armorBar.thickness = 0;
    this.armorBar.cornerRadius = 2;
    armorTrack.addControl(this.armorBar);

    // Health Track
    const hpTrack = new Rectangle('HPTrack');
    hpTrack.width = '340px';
    hpTrack.height = '14px';
    hpTrack.top = '4px';
    hpTrack.background = 'rgba(20, 60, 20, 0.4)';
    hpTrack.thickness = 0;
    hpTrack.cornerRadius = 2;
    vitalsContainer.addControl(hpTrack);

    this.healthBar = new Rectangle('HealthBar');
    this.healthBar.width = '340px';
    this.healthBar.height = '14px';
    this.healthBar.horizontalAlignment = Control.HORIZONTAL_ALIGNMENT_LEFT;
    this.healthBar.background = '#00e676';
    this.healthBar.thickness = 0;
    this.healthBar.cornerRadius = 2;
    hpTrack.addControl(this.healthBar);

    // O2 Reserve Track
    const o2Track = new Rectangle('O2Track');
    o2Track.width = '340px';
    o2Track.height = '8px';
    o2Track.top = '26px';
    o2Track.background = 'rgba(20, 40, 80, 0.4)';
    o2Track.thickness = 0;
    o2Track.cornerRadius = 2;
    vitalsContainer.addControl(o2Track);

    this.o2Bar = new Rectangle('O2Bar');
    this.o2Bar.width = '340px';
    this.o2Bar.height = '8px';
    this.o2Bar.horizontalAlignment = Control.HORIZONTAL_ALIGNMENT_LEFT;
    this.o2Bar.background = '#29b6f6';
    this.o2Bar.thickness = 0;
    this.o2Bar.cornerRadius = 2;
    o2Track.addControl(this.o2Bar);

    const vitalsLegend = new TextBlock('VitalsLegend');
    vitalsLegend.text = 'SHIELD 100%   |   INTEGRITY 100%   |   O2 RESERVE 100%';
    vitalsLegend.color = 'rgba(255, 255, 255, 0.7)';
    vitalsLegend.fontSize = 9;
    vitalsLegend.fontFamily = 'Courier New, monospace';
    vitalsLegend.top = '48px';
    vitalsContainer.addControl(vitalsLegend);

    // 5. 4-SLOT TACTICAL LOADOUT DOCK (Bottom-Right)
    const loadoutContainer = new Rectangle('LoadoutContainer');
    loadoutContainer.width = '420px';
    loadoutContainer.height = '90px';
    loadoutContainer.right = '24px';
    loadoutContainer.bottom = '24px';
    loadoutContainer.horizontalAlignment = Control.HORIZONTAL_ALIGNMENT_RIGHT;
    loadoutContainer.verticalAlignment = Control.VERTICAL_ALIGNMENT_BOTTOM;
    loadoutContainer.background = 'rgba(4, 10, 20, 0.85)';
    loadoutContainer.color = 'rgba(0, 240, 255, 0.4)';
    loadoutContainer.thickness = 1.5;
    loadoutContainer.cornerRadius = 6;
    this.ui.addControl(loadoutContainer);

    const slotNames = [
      { key: '1', name: 'NANITE WELDER', icon: '⚡' },
      { key: '2', name: 'CRYO EXTING.', icon: '❄' },
      { key: '3', name: 'RCS THRUSTER', icon: '🚀' },
      { key: '4', name: 'O2 SIPHON', icon: '🫁' },
    ];

    slotNames.forEach((item, idx) => {
      const slot = new Rectangle(`Slot_${idx}`);
      slot.width = '90px';
      slot.height = '72px';
      slot.left = `${-150 + idx * 100}px`;
      slot.background = idx === 0 ? 'rgba(0, 229, 255, 0.25)' : 'rgba(10, 20, 35, 0.6)';
      slot.color = idx === 0 ? '#00f0ff' : 'rgba(0, 240, 255, 0.3)';
      slot.thickness = idx === 0 ? 2 : 1;
      slot.cornerRadius = 4;
      loadoutContainer.addControl(slot);
      this.loadoutSlots.push(slot);

      const keyLabel = new TextBlock(`SlotKey_${idx}`);
      keyLabel.text = `[${item.key}]`;
      keyLabel.color = idx === 0 ? '#00f0ff' : 'rgba(255, 255, 255, 0.6)';
      keyLabel.fontSize = 10;
      keyLabel.fontFamily = 'Courier New, monospace';
      keyLabel.top = '-22px';
      keyLabel.left = '-26px';
      slot.addControl(keyLabel);

      const iconLabel = new TextBlock(`SlotIcon_${idx}`);
      iconLabel.text = item.icon;
      iconLabel.fontSize = 20;
      iconLabel.top = '-4px';
      slot.addControl(iconLabel);

      const nameLabel = new TextBlock(`SlotName_${idx}`);
      nameLabel.text = item.name;
      nameLabel.color = '#e0f7fa';
      nameLabel.fontSize = 8;
      nameLabel.fontFamily = 'Courier New, monospace';
      nameLabel.fontWeight = 'bold';
      nameLabel.top = '22px';
      slot.addControl(nameLabel);
    });

    // 6. CENTER TACTICAL RETICLE / CROSSHAIR
    const reticleContainer = new Rectangle('ReticleContainer');
    reticleContainer.width = '80px';
    reticleContainer.height = '80px';
    reticleContainer.thickness = 0;
    this.ui.addControl(reticleContainer);

    const centerDot = new Rectangle('CenterDot');
    centerDot.width = '4px';
    centerDot.height = '4px';
    centerDot.background = '#00f0ff';
    centerDot.thickness = 0;
    centerDot.cornerRadius = 2;
    reticleContainer.addControl(centerDot);

    // 4 Corner brackets
    const bracketOffsets = [
      { l: -24, t: 0, w: 10, h: 2 },
      { l: 24, t: 0, w: 10, h: 2 },
      { l: 0, t: -24, w: 2, h: 10 },
      { l: 0, t: 24, w: 2, h: 10 },
    ];
    bracketOffsets.forEach((b, i) => {
      const bracket = new Rectangle(`ReticleB_${i}`);
      bracket.width = `${b.w}px`;
      bracket.height = `${b.h}px`;
      bracket.left = `${b.l}px`;
      bracket.top = `${b.t}px`;
      bracket.background = 'rgba(0, 240, 255, 0.7)';
      bracket.thickness = 0;
      reticleContainer.addControl(bracket);
    });

    // 7. A.R.E.S. NEURAL LINK RADIO TRANSMISSION CARD (Left-Center)
    this.aresContainer = new Rectangle('AresCard');
    this.aresContainer.width = '480px';
    this.aresContainer.height = '140px';
    this.aresContainer.left = '24px';
    this.aresContainer.top = '0px';
    this.aresContainer.horizontalAlignment = Control.HORIZONTAL_ALIGNMENT_LEFT;
    this.aresContainer.verticalAlignment = Control.VERTICAL_ALIGNMENT_CENTER;
    this.aresContainer.background = 'rgba(2, 8, 18, 0.92)';
    this.aresContainer.color = '#00f0ff';
    this.aresContainer.thickness = 1.5;
    this.aresContainer.cornerRadius = 6;
    this.aresContainer.isVisible = false;
    this.ui.addControl(this.aresContainer);

    // Frequency readout
    const aresHeader = new TextBlock('AresHeader');
    aresHeader.text = 'COMMS FREQ: 142.80 MHz // SECURE ENCRYPTION';
    aresHeader.color = 'rgba(0, 240, 255, 0.7)';
    aresHeader.fontSize = 10;
    aresHeader.fontFamily = 'Courier New, monospace';
    aresHeader.top = '-48px';
    aresHeader.left = '16px';
    aresHeader.textHorizontalAlignment = Control.HORIZONTAL_ALIGNMENT_LEFT;
    this.aresContainer.addControl(aresHeader);

    this.aresSpeakerText = new TextBlock('AresSpeaker');
    this.aresSpeakerText.text = 'A.R.E.S. [NEURAL LINK]';
    this.aresSpeakerText.color = '#00f0ff';
    this.aresSpeakerText.fontSize = 14;
    this.aresSpeakerText.fontWeight = 'bold';
    this.aresSpeakerText.fontFamily = 'Courier New, monospace';
    this.aresSpeakerText.top = '-26px';
    this.aresSpeakerText.left = '16px';
    this.aresSpeakerText.textHorizontalAlignment = Control.HORIZONTAL_ALIGNMENT_LEFT;
    this.aresContainer.addControl(this.aresSpeakerText);

    this.aresMessageText = new TextBlock('AresMsg');
    this.aresMessageText.text = 'Awaiting orders...';
    this.aresMessageText.color = '#ffffff';
    this.aresMessageText.fontSize = 13;
    this.aresMessageText.textWrapping = true;
    this.aresMessageText.fontFamily = 'Courier New, monospace';
    this.aresMessageText.top = '14px';
    this.aresMessageText.left = '16px';
    this.aresMessageText.width = '440px';
    this.aresMessageText.textHorizontalAlignment = Control.HORIZONTAL_ALIGNMENT_LEFT;
    this.aresContainer.addControl(this.aresMessageText);

    // Audio Equalizer Waveform Bars (16 bars)
    const wavePanel = new Rectangle('WavePanel');
    wavePanel.width = '140px';
    wavePanel.height = '24px';
    wavePanel.top = '-26px';
    wavePanel.right = '16px';
    wavePanel.horizontalAlignment = Control.HORIZONTAL_ALIGNMENT_RIGHT;
    wavePanel.thickness = 0;
    this.aresContainer.addControl(wavePanel);

    for (let i = 0; i < 16; i++) {
      const bar = new Rectangle(`WaveBar_${i}`);
      bar.width = '5px';
      bar.height = '4px';
      bar.left = `${-60 + i * 8}px`;
      bar.verticalAlignment = Control.VERTICAL_ALIGNMENT_BOTTOM;
      bar.background = '#00f0ff';
      bar.thickness = 0;
      wavePanel.addControl(bar);
      this.waveformBars.push(bar);
    }

    // 8. CINEMATIC LETTERBOX BARS (2.39:1 Anamorphic for cutscenes)
    this.letterboxTop = new Rectangle('LetterboxTop');
    this.letterboxTop.width = '100%';
    this.letterboxTop.height = '0px';
    this.letterboxTop.verticalAlignment = Control.VERTICAL_ALIGNMENT_TOP;
    this.letterboxTop.background = '#000000';
    this.letterboxTop.thickness = 0;
    this.ui.addControl(this.letterboxTop);

    this.letterboxBottom = new Rectangle('LetterboxBottom');
    this.letterboxBottom.width = '100%';
    this.letterboxBottom.height = '0px';
    this.letterboxBottom.verticalAlignment = Control.VERTICAL_ALIGNMENT_BOTTOM;
    this.letterboxBottom.background = '#000000';
    this.letterboxBottom.thickness = 0;
    this.ui.addControl(this.letterboxBottom);

    // Connect A.R.E.S. voice callbacks
    const voice = AresVoiceDirector.getInstance();
    voice.onMessage((msg) => this.handleAresTransmission(msg));
    voice.onWaveform((values) => this.handleWaveformUpdate(values));
  }

  public updateCompass(angleDegrees: number): void {
    const norm = Math.round(((angleDegrees % 360) + 360) % 360);
    this.compassText.text = `··· ${norm.toString().padStart(3, '0')}° ··· N ··· 090° ··· E ··· 180° ··· S ··· 270° ··· W ···`;
  }

  public addTickerEvent(actor: string, action: string, xp = 0): void {
    const item = new Rectangle(`TickerItem_${Date.now()}`);
    item.width = '360px';
    item.height = '28px';
    item.background = 'rgba(2, 12, 28, 0.85)';
    item.color = xp > 0 ? '#ffb300' : '#00f0ff';
    item.thickness = 1;
    item.cornerRadius = 3;

    const label = new TextBlock();
    label.text = `[${actor}] ${action}${xp > 0 ? ` (+${xp} XP)` : ''}`;
    label.color = xp > 0 ? '#ffd54f' : '#80deea';
    label.fontSize = 11;
    label.fontFamily = 'Courier New, monospace';
    label.fontWeight = 'bold';
    item.addControl(label);

    this.tickerStack.addControl(item);
    if (xp > 0) {
      this.currentXP += xp;
      this.xpText.text = `SCORE: ${this.currentXP.toString().padStart(6, '0')} PTS`;
      SoundDirector.playSuccessChime();
    }

    setTimeout(() => {
      this.tickerStack.removeControl(item);
      item.dispose();
    }, 4500);
  }

  public selectLoadoutSlot(slotIndex: number): void {
    this.activeSlot = slotIndex;
    this.loadoutSlots.forEach((slot, i) => {
      if (i === slotIndex) {
        slot.background = 'rgba(0, 229, 255, 0.3)';
        slot.color = '#00f0ff';
        slot.thickness = 2;
      } else {
        slot.background = 'rgba(10, 20, 35, 0.6)';
        slot.color = 'rgba(0, 240, 255, 0.3)';
        slot.thickness = 1;
      }
    });
    SoundDirector.playTacticalClick();
  }

  private handleAresTransmission(msg: AresTransmission | null): void {
    if (!msg) {
      this.aresContainer.isVisible = false;
      return;
    }
    this.aresContainer.isVisible = true;
    this.aresSpeakerText.text = `${msg.sender} [${msg.priority}]`;
    this.aresMessageText.text = msg.message;
  }

  private handleWaveformUpdate(values: number[]): void {
    values.forEach((v, i) => {
      if (this.waveformBars[i]) {
        this.waveformBars[i].height = `${Math.max(4, Math.min(24, Math.round(v * 24)))}px`;
      }
    });
  }

  public setCinematicLetterbox(active: boolean): void {
    this.letterboxTop.height = active ? '110px' : '0px';
    this.letterboxBottom.height = active ? '110px' : '0px';
  }
}
