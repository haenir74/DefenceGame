using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node
{
    [SerializeField] private int _x;
    [SerializeField] private int _y;
    [SerializeField] private Vector3 _worldPosition;
    [SerializeField] private Tile _currentTile;
    [SerializeField] public GameObject Structure;

    public int X => _x;
    public int Y => _y;
    public Vector3 WorldPosition => _worldPosition;

    public Tile CurrentTile
    {
        get => _currentTile;
        set => _currentTile = value;
    }

    public bool IsWalkable => Structure == null;

    public Node(int x, int y, Vector3 worldPosition)
    {
        _x = x;
        _y = y;
        _worldPosition = worldPosition;
    }
}