using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 파견 슬롯 하나. 그리드 유닛 or 인벤토리 유닛 데이터를 받는다.
/// </summary>
public class DispatchSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image unitIconImage;
    [SerializeField] private TextMeshProUGUI unitNameText;
    [SerializeField] private TextMeshProUGUI bonusText;
    [SerializeField] private Button recallButton;
    [SerializeField] private Image emptyIndicator;

    private Unit assignedUnit;       // 그리드 기반 파견
    private UnitDataSO assignedData; // 인벤토리 기반 파견
    private DispatchPanelUI panel;

    public bool IsEmpty => assignedUnit == null && assignedData == null;

    public void Initialize(DispatchPanelUI panel)
    {
        this.panel = panel;
        if (recallButton != null)
            recallButton.onClick.AddListener(OnRecallClicked);
        Refresh();
    }

    // ─── 그리드 유닛 배정 ───────────────────────────────────────────

    public bool TryAssignUnit(Unit unit)
    {
        if (unit == null || !unit.IsPlayerTeam || unit.Data.category == UnitCategory.Core) return false;
        if (!IsEmpty) return false;

        assignedUnit = unit;
        assignedData = null;
        unit.SetDispatchMode(true);
        Refresh();
        panel?.OnSlotChanged();
        return true;
    }

    // ─── 인벤토리 데이터 배정 ─────────────────────────────────────

    public void AssignUnitData(UnitDataSO data)
    {
        if (data == null) return;
        assignedData = data;
        assignedUnit = null;
        Refresh();
        panel?.OnSlotChanged();
    }

    // ─── 해제 ─────────────────────────────────────────────────────

    private void OnRecallClicked()
    {
        if (assignedUnit != null)
        {
            // 그리드 유닛: 파견 해제만
            assignedUnit.SetDispatchMode(false);
            assignedUnit = null;
        }
        else if (assignedData != null)
        {
            // 인벤토리 기반: 인벤토리에 반환
            InventoryManager.Instance.AddItem(assignedData, 1);
            assignedData = null;
        }

        Refresh();
        panel?.OnSlotChanged();
    }

    public void ClearSlot()
    {
        if (assignedUnit != null) assignedUnit.SetDispatchMode(false);
        else if (assignedData != null) InventoryManager.Instance?.AddItem(assignedData, 1);

        assignedUnit = null;
        assignedData = null;
        Refresh();
    }

    // ─── 파견 보너스 계산 ──────────────────────────────────────────

    public int GetDispatchBonus()
    {
        float baseReward = 100f; // 슬롯 기본 골드
        float efficiency = 1f;

        if (assignedUnit != null) efficiency = assignedUnit.Data.dispatchEfficiency;
        else if (assignedData != null) efficiency = assignedData.dispatchEfficiency;

        return Mathf.RoundToInt(baseReward * efficiency);
    }

    // ─── UI 갱신 ───────────────────────────────────────────────────

    private void Refresh()
    {
        bool hasUnit = !IsEmpty;
        UnitDataSO displayData = assignedUnit?.Data ?? assignedData;

        if (unitIconImage != null)
        {
            unitIconImage.gameObject.SetActive(hasUnit);
            if (hasUnit && displayData?.icon != null)
                unitIconImage.sprite = displayData.icon;
        }

        if (unitNameText != null)
            unitNameText.text = hasUnit ? displayData?.Name ?? "" : "";

        if (bonusText != null)
        {
            if (hasUnit && displayData != null)
                bonusText.text = $"효율: {(displayData.dispatchEfficiency * 100f):F0}%";
            else
                bonusText.text = "";
        }

        if (recallButton != null)
            recallButton.gameObject.SetActive(hasUnit);

        if (emptyIndicator != null)
            emptyIndicator.gameObject.SetActive(!hasUnit);
    }
}
