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
}