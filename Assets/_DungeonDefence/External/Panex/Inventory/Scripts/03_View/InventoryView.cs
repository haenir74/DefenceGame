using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Panex.Inventory;
using Panex.Inventory.Model;
using System;

namespace Panex.Inventory.View {
    public class InventoryView : MonoBehaviour
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private RectTransform slotContainer;
        [SerializeField] private SlotUI slotPrefab;
        [SerializeField] private bool hideEmptySlots = false;

        public event Action<int, int> OnSwapRequest;        // 내부 스왑
        public event Action<SlotUI, int> OnTransferRequest; // 외부 인벤토리로 전송
        public event Action<int> OnClickSlot;               // 클릭
        public event Action<int, Vector2> OnDragEnd;        // 드래그 종료
        public event Action<int> OnSlotHoverEnter;          // 마우스 들어옴
        public event Action<int> OnSlotHoverExit;           // 마우스 나감

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

        public void UpdateSlot(int index, Sprite icon, int amount)
        {
            if (index >= 0 && index < uiSlots.Count)
            {
                uiSlots[index].SetItem(icon, amount);
            }
        }

        private void ApplyTheme(Settings settings)
        {
             if (backgroundImage != null && settings.ContainerBackground != null)
                backgroundImage.sprite = settings.ContainerBackground;
        }

        private void ApplyGridLayout(Settings settings)
        {
            slotContainer.anchorMin = new Vector2(0.5f, 0.5f);
            slotContainer.anchorMax = new Vector2(0.5f, 0.5f);
            slotContainer.pivot = new Vector2(0.5f, 0.5f);

            Vector2 containerSize = new Vector2(settings.ContainerWidth, settings.ContainerHeight);
            slotContainer.sizeDelta = containerSize;
            if (backgroundImage != null) backgroundImage.rectTransform.sizeDelta = containerSize;

            GridLayoutGroup grid = slotContainer.GetComponent<GridLayoutGroup>();
            if (grid == null) grid = slotContainer.gameObject.AddComponent<GridLayoutGroup>();
            
            int padding = (int)settings.Padding;
            grid.padding = new RectOffset(padding, padding, padding, padding);

            if (settings.AutoSpacing)
            {
                float w = settings.SlotSize.x * settings.Columns;
                float h = settings.SlotSize.y * settings.Rows;
                float spX = settings.Columns > 1 ? (settings.ContainerWidth - padding * 2 - w) / (settings.Columns - 1) : 0;
                float spY = settings.Rows > 1 ? (settings.ContainerHeight - padding * 2 - h) / (settings.Rows - 1) : 0;
                grid.spacing = new Vector2(spX, spY);
            }
            else
            {
                grid.spacing = settings.Spacing;
            }

            grid.cellSize = settings.SlotSize;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = Mathf.Max(1, settings.Columns);
        }
    }
}