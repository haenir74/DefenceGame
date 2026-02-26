using System.Collections.Generic;
using UnityEngine;
using Panex.Inventory;
using Panex.Inventory.Controller; 
using Panex.Inventory.Model;      

public class InventoryTester : MonoBehaviour
{
    [Header("Inventory Prefabs (Required)")]
    [Tooltip("기본 디자인의 인벤토리 프리팹을 할당하세요.")]
    [SerializeField] private InventoryController defaultInventoryPrefab;
    
    [Tooltip("다른 디자인(창고 등)의 인벤토리 프리팹 (선택사항, 없으면 기본값 사용)")]
    [SerializeField] private InventoryController customInventoryPrefab;

    [Header("Settings Data")]
    [SerializeField] private Settings playerSettings;
    [SerializeField] private Settings chestSettings;

    [Header("Test Items")]
    [SerializeField] private SimpleItem potionItem;
    [SerializeField] private SimpleItem swordItem;

    
    private Inventory playerInventory;
    private Inventory chestInventory;

    
    private List<InventorySnapshot> savedData;

    private void Start()
    {
        InitializeSystem();
    }

    private void InitializeSystem()
    {
        
        
        
        playerInventory = new Inventory("Player", playerSettings, defaultInventoryPrefab);

        
        playerInventory.OnSlotClicked += (item, index) => {
            Debug.Log($"[Player] {item.Name} 클릭됨 (Slot {index}) -> 아이템 사용 로직 연결 가능");
        };
        
        playerInventory.OnItemDroppedOutside += (item, pos) => {
            Debug.Log($"[Player] {item.Name} 버려짐 (좌표 {pos}) -> 아이템 생성 로직 연결 가능");
            
            playerInventory.RemoveItem(item, 1);
        };

        
        
        var prefabToUse = customInventoryPrefab != null ? customInventoryPrefab : defaultInventoryPrefab;
        chestInventory = new Inventory("Chest", chestSettings, prefabToUse);
        
        
        chestInventory.Close();

        PrintInstructions();
    }

    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.I)) 
            playerInventory.Toggle();

        
        if (Input.GetKeyDown(KeyCode.O)) 
            chestInventory.Toggle();

        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var item = Random.value > 0.5f ? potionItem : swordItem;
            int amount = Random.Range(1, 5);
            
            int left = playerInventory.AddItem(item, amount);
            
            if (left == 0) Debug.Log($"획득: {item.Name} x{amount}");
            else Debug.Log($"인벤토리 꽉 참! {item.Name} {left}개 남음.");
        }

        
        if (Input.GetKeyDown(KeyCode.R))
        {
            playerInventory.RemoveItem(0);
            Debug.Log("0번 슬롯 비움");
        }

        
        if (Input.GetKeyDown(KeyCode.S))
        {
            savedData = playerInventory.GetSnapshot();
            Debug.Log($"상태 저장 완료! ({savedData.Count}개의 데이터)");
        }

        
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (savedData != null)
            {
                
                playerInventory.LoadSnapshot(savedData, ResolveItem);
                Debug.Log("상태 불러오기 완료!");
            }
            else
            {
                Debug.LogWarning("저장된 데이터가 없습니다. [S]를 먼저 누르세요.");
            }
        }
    }

    
    private IStorable ResolveItem(int id)
    {
        if (potionItem != null && potionItem.ID == id) return potionItem;
        if (swordItem != null && swordItem.ID == id) return swordItem;
        Debug.LogError($"ID [{id}]에 해당하는 아이템을 찾을 수 없습니다.");
        return null;
    }

    private void PrintInstructions()
    {
        Debug.Log("=== 인벤토리 테스트 가이드 ===");
        Debug.Log("[I]: 플레이어 인벤토리 열기/닫기");
        Debug.Log("[O]: 창고 인벤토리 열기/닫기");
        Debug.Log("[Space]: 랜덤 아이템 획득");
        Debug.Log("[R]: 0번 슬롯 비우기");
        Debug.Log("[S]: 저장 (Snapshot)");
        Debug.Log("[L]: 불러오기");
        Debug.Log("============================");
    }

    private void OnDestroy()
    {
        
        playerInventory?.Destroy();
        chestInventory?.Destroy();
    }
}