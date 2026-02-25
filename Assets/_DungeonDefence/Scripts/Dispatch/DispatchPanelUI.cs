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
    [SerializeField] private int maxSlots = 5;

    [Header("UI References")]
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private TextMeshProUGUI totalBonusText;
    [SerializeField] private TextMeshProUGUI headerText;

    private List<DispatchSlotUI> slots = new List<DispatchSlotUI>();

    // 클릭 배정 모드: 인벤토리에서 유닛을 선택한 뒤 파견 슬롯 클릭 대기
    private UnitDataSO pendingUnitData = null;
    private Unit pendingGridUnit = null;

    protected override void Awake()
    {
        base.Awake();
        CreateSlots();
    }

    private void CreateSlots()
    {
        if (slotPrefab == null || slotsContainer == null) return;

        // 기존 슬롯이 있으면 정리
        foreach (Transform child in slotsContainer)
            Destroy(child.gameObject);
        slots.Clear();

        for (int i = 0; i < maxSlots; i++)
        {
            GameObject obj = Instantiate(slotPrefab, slotsContainer);
            var slot = obj.GetComponent<DispatchSlotUI>();
            if (slot != null)
            {
                slot.Initialize(this);
                slots.Add(slot);
            }

            // IDropHandler는 DispatchDropHandler로 처리
            if (obj.GetComponent<DispatchDropHandler>() == null)
                obj.AddComponent<DispatchDropHandler>();
        }
    }

    // ─── 클릭 선택 방식 ──────────────────────────────────────────────

    /// <summary>인벤토리 유닛 카드 클릭 → 파견 배정 모드 진입</summary>
    public void SelectInventoryUnitForDispatch(UnitDataSO data)
    {
        if (data == null) return;
        pendingUnitData = data;
        pendingGridUnit = null;
        Debug.Log($"[Dispatch] {data.Name} 선택 → 파견 슬롯 클릭으로 배정");
    }

    /// <summary>그리드 유닛 클릭 → 파견 배정 모드 진입</summary>
    public void SelectGridUnitForDispatch(Unit unit)
    {
        if (unit == null || !unit.IsPlayerTeam) return;
        pendingGridUnit = unit;
        pendingUnitData = null;
        Debug.Log($"[Dispatch] 그리드 유닛 {unit.Data.Name} 선택 → 파견 슬롯 클릭으로 배정");
    }

    /// <summary>DispatchDropHandler의 클릭 이벤트에서 호출</summary>
    public void TryAssignPendingToSlot(DispatchSlotUI slot)
    {
        if (slot == null || !slot.IsEmpty) return;

        if (pendingUnitData != null)
        {
            if (InventoryManager.Instance.TryConsumeItem(pendingUnitData))
            {
                slot.AssignUnitData(pendingUnitData);
                Debug.Log($"[Dispatch] {pendingUnitData.Name} → 파견 슬롯 (클릭)");
            }
            pendingUnitData = null;
        }
        else if (pendingGridUnit != null)
        {
            slot.TryAssignUnit(pendingGridUnit);
            pendingGridUnit = null;
        }
    }

    // ─── 패널 공개 API ───────────────────────────────────────────────

    public void OnSlotChanged()
    {
        RefreshBonusDisplay();
    }

    private void RefreshBonusDisplay()
    {
        if (totalBonusText == null) return;
        int total = 0;
        foreach (var s in slots) total += s.GetDispatchBonus();
        totalBonusText.text = $"파견 보너스: +{total}G";
    }

    /// <summary>웨이브 시작 시 슬롯 전체 초기화 (아이템 인벤토리로 반환)</summary>
    public void ClearAllSlots()
    {
        foreach (var slot in slots) slot.ClearSlot();
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
