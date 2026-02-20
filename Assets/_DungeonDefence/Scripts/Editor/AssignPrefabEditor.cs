using UnityEngine;
using UnityEditor;

public class AssignPrefabEditor
{
    [MenuItem("DungeonDefence/Generation/Fix All Unit Prefabs")]
    public static void AssignUnitPrefab()
    {
        string prefabPath = "Assets/_DungeonDefence/Datas/Units/UnitPrefab.prefab";
        GameObject unitPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (unitPrefab == null)
        {
            Debug.LogError($"Could not find UnitPrefab at {prefabPath}! Please follow the guide to create it first.");
            return;
        }

        string exportPath = "Assets/_DungeonDefence/Resources/Data";
        string[] guids = AssetDatabase.FindAssets("t:UnitDataSO", new string[] { exportPath });

        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            UnitDataSO data = AssetDatabase.LoadAssetAtPath<UnitDataSO>(path);

            if (data != null)
            {
                data.prefab = unitPrefab;
                EditorUtility.SetDirty(data);
                count++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[AssignPrefabEditor] Successfully assigned {unitPrefab.name} to {count} Unit SOs.");
    }
}
