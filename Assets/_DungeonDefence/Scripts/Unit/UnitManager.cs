using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class UnitManager : Singleton<UnitManager>
{
    [Header("Settings")]
    [SerializeField] private Transform unitContainer;

    private List<Unit> activeUnits = new List<Unit>();

    public event Action<int, int> OnUnitCountChanged;
    public event Action<Unit> OnUnitDead;
    public event Action<float, float> OnCoreHpChanged;

    public void Initialize()
    {
        // Any initialization needed
    }

    private void Update()
    {
        for (int i = activeUnits.Count - 1; i >= 0; i--)
        {
            var unit = activeUnits[i];
            if (unit != null && unit.gameObject.activeSelf && !unit.IsDead)
            {
                unit.OnUpdate();
            }
        }
    }

    public Unit SpawnUnit(UnitDataSO data, GridNode node)
    {
        if (data == null || data.prefab == null || node == null) return null;

        Unit newUnit = null;

        if (PoolManager.Instance != null)
        {
            Unit prefabComp = data.prefab.GetComponent<Unit>();
            if (prefabComp != null)
                newUnit = PoolManager.Instance.Pop(prefabComp);
        }

        if (newUnit == null)
        {
            GameObject obj = Instantiate(data.prefab);
            newUnit = obj.GetComponent<Unit>();
        }

        if (newUnit != null)
        {
            newUnit.gameObject.SetActive(true);
            newUnit.transform.SetParent(this.unitContainer);
            newUnit.transform.position = node.WorldPosition;
            newUnit.Initialize(data, node);

            RegisterUnit(newUnit);
        }

        return newUnit;
    }

    public Unit SpawnUnit(UnitDataSO data, int x, int y)
    {
        GridNode node = GridManager.Instance.GetNode(x, y);
        return SpawnUnit(data, node);
    }

    public void RegisterUnit(Unit unit)
    {
        if (!activeUnits.Contains(unit))
        {
            activeUnits.Add(unit);
            NotifyUnitCount();
            if (unit.Data != null && unit.Data.category == UnitCategory.Core)
            {
                NotifyCoreHp(unit.Combat.CurrentHp, unit.Combat.MaxHp);
                unit.Combat.OnHpChanged += (hp) => NotifyCoreHp(hp, unit.Combat.MaxHp);
            }
        }
    }

    public void UnregisterUnit(Unit unit)
    {
        if (activeUnits.Contains(unit))
        {
            activeUnits.Remove(unit);
            NotifyUnitCount();

            if (unit.IsDead)
            {
                OnUnitDead?.Invoke(unit);
            }
        }
    }

    private void NotifyCoreHp(float current, float max)
    {
        OnCoreHpChanged?.Invoke(current, max);
    }

    private void NotifyUnitCount()
    {
        int playerCount = this.activeUnits.Count(u => u.IsPlayerTeam && !u.IsDead);
        int enemyCount = this.activeUnits.Count(u => !u.IsPlayerTeam && !u.IsDead);

        OnUnitCountChanged?.Invoke(playerCount, enemyCount);
    }

    public Unit GetOpponentAt(Vector2Int coord, bool myTeam)
    {
        return activeUnits.FirstOrDefault(u =>
            u.Coordinate == coord &&
            !u.IsDead &&
            u.IsPlayerTeam != myTeam &&
            !u.IsDispatched
        );
    }

    public int GetEnemyCount()
    {
        return activeUnits.Count(u => !u.IsPlayerTeam && !u.IsDead && !u.IsDispatched);
    }

    public List<Unit> GetAllUnits() => activeUnits;

    public void MoveUnit(Unit unit, GridNode from, GridNode to)
    {
        // Can be used to update nodes if we cache Unit by Node
    }

    public List<Unit> GetUnitsOnNode(GridNode node)
    {
        if (node == null) return new List<Unit>();
        return activeUnits.FindAll(u => u.Coordinate == node.Coordinate && !u.IsDead);
    }

    public void AttackUnit(Unit attacker, Unit target)
    {
        if (attacker != null && !attacker.IsDead && target != null && !target.IsDead)
        {
            attacker.Combat.Attack(target);
        }
    }

    public void DamageUnit(Unit target, float amount)
    {
        if (target != null && !target.IsDead)
        {
            target.Combat.TakeDamage(amount);
        }
    }

    public void HealUnit(Unit unit, float amount)
    {
        if (unit != null && !unit.IsDead)
        {
            unit.Combat.Heal(amount);
        }
    }

    public float GetUnitHpRatio(Unit unit)
    {
        if (unit != null && !unit.IsDead)
        {
            return unit.Combat.GetHpRatio();
        }
        return 0f;
    }
}
