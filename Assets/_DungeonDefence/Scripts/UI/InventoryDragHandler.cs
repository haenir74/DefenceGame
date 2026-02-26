using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Panex.Inventory;

public class InventoryDragHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    
    
    public UnitDataSO UnitData { get; set; }
    public TileDataSO TileData { get; set; }

    private Image slotIconImage;

    private void Awake()
    {
        
        
        var images = GetComponentsInChildren<Image>(true);
        foreach (var img in images)
        {
            if (img.gameObject.name == "Icon" || img.gameObject.name == "iconImage")
            {
                slotIconImage = img;
                break;
            }
        }

        
        if (slotIconImage == null && images.Length > 0)
            slotIconImage = images[images.Length - 1];
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (UnitData == null && TileData == null) return;
        if (!GameManager.Instance.IsMaintenancePhase) return;

        var payload = new DragPayload
        {
            Source = DragPayload.SourceType.Inventory,
            UnitData = UnitData,
            TileData = TileData
        };

        Sprite icon = (UnitData != null) ? UnitData.icon : TileData.icon;
        DragDropManager.Instance.BeginDrag(payload, icon);

        
        if (slotIconImage != null)
            slotIconImage.gameObject.SetActive(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!DragDropManager.Instance.IsDragging) return;
        DragDropManager.Instance.UpdateGhostPosition(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DragDropManager.Instance.EndDrag();

        
        if (slotIconImage != null)
            slotIconImage.gameObject.SetActive(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        
        
    }
}



