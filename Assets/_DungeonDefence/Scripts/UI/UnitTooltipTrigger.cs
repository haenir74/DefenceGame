using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Unit))]
public class UnitTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float hoverDelay = 0.4f;
    private Unit unit;
    private Coroutine hoverCoroutine;

    private void Awake()
    {
        unit = GetComponent<Unit>();
    }

    private void Update()
    {
        // Continuously check if dragging, if so hide tooltip
        if (DragDropManager.Instance != null && DragDropManager.Instance.IsDragging)
        {
            CancelTooltip();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (DragDropManager.Instance != null && DragDropManager.Instance.IsDragging) return;
        
        // Ensure only one coroutine running
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
        
        if (TooltipManager.Instance != null && unit != null)
        {
            TooltipManager.Instance.ShowTooltip(unit);
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
