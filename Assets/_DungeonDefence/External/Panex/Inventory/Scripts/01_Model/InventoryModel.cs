using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Panex.Inventory.Model {
    public class InventoryModel
    {
        public Slot[] Slots { get; private set; }
        public int Capacity { get; private set; }

        public event Action<Slot[]> OnInventoryUpdated;

        public InventoryModel(int capacity)
        {
            Capacity = capacity;
            Slots = new Slot[capacity];
            for (int i = 0; i < capacity; i++)
            {
                Slots[i] = new Slot();
            }
        }

        public int AddItem(IStorable item, int amount)
        {
            int remaining = amount;
            
            for (int i = 0; i < Capacity; i++)
            {
                if (remaining <= 0) break;
                
                if (!Slots[i].IsEmpty && Slots[i].ItemData.ID == item.ID)
                {
                    int currentAmount = Slots[i].Amount;
                    int maxStack = item.MaxStack;

                    if (currentAmount < maxStack)
                    {
                        int space = maxStack - currentAmount;
                        int amountToAdd = Mathf.Min(space, remaining);

                        Slots[i].AddItem(amountToAdd);
                        remaining -= amountToAdd;
                    }
                }
            }
            for (int i = 0; i < Capacity; i++)
            {
                if (remaining <= 0) break;

                if (Slots[i].IsEmpty)
                {
                    int maxStack = item.MaxStack;
                    int amountToAdd = Mathf.Min(maxStack, remaining);

                    Slots[i].SetItem(item, amountToAdd);
                    remaining -= amountToAdd;
                }
            }
            if (remaining != amount) OnInventoryUpdated?.Invoke(Slots);
            return remaining;
        }

        public void AddItem(int index, IStorable item, int amount)
        {
            if (index < 0 || index >= Capacity) return;
            
            if (Slots[index].IsEmpty)
            {
                int addAmount = Mathf.Min(amount, item.MaxStack);
                Slots[index].SetItem(item, addAmount);
            }
            else if (Slots[index].ItemData.ID == item.ID)
            {
                int space = item.MaxStack - Slots[index].Amount;
                int addAmount = Mathf.Min(amount, space);
                if (addAmount > 0)
                {
                    Slots[index].AddItem(addAmount);
                }
            }
            
            OnInventoryUpdated?.Invoke(Slots);
        }

        public void RemoveItem(int index)
        {
            if (index < 0 || index >= Capacity) return;
            
            Slots[index].Clear();
            
            OnInventoryUpdated?.Invoke(Slots);
        }

        public void SetItem(int index, IStorable item, int amount)
        {
            if (index < 0 || index >= Capacity) return;

            Slots[index].SetItem(item, amount);
            OnInventoryUpdated?.Invoke(Slots);
        }

        public void SwapItem(int indexA, int indexB)
        {
            if (indexA == indexB) return;
            if (indexA < 0 || indexA >= Capacity || indexB < 0 || indexB >= Capacity) return;
            
            Slot tempSlot = Slots[indexA];
            Slots[indexA] = Slots[indexB];
            Slots[indexB] = tempSlot;

            OnInventoryUpdated?.Invoke(Slots);
        }


        public Slot GetSlot(int index)
        {
            if (index < 0 || index >= Capacity) return null;
            return Slots[index];
        }

        public int GetItemAmount(int itemId)
        {
            int total = 0;
            foreach (var slot in Slots)
            {
                if (!slot.IsEmpty && slot.ItemData.ID == itemId)
                {
                    total += slot.Amount;
                }
            }
            return total;
        }
    }
}
