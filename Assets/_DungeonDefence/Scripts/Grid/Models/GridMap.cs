using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 맵 전체 데이터를 담는 컨테이너
[System.Serializable]
public class GridMap
{
    public GridNode[,] Nodes { get; private set; }
    
    public GridNode SpawnNode { get; set; }
    public GridNode CoreNode { get; set; }

    public int Width { get; private set; }
    public int Height { get; private set; }

    public void Initialize(int width, int height)
    {
        Width = width;
        Height = height;
        Nodes = new GridNode[width, height];
    }

    public GridNode GetNode(int x, int y)
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