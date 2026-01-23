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
        if (controller == null)
        {
            Debug.LogWarning("InventoryController를 찾을 수 없습니다.");
            return;
        }

        var slotData = controller.GetSlot(index);
        if (slotData == null || slotData.IsEmpty) return;
        if (slotData.ItemData is BaseItemSO gameItem)
        {
            Debug.Log($"[Inventory Click] Selected Item: {gameItem.ID}");
            GameManager.Instance.ChangeState(new PlacementState(gameItem));
            controller.CloseInventory(); 
        }
    }
}