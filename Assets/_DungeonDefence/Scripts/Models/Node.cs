using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node
{
    [SerializeField] private int x;
    [SerializeField] private int y;
    [SerializeField] private Vector3 worldPosition;
    [SerializeField] private TileView view;

    private Tile tileEffect;
    private List<Unit> unitsOnNode = new List<Unit>();

    public int X => x;
    public int Y => y;
    public Vector3 WorldPosition => worldPosition;

    public TileView CurrentTile
    {
        get => view;
        set => view = value;
    }

    public Tile TileEffect
    {
        get => tileEffect;
        set => tileEffect = value;
    }

    public bool IsEmpty => tileEffect == null;
    
    public List<Unit> UnitsOnNode => unitsOnNode;

    public Node(int x, int y, Vector3 worldPosition)
    {
        this.x = x;
        this.y = y;
        this.worldPosition = worldPosition;
        this.unitsOnNode = new List<Unit>();
    }

    public int GetAttractiveness()
    {
        return tileEffect != null ? tileEffect.GetTotalAttractiveness() : 0;
    }
}