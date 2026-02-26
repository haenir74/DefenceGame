using UnityEngine;
using UnityEngine.EventSystems;

public class UniversalDropHandler : MonoBehaviour, IDropHandler
{
    public virtual void OnDrop(PointerEventData eventData)
    {
        if (DragDropManager.Instance == null || !DragDropManager.Instance.IsDragging) return;
        if (GameManager.Instance == null || !GameManager.Instance.IsMaintenancePhase) return;

        var payload = DragDropManager.Instance.CurrentPayload;

        if (PlacementManager.Instance != null)
        {
            PlacementManager.Instance.ExecutePlacement(payload, gameObject);
        }
    }
}



