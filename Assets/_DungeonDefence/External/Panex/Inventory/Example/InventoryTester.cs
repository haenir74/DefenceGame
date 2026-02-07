using System.Collections.Generic;
using UnityEngine;
using Panex.Inventory;
using Panex.Inventory.Controller; // 프리팹 참조용
using Panex.Inventory.Model;      // 스냅샷 데이터용

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

    // 인벤토리 시스템 인스턴스 (Controller가 아닌 Wrapper 클래스)
    private Inventory playerInventory;
    private Inventory chestInventory;

    // 저장/불러오기 테스트용 메모리
    private List<InventorySnapshot> savedData;

    private void Start()
    {
        InitializeSystem();
    }

    private void InitializeSystem()
    {
        // 1. 플레이어 인벤토리 생성
        // - 생성자: new Inventory(ID, 설정, 프리팹, 부모트랜스폼)
        // - this.transform을 부모로 넘겨서 하이어라키를 깔끔하게 정리
        playerInventory = new Inventory("Player", playerSettings, defaultInventoryPrefab);

        // [이벤트 구독]
        playerInventory.OnSlotClicked += (item, index) => {
            Debug.Log($"[Player] {item.Name} 클릭됨 (Slot {index}) -> 아이템 사용 로직 연결 가능");
        };
        
        playerInventory.OnItemDroppedOutside += (item, pos) => {
            Debug.Log($"[Player] {item.Name} 버려짐 (좌표 {pos}) -> 아이템 생성 로직 연결 가능");
            // 테스트를 위해 버리면 바로 삭제
            playerInventory.RemoveItem(item, 1);
        };

        // 2. 창고 인벤토리 생성
        // - 커스텀 프리팹이 있으면 사용, 없으면 기본 프리팹 사용
        var prefabToUse = customInventoryPrefab != null ? customInventoryPrefab : defaultInventoryPrefab;
        chestInventory = new Inventory("Chest", chestSettings, prefabToUse);
        
        // 창고는 시작할 때 닫아둠
        chestInventory.Close();

        PrintInstructions();
    }

    private void Update()
    {
        // 1. [I] 키: 플레이어 인벤토리 토글 (Open/Close)
        if (Input.GetKeyDown(KeyCode.I)) 
            playerInventory.Toggle();

        // 2. [O] 키: 창고 인벤토리 토글
        if (Input.GetKeyDown(KeyCode.O)) 
            chestInventory.Toggle();

        // 3. [Space] 키: 랜덤 아이템 추가 (Add API)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var item = Random.value > 0.5f ? potionItem : swordItem;
            int amount = Random.Range(1, 5);
            
            int left = playerInventory.AddItem(item, amount);
            
            if (left == 0) Debug.Log($"획득: {item.Name} x{amount}");
            else Debug.Log($"인벤토리 꽉 참! {item.Name} {left}개 남음.");
        }

        // 4. [R] 키: 첫 번째 칸 아이템 제거 (Remove API)
        if (Input.GetKeyDown(KeyCode.R))
        {
            playerInventory.RemoveItem(0);
            Debug.Log("0번 슬롯 비움");
        }

        // 5. [S] 키: 현재 상태 저장 (Snapshot API)
        if (Input.GetKeyDown(KeyCode.S))
        {
            savedData = playerInventory.GetSnapshot();
            Debug.Log($"상태 저장 완료! ({savedData.Count}개의 데이터)");
        }

        // 6. [L] 키: 저장된 상태 불러오기 (Load API)
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (savedData != null)
            {
                // ItemResolver: 저장된 ID(int)를 실제 아이템(SO)으로 바꿔주는 함수
                playerInventory.LoadSnapshot(savedData, ResolveItem);
                Debug.Log("상태 불러오기 완료!");
            }
            else
            {
                Debug.LogWarning("저장된 데이터가 없습니다. [S]를 먼저 누르세요.");
            }
        }
    }

    // ID로 아이템 데이터를 찾는 헬퍼 함수 (실제 게임에선 데이터베이스 매니저가 담당)
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
        // 씬 전환/종료 시 인벤토리 파괴 (메모리 누수 방지)
        playerInventory?.Destroy();
        chestInventory?.Destroy();
    }
}