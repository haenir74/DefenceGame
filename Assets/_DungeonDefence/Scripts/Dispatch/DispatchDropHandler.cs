using UnityEngine;
using UnityEngine.EventSystems;

public class DispatchDropHandler : UniversalDropHandler, IPointerClickHandler
{
    private DispatchSlotUI slotUI;

    private void Awake()
    {
        slotUI = GetComponent<DispatchSlotUI>() ?? GetComponentInParent<DispatchSlotUI>();
    }

    

    public void OnPointerClick(PointerEventData eventData)
    {
        
    }
}



