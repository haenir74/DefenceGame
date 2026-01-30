using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileData", menuName = "DungeonDefence/Tiles/Tile Data")]
public class TileDataSO : ScriptableObject
{
    public string tileName;
    public Sprite tileSprite;
}