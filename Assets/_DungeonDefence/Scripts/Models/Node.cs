using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node
{
    [SerializeField] private int _x;
    [SerializeField] private int _y;
    [SerializeField] private Vector3 _worldPosition;
    [SerializeField] private TileView _view;

    private Tile _tileEffect;
    private List<Unit> _unitsOnNode = new List<Unit>();

    public int X => _x;
    public int Y => _y;
    public Vector3 WorldPosition => _worldPosition;

    public TileView CurrentTile
    {
        get => _view;
        set => _view = value;
    }

    public Tile TileEffect
    {
        get => _tileEffect;
        set => _tileEffect = value;
    }

    public bool IsEmpty => _tileEffect == null;
    
    public List<Unit> UnitsOnNode => _unitsOnNode;

    public Node(int x, int y, Vector3 worldPosition)
    {
        _x = x;
        _y = y;
        _worldPosition = worldPosition;
        _unitsOnNode = new List<Unit>();
    }

    public int GetAttractiveness()
    {
        return _tileEffect != null ? _tileEffect.GetTotalAttractiveness() : 0;
    }
}