using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Panex.Inventory
{
    [System.Serializable]
    public class InventorySnapshot
    {
        public int slotIndex;
        public int itemId;
        public int amount;
    }
}