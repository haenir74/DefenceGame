using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileView : MonoBehaviour
{
    public Node Node { get; private set; }
    public TileData Data { get; private set; }

    [Header("Visuals")]
    [SerializeField] private MeshRenderer meshRenderer;
    private Color originalColor;

    public void Setup(Node node, TileData data)
    {
        this.Node = node;
        this.Data = data;

        if (this.Node != null)
        {
            this.Node.CurrentTile = this;
        }

        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null) 
        {
            originalColor = meshRenderer.material.color; 
        }
    }

    public void SetHighlight(bool isActive)
    {
        if (meshRenderer == null) return;
        meshRenderer.material.color = isActive ? originalColor * 0.7f : originalColor;
    }
}