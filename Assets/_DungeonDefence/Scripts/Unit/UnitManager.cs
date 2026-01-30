using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UnitManager : Singleton<UnitManager>
{
    private UnitController _controller;
    private List<Unit> _activeUnits = new List<Unit>();

    public event Action<Unit> OnUnitSpawned;
    public event Action<Unit> OnUnitDied;

    public void Initialize(UnitController controller)
    {
        this._controller = controller;
        if (_controller == null) return;
    }

    public void RegisterUnit(Unit unit)
    {
        if (!_activeUnits.Contains(unit))
        {
            _activeUnits.Add(unit);
            OnUnitSpawned?.Invoke(unit);
        }
    }

    public void UnregisterUnit(Unit unit)
    {
        if (_activeUnits.Contains(unit))
        {
            _activeUnits.Remove(unit);
            OnUnitDied?.Invoke(unit);
        }
    }

    public Unit SpawnUnit(UnitDataSO data, GridNode node)
    {
        if (_controller == null) return null;
        return _controller.SpawnUnit(data, node);
    }

    public List<Unit> GetAllUnits() => _activeUnits;
}
