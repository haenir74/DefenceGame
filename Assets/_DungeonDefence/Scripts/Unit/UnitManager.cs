using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class UnitManager : Singleton<UnitManager>
{
    private UnitController _controller; 
    private UnitRegistry _registry = new UnitRegistry();

    public event Action<Unit> OnUnitSpawned;
    public event Action<Unit> OnUnitDied;

    protected override void Awake()
    {
        base.Awake();
        _registry.OnUnitRegistered += (u) => OnUnitSpawned?.Invoke(u);
        _registry.OnUnitUnregistered += (u) => OnUnitDied?.Invoke(u);
    }

    public void Initialize(UnitController controller)
    {
        this._controller = controller;
        if (_controller == null) return;
    }

    // UnitRegistry
    public void RegisterUnit(Unit unit) => _registry.Register(unit);
    public void UnregisterUnit(Unit unit) => _registry.Unregister(unit);
    public void MoveUnit(Unit unit, GridNode fromNode, GridNode toNode) => _registry.Move(unit, fromNode, toNode);
    public List<Unit> GetUnitsOnNode(GridNode node) => _registry.GetUnitsAt(node);
    public List<Unit> GetAllUnits() => _registry.GetAllUnits();
    public int GetEnemyCount() => _registry.GetEnemyCount();

    public Unit SpawnUnit(UnitDataSO data, GridNode node)
    {
        if (_controller == null) return null;
        return _controller.SpawnUnit(data, node);
    }
}
