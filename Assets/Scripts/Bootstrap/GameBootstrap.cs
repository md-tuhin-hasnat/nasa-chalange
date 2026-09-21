using UnityEngine;
using AresResurgence.Environment;

namespace AresResurgence.Bootstrap
{
    /// <summary>
    /// Automatic game runtime bootstrapper.
    /// Ensures that upon entering Play Mode in any scene, the complete
    /// Hollywood Martian Mission 0 world and all systems are initialized.
    /// </summary>
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeGameOnPlay()
        {
            if (Object.FindAnyObjectByType<ProceduralMartianWorld>() == null)
            {
                Debug.Log("[Ares Resurgence] Initializing Mission 0: The Jezero Anomaly...");
                GameObject worldBootstrap = new GameObject("Ares_MissionZero_WorldBootstrap");
                worldBootstrap.AddComponent<ProceduralMartianWorld>();
            }
        }
    }
}
