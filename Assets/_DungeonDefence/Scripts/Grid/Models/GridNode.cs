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

    // ─── 배치 포인트 슬롯 (아군 3 + 적군 3) ───────────────────────────
    private const int SLOT_COUNT = 3;

    // 타일 크기 대비 슬롯 오프셋 (x: 좌우 구분, z: 위아래 3단)
    // 아군: 음의 x, 적군: 양의 x, z 간격 0.22 * cellSize
    private static readonly Vector3[] AlliedOffsets = new Vector3[]
    {
        new Vector3(-0.28f,  0f,  0.22f),
        new Vector3(-0.28f,  0f,  0.00f),
        new Vector3(-0.28f,  0f, -0.22f),
    };
    private static readonly Vector3[] EnemyOffsets = new Vector3[]
    {
        new Vector3( 0.28f,  0f,  0.22f),
        new Vector3( 0.28f,  0f,  0.00f),
        new Vector3( 0.28f,  0f, -0.22f),
    };

    private Unit[] alliedSlots = new Unit[SLOT_COUNT];
    private Unit[] enemySlots  = new Unit[SLOT_COUNT];

    public GridNode(int x, int y, Vector3 worldPosition)
    {
        this.x = x;
        this.y = y;
        this.worldPosition = worldPosition;
        this.coordinate = new Vector2Int(x, y);
    }

    // ─── 슬롯 공개 API ───────────────────────────────────────────────

    /// <summary>빈 슬롯에 유닛을 등록하고 해당 월드 포지션을 반환. 슬롯이 꽉 찼으면 null 반환.</summary>
    public Vector3? TryOccupySlot(Unit unit, float cellSize)
    {
        Unit[] slots = unit.IsPlayerTeam ? alliedSlots : enemySlots;
        Vector3[] offsets = unit.IsPlayerTeam ? AlliedOffsets : EnemyOffsets;

        // 이미 슬롯에 있는지 확인 (중복 등록 방지)
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            if (slots[i] == unit)
                return worldPosition + offsets[i] * cellSize;
        }

        for (int i = 0; i < SLOT_COUNT; i++)
        {
            if (slots[i] == null || (slots[i] != null && slots[i].IsDead))
            {
                slots[i] = unit;
                return worldPosition + offsets[i] * cellSize;
            }
        }
        return null; // 슬롯 없음
    }

    /// <summary>유닛의 슬롯 등록을 해제한다.</summary>
    public void ReleaseSlot(Unit unit)
    {
        Unit[] slots = unit.IsPlayerTeam ? alliedSlots : enemySlots;
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            if (slots[i] == unit)
            {
                slots[i] = null;
                return;
            }
        }
    }

    /// <summary>해당 팀 기준 빈 슬롯이 존재하는지 여부.</summary>
    public bool HasFreeSlot(bool isPlayerTeam)
    {
        Unit[] slots = isPlayerTeam ? alliedSlots : enemySlots;
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            if (slots[i] == null || slots[i].IsDead) return true;
        }
        return false;
    }

    // ─── 레거시 호환 ─────────────────────────────────────────────────

    /// <summary>[레거시] 이 노드에 아군 유닛이 배치될 수 있는지 (인벤토리 배치 시 사용).</summary>
    public bool CanPlaceUnit
    {
        get
        {
            if (CurrentTileData == null || CurrentTileData.MaxUnitCapacity <= 0) return false;
            return HasFreeSlot(true);
        }
    }

    // ─── 거리/매력도 ──────────────────────────────────────────────────

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

            int baseScore = 10000 - (DistanceToCore * 10);
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