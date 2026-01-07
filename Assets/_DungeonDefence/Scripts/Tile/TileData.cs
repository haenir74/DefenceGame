using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New TileData", menuName = "Game/Tiles/Tile Data")]
public class TileData : ScriptableObject
{
    public string tileName;
    public Sprite tileSprite;
}
