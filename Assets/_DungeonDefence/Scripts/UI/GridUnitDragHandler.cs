using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 그리드에 배치된 유닛의 월드 오브젝트(또는 그 위의 UI)에 붙여 사용.
/// 유닛을 드래그해서 파견 슬롯 또는 인벤토리로 보내는 기능.
/// </summary>
[RequireComponent(typeof(Unit))]
public class GridUnitDragHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Unit unit;

    private void Awake()
    {
        unit = GetComponent<Unit>();
    }

    // [REMOVED] OnPointerClick: Handled by InputController and MaintenanceState to avoid double-events.


    public void OnBeginDrag(PointerEventData eventData)
    {
        if (unit == null || unit.IsDead || unit.IsDispatched || !GameManager.Instance.IsMaintenancePhase) return;

        // [FIX] Picking up from Grid -> Select it and start Dragging
        var payload = new DragPayload
        {
            Source = DragPayload.SourceType.Grid,
            UnitData = unit.Data,
            GridUnit = unit,
            OriginalNode = unit.CurrentNode
        };
        DragDropManager.Instance.BeginDrag(payload, unit.Data?.icon);

        // [FIX] Explicitly hide the unit during drag (replaces the old click-to-pickup logic)
        unit.SetVisualVisible(false);

        // Ensure GameManager knows this unit is "picked up" for potential cancellation
        GameManager.Instance.SelectUnitToPlace(unit.Data, GameManager.SelectionSource.Grid, unit.CurrentNode, unit);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (DragDropManager.Instance.IsDragging)
            DragDropManager.Instance.UpdateGhostPosition(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // [NOTE] EndDrag will call PlacementManager which handles cleanup or restoration.
        DragDropManager.Instance.EndDrag();

        // Safety check: if for some reason the unit is still hidden but not placed/despawned
        if (unit != null && !unit.IsDead && !DragDropManager.Instance.IsDragging)
        {
            // PlacementManager should have handled this, but just in case of errors:
            // unit.SetVisualVisible(true); 
        }
    }
}
