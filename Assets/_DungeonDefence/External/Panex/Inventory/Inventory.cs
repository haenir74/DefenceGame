using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panex.Inventory.Controller;
using System;

namespace Panex.Inventory
{
    public class Inventory
    {
        private InventoryController controller;
        public string ID { get; private set; }
        public InventoryController Controller { get; private set; }

        public Inventory(string id, Settings settings, InventoryController prefab = null, Transform parent = null)
        {
            this.ID = id;

            
            if (prefab != null)
            {
                if (parent == null)
                {
                    var canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
                    if (canvas != null) parent = canvas.transform;
                }
                Controller = UnityEngine.Object.Instantiate(prefab, parent);
            }
            else
            {
                GameObject go = new GameObject($"Inventory_{id}");
                if (parent != null) go.transform.SetParent(parent);
                Controller = go.AddComponent<InventoryController>();
            }

            Controller.gameObject.name = $"Inventory_{id}";
            Controller.Configure(settings);
        }

        public event Action OnInventoryChanged
        {
            add { if (Controller != null) Controller.OnInventoryChanged += value; }
            remove { if (Controller != null) Controller.OnInventoryChanged -= value; }
        }

        public event Action<IStorable, int> OnSlotClicked
        {
            add { if (Controller != null) Controller.OnSlotClicked += value; }
            remove { if (Controller != null) Controller.OnSlotClicked -= value; }
        }

        public event Action<IStorable, Vector2> OnItemDroppedOutside
        {
            add { if (Controller != null) Controller.OnItemDroppedOutside += value; }
            remove { if (Controller != null) Controller.OnItemDroppedOutside -= value; }
        }

        public void Open() => Controller.Open();
        public void Close() => Controller.Close();
        public void Toggle() => Controller.Toggle();
        public bool IsOpen => Controller.IsOpen;

        public List<InventorySnapshot> GetSnapshot() => Controller.GetSnapshot();
        public void LoadSnapshot(List<InventorySnapshot> snapshot, System.Func<int, IStorable> itemResolver) => Controller.LoadSnapshot(snapshot, itemResolver);

        public int AddItem(IStorable item, int amount = 1) => Controller.AddItem(item, amount);
        public void RemoveItem(int index) => Controller.RemoveItem(index);
        public bool RemoveItem(int index, int amount = 1) => Controller.RemoveItem(index, amount);
        public bool RemoveItem(IStorable item, int amount) => Controller.RemoveItem(item, amount);
        public int GetItemAmount(int itemId) => Controller.GetItemAmount(itemId);

        public void Destroy()
        {
            if (Controller != null) UnityEngine.Object.Destroy(Controller.gameObject);
        }
    }
}
