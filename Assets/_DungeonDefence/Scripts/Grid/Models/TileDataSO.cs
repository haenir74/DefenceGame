using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panex.Inventory;

public enum TileEnvironment
{
    Normal = 0,
    Swamp,
    SpiderWeb
}

[CreateAssetMenu(fileName = "New Tile Data", menuName = "DungeonDefence/Datas/Tile Data")]
public class TileDataSO : ScriptableObject, IStorable, ITradable
{

    public int tileIdNumber;
    public string tileId;
    public string tileName;
    [TextArea] public string description;


    public Sprite icon;
    public Sprite tileSprite;
    public GameObject prefab;


    public int attractivenessBonus;
    public int baseAttractiveness;
    public int MaxUnitCapacity = 1;
    public bool IsDefaultTile => tileIdNumber == 0;

    public TileEnvironment environment;

    public TileEffectDataSO tileEffect;
    public UnitTier tier;


    [SerializeField] private List<ResourceCost> costs;
    public List<ResourceCost> GetCosts() => costs;
    public int sellPrice;

    public int shopStock = 5;


    public int ID => tileIdNumber;
    public string Name => tileName;
    public string Description => description;
    public Sprite Icon => icon;
    public int MaxStack => 999;
}


