using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Panex.Inventory.Model {
    [System.Serializable]
    public class Slot
    {
        public IStorable ItemData { get; set; }
        public int Amount { get; set; }
        public bool IsEmpty => ItemData == null;

        public void SetItem(IStorable item, int amount)
        {
            ItemData = item;
            Amount = amount;
        }

        public void AddItem(int value)
        {
            Amount = (int)Math.Clamp((long)Amount + value, 0, int.MaxValue);
        }

        public void Clear()
        {
            ItemData = null;
            Amount = 0;
        }
    }
}