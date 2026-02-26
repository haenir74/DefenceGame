using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Unit))]
public class GridUnitDragHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Unit unit;

    private void Awake()
    {
        unit = GetComponent<Unit>();
    }

    

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (unit == null || unit.IsDead || unit.IsDispatched || !GameManager.Instance.IsMaintenancePhase) return;

        
        var payload = new DragPayload
        {
            Source = DragPayload.SourceType.Grid,
            UnitData = unit.Data,
            GridUnit = unit,
            OriginalNode = unit.CurrentNode
        };
        DragDropManager.Instance.BeginDrag(payload, unit.Data?.icon);

        
        unit.SetVisualVisible(false);

        
        GameManager.Instance.SelectUnitToPlace(unit.Data, GameManager.SelectionSource.Grid, unit.CurrentNode, unit);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (DragDropManager.Instance.IsDragging)
            DragDropManager.Instance.UpdateGhostPosition(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        
        DragDropManager.Instance.EndDrag();

        
        if (unit != null && !unit.IsDead && !DragDropManager.Instance.IsDragging)
        {
            
            
        }
    }
}



