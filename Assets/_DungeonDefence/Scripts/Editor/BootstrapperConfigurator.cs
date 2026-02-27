using UnityEditor;
using UnityEngine;

namespace DungeonDefence.Editor
{
    public class BootstrapperConfigurator
    {
        private const string MENU_PATH = "Tools/Setup Bootstrapper Execution Order";

        [MenuItem(MENU_PATH)]
        public static void SetExecutionOrder()
        {
            // Find the MonoScript for GameBootstrapper
            string[] guids = AssetDatabase.FindAssets("GameBootstrapper t:MonoScript");
            
            if (guids.Length == 0)
            {
                Debug.LogError("Could not find GameBootstrapper.cs in the project.");
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

            if (script == null)
            {
                Debug.LogError("Failed to load GameBootstrapper MonoScript.");
                return;
            }

            // Set Execution Order to -100
            if (MonoImporter.GetExecutionOrder(script) != -100)
            {
                MonoImporter.SetExecutionOrder(script, -100);
                Debug.Log("<color=green>GameBootstrapper execution order successfully set to -100.</color>");
            }
            else
            {
                Debug.Log("GameBootstrapper execution order is already set to -100.");
            }
        }
    }
}
