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
            if (Object.FindAnyObjectByType<NASAMasterExperience>() == null)
            {
                Debug.Log("[NASA Odyssey] Initializing Hollywood Junior Astronaut Odyssey: From Zero-G to the ISS...");
                GameObject worldBootstrap = new GameObject("NASA_Odyssey_MasterBootstrap");
                worldBootstrap.AddComponent<NASAMasterExperience>();
            }
        }
    }
}
