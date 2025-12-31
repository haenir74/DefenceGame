using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class Node
{
    public Vector2Int Coordinate { get; private set; }
    public Vector3 WorldPosition { get; private set; }

    public Tile CurrentTile;
    public GameObject TileObject;

    public Node Parent;

    public int X => this.Coordinate.x;
    public int Y => this.Coordinate.y;

    public Node(Vector2Int coord, Vector3 worldPos, Tile initialTile)
    {
        this.Coordinate = coord;
        this.WorldPosition = worldPos;
        this.CurrentTile = initialTile;
    }

    public void UpdateTile(Tile newTile, GameObject newObj)
    {
        this.CurrentTile = newTile;
        this.TileObject = newObj;
    }

    public int GetPriority()
    {
        if (CurrentTile == null) return 0;    
        return CurrentTile.basePriority; 
    }
}
