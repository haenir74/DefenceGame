using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panex.Inventory;

[CreateAssetMenu(fileName = "New Tile Data", menuName = "DungeonDefence/Datas/Tile Data")]
public class TileDataSO : ScriptableObject, IStorable
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
    // public SkillDataSO tileEffect; // 타일을 밟았을 때 지속 효과

    [Header("Economy")]
    public int cost;
    public int sellPrice;

    // IStorable
    public int ID => tileIdNumber;
    public string Name => tileName;
    public string Description => description;
    public Sprite Icon => icon;
    public int MaxStack => 999;
}