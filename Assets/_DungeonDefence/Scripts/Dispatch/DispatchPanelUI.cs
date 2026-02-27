using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DispatchPanelUI : Singleton<DispatchPanelUI>
{




    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private TextMeshProUGUI totalBonusText;
    [SerializeField] private TextMeshProUGUI headerText;

    private List<DispatchSlotUI> slots = new List<DispatchSlotUI>();

    protected override void Awake()
    {
        base.Awake();

        slots.Clear();


        if (GetComponent<DispatchDropHandler>() == null)
            gameObject.AddComponent<DispatchDropHandler>();
    }

    private void CreateSlots()
    {

    }


    public DispatchSlotUI CreateSlotAndAssign(DragPayload payload)
    {
        if (slotPrefab == null || slotsContainer == null) return null;

        GameObject obj = Instantiate(slotPrefab, slotsContainer);
        var slot = obj.GetComponentInChildren<DispatchSlotUI>();

        if (slot != null)
        {
            slot.Initialize(this);
            slots.Add(slot);


            if (slot.gameObject.GetComponent<DispatchDropHandler>() == null)
                slot.gameObject.AddComponent<DispatchDropHandler>();



            bool assigned = false;
            if (payload.UnitData != null || payload.GridUnit != null)
            {
                UnitDataSO data = payload.UnitData ?? payload.GridUnit?.Data;
                slot.AssignUnitData(data);
                assigned = true;
            }

            if (assigned)
            {
                RefreshBonusDisplay();
                UpdateScrollPosition();
                return slot;
            }
        }

        Destroy(obj);
        return null;
    }

    private void UpdateScrollPosition()
    {

        if (scrollRect == null || scrollRect.content == null) return;


        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }



    public void OnSlotChanged()
    {

        slots.RemoveAll(s => s == null);



        RefreshBonusDisplay();
    }

    private void RefreshBonusDisplay()
    {
        if (totalBonusText == null) return;
        int total = 0;
        foreach (var s in slots)
        {
            if (s != null) total += s.GetDispatchBonus();
        }
        totalBonusText.text = $"파견 보너스: +{total}G";
    }


    public void ClearAllSlots()
    {
        foreach (var slot in slots)
        {
            if (slot != null)
            {
                slot.ClearSlot();
                Destroy(slot.transform.parent.gameObject);
            }
        }
        slots.Clear();
        RefreshBonusDisplay();
    }


    public int GetTotalDispatchBonus()
    {
        int total = 0;
        foreach (var s in slots) total += s.GetDispatchBonus();
        return total;
    }
}



