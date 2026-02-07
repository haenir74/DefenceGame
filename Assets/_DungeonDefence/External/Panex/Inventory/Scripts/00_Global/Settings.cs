using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Panex.Inventory
{
    [CreateAssetMenu(fileName = "Settings", menuName = "Panex/Inventory/Settings")]
    public class Settings : ScriptableObject
    {
        [Header("Data")]
        public int Capacity = 24;

        [Header("Container Layout")]
        public float ContainerWidth = 800;
        public float ContainerHeight = 600;
        public float Padding = 10f;

        [Header("Grid Layout")]
        public int Columns = 6; 
        public int Rows = 4;
        public bool AutoSpacing = false;
        public Vector2 Spacing = new Vector2(5, 5);
        public Vector2 SlotSize = new Vector2(100, 100);

        [Header("UI Theme")]
        public Sprite ContainerBackground;
        public Sprite SlotBackground;
        public Sprite SlotHighlight;
        
        [Header("Icon Settings")]
        public Sprite DefaultItemIcon;
        
        [Header("Interaction")]
        public bool Draggable = true;
        public bool EnableTooltip = true;
    }
}