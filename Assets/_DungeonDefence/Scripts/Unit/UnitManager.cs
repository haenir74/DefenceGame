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

            // 슬롯 배정: 빈 슬롯 위치로 스폰
            float cellSize = GridManager.Instance?.Data?.cellSize ?? 1f;
            Vector3? slotPos = node.TryOccupySlot(newUnit, cellSize);
            newUnit.transform.position = slotPos ?? node.WorldPosition;

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
        if (unit == null || activeUnits.Contains(unit)) return;

        activeUnits.Add(unit);
        NotifyUnitCount();

        // 코어 유닛 특수 처리
        if (unit.Data != null && unit.Data.category == UnitCategory.Core)
        {
            NotifyCoreHp(unit.Combat.CurrentHp, unit.Combat.MaxHp);

            // 기존 이벤트 제거 후 등록 (중복 방지)
            unit.Combat.OnHpChanged -= OnCoreHpChangedCallback;
            unit.Combat.OnHpChanged += OnCoreHpChangedCallback;
        }
    }

    private void OnCoreHpChangedCallback(float hp)
    {
        // 모든 코어 중 첫 번째 것의 체력을 UI에 표시 (보통 코어는 1개)
        var core = activeUnits.FirstOrDefault(u => u.Data != null && u.Data.category == UnitCategory.Core);
        if (core != null)
        {
            NotifyCoreHp(hp, core.Combat.MaxHp);
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

    /// <summary>같은 타일의 상대 팀 유닛 중 무작위 1명 반환 (전투 시 랜덤 타겟 선택용)</summary>
    public Unit GetRandomOpponentAt(Vector2Int coord, bool myTeam)
    {
        List<Unit> opponents = activeUnits.FindAll(u =>
            u.Coordinate == coord &&
            !u.IsDead &&
            u.IsPlayerTeam != myTeam &&
            !u.IsDispatched
        );
        if (opponents.Count == 0) return null;
        return opponents[UnityEngine.Random.Range(0, opponents.Count)];
    }

    public int GetEnemyCount()
    {
        return activeUnits.Count(u => !u.IsPlayerTeam && !u.IsDead && !u.IsDispatched);
    }

    public List<Unit> GetAllUnits() => activeUnits;

    public void MoveUnit(Unit unit, GridNode from, GridNode to)
    {
        // 이전 노드 슬롯 해제
        if (from != null)
            from.ReleaseSlot(unit);
        // 새 노드 슬롯 배정은 UnitMovement.OnUpdate()에서 도착 시 처리
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
