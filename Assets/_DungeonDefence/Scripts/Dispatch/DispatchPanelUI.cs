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

    private void Start()
    {
        if (DispatchManager.Instance != null)
        {
            DispatchManager.Instance.OnDispatchStateChanged += Refresh;
            Refresh();
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (DispatchManager.Instance != null)
        {
            DispatchManager.Instance.OnDispatchStateChanged -= Refresh;
        }
    }

    public void Refresh()
    {
        if (DispatchManager.Instance == null) return;
        var entries = DispatchManager.Instance.DispatchSlots;

        while(slots.Count < entries.Count)
        {
            GameObject obj = Instantiate(slotPrefab, slotsContainer);
            var slot = obj.GetComponentInChildren<DispatchSlotUI>();
            if (slot.gameObject.GetComponent<DispatchDropHandler>() == null)
                slot.gameObject.AddComponent<DispatchDropHandler>();
            slot.Initialize(this);
            slots.Add(slot);
        }
        while(slots.Count > entries.Count)
        {
            var s = slots[slots.Count - 1];
            slots.RemoveAt(slots.Count - 1);
            if(s != null && s.gameObject != null) Destroy(s.gameObject);
        }

        for (int i = 0; i < entries.Count; i++)
        {
            slots[i].Refresh(i, entries[i]);
        }

        RefreshBonusDisplay();
        UpdateScrollPosition();
    }

    public DispatchSlotUI CreateSlotAndAssign(DragPayload payload)
    {
        if (payload.UnitData != null)
            DispatchManager.Instance.RequestAssignData(-1, payload.UnitData);
        else if (payload.GridUnit != null)
            DispatchManager.Instance.RequestAssignUnit(-1, payload.GridUnit);

        if (slots.Count > 0) return slots[slots.Count - 1];
        return null;
    }

    private void UpdateScrollPosition()
    {
        if (scrollRect == null || scrollRect.content == null) return;
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void OnSlotChanged() { }

    private void RefreshBonusDisplay()
    {
        if (totalBonusText == null) return;
        int total = DispatchManager.Instance.CalculateTotalBonus();
        totalBonusText.text = $"파견 보너스: +{total}G";
    }

    public void ClearAllSlots()
    {
        for (int i = slots.Count - 1; i >= 0; i--)
            DispatchManager.Instance.RequestRecall(i);
    }

    public int GetTotalDispatchBonus()
    {
        return DispatchManager.Instance.CalculateTotalBonus();
    }
}



