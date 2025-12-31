using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New TileData", menuName = "Game/Tiles/Tile Data")]
public class Tile : ScriptableObject
{
    [Header("Basic Info")]
    public string tileName;
    public Sprite icon; // UI 표시용
    public GameObject prefab; // 맵에 생성될 프리팹

    [Header("Pathfinding")]
    [Tooltip("적이 이 타일을 얼마나 선호하는가? (높을수록 선호)")]
    [Range(0, 100)]
    public int basePriority;
    // 예: 평지=10, 늪=20, 가시밭=50, 벽=9999

    [Header("Interaction (Wall Breaking)")]
    public bool isDestructible; // 부수기 가능 여부
    public Tile transformToOnDestroy; // 부서지면 이 타일로 바뀐다

    [Header("Game Logic")]
    public bool isWalkable; // 기본적으로 이동 가능한가? (벽은 false, 나머지는 true)
    public bool isBuildable; // 플레이어가 타워/함정을 지을 수 있는가?

    [Header("Events")]
    [Tooltip("플레이어가 이 타일을 코어와 연결했을 때 발동")]
    public List<TileAction> onConnectedActions;

    [Tooltip("적 유닛이 이 타일 위에 도착했을 때 발동")]
    public List<TileAction> onEnemyEnterActions;

    [Tooltip("이 타일이 파괴되었을 때 발동 (예: 폭발 데미지)")]
    public List<TileAction> onDestroyActions;
}