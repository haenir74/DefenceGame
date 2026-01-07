using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public Vector3 WorldPosition { get; private set; }

    public Tile CurrentTile;

    public Node(int x, int y, Vector3 worldPosition)
    {
        this.X = x;
        this.Y = y;
        this.WorldPosition = worldPosition;
    }
}
