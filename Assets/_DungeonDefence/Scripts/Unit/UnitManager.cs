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
        if (unitContainer == null)
        {
            var containerObj = GameObject.Find("UnitContainer");
            if (containerObj == null)
                containerObj = new GameObject("UnitContainer");
            unitContainer = containerObj.transform;
        }
    }

    private void Update()
    {
        // 자폭 연쇄 반응 등으로 activeUnits 리스트가 실시간 수정될 수 있으므로
        // 리스트를 복사하여 순회함으로써 IndexOutOfRange 에러를 방지합니다.
        var unitsToUpdate = new List<Unit>(activeUnits);
        for (int i = unitsToUpdate.Count - 1; i >= 0; i--)
        {
            var unit = unitsToUpdate[i];

            // 유닛이 아직 유효하고(제거되지 않았고) 살아있는 경우에만 업데이트
            if (unit != null && unit.gameObject.activeSelf && !unit.IsDead && activeUnits.Contains(unit))
            {
                unit.OnUpdate();
            }
        }
    }

    public Unit SpawnUnit(UnitDataSO data, GridNode node)
    {
        if (data == null)
        {
            Debug.LogError("[UnitManager] SpawnUnit failed: UnitDataSO is null");
            return null;
        }
        if (data.prefab == null)
        {
            Debug.LogError($"[UnitManager] SpawnUnit failed: Prefab is null for {data.unitId}");
            return null;
        }
        if (node == null)
        {
            Debug.LogError($"[UnitManager] SpawnUnit failed: Target node is null for {data.unitId}");
            return null;
        }

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

            // 코어는 슬롯을 점유하지 않고 항상 중앙에 배치
            bool isCore = data.category == UnitCategory.Core;
            if (isCore)
            {
                Vector3 center = node.WorldPosition;
                newUnit.transform.position = new Vector3(center.x, UnitConstants.UNIT_HEIGHT, center.z);
                Debug.Log($"[UnitManager] Spawning Core at {newUnit.transform.position}");
            }
            else
            {
                // 일반 유닛: 빈 슬롯 위치로 스폰 (X·Z만 사용, Y는 UNIT_HEIGHT로 고정)
                float cellSize = GridManager.Instance?.Data?.cellSize ?? 1f;
                Vector3? slotPos = node.TryOccupySlot(newUnit, cellSize);
                Vector3 rawPos = slotPos ?? node.WorldPosition;
                newUnit.transform.position = new Vector3(rawPos.x, UnitConstants.UNIT_HEIGHT, rawPos.z);
            }

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
            Debug.Log($"[UnitManager] Core Registered: {unit.name}");
            NotifyCoreHp(unit.Combat.CurrentHp, unit.Data.maxHp);

            // 기존 이벤트 제거 후 등록 (중복 방지)
            unit.Combat.OnHpChanged -= OnCoreHpChangedCallback;
            unit.Combat.OnHpChanged += OnCoreHpChangedCallback;
        }
    }

    private void OnCoreHpChangedCallback(float hp)
    {
        // 모든 코어 중 첫 번째 것의 체력을 UI에 표시 (보통 코어는 1개)
        var core = activeUnits.FirstOrDefault(u => u != null && u.Data != null && u.Data.category == UnitCategory.Core);
        if (core != null)
        {
            NotifyCoreHp(hp, core.Data.maxHp);
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

    public void NotifyWaveClear()
    {
        // 현재 맵의 모든 유닛에게 웨이브 클리어 알림 (주로 보너스 골드 지급 등)
        for (int i = activeUnits.Count - 1; i >= 0; i--)
        {
            var unit = activeUnits[i];
            if (unit != null && !unit.IsDead)
            {
                unit.OnWaveClear();
            }
        }
    }

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

    public void DamageUnit(Unit target, float amount, Unit attacker = null)
    {
        if (target != null && !target.IsDead)
        {
            target.Combat.TakeDamage(amount, attacker);
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
