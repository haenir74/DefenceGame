using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class UnitManager : Singleton<UnitManager>
{

    [SerializeField] private Transform unitContainer;

    private List<Unit> activeUnits = new List<Unit>();
    private Dictionary<UnitTag, HashSet<Unit>> unitTagRegistry = new Dictionary<UnitTag, HashSet<Unit>>();

    public event Action<int, int> OnUnitCountChanged;
    public event Action<Unit> OnUnitSpawned;
    public event Action<Unit> OnUnitDead;
    public event Action<Unit> OnUnitDespawned;
    public event Action<float, float> OnCoreHpChanged;

    public void Initialize()
    {
        if (UnitManager.IsQuitting) return;
        if (unitContainer == null)
        {
            var containerObj = GameObject.Find("UnitContainer");
            if (containerObj == null)
                containerObj = new GameObject("UnitContainer");
            unitContainer = containerObj.transform;
        }
    }

    public void Reset()
    {
        activeUnits.Clear();
        NotifyUnitCount();
    }

    private void Update()
    {
    }

    public IReadOnlyCollection<Unit> GetUnitsByTag(UnitTag tag)
    {
        if (unitTagRegistry.TryGetValue(tag, out var set))
            return set;
        return Array.Empty<Unit>();
    }

    public Unit SpawnUnit(UnitDataSO data, GridNode node)
    {
        if (data == null)
        {

            return null;
        }
        if (data.prefab == null)
        {

            return null;
        }
        if (node == null)
        {

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


            bool isCore = data.category == UnitCategory.Core;
            if (isCore)
            {
                Vector3 center = node.WorldPosition;
                newUnit.transform.position = new Vector3(center.x, UnitConstants.UNIT_HEIGHT, center.z);

            }
            else
            {

                float cellSize = GridManager.Instance?.Data?.cellSize ?? 1f;
                Vector3? slotPos = node.TryOccupySlot(newUnit, cellSize);
                Vector3 rawPos = slotPos ?? node.WorldPosition;
                newUnit.transform.position = new Vector3(rawPos.x, UnitConstants.UNIT_HEIGHT, rawPos.z);
            }

            newUnit.Initialize(data, node);


            int interactionLayer = LayerMask.NameToLayer("Unit");
            if (interactionLayer == -1) interactionLayer = LayerMask.NameToLayer("Allies");
            if (interactionLayer == -1) interactionLayer = 0;

            SetLayerRecursive(newUnit.gameObject, interactionLayer);


            var col = newUnit.GetComponent<Collider>();
            if (col == null) col = newUnit.gameObject.AddComponent<BoxCollider>();
            if (col is BoxCollider box)
            {
                box.center = new Vector3(0, 0.5f, 0);
                box.size = new Vector3(0.6f, 1f, 0.6f);
            }


            if (newUnit.IsPlayerTeam && data.category != UnitCategory.Core)
            {
                if (newUnit.GetComponent<GridUnitDragHandler>() == null)
                    newUnit.gameObject.AddComponent<GridUnitDragHandler>();
            }

            RegisterUnit(newUnit);
            OnUnitSpawned?.Invoke(newUnit);
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
        
        if (unit.Data != null)
        {
            foreach (UnitTag tag in Enum.GetValues(typeof(UnitTag)))
            {
                if (tag != UnitTag.None && unit.Data.HasTag(tag))
                {
                    if (!unitTagRegistry.ContainsKey(tag))
                        unitTagRegistry[tag] = new HashSet<Unit>();
                    unitTagRegistry[tag].Add(unit);
                }
            }
        }
        
        NotifyUnitCount();


        if (unit.Data != null && unit.Data.category == UnitCategory.Core)
        {

            NotifyCoreHp(unit.Combat.CurrentHp, unit.Data.maxHp);


            unit.Combat.OnHpChanged -= OnCoreHpChangedCallback;
            unit.Combat.OnHpChanged += OnCoreHpChangedCallback;
        }
    }

    private void OnCoreHpChangedCallback(float hp)
    {

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
            
            if (unit.Data != null)
            {
                foreach (UnitTag tag in Enum.GetValues(typeof(UnitTag)))
                {
                    if (tag != UnitTag.None && unit.Data.HasTag(tag))
                    {
                        if (unitTagRegistry.ContainsKey(tag))
                            unitTagRegistry[tag].Remove(unit);
                    }
                }
            }
            
            NotifyUnitCount();

            if (unit.IsDead)
            {
                OnUnitDead?.Invoke(unit);
            }
            OnUnitDespawned?.Invoke(unit);
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

        if (from != null)
            from.ReleaseSlot(unit);

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


    public void DespawnUnit(Unit unit)
    {
        if (unit == null) return;


        unit.CurrentNode?.ReleaseSlot(unit);


        UnregisterUnit(unit);


        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Despawn(unit);
        }
        else
        {
            Destroy(unit.gameObject);
        }
    }

    private void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }
}



