using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 파견 슬롯 드롭 수신기.
/// DispatchSlot 루트 GameObject에 추가한다.
/// 인벤토리 드래그, 그리드 유닛 드래그를 모두 받는다.
/// </summary>
public class DispatchDropHandler : UniversalDropHandler, IPointerClickHandler
{
    private DispatchSlotUI slotUI;

    private void Awake()
    {
        slotUI = GetComponent<DispatchSlotUI>() ?? GetComponentInParent<DispatchSlotUI>();
    }

    // OnDrop is inherited from UniversalDropHandler

    public void OnPointerClick(PointerEventData eventData)
    {
        // [REFINED] Click-to-Select removed.
    }
}
