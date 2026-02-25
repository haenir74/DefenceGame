using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 파견 슬롯 드롭 수신기.
/// DispatchSlot 루트 GameObject에 추가한다.
/// 인벤토리 드래그, 그리드 유닛 드래그를 모두 받는다.
/// </summary>
public class DispatchDropHandler : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    private DispatchSlotUI slotUI;

    private void Awake()
    {
        slotUI = GetComponent<DispatchSlotUI>() ?? GetComponentInParent<DispatchSlotUI>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!DragDropManager.Instance.IsDragging) return;
        if (!GameManager.Instance.IsMaintenancePhase) return;

        var payload = DragDropManager.Instance.CurrentPayload;

        if (payload.Source == DragPayload.SourceType.Inventory)
        {
            // 인벤토리 유닛 → 파견 슬롯
            // 인벤토리에서 소모하지 않고 파견 예약만 함 (데이터 기반 파견)
            HandleInventoryDrop(payload);
        }
        else if (payload.Source == DragPayload.SourceType.GridUnit)
        {
            // 그리드의 유닛 → 파견 슬롯
            HandleGridUnitDrop(payload);
        }
    }

    private void HandleInventoryDrop(DragPayload payload)
    {
        if (payload.UnitData == null) return;
        if (slotUI == null || !slotUI.IsEmpty) return;

        // 인벤토리에서 소모하고 파견 슬롯에 데이터 기록
        if (InventoryManager.Instance.TryConsumeItem(payload.UnitData))
        {
            slotUI.AssignUnitData(payload.UnitData);
            Debug.Log($"[Dispatch] 인벤토리에서 {payload.UnitData.Name} → 파견 슬롯");
        }
        else
        {
            Debug.LogWarning("[Dispatch] 인벤토리에 해당 유닛이 없습니다.");
        }
    }

    private void HandleGridUnitDrop(DragPayload payload)
    {
        if (payload.GridUnit == null) return;
        if (slotUI == null || !slotUI.IsEmpty) return;

        bool success = slotUI.TryAssignUnit(payload.GridUnit);
        if (success)
            Debug.Log($"[Dispatch] 그리드에서 {payload.GridUnit.Data.Name} → 파견 슬롯");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 클릭 배정 모드: DispatchPanelUI.pendingUnit 있으면 배정
        if (!GameManager.Instance.IsMaintenancePhase) return;
        if (slotUI == null || !slotUI.IsEmpty) return;

        DispatchPanelUI.Instance?.TryAssignPendingToSlot(slotUI);
    }
}
