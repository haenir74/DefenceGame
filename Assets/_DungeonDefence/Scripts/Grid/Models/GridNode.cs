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

    // 타일 크기 대비 슬롯 오프셋 (x: 좌우 구분, z: 앞뒤 3단)
    // 적군: 음의 x (왼쪽), 아군: 양의 x (오른쪽), z 간격 0.22 * cellSize
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

    // 슬롯마다 여러 유닛 공유 가능 (List<Unit> per slot)
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

    // ─── 슬롯 공개 API ───────────────────────────────────────────────

    /// <summary>
    /// 유닛을 가장 비어 있는 슬롯에 등록하고 해당 월드 포지션을 반환.
    /// 모든 슬롯이 찼더라도 유닛 수가 가장 적은 슬롯을 사용하므로 null을 반환하지 않음.
    /// </summary>
    public Vector3? TryOccupySlot(Unit unit, float cellSize)
    {
        List<Unit>[] slots = unit.IsPlayerTeam ? alliedSlots : enemySlots;
        Vector3[] offsets = unit.IsPlayerTeam ? AlliedOffsets : EnemyOffsets;

        // ① 이미 등록된 슬롯이면 해당 위치 반환 (중복 방지)
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            if (slots[i].Contains(unit))
                return worldPosition + offsets[i] * cellSize;
        }

        // ② 죽은 유닛 정리 후 유닛 수가 가장 적은 슬롯 선택
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

    /// <summary>유닛의 슬롯 등록을 해제한다.</summary>
    public void ReleaseSlot(Unit unit)
    {
        List<Unit>[] slots = unit.IsPlayerTeam ? alliedSlots : enemySlots;
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            if (slots[i].Remove(unit))
                return;
        }
    }

    /// <summary>
    /// 해당 팀 기준 완전히 비어 있는 슬롯(살아있는 유닛 0명)이 하나라도 있는지 여부.
    /// 인벤토리에서 유닛 배치 가능 여부 판단에 사용.
    /// </summary>
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