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
    public int DistanceToCore { get; set; } = int.MaxValue;
    public int TileInfluence { get; set; } = 0;


    private const int SLOT_COUNT = 3;



    private static readonly Vector3[] AlliedOffsets = new Vector3[]
    {
        new Vector3( 0.28f,  0f,  0.22f),
        new Vector3( 0.28f,  0f,  0.00f),
        new Vector3( 0.28f,  0f, -0.22f),
    };
    private static readonly Vector3[] EnemyOffsets = new Vector3[]
    {
        new Vector3(-0.28f,  0f,  0.22f),
        new Vector3(-0.28f,  0f,  0.00f),
        new Vector3(-0.28f,  0f, -0.22f),
    };


    private List<Unit>[] alliedSlots;
    private List<Unit>[] enemySlots;

    public GridNode(int x, int y, Vector3 worldPosition)
    {
        this.x = x;
        this.y = y;
        this.worldPosition = worldPosition;
        this.coordinate = new Vector2Int(x, y);

        alliedSlots = new List<Unit>[SLOT_COUNT];
        enemySlots = new List<Unit>[SLOT_COUNT];
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            alliedSlots[i] = new List<Unit>();
            enemySlots[i] = new List<Unit>();
        }
    }







    public Vector3? TryOccupySlot(Unit unit, float cellSize)
    {
        List<Unit>[] slots = unit.IsPlayerTeam ? alliedSlots : enemySlots;
        Vector3[] offsets = unit.IsPlayerTeam ? AlliedOffsets : EnemyOffsets;


        for (int i = 0; i < SLOT_COUNT; i++)
        {
            if (slots[i].Contains(unit))
                return worldPosition + offsets[i] * cellSize;
        }


        int minCount = int.MaxValue;
        int minIndex = 0;
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            slots[i].RemoveAll(u => u == null || u.IsDead);
            if (slots[i].Count < minCount)
            {
                minCount = slots[i].Count;
                minIndex = i;
            }
        }

        slots[minIndex].Add(unit);
        return worldPosition + offsets[minIndex] * cellSize;
    }


    public void ReleaseSlot(Unit unit)
    {
        List<Unit>[] slots = unit.IsPlayerTeam ? alliedSlots : enemySlots;
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            if (slots[i].Remove(unit))
                return;
        }
    }





    public bool HasFreeSlot(bool isPlayerTeam)
    {
        List<Unit>[] slots = isPlayerTeam ? alliedSlots : enemySlots;
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            int aliveCount = 0;
            foreach (var u in slots[i])
                if (u != null && !u.IsDead) aliveCount++;
            if (aliveCount == 0) return true;
        }
        return false;
    }




    public bool CanPlaceUnit
    {
        get
        {
            if (CurrentTileData == null || CurrentTileData.MaxUnitCapacity <= 0) return false;
            return HasFreeSlot(true);
        }
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
            if (DistanceToCore == int.MaxValue) return int.MinValue;



            int baseScore = 10000 - (DistanceToCore * 2);


            baseScore += TileInfluence;


            if (Tile != null && Tile.Data != null)
            {
                baseScore += Tile.Data.attractivenessBonus;
            }
            return baseScore;
        }
    }

    public void SetTileData(TileDataSO newData)
    {
        CurrentTileData = newData;
    }
}


