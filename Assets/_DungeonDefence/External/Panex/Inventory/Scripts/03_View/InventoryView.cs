using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Panex.Inventory;
using Panex.Inventory.Model;
using System;

namespace Panex.Inventory.View
{
    public class InventoryView : MonoBehaviour
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private RectTransform slotContainer;
        [SerializeField] private SlotUI slotPrefab;
        [SerializeField] private bool hideEmptySlots = false;

        public event Action<int, int> OnSwapRequest;
        public event Action<SlotUI, int> OnTransferRequest;
        public event Action<int> OnClickSlot;
        public event Action<int, Vector2> OnDragEnd;
        public event Action<int> OnSlotHoverEnter;
        public event Action<int> OnSlotHoverExit;

        private List<SlotUI> uiSlots = new List<SlotUI>();

        public void InitializeUI(Settings settings)
        {
            ApplyTheme(settings);
            ApplyGridLayout(settings);
            GenerateSlots(settings.Capacity, settings);
        }

        private void GenerateSlots(int capacity, Settings settings)
        {
            foreach (Transform child in slotContainer)
            {
                Destroy(child.gameObject);
            }
            uiSlots.Clear();

            for (int i = 0; i < capacity; i++)
            {
                SlotUI ui = Instantiate(slotPrefab, slotContainer);
                ui.Initialize(i);
                ui.ApplyTheme(settings);

                ui.OnDropAction += HandleSlotDrop;
                ui.OnClickAction += (index) => OnClickSlot?.Invoke(index);
                ui.OnDragEndAction += (index, pos) => OnDragEnd?.Invoke(index, pos);
                ui.OnPointerEnterAction += (slot) => OnSlotHoverEnter?.Invoke(slot.SlotIndex);
                ui.OnPointerExitAction += (slot) => OnSlotHoverExit?.Invoke(slot.SlotIndex);

                uiSlots.Add(ui);

                if (hideEmptySlots)
                {
                    ui.gameObject.SetActive(false);
                }
                else
                {
                    ui.gameObject.SetActive(true);
                    ui.SetItem(null, 0);
                }
            }
        }

        private void HandleSlotDrop(SlotUI sourceSlot, SlotUI targetSlot)
        {
            if (uiSlots.Contains(sourceSlot))
            {
                OnSwapRequest?.Invoke(sourceSlot.SlotIndex, targetSlot.SlotIndex);
            }
            else
            {
                OnTransferRequest?.Invoke(sourceSlot, targetSlot.SlotIndex);
            }
        }

        public void UpdateSlot(int index, IStorable item, int amount)
        {
            if (index < 0 || index >= uiSlots.Count) return;

            SlotUI slotUI = uiSlots[index];
            bool isEmpty = (amount <= 0 || item == null);

            if (hideEmptySlots && isEmpty)
            {
                slotUI.gameObject.SetActive(false);
            }
            else
            {
                slotUI.gameObject.SetActive(true);
                slotUI.SetItem(item, amount);
            }
        }

        private void ApplyTheme(Settings settings)
        {
            if (backgroundImage != null && settings.ContainerBackground != null)
                backgroundImage.sprite = settings.ContainerBackground;
        }

        private void ApplyGridLayout(Settings settings)
        {
            if (slotContainer == null) return;


            var existingLayout = slotContainer.GetComponent<LayoutGroup>();
            if (existingLayout != null && !(existingLayout is GridLayoutGroup))
            {
                return;
            }

            var gridLayout = slotContainer.GetComponent<GridLayoutGroup>();
            if (gridLayout == null)
            {
                gridLayout = slotContainer.gameObject.AddComponent<GridLayoutGroup>();
            }

            if (gridLayout != null && settings != null)
            {
                gridLayout.cellSize = settings.SlotSize;
                gridLayout.spacing = settings.Spacing;
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = settings.Columns;
            }
        }
    }
}