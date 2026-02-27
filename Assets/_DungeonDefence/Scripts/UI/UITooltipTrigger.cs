using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class UITooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float hoverDelay = 0.4f;
    
    // We can either pass the data directly or have a way to fetch it from the sibling/parent component
    public System.Func<UnitDataSO> DataProvider;
    
    private Coroutine hoverCoroutine;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (DragDropManager.Instance != null && DragDropManager.Instance.IsDragging) return;
        
        if (hoverCoroutine != null) StopCoroutine(hoverCoroutine);
        hoverCoroutine = StartCoroutine(ShowTooltipAfterDelay());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CancelTooltip();
    }

    private IEnumerator ShowTooltipAfterDelay()
    {
        yield return new WaitForSeconds(hoverDelay);
        
        UnitDataSO data = DataProvider?.Invoke();
        if (TooltipManager.Instance != null && data != null)
        {
            TooltipManager.Instance.ShowTooltip(data);
        }
        
        hoverCoroutine = null;
    }

    private void CancelTooltip()
    {
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
            hoverCoroutine = null;
        }

        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }

    private void OnDisable()
    {
        CancelTooltip();
    }
}
