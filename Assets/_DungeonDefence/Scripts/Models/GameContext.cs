using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameContext
{
    public int currentTurn = 1;
    public int playerGold = 100;

    public MapContext map;

    public GameContext()
    {
        currentTurn = 1;
        playerGold = 100;
        map = new MapContext();
    }
}