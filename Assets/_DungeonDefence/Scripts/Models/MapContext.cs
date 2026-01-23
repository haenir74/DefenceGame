using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapContext
{
    public Node[,] Nodes { get; private set; }
    
    public Node SpawnNode { get; set; }
    public Node CoreNode { get; set; }

    public int Width { get; private set; }
    public int Height { get; private set; }

    public void Initialize(int width, int height)
    {
        Width = width;
        Height = height;
        Nodes = new Node[width, height];
    }

    public Node GetNode(int x, int y)
    {
        if (IsValid(x, y))
        {
            return Nodes[x, y];
        }
        return null;
    }

    public bool IsValid(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }
}