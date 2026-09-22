using System;
using System.Collections;
using UnityEngine;
using AresResurgence.Player;
using AresResurgence.UI;

namespace AresResurgence.Story
{
    /// <summary>
    /// Master Story Director for NASA Junior Astronaut Odyssey:
    /// Phase 1: Recruitment & Zero-G Physics Training (Newton's Laws)
    /// Phase 2: Cape Canaveral Rocket Launch Cutscene (Apollo Liftoff & Ascent)
    /// Phase 3: Manual ISS Docking Mini-Game (6-DOF Navigation & Alignment)
    /// Phase 4: ISS Ingress & Cupola Observation Window (Earth View & Cosmic Symphony)
    /// </summary>
    public class NASAOdysseyDirector : MonoBehaviour
    {
        public static NASAOdysseyDirector Instance { get; private set; }

        public enum StoryPhase
        {
            ZeroGTraining,
            RocketLaunch,
            ISSDocking,
            CupolaEarthView
        }

        [Header("State")]
        public StoryPhase CurrentPhase = StoryPhase.ZeroGTraining;

        [Header("Zero-G Training Progress")]
        public int CurrentRingIndex = 0;
        public int TotalRings = 4;
        public int RingsCompleted = 0;

        [Header("Launch Cutscene Telemetry")]
        public float LaunchTimeElapsed = 0f;
        public float LaunchAltitudeKm = 0f;
        public float LaunchMachSpeed = 0f;
        public string LaunchStageText = "Stage 1: Main Engine Burn";

        [Header("ISS Docking Telemetry")]
        public float DockingDistanceMeters = 22.0f;
        public float DockingClosureRate = 0.0f;
        public float DockingAlignmentError = 4.5f;
        public string DockingStatusText = "APPROACH RADAR: ACTIVE";
        public bool IsDocked = false;

        [Header("Capcom Comms")]
        public string CurrentCapcomMessage = "";

        [Header("Audio Sources")]
        public AudioSource radioAudioSource;
        public AudioSource musicAudioSource;
        public AudioSource sfxAudioSource;

        [Header("Clips")]
        public AudioClip quindarBeepClip;
        public AudioClip launchAudioClip;
        public AudioClip ringChimeClip;
        public AudioClip dockingLatchClip;
        public AudioClip cupolaSymphonyClip;

        // Scene References managed by NASAMasterExperience
        public Transform trainingRig;
        public Transform launchPadRig;
        public Transform issRig;
        public Transform cupolaRig;
        public Transform rocketTransform;
        public Camera mainCamera;
        public ZeroGAstronautController astronautController;

        private void Awake()
        {
            Instance = this;

            if (radioAudioSource == null) radioAudioSource = gameObject.AddComponent<AudioSource>();
            if (musicAudioSource == null) musicAudioSource = gameObject.AddComponent<AudioSource>();
            if (sfxAudioSource == null) sfxAudioSource = gameObject.AddComponent<AudioSource>();

            radioAudioSource.spatialBlend = 0f;
            musicAudioSource.spatialBlend = 0f;
            sfxAudioSource.spatialBlend = 0f;

            LoadAudioClips();
        }

        private void LoadAudioClips()
        {
            quindarBeepClip = Resources.Load<AudioClip>("Audio/NASA_Quindar_Beep");
            launchAudioClip = Resources.Load<AudioClip>("Audio/NASA_Launch_Audio");
            ringChimeClip = Resources.Load<AudioClip>("Audio/ZeroG_Ring_Chime");
            dockingLatchClip = Resources.Load<AudioClip>("Audio/Docking_Latch_Lock");
            cupolaSymphonyClip = Resources.Load<AudioClip>("Audio/ISS_Cupola_Earth_Symphony");
        }

        private void Start()
        {
            StartCoroutine(BeginStorySequence());
        }

        private IEnumerator BeginStorySequence()
        {
            yield return new WaitForSeconds(0.6f);
            SetPhase(StoryPhase.ZeroGTraining);

            PlayQuindarBeep();
            SetCapcomMessage("Junior Astronaut, welcome to the Neutral Buoyancy & Zero-G Simulator. Test Newton's 1st Law: Press W to thrust forward. Notice inertia keeps you drifting until counter-thrust is applied.");
        }

        public void SetPhase(StoryPhase newPhase)
        {
            CurrentPhase = newPhase;

            if (trainingRig != null) trainingRig.gameObject.SetActive(newPhase == StoryPhase.ZeroGTraining);
            if (launchPadRig != null) launchPadRig.gameObject.SetActive(newPhase == StoryPhase.RocketLaunch);
            if (issRig != null) issRig.gameObject.SetActive(newPhase == StoryPhase.ISSDocking || newPhase == StoryPhase.CupolaEarthView);
            if (cupolaRig != null) cupolaRig.gameObject.SetActive(newPhase == StoryPhase.CupolaEarthView);

            switch (newPhase)
            {
                case StoryPhase.ZeroGTraining:
                    if (astronautController != null)
                    {
                        astronautController.enabled = true;
                        astronautController.controlsEnabled = true;
                        astronautController.transform.position = new Vector3(0f, 2f, -12f);
                        astronautController.transform.rotation = Quaternion.identity;
                        astronautController.StopAllVelocity();
                    }
                    break;

                case StoryPhase.RocketLaunch:
                    if (astronautController != null)
                    {
                        astronautController.controlsEnabled = false;
                    }
                    StartCoroutine(ExecuteRocketLaunchCutscene());
                    break;

                case StoryPhase.ISSDocking:
                    if (astronautController != null)
                    {
                        astronautController.enabled = true;
                        astronautController.controlsEnabled = true;
                        astronautController.transform.position = new Vector3(0f, 0f, -22f);
                        astronautController.transform.rotation = Quaternion.identity;
                        astronautController.StopAllVelocity();
                    }
                    StartCoroutine(ExecuteISSDockingPhase());
                    break;

                case StoryPhase.CupolaEarthView:
                    if (astronautController != null)
                    {
                        astronautController.controlsEnabled = true;
                        astronautController.transform.position = new Vector3(0f, 0.5f, -0.5f);
                        astronautController.StopAllVelocity();
                    }
                    StartCoroutine(ExecuteCupolaSequence());
                    break;
            }
        }

        public void OnWaypointRingPassed(int ringIndex)
        {
            if (ringIndex != CurrentRingIndex) return;

            RingsCompleted++;
            CurrentRingIndex++;

            if (ringChimeClip != null) sfxAudioSource.PlayOneShot(ringChimeClip, 0.75f);
            PlayQuindarBeep();

            switch (CurrentRingIndex)
            {
                case 1:
                    SetCapcomMessage("Ring 1 Cleared! Newton's 3rd Law in microgravity: Every action creates an opposite reaction. Use A and D to translate laterally.");
                    break;
                case 2:
                    SetCapcomMessage("Ring 2 Cleared! Vertical translation: Press Space to fire downward RCS jets to ascend, or Left Shift to descend.");
                    break;
                case 3:
                    SetCapcomMessage("Ring 3 Cleared! Final calibration: Align with Ring 4 and use X to damp your inertia to a complete halt.");
                    break;
                case 4:
                    SetCapcomMessage("ALL RINGS CLEARED! Flight Director certification achieved. All systems go for launch to the International Space Station!");
                    StartCoroutine(TransitionToLaunchAfterDelay(3.5f));
                    break;
            }
        }

        private IEnumerator TransitionToLaunchAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            SetPhase(StoryPhase.RocketLaunch);
        }

        private IEnumerator ExecuteRocketLaunchCutscene()
        {
            PlayQuindarBeep();
            SetCapcomMessage("Flight Director: 'T-minus 10 seconds. We have an all-green board. Good luck Junior Astronaut, see you on orbit.'");

            if (launchAudioClip != null)
            {
                radioAudioSource.clip = launchAudioClip;
                radioAudioSource.volume = 0.95f;
                radioAudioSource.Play();
            }

            LaunchTimeElapsed = 0f;
            Vector3 rocketStartPos = rocketTransform != null ? rocketTransform.position : Vector3.zero;

            float launchDuration = 14.5f;
            while (LaunchTimeElapsed < launchDuration)
            {
                LaunchTimeElapsed += Time.deltaTime;
                float progress = LaunchTimeElapsed / launchDuration;

                // Move rocket upward with acceleration
                if (rocketTransform != null)
                {
                    float yOffset = Mathf.Pow(LaunchTimeElapsed, 2.2f) * 1.8f;
                    rocketTransform.position = rocketStartPos + new Vector3(0f, yOffset, 0f);
                }

                // Telemetry progression
                LaunchAltitudeKm = Mathf.Pow(progress, 2f) * 160f;
                LaunchMachSpeed = progress * 24.5f;

                if (LaunchTimeElapsed > 8f && LaunchTimeElapsed < 11f)
                {
                    LaunchStageText = "Stage 1 Separation Confirmed. Stage 2 Vacuum Engine Fired.";
                }
                else if (LaunchTimeElapsed >= 11f)
                {
                    LaunchStageText = "Orbital Insertion Established: 400x400 km Circular Orbit.";
                }

                yield return null;
            }

            PlayQuindarBeep();
            SetCapcomMessage("Houston CAPCOM: 'Spacecraft in orbital flight. Station in visual radar range. Prepare for manual docking.'");
            yield return new WaitForSeconds(2.0f);

            SetPhase(StoryPhase.ISSDocking);
        }

        private IEnumerator ExecuteISSDockingPhase()
        {
            PlayQuindarBeep();
            SetCapcomMessage("Approach guidance online. Use 6-DOF RCS thrusters to align with ISS PMA-2 docking ring. Maintain closure rate under 0.35 m/s.");

            DockingDistanceMeters = 22.0f;
            IsDocked = false;

            while (!IsDocked && CurrentPhase == StoryPhase.ISSDocking)
            {
                if (astronautController != null)
                {
                    // Calculate distance to docking port at (0, 0, 0)
                    Vector3 toPort = Vector3.zero - astronautController.transform.position;
                    DockingDistanceMeters = toPort.magnitude;

                    // Closure rate = forward velocity toward port
                    DockingClosureRate = Mathf.Max(0f, Vector3.Dot(astronautController.currentVelocity, toPort.normalized));

                    // Alignment error based on forward facing
                    DockingAlignmentError = Vector3.Angle(astronautController.transform.forward, toPort.normalized);

                    if (DockingDistanceMeters < 0.65f)
                    {
                        if (DockingClosureRate <= 0.45f && DockingAlignmentError < 25f)
                        {
                            IsDocked = true;
                            astronautController.StopAllVelocity();
                            astronautController.controlsEnabled = false;

                            if (dockingLatchClip != null) sfxAudioSource.PlayOneShot(dockingLatchClip, 0.95f);
                            PlayQuindarBeep();
                            SetCapcomMessage("CAPTURE CONFIRMED! Hard latches locked! PMA-2 pressurized. Welcome aboard the International Space Station, Junior Astronaut!");
                            yield return new WaitForSeconds(3.5f);
                            SetPhase(StoryPhase.CupolaEarthView);
                            yield break;
                        }
                        else
                        {
                            DockingStatusText = "WARNING: EXCESSIVE SPEED OR MISALIGNMENT! BACK OFF!";
                        }
                    }
                    else
                    {
                        DockingStatusText = DockingClosureRate < 0.35f ? "APPROACH RATE: SAFE" : "CAUTION: REDUCE CLOSURE RATE";
                    }
                }
                yield return null;
            }
        }

        private IEnumerator ExecuteCupolaSequence()
        {
            PlayQuindarBeep();
            SetCapcomMessage("Flight Director: 'Junior Astronaut, you have completed training, launch, and manual docking with flying colors. Behold planet Earth.'");

            if (cupolaSymphonyClip != null)
            {
                musicAudioSource.clip = cupolaSymphonyClip;
                musicAudioSource.loop = true;
                musicAudioSource.volume = 0.85f;
                musicAudioSource.Play();
            }

            yield return new WaitForSeconds(6.0f);
            SetCapcomMessage("Expedition 1: Gaze out the 7 panoramic windows of the Cupola. Move your mouse to explore the view of our living world.");
        }

        private void PlayQuindarBeep()
        {
            if (quindarBeepClip != null && radioAudioSource != null)
            {
                radioAudioSource.PlayOneShot(quindarBeepClip, 0.7f);
            }
        }

        public void SetCapcomMessage(string msg)
        {
            CurrentCapcomMessage = msg;
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                Application.ExternalCall("speakJarvis", msg);
            }
            catch {}
#endif
        }
    }
}
