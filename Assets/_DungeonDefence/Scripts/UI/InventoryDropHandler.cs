using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryDropHandler : UniversalDropHandler
{
    

    private void HandleGridUnitRecall(Unit unit)
    {
        if (unit == null || unit.Data.category == UnitCategory.Core) return;

        
        InventoryManager.Instance.AddItem(unit.Data, 1);

        
        UnitManager.Instance.DespawnUnit(unit);

        
    }

}



