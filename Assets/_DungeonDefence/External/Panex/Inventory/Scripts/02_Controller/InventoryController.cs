using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Panex.Inventory.Model;
using Panex.Inventory.View;


namespace Panex.Inventory.Controller {
    public class InventoryController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private Settings settings;

        private InventoryModel model;
        private InventoryView view;

        public event Action OnInventoryChanged;
        public event Action<IStorable, int> OnSlotClicked;
        public event Action<IStorable, Vector2> OnItemDroppedOutside;

        private void OnDestroy()
        {
            if (model != null) model.OnInventoryUpdated -= HandleModelUpdate;
            
            if (view != null)
            {
                view.OnSwapRequest -= SwapSlots;
                view.OnTransferRequest -= HandleTransferRequest;
                view.OnClickSlot -= HandleViewClick;
                view.OnDragEnd -= HandleDragEnd;
            }
        }

        public void Configure(Settings newSettings)
        {
            this.settings = newSettings;
            Initialize();
        }

        private void Initialize()
        {
            if (settings == null) return;

            // 1. Model 연결
            if (model != null) model.OnInventoryUpdated -= HandleModelUpdate;

            model = new InventoryModel(settings.Capacity);
            model.OnInventoryUpdated += HandleModelUpdate;

            // 2. View 연결
            if (view == null) view = GetComponentInChildren<InventoryView>();
            if (view != null)
            {
                view.InitializeUI(settings);

                view.OnSwapRequest -= SwapSlots;
                view.OnTransferRequest -= HandleTransferRequest;
                view.OnClickSlot -= HandleViewClick;
                view.OnDragEnd -= HandleDragEnd;            

                view.OnSwapRequest += SwapSlots;
                view.OnTransferRequest += HandleTransferRequest;
                view.OnClickSlot += HandleViewClick;
                view.OnDragEnd += HandleDragEnd;
            }
            HandleModelUpdate(model.Slots);
        }


        // ========================================================================
        // 내부 로직
        // ========================================================================
        private void HandleModelUpdate(Panex.Inventory.Model.Slot[] slots)
        {
            if (view != null)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    if (slots[i].IsEmpty) 
                        view.UpdateSlot(i, null, 0);
                    else 
                        view.UpdateSlot(i, slots[i].ItemData.Icon, slots[i].Amount);
                }
            }
            OnInventoryChanged?.Invoke();
        }

        private void HandleViewClick(int index)
        {
            var slot = model.GetSlot(index);
            if (!slot.IsEmpty) OnSlotClicked?.Invoke(slot.ItemData, index);
        }

        private void HandleDragEnd(int index, Vector2 position)
        {
            var slot = model.GetSlot(index);
            if (!slot.IsEmpty) OnItemDroppedOutside?.Invoke(slot.ItemData, position);
        }

        // 인벤토리 간 아이템 이동 처리 (복잡한 로직은 별도 함수 유지)
        private void HandleTransferRequest(SlotUI sourceSlotUI, int targetIndex)
        {
            var sourceController = sourceSlotUI.GetComponentInParent<InventoryController>();
            if (sourceController == null || sourceController == this) return;

            var sourceSlotData = sourceController.GetSlot(sourceSlotUI.SlotIndex);
            if (sourceSlotData == null || sourceSlotData.IsEmpty) return;

            var targetSlotData = model.GetSlot(targetIndex);

            // 1. 빈 슬롯이면 단순 이동
            if (targetSlotData.IsEmpty)
            {
                model.SetItem(targetIndex, sourceSlotData.ItemData, sourceSlotData.Amount);
                sourceController.RemoveItem(sourceSlotUI.SlotIndex);
            }
            // 2. 같은 아이템이면 합치기
            else if (targetSlotData.ItemData.ID == sourceSlotData.ItemData.ID)
            {
                model.SetItem(targetIndex, sourceSlotData.ItemData, targetSlotData.Amount + sourceSlotData.Amount);
                sourceController.RemoveItem(sourceSlotUI.SlotIndex);
            }
            // 3. 다른 아이템이면 교환
            else
            {
                sourceController.SetItem(sourceSlotUI.SlotIndex, targetSlotData.ItemData, targetSlotData.Amount);
                model.SetItem(targetIndex, sourceSlotData.ItemData, sourceSlotData.Amount);
            }
        }

        
        // ========================================================================
        // Public API
        // ========================================================================

        // UI 조작
        public void Open() => view?.gameObject.SetActive(true);
        public void Close() => view?.gameObject.SetActive(false);
        public void Toggle() => view?.gameObject.SetActive(!view.gameObject.activeSelf);
        public bool IsOpen => view != null && view.gameObject.activeSelf;

        // 데이터 접근
        public int Capacity => settings != null ? settings.Capacity : 0;
        public Slot GetSlot(int index) => model?.GetSlot(index);
        public int GetItemAmount(int itemId) => model != null ? model.GetItemAmount(itemId) : 0;

        // 아이템 조작
        public void SwapSlots(int indexA, int indexB)
        {
            if (settings.Draggable) model?.SwapItem(indexA, indexB);
        }

        public int AddItem(IStorable item, int amount) => model != null ? model.AddItem(item, amount) : amount;
        
        public int AddItem(int index, IStorable item, int amount)
        {
            var slot = model.GetSlot(index);
            if (slot.IsEmpty || slot.ItemData.ID == item.ID)
            {
                model.AddItem(index, item, amount);
                return 0; // 성공적으로 모두 넣음
            }
            return amount; // 실패 (넣지 못한 수량 반환)
        }

        public void RemoveItem(int index) => model?.RemoveItem(index);

        public bool RemoveItem(int index, int amount)
        {
            var slot = model.GetSlot(index);
            if (slot.IsEmpty || slot.Amount < amount) return false;

            if (slot.Amount == amount) model.RemoveItem(index);
            else model.SetItem(index, slot.ItemData, slot.Amount - amount);
            
            return true;
        }

        public bool RemoveItem(IStorable item, int amount)
        {
            int index = FindIndex(item);
            if (index == -1) return false;
            
            int remaining = amount;
            while (remaining > 0)
            {
                index = FindIndex(item);
                if (index == -1) return false;

                var slot = model.GetSlot(index);
                int take = Mathf.Min(slot.Amount, remaining);
                
                RemoveItem(index, take);
                remaining -= take;
            }
            return true;
        }

        public int FindIndex(IStorable item)
        {
            if (model == null || item == null) return -1;
            for (int i = 0; i < settings.Capacity; i++)
            {
                var slot = model.GetSlot(i);
                if (!slot.IsEmpty && slot.ItemData.ID == item.ID) return i;
            }
            return -1;
        }

        // 세이브 & 로드 시스템
        public List<InventorySnapshot> GetSnapshot()
        {
            var snapshot = new List<InventorySnapshot>();
            if (model == null) return snapshot;

            for (int i = 0; i < settings.Capacity; i++)
            {
                var slot = model.GetSlot(i);
                if (slot != null && !slot.IsEmpty)
                {
                    snapshot.Add(new InventorySnapshot 
                    { 
                        slotIndex = i, 
                        itemId = slot.ItemData.ID, 
                        amount = slot.Amount 
                    });
                }
            }
            return snapshot;
        }

        public void LoadSnapshot(List<InventorySnapshot> snapshot, Func<int, IStorable> itemResolver)
        {
            if (model == null) return;
            
            for (int i = 0; i < settings.Capacity; i++)
            {
                model.RemoveItem(i);
            }

            // 2. 스냅샷 데이터 채우기
            foreach (var data in snapshot)
            {
                IStorable item = itemResolver(data.itemId);
                if (item != null)
                {
                    model.SetItem(data.slotIndex, item, data.amount);
                }
            }
        }
        // 디버그용
        public void SetItem(int index, IStorable item, int amount) => model?.SetItem(index, item, amount);
    }
}