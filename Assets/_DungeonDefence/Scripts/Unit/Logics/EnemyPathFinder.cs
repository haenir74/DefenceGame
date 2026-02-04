using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPathFinder : MonoBehaviour
{
    private Unit unit;
    private Vector2Int? currentTarget;

    private void Awake()
    {
        this.unit = GetComponent<Unit>();
    }

    public void OnReachTile()
    {
        currentTarget = null;
    }

    public void FindNextStep()
    {
        if (unit == null) return;
        Vector2Int nextPos = GridManager.Instance.GetNextPosition(unit.Coordinate);
        if (nextPos != unit.Coordinate) currentTarget = nextPos;
        else currentTarget = null;
    }

    public Vector2Int? GetTargetStep()
    {
        if (currentTarget == null) FindNextStep();
        return currentTarget;
    }
}