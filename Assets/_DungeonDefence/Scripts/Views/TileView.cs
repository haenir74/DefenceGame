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
    private bool isHighlighted;

    private void Awake()
    {
        if (meshRenderer == null) 
            meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Setup(Node node, TileData data)
    {
        this.Node = node;
        this.Data = data;

        if (this.Node != null)
        {
            this.Node.CurrentTile = this;
        }

        if (meshRenderer != null) 
        {
            originalColor = meshRenderer.material.color; 
        }
        
        if (node != null)
        {
            gameObject.name = $"Tile_{node.X}_{node.Y}";
        }
    }

    public void SetHighlight(bool isActive)
    {
        if (meshRenderer == null || isHighlighted == isActive) return;

        isHighlighted = isActive;
        meshRenderer.material.color = isActive ? originalColor * 0.6f : originalColor;
    }

    public void UpdateVisual(Sprite newSprite)
    {
        
    }
}