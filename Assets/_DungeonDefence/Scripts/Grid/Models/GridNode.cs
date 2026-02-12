using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridNode
{
    [SerializeField] private int x;
    [SerializeField] private int y;
    [SerializeField] private Vector3 worldPosition;
    [SerializeField] private Vector2Int coordinate;

    public TileView CurrentTile { get; set; }
    public GridTile Tile { get; set; }

    public int X => x;
    public int Y => y;
    public Vector3 WorldPosition => worldPosition;
    public Vector2Int Coordinate => coordinate;

    public TileDataSO CurrentTileData { get; private set; }
    public GameObject UnitObject { get; set; }

    public GridNode(int x, int y, Vector3 worldPosition)
    {
        this.x = x;
        this.y = y;
        this.worldPosition = worldPosition;
        this.coordinate = new Vector2Int(x, y);
    }

    public int GetDistance(GridNode target)
    {
        if (target == null) return int.MaxValue;
        return Mathf.Abs(this.x - target.x) + Mathf.Abs(this.y - target.y);
    }

    public int GetTileBonus()
    {
        if (Tile != null && Tile.Data != null)
            return Tile.Data.attractivenessBonus;
        return 0;
    }

    public bool Equals(GridNode other)
    {
        if (other == null) return false;
        return this.x == other.x && this.y == other.y;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as GridNode);
    }

    public override int GetHashCode()
    {
        return System.HashCode.Combine(x, y);
    }

    public int Attractiveness
    {
        get
        {
            if (Tile != null && Tile.Data != null)
            {
                return Tile.Data.attractivenessBonus; 
            }
            return 0;
        }
    }

    public bool CanPlaceUnit
    {
        get
        {
            if (CurrentTileData == null || CurrentTileData.MaxUnitCapacity <= 0) return false;
            if (UnitObject != null) return false;

            return true;
        }
    }

    public void SetTileData(TileDataSO newData)
    {
        CurrentTileData = newData;
    }
}