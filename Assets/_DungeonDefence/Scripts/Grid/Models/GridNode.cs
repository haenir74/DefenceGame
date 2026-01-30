using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridNode
{
    [SerializeField] private int x;
    [SerializeField] private int y;
    [SerializeField] private Vector3 worldPosition;

    public TileView CurrentTile { get; set; }
    public GridTile Tile { get; set; }

    public int X => x;
    public int Y => y;
    public Vector3 WorldPosition => worldPosition;

    public GridNode(int x, int y, Vector3 worldPosition)
    {
        this.x = x;
        this.y = y;
        this.worldPosition = worldPosition;
    }
}