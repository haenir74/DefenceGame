using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 개별 타일의 시각적 표현 담당
public class TileView : MonoBehaviour
{
    public GridNode Node { get; private set; }
    public TileDataSO Data { get; private set; }

    [Header("Visuals")]
    [SerializeField] private MeshRenderer meshRenderer;
    private Color originalColor;
    private bool isHighlighted;

    private void Awake()
    {
        if (meshRenderer == null) 
            meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Setup(GridNode node, TileDataSO data)
    {
        this.Node = node;
        this.Data = data;

        if (this.Node != null)
        {
            this.Node.CurrentTile = this;
            gameObject.name = $"Tile_{node.X}_{node.Y}";
        }

        if (meshRenderer != null) 
        {
            originalColor = meshRenderer.material.color; 
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