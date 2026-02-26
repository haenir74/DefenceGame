using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 화면 왼쪽의 파견 패널. 인벤토리와 함께 열리고 닫힌다.
/// 클릭-선택 방식과 드래그 앤 드롭 방식 모두를 지원.
/// </summary>
public class DispatchPanelUI : Singleton<DispatchPanelUI>
{
    [Header("Settings")]
    // [FIX] Removed unused maxSlots

    [Header("UI References")]
    [SerializeField] private ScrollRect scrollRect; // [FIX] Added for scrolling support
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private TextMeshProUGUI totalBonusText;
    [SerializeField] private TextMeshProUGUI headerText;


    private List<DispatchSlotUI> slots = new List<DispatchSlotUI>();

    protected override void Awake()
    {
        base.Awake();
        // [FIX] No longer pre-creating slots. They will be added dynamically on drop.
        slots.Clear();

        // [FIX] Add DropHandler to the background to support "drop anywhere"
        if (GetComponent<DispatchDropHandler>() == null)
            gameObject.AddComponent<DispatchDropHandler>();
    }

    private void CreateSlots()
    {
        // Deprecated: Slots are created dynamically via AddDispatchSlot
    }

    /// <summary>새로운 파견 슬롯을 동적으로 생성하고 데이터를 배정합니다.</summary>
    public DispatchSlotUI CreateSlotAndAssign(DragPayload payload)
    {
        if (slotPrefab == null || slotsContainer == null) return null;

        GameObject obj = Instantiate(slotPrefab, slotsContainer);
        var slot = obj.GetComponentInChildren<DispatchSlotUI>();

        if (slot != null)
        {
            slot.Initialize(this);
            slots.Add(slot);

            // 드롭 핸들러 추가
            if (slot.gameObject.GetComponent<DispatchDropHandler>() == null)
                slot.gameObject.AddComponent<DispatchDropHandler>();

            // 배정 처리 (PlacementManager가 이미 소모/검증을 했다고 가정하거나, 여기서 연동)
            // [NOTE] 여기서 바로 배정까지 완료하여 slot을 반환함
            bool assigned = false;
            if (payload.UnitData != null || payload.GridUnit != null)
            {
                UnitDataSO data = payload.UnitData ?? payload.GridUnit?.Data;
                slot.AssignUnitData(data);
                assigned = true;
            }

            if (assigned)
            {
                RefreshBonusDisplay();
                UpdateScrollPosition();
                return slot;
            }
        }

        Destroy(obj);
        return null;
    }

    private void UpdateScrollPosition()
    {
        // [FIX] scrollRect는 연결되어 있어도 내부의 content가 없으면 오류가 발생함
        if (scrollRect == null || scrollRect.content == null) return;

        // 새로운 아이템이 아래에 추가된다고 가정하고 가장 아래로 스크롤
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }




    // ─── 패널 공개 API ───────────────────────────────────────────────

    public void OnSlotChanged()
    {
        // 슬롯이 비워지면(회수되면) 리스트에서 제거하고 오브젝트 파괴
        slots.RemoveAll(s => s == null);

        // DispatchSlotUI 측에서 회수 시 Destroy(gameObject)를 호출하거나,
        // 여기서 체크하여 정리.
        RefreshBonusDisplay();
    }

    private void RefreshBonusDisplay()
    {
        if (totalBonusText == null) return;
        int total = 0;
        foreach (var s in slots)
        {
            if (s != null) total += s.GetDispatchBonus();
        }
        totalBonusText.text = $"파견 보너스: +{total}G";
    }

    /// <summary>웨이브 시작 시 슬롯 전체 초기화 (아이템 인벤토리로 반환)</summary>
    public void ClearAllSlots()
    {
        foreach (var slot in slots)
        {
            if (slot != null)
            {
                slot.ClearSlot();
                Destroy(slot.transform.parent.gameObject); // DispatchSlot (Prefab Root) 파괴
            }
        }
        slots.Clear();
        RefreshBonusDisplay();
    }


    /// <summary>파견 슬롯 총 보너스 골드 반환 (RewardManager에서 호출)</summary>
    public int GetTotalDispatchBonus()
    {
        int total = 0;
        foreach (var s in slots) total += s.GetDispatchBonus();
        return total;
    }
}
