using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Panex.Inventory.View;
using Panex.Inventory.Controller;
using Panex.Inventory;

[RequireComponent(typeof(SlotUI))]
public class InventorySlotClickListener : MonoBehaviour
{
    private SlotUI slotUI;
    private InventoryController controller;

    private void Awake()
    {
        slotUI = GetComponent<SlotUI>();
        controller = GetComponentInParent<InventoryController>();
    }

    private void Start()
    {
        if (slotUI != null)
        {
            slotUI.OnClickAction += HandleClick;
        }
    }

    private void OnDestroy()
    {
        if (slotUI != null)
        {
            slotUI.OnClickAction -= HandleClick;
        }
    }

    private void HandleClick(int index)
    {
        if (controller == null) return;

        var slotData = controller.GetSlot(index);
        
        if (slotData != null && !slotData.IsEmpty)
        {
            if (slotData.ItemData is BaseItemSO gameItem)
            {
                Debug.Log($"[Click] Selected: {gameItem.Name} (ID: {gameItem.ID})");
                
                // [수정] 컨트롤러와 인덱스 정보를 함께 전달!
                GameManager.Instance.ChangeState(new PlacementState(gameItem, controller, index));
            }
        }
    }
}