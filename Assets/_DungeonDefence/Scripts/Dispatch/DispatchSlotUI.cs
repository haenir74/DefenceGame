using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class DispatchSlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image unitIconImage;
    [SerializeField] private TextMeshProUGUI unitNameText;
    [SerializeField] private TextMeshProUGUI bonusText;
    [SerializeField] private Button recallButton;
    [SerializeField] private Image emptyIndicator;

    private int slotIndex = -1;
    private DispatchPanelUI panel;
    private bool hasUnit = false;

    public bool IsEmpty => !hasUnit;

    public void Initialize(DispatchPanelUI panel)
    {
        this.panel = panel;
        if (recallButton != null)
        {
            recallButton.onClick.RemoveAllListeners();
            recallButton.onClick.AddListener(OnRecallClicked);
        }
    }

    public void Refresh(int index, DispatchManager.DispatchEntry entry)
    {
        this.slotIndex = index;

        hasUnit = entry != null && !entry.IsEmpty;
        UnitDataSO displayData = entry?.Data;
        if (entry?.Unit != null) displayData = entry.Unit.Data;

        if (unitIconImage != null)
        {
            unitIconImage.gameObject.SetActive(hasUnit);
            if (hasUnit && displayData?.icon != null)
                unitIconImage.sprite = displayData.icon;
        }

        if (unitNameText != null)
            unitNameText.text = hasUnit ? displayData?.Name ?? "" : "";

        if (bonusText != null)
        {
            if (hasUnit && displayData != null)
                bonusText.text = $"효율: {(displayData.dispatchEfficiency * 100f):F0}%";
            else
                bonusText.text = "";
        }

        if (recallButton != null)
            recallButton.gameObject.SetActive(hasUnit);

        if (emptyIndicator != null)
            emptyIndicator.gameObject.SetActive(!hasUnit);

        var tooltip = gameObject.GetComponent<UITooltipTrigger>();
        if (tooltip == null) tooltip = gameObject.AddComponent<UITooltipTrigger>();
        tooltip.DataProvider = () => displayData;
    }

    public bool TryAssignUnit(Unit unit)
    {
        return DispatchManager.Instance.RequestAssignUnit(slotIndex, unit);
    }

    public void AssignUnitData(UnitDataSO data)
    {
        DispatchManager.Instance.RequestAssignData(slotIndex, data);
    }

    private void OnRecallClicked()
    {
        DispatchManager.Instance.RequestRecall(slotIndex);
    }

    public void ClearSlot(bool returnToInventory = true)
    {
        DispatchManager.Instance.RequestRecall(slotIndex, returnToInventory);
    }

    public int GetDispatchBonus()
    {
        return 0;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (slotIndex < 0 || !hasUnit || DispatchManager.Instance == null) return;
        var entries = DispatchManager.Instance.DispatchSlots;
        if (slotIndex >= entries.Count) return;

        var entry = entries[slotIndex];
        UnitDataSO data = entry.Unit?.Data ?? entry.Data;

        if (data != null && DragDropManager.Instance != null)
        {
            var payload = new DragPayload();
            payload.Source = DragPayload.SourceType.Dispatch;
            payload.UnitData = data;
            payload.FromSlot = this;

            DragDropManager.Instance.BeginDrag(payload, unitIconImage.sprite);
            SetVisualVisible(false);
            GameManager.Instance?.SelectUnitToPlace(data, GameManager.SelectionSource.Dispatch);
        }
    }

    private void SetVisualVisible(bool visible)
    {
        if (unitIconImage != null) unitIconImage.gameObject.SetActive(visible);
        if (unitNameText != null) unitNameText.gameObject.SetActive(visible);
        if (bonusText != null) bonusText.gameObject.SetActive(visible);
        if (recallButton != null) recallButton.gameObject.SetActive(visible && hasUnit);
        if (emptyIndicator != null) emptyIndicator.gameObject.SetActive(visible && !hasUnit);
        if (unitIconImage != null) unitIconImage.raycastTarget = visible;
    }

    public void RestoreSlot()
    {
        SetVisualVisible(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (DragDropManager.Instance != null)
            DragDropManager.Instance.UpdateGhostPosition(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (DragDropManager.Instance != null)
            DragDropManager.Instance.EndDrag();
    }
}



