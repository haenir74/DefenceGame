using UnityEngine;

namespace Panex.Inventory
{
    [CreateAssetMenu(fileName = "Simple Item", menuName = "Panex/Inventory/Simple Item")]
    public class SimpleItem : ScriptableObject, IStorable
    {
        [Header("Item Data")]
        [SerializeField] private int id;
        [SerializeField] private string itemName;
        [TextArea] [SerializeField] private string description;
        [SerializeField] private Sprite icon;

        public int ID => id;
        public string Name => itemName;
        public string Description => description;
        public Sprite Icon => icon;
        public int MaxStack => 999;        
    }
}