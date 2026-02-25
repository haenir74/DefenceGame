using UnityEngine;
using UnityEngine.EventSystems;
using Panex.Inventory;

/// <summary>
/// 인벤토리 슬롯 카드에 붙어 드래그를 시작하는 컴포넌트.
/// CardSlotUI 프리팹의 Button 오브젝트에 추가하면 된다.
/// </summary>
public class InventoryDragHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    // 이 슬롯에 저장된 아이템 (InventoryController가 세팅해줘야 함)
    public UnitDataSO UnitData { get; set; }

    private bool wasDragging = false;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (UnitData == null) return;
        if (!GameManager.Instance.IsMaintenancePhase) return;

        wasDragging = true;

        var payload = new DragPayload
        {
            Source = DragPayload.SourceType.Inventory,
            UnitData = UnitData
        };
        DragDropManager.Instance.BeginDrag(payload, UnitData.icon);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!DragDropManager.Instance.IsDragging) return;
        DragDropManager.Instance.UpdateGhostPosition(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        wasDragging = false;
        DragDropManager.Instance.EndDrag();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 드래그가 아닌 단순 클릭이면 기존 선택 로직 유지
        if (wasDragging) return;
        if (UnitData == null) return;
        if (!GameManager.Instance.IsMaintenancePhase) return;

        GameManager.Instance.SelectUnitToPlace(UnitData);
    }
}
