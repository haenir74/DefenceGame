using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile : Tile
{
    
    protected override void OnPlaced()
    {
        Debug.Log("Tile placed");
    }

    public override void OnStepped()
    {
        Debug.Log("Tile stepped");
    }
}
