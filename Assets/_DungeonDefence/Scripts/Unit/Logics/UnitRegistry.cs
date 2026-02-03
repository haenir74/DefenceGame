using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class UnitRegistry
{
    private List<Unit> _activeUnits = new List<Unit>();
    private Dictionary<GridNode, List<Unit>> _nodeUnitMap = new Dictionary<GridNode, List<Unit>>();
    
    public event Action<Unit> OnUnitRegistered;
    public event Action<Unit> OnUnitUnregistered;

    public void Register(Unit unit)
    {
        if (!_activeUnits.Contains(unit))
        {
            _activeUnits.Add(unit);
            OnUnitRegistered?.Invoke(unit);
        }
    }

    public void Unregister(Unit unit)
    {
        if (_activeUnits.Contains(unit))
        {
            RemoveFromNode(unit, unit.CurrentNode);
            _activeUnits.Remove(unit);
            OnUnitUnregistered?.Invoke(unit);
        }
    }

    public void Move(Unit unit, GridNode fromNode, GridNode toNode)
    {
        if (fromNode != null)
        {
            RemoveFromNode(unit, fromNode);
        }

        if (toNode != null)
        {
            AddToNode(unit, toNode);
            UpdateBattleState(toNode);
        }
    }

    public List<Unit> GetUnitsAt(GridNode node)
    {
        if (node != null && _nodeUnitMap.TryGetValue(node, out var list))
            return list;
        return new List<Unit>();
    }

    public List<Unit> GetAllUnits() => _activeUnits;

    public int GetEnemyCount()
    {
        return _activeUnits.Count(u => !u.IsPlayerTeam && !u.Combat.IsDead);
    }

    private void AddToNode(Unit unit, GridNode node)
    {
        if (!_nodeUnitMap.ContainsKey(node))
            _nodeUnitMap[node] = new List<Unit>();

        if (!_nodeUnitMap[node].Contains(unit))
            _nodeUnitMap[node].Add(unit);
    }

    private void RemoveFromNode(Unit unit, GridNode node)
    {
        if (node != null && _nodeUnitMap.ContainsKey(node))
        {
            _nodeUnitMap[node].Remove(unit);
            UpdateBattleState(node);
        }
    }

    private void UpdateBattleState(GridNode node)
    {
        if (node == null || !_nodeUnitMap.ContainsKey(node)) return;

        var units = _nodeUnitMap[node];
        
        var aliveUnits = units.Where(u => u != null && !u.Combat.IsDead).ToList();

        bool hasPlayer = aliveUnits.Any(u => u.IsPlayerTeam);
        bool hasEnemy = aliveUnits.Any(u => !u.IsPlayerTeam);
        bool isBattle = hasPlayer && hasEnemy;

        foreach (var unit in aliveUnits)
        {
            if (isBattle) unit.StartCombat();
            else unit.EndCombat();
        }
    }
}