using UnityEngine;

namespace CompositionRoot
{
    public static class ProjectAppInitializationPoint
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeApplication()
        {
            Debug.LogError("Project App Initialization Point");
            AppInitialization.InitializeApplication();
        }
    }
}