using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DungeonDefence.DebugTools
{
    /// <summary>
    /// 테스트를 위해 모든 아군 유닛을 인벤토리에 추가해주는 헬퍼 클래스입니다.
    /// </summary>
    public class UnitTestHelper : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool addAtStart = true;
        [SerializeField] private int amountPerUnit = 1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnProjectStart()
        {
            GameObject go = new GameObject("UnitTestHelper");
            go.AddComponent<UnitTestHelper>();
            DontDestroyOnLoad(go);
        }

        private void Start()
        {
            if (addAtStart)
            {
                StartCoroutine(AddUnitsAfterInitialization());
            }
        }

        private IEnumerator AddUnitsAfterInitialization()
        {
            // InventoryManager와 GameManager가 초기화될 때까지 대기
            while (InventoryManager.Instance == null || GameManager.Instance == null)
            {
                yield return null;
            }

            // 약간의 지연 후 추가 (다른 시스템 초기화 대기)
            yield return new WaitForSeconds(0.5f);

            AddAllAllyUnits();
        }

        [ContextMenu("Add All Ally Units to Inventory")]
        public void AddAllAllyUnits()
        {
            if (InventoryManager.Instance == null)
            {
                Debug.LogError("[UnitTestHelper] InventoryManager.Instance is null!");
                return;
            }

            // Resources/Data/Units/Allies 경로의 모든 UnitDataSO 로드
            UnitDataSO[] allAllyDatas = Resources.LoadAll<UnitDataSO>("Data/Units/Allies");

            if (allAllyDatas == null || allAllyDatas.Length == 0)
            {
                Debug.LogWarning("[UnitTestHelper] No UnitDataSO found in Resources/Data/Units/Allies");
                return;
            }

            int count = 0;
            foreach (var data in allAllyDatas)
            {
                if (data.isPlayerTeam)
                {
                    InventoryManager.Instance.AddItem(data, amountPerUnit);
                    count++;
                }
            }

            Debug.Log($"[UnitTestHelper] {count} types of ally units added to inventory.");
        }
    }
}
