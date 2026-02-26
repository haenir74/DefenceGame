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

    private Unit assignedUnit;       
    private UnitDataSO assignedData; 
    private DispatchPanelUI panel;

    public bool IsEmpty => assignedUnit == null && assignedData == null;

    public void Initialize(DispatchPanelUI panel)
    {
        this.panel = panel;
        if (recallButton != null)
            recallButton.onClick.AddListener(OnRecallClicked);
        Refresh();
    }

    

    public bool TryAssignUnit(Unit unit)
    {
        if (unit == null || !unit.IsPlayerTeam || unit.Data.category == UnitCategory.Core) return false;
        if (!IsEmpty) return false;

        assignedUnit = unit;
        assignedData = null;
        unit.SetDispatchMode(true);
        Refresh();
        panel?.OnSlotChanged();
        return true;
    }

    

    public void AssignUnitData(UnitDataSO data)
    {
        if (data == null) return;
        assignedData = data;
        assignedUnit = null;
        Refresh();
        panel?.OnSlotChanged();
    }

    

    private void OnRecallClicked()
    {
        if (assignedUnit != null)
        {
            assignedUnit.SetDispatchMode(false);
            assignedUnit = null;
        }
        else if (assignedData != null)
        {
            InventoryManager.Instance?.AddItem(assignedData, 1);
            assignedData = null;
        }

        panel?.OnSlotChanged();

        
        if (transform.parent != null)
            Destroy(transform.parent.gameObject); 
        else
            Destroy(gameObject);
    }

    public void ClearSlot(bool returnToInventory = true)
    {
        if (assignedUnit != null)
        {
            assignedUnit.SetDispatchMode(false);
        }
        else if (assignedData != null && returnToInventory)
        {
            InventoryManager.Instance?.AddItem(assignedData, 1);
        }

        assignedUnit = null;
        assignedData = null;

        
        if (transform.parent != null)
            Destroy(transform.parent.gameObject);
        else
            Destroy(gameObject);
    }

    

    public int GetDispatchBonus()
    {
        if (DispatchManager.Instance == null) return 0;
        return DispatchManager.Instance.CalculateUnitBonus(assignedUnit, assignedData);
    }

    

    private void Refresh()
    {
        bool hasUnit = !IsEmpty;
        UnitDataSO displayData = assignedUnit?.Data ?? assignedData;

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
                bonusText.text = $"?⑥쑉: {(displayData.dispatchEfficiency * 100f):F0}%";
            else
                bonusText.text = "";
        }

        if (recallButton != null)
            recallButton.gameObject.SetActive(hasUnit);

        if (emptyIndicator != null)
            emptyIndicator.gameObject.SetActive(!hasUnit);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsEmpty) return;

        UnitDataSO data = assignedUnit?.Data ?? assignedData;
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
        if (recallButton != null) recallButton.gameObject.SetActive(visible && !IsEmpty);
        if (emptyIndicator != null) emptyIndicator.gameObject.SetActive(visible && IsEmpty);

        
        
        if (unitIconImage != null) unitIconImage.raycastTarget = visible;
    }

    
    public void RestoreSlot()
    {
        SetVisualVisible(true);
        Refresh();
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



