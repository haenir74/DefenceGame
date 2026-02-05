using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UnitManager : Singleton<UnitManager>
{
    private UnitController controller;

    public event Action<int, int> OnUnitCountChanged;
    public event Action<Unit> OnUnitDead;
    public event Action<float, float> OnCoreHpChanged;

    public void Initialize(UnitController controller)
    {
        this.controller = controller;

        if (this.controller != null)
        {
            this.controller.OnUnitCountChanged += (p, e) => OnUnitCountChanged?.Invoke(p, e);
            this.controller.OnUnitDead += (unit) => OnUnitDead?.Invoke(unit);
            this.controller.OnCoreHpChanged += (cur, max) => OnCoreHpChanged?.Invoke(cur, max);
        }
    }

    private void Update()
    {
        if (controller != null)
            controller.OnUpdate();
    }

    public Unit SpawnUnit(UnitDataSO data, GridNode node)
    {
        return controller?.SpawnUnit(data, node);
    }

    public Unit SpawnUnit(UnitDataSO data, int x, int y)
    {
        GridNode node = GridManager.Instance.GetNode(x, y);
        return SpawnUnit(data, node);
    }

    public void RegisterUnit(Unit unit) => controller?.RegisterUnit(unit);
    public void UnregisterUnit(Unit unit) => controller?.UnregisterUnit(unit);        

    public Unit GetOpponentAt(Vector2Int coord, bool myTeam) 
        => controller?.GetOpponentAt(coord, myTeam);

    public int GetEnemyCount() 
        => controller != null ? controller.GetEnemyCount() : 0;

    public List<Unit> GetAllUnits() 
        => controller?.GetAllUnits();

    public void MoveUnit(Unit unit, GridNode from, GridNode to)
    {
        // 필요하다면 구현
    }

    public List<Unit> GetUnitsOnNode(GridNode node)
    {
         if(controller == null || node == null) return new List<Unit>();
         var allUnits = controller.GetAllUnits();
         return allUnits.FindAll(u => u.Coordinate == node.Coordinate && !u.IsDead);
    }

    public void AttackUnit(Unit attacker, Unit target)
    {
        this.controller?.AttackUnit(attacker, target);
    }
    
    public void DamageUnit(Unit target, float amount)
    {
        this.controller?.DamageUnit(target, amount);
    }

    public void HealUnit(Unit unit, float amount)
    {
        this.controller?.HealUnit(unit, amount);
    }

    public float GetUnitHpRatio(Unit unit)
    {
        return this.controller != null ? this.controller.GetUnitHpRatio(unit) : 0f;
    }
}
