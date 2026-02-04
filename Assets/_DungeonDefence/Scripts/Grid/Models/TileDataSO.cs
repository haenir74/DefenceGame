using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tile Data", menuName = "DungeonDefence/Tiles/Tile Data")]
public class TileDataSO : ScriptableObject//, ITradable
{
    [Header("Basic Info")]
    public string tileId;
    public string tileName;
    [TextArea] public string description;

    [Header("Visuals")]
    public Sprite icon;
    public Sprite tileSprite;

    [Header("Game Logic")]
    public int attractivenessBonus;
    // public SkillDataSO tileEffect; // 타일을 밟았을 때 지속 효과

    [Header("Economy")]
    public int cost;
    public int sellPrice;

    // ITradable
    public string Name => tileName;
    public int Cost => cost;
    public Sprite Icon => icon;
}