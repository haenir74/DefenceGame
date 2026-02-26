using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// 파견 슬롯 하나. 그리드 유닛 or 인벤토리 유닛 데이터를 받는다.
/// </summary>
public class DispatchSlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
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
            assignedUnit.SetDispatchMode(false);
            assignedUnit = null;
        }
        else if (assignedData != null)
        {
            InventoryManager.Instance?.AddItem(assignedData, 1);
            assignedData = null;
        }

        panel?.OnSlotChanged();

        // [FIX] In dynamic system, the slot object should be destroyed when empty
        if (transform.parent != null)
            Destroy(transform.parent.gameObject); // Destroy DispatchSlot root
        else
            Destroy(gameObject);
    }

    public void ClearSlot(bool returnToInventory = true)
    {
        if (assignedUnit != null)
        {
            assignedUnit.SetDispatchMode(false);
        }
        else if (assignedData != null && returnToInventory)
        {
            InventoryManager.Instance?.AddItem(assignedData, 1);
        }

        assignedUnit = null;
        assignedData = null;

        // [FIX] In dynamic system, the slot object should be destroyed when empty
        if (transform.parent != null)
            Destroy(transform.parent.gameObject);
        else
            Destroy(gameObject);
    }


    // ─── 파견 보너스 계산 ──────────────────────────────────────────

    public int GetDispatchBonus()
    {
        if (DispatchManager.Instance == null) return 0;
        return DispatchManager.Instance.CalculateUnitBonus(assignedUnit, assignedData);
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

    public void OnPointerClick(PointerEventData eventData)
    {
        // [REFINED] Click-to-Select removed.
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsEmpty) return;

        UnitDataSO data = assignedUnit?.Data ?? assignedData;
        if (data != null && DragDropManager.Instance != null)
        {
            Debug.Log($"[Dispatch] Picking up unit via drag: {data.Name}");

            var payload = new DragPayload();
            payload.Source = DragPayload.SourceType.Dispatch;
            payload.UnitData = data;
            payload.FromSlot = this; // [IMPORTANT] Store origin for cleanup/restore

            DragDropManager.Instance.BeginDrag(payload, unitIconImage.sprite);

            // [FIX] DO NOT destroy/clear yet. Just hide visuals to keep event chain alive.
            SetVisualVisible(false);

            // Ensure GameManager knows this unit is "picked up" for potential cancellation
            GameManager.Instance?.SelectUnitToPlace(data, GameManager.SelectionSource.Dispatch);
        }
    }

    private void SetVisualVisible(bool visible)
    {
        if (unitIconImage != null) unitIconImage.gameObject.SetActive(visible);
        if (unitNameText != null) unitNameText.gameObject.SetActive(visible);
        if (bonusText != null) bonusText.gameObject.SetActive(visible);
        if (recallButton != null) recallButton.gameObject.SetActive(visible && !IsEmpty);
        if (emptyIndicator != null) emptyIndicator.gameObject.SetActive(visible && IsEmpty);

        // Disable raycast on icon so we can drop "through" it if needed, 
        // though usually the background panel handles it.
        if (unitIconImage != null) unitIconImage.raycastTarget = visible;
    }

    /// <summary>취소 시 슬롯 비주얼을 복구합니다.</summary>
    public void RestoreSlot()
    {
        SetVisualVisible(true);
        Refresh();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (DragDropManager.Instance != null)
            DragDropManager.Instance.UpdateGhostPosition(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // [FIX] Always call EndDrag on manager. 
        // PlacementManager will call either ClearSlot(false) on success or RestoreSlot() on cancel.
        if (DragDropManager.Instance != null)
            DragDropManager.Instance.EndDrag();
    }
}
