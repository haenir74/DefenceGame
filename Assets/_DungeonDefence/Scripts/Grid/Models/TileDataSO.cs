using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panex.Inventory;

[CreateAssetMenu(fileName = "New Tile Data", menuName = "DungeonDefence/Datas/Tile Data")]
public class TileDataSO : ScriptableObject, IStorable, ITradable
{
    [Header("Basic Info")]
    public int tileIdNumber;
    public string tileId;
    public string tileName;
    [TextArea] public string description;

    [Header("Visuals")]
    public Sprite icon;
    public Sprite tileSprite;
    public GameObject prefab;

    [Header("Game Logic")]
    public int attractivenessBonus;
    public int MaxUnitCapacity = 1;
    public bool IsDefaultTile => tileIdNumber == 0;
    [Tooltip("타일 진입/체류/이탈/사망 시 발동되는 효과 SO")]
    public TileEffectDataSO tileEffect;

    [Header("Dispatch System")]
    public bool IsDispatchTile;
    [Tooltip("파견 시 획득하는 기본 골드")]
    public int baseDispatchReward = 0;
    public float EfficiencyMultiplier = 1.0f;

    [Header("Economy")]
    [SerializeField] private List<ResourceCost> costs;
    public List<ResourceCost> GetCosts() => costs;
    public int sellPrice;
    [Tooltip("상점에서 구매 가능한 횟수. 0 = 무한")]
    public int shopStock = 5;

    // IStorable
    public int ID => tileIdNumber;
    public string Name => tileName;
    public string Description => description;
    public Sprite Icon => icon;
    public int MaxStack => 999;
}