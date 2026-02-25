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

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (unit == null || unit.IsDead || unit.IsDispatched) return;
        if (!GameManager.Instance.IsMaintenancePhase) return;

        var payload = new DragPayload
        {
            Source = DragPayload.SourceType.GridUnit,
            GridUnit = unit
        };
        DragDropManager.Instance.BeginDrag(payload, unit.Data?.icon);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!DragDropManager.Instance.IsDragging) return;
        DragDropManager.Instance.UpdateGhostPosition(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DragDropManager.Instance.EndDrag();
    }
}
