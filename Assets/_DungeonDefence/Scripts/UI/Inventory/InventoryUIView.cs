using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InventoryUIView : MonoBehaviour
{
    [Header("Inv Container")]
    [SerializeField] private RectTransform drawerRect;
    [SerializeField] private float openY = 0f;
    [SerializeField] private float closedY = -250f;
    [SerializeField] private float duration = 0.3f;
    
    [Header("Tabs")]
    [SerializeField] private Button unitTabButton;
    [SerializeField] private Button tileTabButton;
    [SerializeField] private Image unitTabImage;
    [SerializeField] private Image tileTabImage;

    [Header("Lists")]
    [SerializeField] private GameObject unitScrollView;
    [SerializeField] private GameObject tileScrollView;

    [Header("Toggle Button")]
    [SerializeField] private Button toggleButton;

    private bool isOpen = false;
    private Coroutine slideCoroutine;

    private void Start()
    {
        if (drawerRect != null)
        {
            Vector2 pos = drawerRect.anchoredPosition;
            pos.y = closedY;
            drawerRect.anchoredPosition = pos;
        }

        isOpen = false;

        unitTabButton.onClick.AddListener(OnUnitTabClicked);
        tileTabButton.onClick.AddListener(OnTileTabClicked);
        OnUnitTabClicked();
    }

    public void ToggleInventory()
    {
        if (isOpen) Close();
        else Open();
    }

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;
        StopSlide();
        slideCoroutine = StartCoroutine(SlideRoutine(openY));
    }

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;
        StopSlide();
        slideCoroutine = StartCoroutine(SlideRoutine(closedY));
    }

    private void OnUnitTabClicked()
    {
        ShowTab(true);
        if (!isOpen) Open(); 
    }

    private void OnTileTabClicked()
    {
        ShowTab(false);
        if (!isOpen) Open();
    }

    private void ShowTab(bool isUnit)
    {
        if (unitScrollView) unitScrollView.SetActive(isUnit);
        if (tileScrollView) tileScrollView.SetActive(!isUnit);

        if (unitTabImage) unitTabImage.color = isUnit ? Color.white : Color.gray;
        if (tileTabImage) tileTabImage.color = !isUnit ? Color.white : Color.gray;
    }

    private void StopSlide()
    {
        if (slideCoroutine != null) StopCoroutine(slideCoroutine);
    }

    private IEnumerator SlideRoutine(float targetY)
    {
        float startY = drawerRect.anchoredPosition.y;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = t * (2 - t);

            float currentY = Mathf.Lerp(startY, targetY, t);
            
            Vector2 pos = drawerRect.anchoredPosition;
            pos.y = currentY;
            drawerRect.anchoredPosition = pos;

            yield return null;
        }

        Vector2 finalPos = drawerRect.anchoredPosition;
        finalPos.y = targetY;
        drawerRect.anchoredPosition = finalPos;
    }
}