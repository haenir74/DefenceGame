using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class GridNode
{
    [SerializeField] private int x;
    [SerializeField] private int y;
    [SerializeField] private Vector3 worldPosition;

    public TileView CurrentTile { get; set; }
    public GridTile Tile { get; set; }

    public int Attractive { get; set; } = -9999;
    private List<Unit> _unitsOnTile = new List<Unit>();

    public int X => x;
    public int Y => y;
    public Vector3 WorldPosition => worldPosition;
    public List<Unit> UnitsOnTile => _unitsOnTile;

    public GridNode(int x, int y, Vector3 worldPosition)
    {
        this.x = x;
        this.y = y;
        this.worldPosition = worldPosition;
    }

    public void OnUnitEnter(Unit unit)
    {
        if (!_unitsOnTile.Contains(unit))
        {
            _unitsOnTile.Add(unit);
        }
        CheckBattleCondition();
    }

    public void OnUnitExit(Unit unit)
    {
        if (_unitsOnTile.Contains(unit))
        {
            _unitsOnTile.Remove(unit);
        }
        CheckBattleCondition();
    }

    public void CheckBattleCondition()
    {
        bool hasPlayer = _unitsOnTile.Any(u => u.IsPlayerTeam && !u.Combat.IsDead);
        bool hasEnemy = _unitsOnTile.Any(u => !u.IsPlayerTeam && !u.Combat.IsDead);

        bool isBattlePhase = hasPlayer && hasEnemy;

        foreach (var unit in _unitsOnTile)
        {
            if (unit.Combat.IsDead) continue;

            if (isBattlePhase)
                unit.StartCombat();
            else
                unit.EndCombat();
        }
    }
}