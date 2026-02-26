using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 모든 드롭 핸들러의 기본형.
/// 드롭 이벤트를 받아 PlacementManager에게 통합 처리를 요청합니다.
/// </summary>
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
