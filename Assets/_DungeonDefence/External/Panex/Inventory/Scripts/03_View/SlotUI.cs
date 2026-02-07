using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

namespace Panex.Inventory.View {
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

        public void SetItem(Sprite icon, int amount)
        {
            if (amount <= 0)
            {
                ResetUI();
                return;
            }

            hasItem = true;
            Sprite displaySprite = (icon != null) ? icon : defaultIconSprite;

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
            if (!eventData.dragging) OnClickAction?.Invoke(SlotIndex);
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
            if(iconImage != null) iconImage.transform.localScale = Vector3.one * 0.95f;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if(iconImage != null) iconImage.transform.localScale = Vector3.one;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!hasItem) return;

            isDragging = true;

            originalParent = iconImage.transform.parent;

            iconImage.transform.SetParent(parentCanvas.transform, true); 
            iconImage.transform.SetAsLastSibling();
            iconImage.raycastTarget = false;

            Color color = iconImage.color;
            color.a = 0.5f;
            iconImage.color = color;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isDragging)
            {
                iconImage.transform.position = eventData.position;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            isDragging = false;
            if (originalParent != null)
            {
                iconImage.transform.SetParent(originalParent, true);
                iconImage.rectTransform.anchoredPosition = Vector2.zero;
            }
            iconImage.raycastTarget = true;
            Color color = iconImage.color;
            color.a = 1.0f;
            iconImage.color = color;
            
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
