using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

namespace Panex.Inventory.View
{
    public class SlotUI : MonoBehaviour,
        IPointerClickHandler,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        [SerializeField] private Image slotImage;
        [SerializeField] private Image highlightImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI amountText;

        public int SlotIndex { get; private set; }
        private Sprite defaultIconSprite;
        private bool enableTooltip = true;
        private bool hasItem = false;
        private IStorable currentItem;

        public event Action<SlotUI, SlotUI> OnDropAction;
        public event Action<int> OnClickAction;
        public event Action<int, Vector2> OnDragEndAction;
        public event Action<SlotUI> OnPointerEnterAction;
        public event Action<SlotUI> OnPointerExitAction;

        private Canvas parentCanvas;
        private Transform originalParent;
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private bool isDragging = false;

        public bool HasItem => hasItem;
        public Image Icon => iconImage;
        public RectTransform Rect => rectTransform;

        private void Awake()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null) parentCanvas = canvas.rootCanvas;

            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            parentCanvas = GetComponentInParent<Canvas>();

            if (highlightImage != null) highlightImage.gameObject.SetActive(false);
        }

        public void Initialize(int index)
        {
            SlotIndex = index;
            ResetUI();
        }

        public void SetItem(IStorable item, int amount)
        {
            this.currentItem = item;
            if (amount <= 0 || item == null)
            {
                ResetUI();
                return;
            }

            hasItem = true;
            Sprite displaySprite = (item.Icon != null) ? item.Icon : defaultIconSprite;

            if (displaySprite != null)
            {
                iconImage.sprite = displaySprite;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }
            if (amountText) amountText.text = amount > 1 ? amount.ToString() : "";
        }

        private void ResetUI()
        {
            hasItem = false;
            currentItem = null;
            iconImage.sprite = null;
            iconImage.enabled = false;

            if (amountText) amountText.text = "";
        }

        public void ApplyTheme(Settings settings)
        {
            this.enableTooltip = settings.EnableTooltip;

            if (slotImage != null && settings.SlotBackground != null)
                slotImage.sprite = settings.SlotBackground;

            if (highlightImage != null && settings.SlotHighlight != null)
                highlightImage.sprite = settings.SlotHighlight;

            if (settings.DefaultItemIcon != null)
            {
                defaultIconSprite = settings.DefaultItemIcon;
            }
        }

        // ========================================================================
        // 드래그 & 클릭 구현
        // ========================================================================

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!eventData.dragging)
            {
                OnClickAction?.Invoke(SlotIndex);

                // [REFINED] Click-to-Select removed to enforce Drag-and-Drop only.
                // Placement logic should only be triggered by OnBeginDrag.
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (highlightImage != null) highlightImage.gameObject.SetActive(true);
            if (enableTooltip) OnPointerEnterAction?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (highlightImage != null) highlightImage.gameObject.SetActive(false);
            if (enableTooltip) OnPointerExitAction?.Invoke(this);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (iconImage != null) iconImage.transform.localScale = Vector3.one * 0.95f;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (iconImage != null) iconImage.transform.localScale = Vector3.one;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!hasItem || currentItem == null) return;

            isDragging = true;

            // 유출된 유니티 시스템과의 통합: DragDropManager 사용
            if (DragDropManager.Instance != null)
            {
                var payload = new DragPayload();
                payload.Source = DragPayload.SourceType.Inventory;

                if (currentItem is UnitDataSO unitData)
                {
                    payload.UnitData = unitData;
                    GameManager.Instance?.SelectUnitToPlace(unitData, GameManager.SelectionSource.Inventory);
                }
                else if (currentItem is TileDataSO tileData)
                {
                    payload.TileData = tileData;
                    GameManager.Instance?.SelectTileToPlace(tileData, GameManager.SelectionSource.Inventory);
                }

                DragDropManager.Instance.BeginDrag(payload, iconImage.sprite);
            }

            // 기존 아이콘 이동 로직은 비활성화 (고스트 이미지가 대신함)
            iconImage.raycastTarget = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isDragging && DragDropManager.Instance != null)
            {
                DragDropManager.Instance.UpdateGhostPosition(eventData.position);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            isDragging = false;

            iconImage.raycastTarget = true;

            if (DragDropManager.Instance != null)
            {
                DragDropManager.Instance.EndDrag();
            }

            if (eventData.pointerEnter == null)
            {
                OnDragEndAction?.Invoke(SlotIndex, eventData.position);
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            var sourceSlot = eventData.pointerDrag?.GetComponent<SlotUI>();
            if (sourceSlot != null)
            {
                OnDropAction?.Invoke(sourceSlot, this);
            }
        }
    }
}
