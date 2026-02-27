using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace DungeonDefence.Editor
{
    public class ReferenceAutoFixer
    {
        [MenuItem("Tools/Auto Fix Missing References")]
        public static void AutoFixMissingReferences()
        {
            bool anyChanged = false;

            // Fix Unit Scroll View Settings
            if (FixInventoryControllerSettings(
                "Canvas/Inventory_Root/Content/Inventory_BG/Unit_Scroll_View/Viewport/Content",
                "Assets/_DungeonDefence/Resources/Data/Settings/Inventory/Settings_Unit.asset"))
            {
                anyChanged = true;
            }

            // Fix Tile Scroll View Settings
            if (FixInventoryControllerSettings(
                "Canvas/Inventory_Root/Content/Inventory_BG/Tile_Scroll_View/Viewport/Content",
                "Assets/_DungeonDefence/Resources/Data/Settings/Inventory/Settings_Tile.asset"))
            {
                anyChanged = true;
            }

            if (anyChanged)
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                Debug.Log("<color=green>Missing references have been automatically fixed!</color>");
            }
            else
            {
                Debug.LogWarning("No missing references were fixed. Make sure the Canvas exists in the current scene.");
            }
        }

        [MenuItem("Tools/Auto Fix Null References (Scene Objects)")]
        public static void AutoFixNullReferences()
        {
            Debug.Log("<color=green>ResultUI is legacy, removed from auto fix.</color>");
        }

        private static bool FixInventoryControllerSettings(string objectPath, string assetPath)
        {
            GameObject obj = GameObject.Find(objectPath);
            if (obj == null)
            {
                Debug.LogError($"Could not find GameObject at path: {objectPath}");
                return false;
            }

            // Find InventoryController using reflection or GetComponent if type is known
            // Since we don't have direct access to Panex.Inventory.InventoryController namespace,
            // we will find the component by its name via GetComponent("InventoryController").
            Component controller = obj.GetComponent("InventoryController");
            if (controller == null)
            {
                Debug.LogError($"InventoryController not found on {obj.name}");
                return false;
            }

            ScriptableObject settingsAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (settingsAsset == null)
            {
                Debug.LogError($"Could not load asset at path: {assetPath}");
                return false;
            }

            SerializedObject so = new SerializedObject(controller);
            SerializedProperty settingsProp = so.FindProperty("settings");

            if (settingsProp != null)
            {
                settingsProp.objectReferenceValue = settingsAsset;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(controller);

                Debug.Log($"Successfully assigned {settingsAsset.name} to {obj.name}'s settings.");
                return true;
            }
            else
            {
                Debug.LogError($"'settings' property not found on {controller.GetType().Name}");
                return false;
            }
        }
    }
}
