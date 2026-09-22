using UnityEngine;
using AresResurgence.Story;

namespace AresResurgence.Interaction
{
    /// <summary>
    /// Holographic Navigation Waypoint Ring used in NASA Zero-G Training.
    /// Detects when the candidate astronaut flies through the aperture,
    /// verifying mastery of 6-DOF thrusters and inertia.
    /// </summary>
    public class ZeroGWaypointRing : MonoBehaviour
    {
        public int ringIndex = 0;
        public bool isPassed = false;
        private Renderer ringRenderer;
        private Material ringMat;

        private void Awake()
        {
            ringRenderer = GetComponent<Renderer>();
            if (ringRenderer != null)
            {
                ringMat = ringRenderer.material;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isPassed) return;

            // Check if player collided
            if (other.GetComponentInParent<AresResurgence.Player.ZeroGAstronautController>() != null ||
                other.CompareTag("Player") || other.name.Contains("Astronaut"))
            {
                if (NASAOdysseyDirector.Instance != null && NASAOdysseyDirector.Instance.CurrentRingIndex == ringIndex)
                {
                    isPassed = true;
                    if (ringMat != null)
                    {
                        ringMat.color = new Color(0.2f, 1.0f, 0.4f, 0.9f); // Green passed
                    }
                    NASAOdysseyDirector.Instance.OnWaypointRingPassed(ringIndex);
                }
            }
        }

        public void SetHighlight(bool active)
        {
            if (ringMat == null) return;
            if (isPassed)
            {
                ringMat.color = new Color(0.2f, 1.0f, 0.4f, 0.9f);
            }
            else if (active)
            {
                ringMat.color = new Color(0.1f, 0.85f, 1.0f, 0.95f); // Glowing cyan for current target
            }
            else
            {
                ringMat.color = new Color(0.3f, 0.4f, 0.5f, 0.45f); // Dim inactive
            }
        }
    }
}
