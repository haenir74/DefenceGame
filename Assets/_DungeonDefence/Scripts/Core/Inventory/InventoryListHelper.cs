using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Panex.Inventory.View; // SlotUI 참조

public class InventoryListHelper : MonoBehaviour
{
    public void RefreshSlots()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(RefreshRoutine());
        }
    }

    private IEnumerator RefreshRoutine()
    {
        yield return null;
        var slots = GetComponentsInChildren<SlotUI>(true);

        foreach (var slot in slots)
        {
            bool hasItem = IsSlotOccupied(slot);
            slot.gameObject.SetActive(hasItem);
        }
    }

    private bool IsSlotOccupied(SlotUI slot)
    {
        return slot.HasItem;
    }
}