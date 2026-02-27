using UnityEngine;



namespace BLDebug
{
#if UNITY_EDITOR
    using UnityEditor;
#endif

    public static class Debug
    {
        private static bool stoppingGame = false;

        public static void StopGame()
        {
            if (!stoppingGame)
            {
                stoppingGame = true;
                UnityEngine.Debug.Log("Stopping program...");
                Logger.loggingEnabled = false;
#if UNITY_EDITOR
                EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
            }
        }
    }

}
