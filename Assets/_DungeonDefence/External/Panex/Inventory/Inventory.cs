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

        public Inventory(string id, Settings settings, InventoryController prefab = null, Transform parent = null)
        {
            this.ID = id;

            // 프리팹 로드 (없으면 기본값)
            if (prefab != null)
            {
                if (parent == null)
                {
                    var canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
                    if (canvas != null) parent = canvas.transform;
                }
                controller = UnityEngine.Object.Instantiate(prefab, parent);
            }
            else
            {
                GameObject go = new GameObject($"Inventory_{id}");
                if (parent != null) go.transform.SetParent(parent);
                controller = go.AddComponent<InventoryController>();
            }

            controller.gameObject.name = $"Inventory_{id}";
            controller.Configure(settings);
        }

        public event Action OnInventoryChanged
        {
            add { if (controller != null) controller.OnInventoryChanged += value; }
            remove { if (controller != null) controller.OnInventoryChanged -= value; }
        }

        public event Action<IStorable, int> OnSlotClicked
        {
            add { if (controller != null) controller.OnSlotClicked += value; }
            remove { if (controller != null) controller.OnSlotClicked -= value; }
        }

        public event Action<IStorable, Vector2> OnItemDroppedOutside
        {
            add { if (controller != null) controller.OnItemDroppedOutside += value; }
            remove { if (controller != null) controller.OnItemDroppedOutside -= value; }
        }

        public void Open() => controller.Open();
        public void Close() => controller.Close();
        public void Toggle() => controller.Toggle();
        public bool IsOpen => controller.IsOpen;

        public List<InventorySnapshot> GetSnapshot() => controller.GetSnapshot();
        public void LoadSnapshot(List<InventorySnapshot> snapshot, System.Func<int, IStorable> itemResolver) => controller.LoadSnapshot(snapshot, itemResolver);

        public int AddItem(IStorable item, int amount = 1) => controller.AddItem(item, amount);
        public void RemoveItem(int index) => controller.RemoveItem(index);
        public bool RemoveItem(int index, int amount = 1) => controller.RemoveItem(index, amount);
        public bool RemoveItem(IStorable item, int amount) => controller.RemoveItem(item, amount);
        public int GetItemAmount(int itemId) => controller.GetItemAmount(itemId);

        public void Destroy()
        {
            if (controller != null) UnityEngine.Object.Destroy(controller.gameObject);
        }
    }
}
