using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileData", menuName = "Game/Tiles/Tile Data")]
public class TileDataSO : ScriptableObject
{
    public string tileName;
    public Sprite tileSprite;
}