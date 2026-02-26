using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Panex.Inventory;

/// <summary>
/// 인벤토리 슬롯 카드에 붙어 드래그를 시작하는 컴포넌트.
/// CardSlotUI 프리팹의 Button 오브젝트에 추가하면 된다.
/// </summary>
public class InventoryDragHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    // 이 슬롯에 저장된 아이템 (InventoryController가 세팅해줘야 함)
    // [FIX] 유닛 뿐만 아니라 타일 데이터도 지원하도록 확장
    public UnitDataSO UnitData { get; set; }
    public TileDataSO TileData { get; set; }

    private Image slotIconImage;

    private void Awake()
    {
        // [FIX] Try to find the icon image in children. 
        // In CardSlotUI prefab, it's typically named "Icon" or found via GetComponentInChildren.
        var images = GetComponentsInChildren<Image>(true);
        foreach (var img in images)
        {
            if (img.gameObject.name == "Icon" || img.gameObject.name == "iconImage")
            {
                slotIconImage = img;
                break;
            }
        }

        // Fallback: first image that isn't the background (this component is usually on the button/root)
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

        // [FIX] Hide the icon in the inventory slot during drag
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

        // [FIX] Show the icon back
        if (slotIconImage != null)
            slotIconImage.gameObject.SetActive(true);
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        // [REFINED] Click-to-Select removed to enforce Drag-and-Drop only.
        // Optional: Show item description or info here.
    }
}

