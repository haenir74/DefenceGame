using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStarter : MonoBehaviour
{
    [Header("Starting Items")]
    [SerializeField] private UnitDataSO allyUnitData;
    [SerializeField] private int allyCount = 5;

    [SerializeField] private TileDataSO roadTileData;
    [SerializeField] private int roadCount = 10;

    [Header("Starting Resources")]
    [SerializeField] private int startingGold = 1000;

    private IEnumerator Start()
    {
        // 매니저들이 초기화될 때까지 잠시 대기
        yield return null; 

        // 1. 골드 지급
        GameManager.Instance?.AddGold(startingGold);

        // 2. 인벤토리에 유닛/타일 지급
        if (InventoryManager.Instance != null)
        {
            if (allyUnitData != null)
            {
                InventoryManager.Instance.AddItem(allyUnitData, allyCount);
                Debug.Log($"[GameStarter] {allyUnitData.Name} x{allyCount} 지급 완료");
            }

            if (roadTileData != null)
            {
                InventoryManager.Instance.AddItem(roadTileData, roadCount);
                Debug.Log($"[GameStarter] {roadTileData.Name} x{roadCount} 지급 완료");
            }
        }
        else
        {
            Debug.LogError("[GameStarter] InventoryManager가 없습니다!");
        }
    }
}