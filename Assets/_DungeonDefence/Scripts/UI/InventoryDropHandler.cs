using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 인벤토리 패널 배경에 붙어 드롭을 수신하는 컴포넌트.
/// 그리드 유닛을 드래그해서 여기 놓으면 인벤토리로 '회수'된다.
/// </summary>
public class InventoryDropHandler : UniversalDropHandler
{
    // OnDrop is inherited from UniversalDropHandler

    private void HandleGridUnitRecall(Unit unit)
    {
        if (unit == null || unit.Data.category == UnitCategory.Core) return;

        // 인벤토리에 추가
        InventoryManager.Instance.AddItem(unit.Data, 1);

        // 월드에서 안전하게 제거 (Despawn)
        UnitManager.Instance.DespawnUnit(unit);

        Debug.Log($"[Inventory] {unit.Data.Name} 회수 완료");
    }

}
