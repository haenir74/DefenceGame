using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AssignWavesEditor
{
    [MenuItem("DungeonDefence/Generation/Assign Waves To Manager")]
    public static void AssignWaves()
    {
        WaveManager manager = Object.FindObjectOfType<WaveManager>(true);
        if (manager == null)
        {
            Debug.LogError("WaveManager not found in the current scene! Please open the Game Scene where WaveManager exists and try again.");
            return;
        }

        string exportPath = "Assets/_DungeonDefence/Resources/Data/Waves";
        string[] guids = AssetDatabase.FindAssets("t:WaveDataSO", new[] { exportPath });
        
        List<WaveDataSO> allWaves = new List<WaveDataSO>();
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            WaveDataSO wave = AssetDatabase.LoadAssetAtPath<WaveDataSO>(path);
            if (wave != null) allWaves.Add(wave);
        }

        // 정렬 (1 ~ 30 웨이브 순으로)
        allWaves.Sort((a, b) => a.waveIndex.CompareTo(b.waveIndex));

        // SerializedObject를 사용하여 private 필드(wave 리스트) 수정
        SerializedObject so = new SerializedObject(manager);
        SerializedProperty wavesProp = so.FindProperty("waves");
        
        if (wavesProp == null)
        {
            Debug.LogError("Failed to find 'waves' field in WaveManager. Check the variable name.");
            return;
        }

        wavesProp.ClearArray();
        
        for (int i = 0; i < allWaves.Count; i++)
        {
            wavesProp.InsertArrayElementAtIndex(i);
            SerializedProperty configProp = wavesProp.GetArrayElementAtIndex(i);
            
            // WaveConfig 구조체 내부의 waveDatas 리스트 찾기
            SerializedProperty waveDatasProp = configProp.FindPropertyRelative("waveDatas");
            waveDatasProp.ClearArray();
            waveDatasProp.InsertArrayElementAtIndex(0);
            waveDatasProp.GetArrayElementAtIndex(0).objectReferenceValue = allWaves[i];
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(manager);

        // 변경사항 저장 지시 (씬이나 프리팹 갱신)
        #if UNITY_2021_3_OR_NEWER
        var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        #else
        var prefabStage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        #endif

        if (prefabStage != null)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(prefabStage.scene);
        }
        else
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        Debug.Log($"[AssignWavesEditor] 30개 웨이브 데이터를 WaveManager에 성공적으로 자동 할당했습니다!");
    }
}
