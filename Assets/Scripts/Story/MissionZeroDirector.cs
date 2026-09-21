using System.Collections;
using UnityEngine;
using AresResurgence.Audio;
using AresResurgence.Interaction;
using AresResurgence.UI;

namespace AresResurgence.Story
{
    /// <summary>
    /// Narrative director for Mission 0: The Jezero Anomaly.
    /// Manages the Hollywood-style pacing, radio dialogue transmissions,
    /// objective tracking, and scripted events.
    /// </summary>
    public class MissionZeroDirector : MonoBehaviour
    {
        public static MissionZeroDirector Instance { get; private set; }

        public enum MissionStage
        {
            IntroPowerFailure = 0,
            AuxiliaryPowerRestored = 1,
            AirlockCycled = 2,
            BaseStationReached = 3,
            PerseveranceSiteReached = 4,
            AnomalyDiscovered = 5,
            MissionComplete = 6
        }

        [Header("Mission State")]
        [SerializeField] private MissionStage currentStage = MissionStage.IntroPowerFailure;

        [Header("Waypoints & Target Locations")]
        public Transform auxiliaryConsoleTarget;
        public Transform airlockConsoleTarget;
        public Transform baseStationTarget;
        public Transform perseveranceRoverTarget;
        public Transform anomalySampleTarget;

        [Header("Interactive Consoles")]
        public ConsoleInteractable auxiliaryConsole;
        public ConsoleInteractable airlockConsole;
        public ConsoleInteractable baseStationConsole;
        public ConsoleInteractable anomalyScanPoint;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            StartCoroutine(MissionIntroSequence());
        }

        private IEnumerator MissionIntroSequence()
        {
            yield return new WaitForSeconds(1.0f);

            if (HelmetHUD.Instance != null)
            {
                HelmetHUD.Instance.ShowTransmission("ELYSIUM AI", "CRITICAL ALERT: Primary solar array damaged by nocturnal dust tempest. Life support operating on emergency cell.", 6.0f);
                HelmetHUD.Instance.SetObjective("RESTORE AUXILIARY LIFE SUPPORT AT WALL TERMINAL", auxiliaryConsoleTarget);
            }

            // Hook console events
            if (auxiliaryConsole != null)
            {
                auxiliaryConsole.onInteracted.AddListener(OnAuxiliaryRestored);
            }
            if (airlockConsole != null)
            {
                airlockConsole.onInteracted.AddListener(OnAirlockCycled);
            }
            if (baseStationConsole != null)
            {
                baseStationConsole.onInteracted.AddListener(OnBaseStationDiagnosed);
            }
            if (anomalyScanPoint != null)
            {
                anomalyScanPoint.onInteracted.AddListener(OnAnomalyScanned);
            }
        }

        public void OnAuxiliaryRestored()
        {
            if (currentStage != MissionStage.IntroPowerFailure) return;
            currentStage = MissionStage.AuxiliaryPowerRestored;

            StartCoroutine(AuxiliaryRestoredSequence());
        }

        private IEnumerator AuxiliaryRestoredSequence()
        {
            yield return new WaitForSeconds(0.8f);

            if (HelmetHUD.Instance != null)
            {
                HelmetHUD.Instance.ShowTransmission("ELYSIUM AI", "Auxiliary reactor online. Life support stabilized at 98%. All surface antenna links dark. EVA required.", 6.0f);
                HelmetHUD.Instance.SetObjective("CYCLE AIRLOCK & STEP ONTO MARTIAN SURFACE", airlockConsoleTarget);
            }
        }

        public void OnAirlockCycled()
        {
            if (currentStage != MissionStage.AuxiliaryPowerRestored) return;
            currentStage = MissionStage.AirlockCycled;

            StartCoroutine(AirlockCycleSequence());
        }

        private IEnumerator AirlockCycleSequence()
        {
            if (CinematicAudioDirector.Instance != null)
            {
                CinematicAudioDirector.Instance.PlayAirlockCycle();
            }

            yield return new WaitForSeconds(1.2f);

            if (HelmetHUD.Instance != null)
            {
                HelmetHUD.Instance.ShowTransmission("MISSION CONTROL (HOUSTON)", "[HEAVY STATIC] ...Elysium Base, this is Houston Flight... geomagnetic pulse detected across Jezero... do you copy?...", 6.5f);
                HelmetHUD.Instance.SetObjective("NAVIGATE THROUGH DUST STORM TO BASE STATION COMMS DISH", baseStationTarget);
            }
        }

        public void OnBaseStationDiagnosed()
        {
            if (currentStage != MissionStage.AirlockCycled) return;
            currentStage = MissionStage.BaseStationReached;

            StartCoroutine(BaseStationSequence());
        }

        private IEnumerator BaseStationSequence()
        {
            yield return new WaitForSeconds(0.5f);

            if (HelmetHUD.Instance != null)
            {
                HelmetHUD.Instance.ShowTransmission("CMDR. ALEX RAY", "Houston, antenna mechanics are intact. But the relay tripped from a massive electromagnetic surge originating from the Perseverance Rover survey site.", 7.0f);
            }

            yield return new WaitForSeconds(7.2f);

            if (HelmetHUD.Instance != null)
            {
                HelmetHUD.Instance.ShowTransmission("ELYSIUM AI", "Seismic telemetry localized: Ground fissure detected at coordinates 18.38°N 77.58°E. Rover telemetry transmitting anomalous frequency.", 6.5f);
                HelmetHUD.Instance.SetObjective("TREK TO PERSEVERANCE ROVER & INVESTIGATE FISSURE", perseveranceRoverTarget);
            }
        }

        public void OnAnomalyScanned()
        {
            if (currentStage != MissionStage.BaseStationReached && currentStage != MissionStage.PerseveranceSiteReached) return;
            currentStage = MissionStage.AnomalyDiscovered;

            StartCoroutine(AnomalyDiscoveredSequence());
        }

        private IEnumerator AnomalyDiscoveredSequence()
        {
            if (CinematicAudioDirector.Instance != null)
            {
                CinematicAudioDirector.Instance.PlayAnomalyDiscovery();
            }

            if (HelmetHUD.Instance != null)
            {
                HelmetHUD.Instance.ShowTransmission("CMDR. ALEX RAY", "Houston... this isn't impact debris. The drill trench split open. There are crystalline structures pulsing inside the regolith...", 8.0f);
            }

            yield return new WaitForSeconds(8.2f);

            if (HelmetHUD.Instance != null)
            {
                HelmetHUD.Instance.ShowTransmission("HELMET SCANNER", "ALERT: Bio-resonant signature confirmed. Signal harmonic matches no known geological taxonomy. Mission 0 Accomplished.", 8.0f);
                HelmetHUD.Instance.SetObjective("MISSION 0 ACCOMPLISHED: TELEMETRY SECURED — PREPARE FOR MISSION 1", null);
            }

            currentStage = MissionStage.MissionComplete;
        }
    }
}
