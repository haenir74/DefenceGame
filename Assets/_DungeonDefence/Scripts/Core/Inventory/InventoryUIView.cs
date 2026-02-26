using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIView : MonoBehaviour
{
    
    [SerializeField] private RectTransform drawerRect;
    [SerializeField] private float openY = 0f;
    [SerializeField] private float closedY = -250f;
    [SerializeField] private float duration = 0.3f;

    
    [SerializeField] private Button unitTabButton;
    [SerializeField] private Button tileTabButton;
    [SerializeField] private Image unitTabImage;
    [SerializeField] private Image tileTabImage;

    
    [SerializeField] private GameObject unitScrollView;
    [SerializeField] private GameObject tileScrollView;

    
    [SerializeField] private List<RectTransform> linkedRectTransforms;

    
    [SerializeField] private Button toggleButton;

    public bool IsOpen { get; private set; } = false;
    public System.Action<bool> OnOpenStatusChanged;
    private Coroutine slideCoroutine;

    private void Start()
    {
        if (drawerRect != null)
        {
            Vector2 pos = drawerRect.anchoredPosition;
            pos.y = closedY;
            drawerRect.anchoredPosition = pos;
        }
        IsOpen = false;

        unitTabButton?.onClick.AddListener(OnUnitTabClicked);
        tileTabButton?.onClick.AddListener(OnTileTabClicked);
        OnUnitTabClicked();
    }

    public void AddLinkedRectTransform(RectTransform rt)
    {
        if (linkedRectTransforms == null) linkedRectTransforms = new List<RectTransform>();
        if (!linkedRectTransforms.Contains(rt)) linkedRectTransforms.Add(rt);
    }

    public void ToggleInventory()
    {
        if (IsOpen) Close();
        else Open();
    }

    public void Open()
    {
        this.gameObject.SetActive(true);
        if (IsOpen) return;
        IsOpen = true;
        OnOpenStatusChanged?.Invoke(true);
        StopSlide();
        slideCoroutine = StartCoroutine(SlideRoutine(openY));
    }

    public void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;
        OnOpenStatusChanged?.Invoke(false);
        StopSlide();
        slideCoroutine = StartCoroutine(SlideRoutine(closedY));
    }

    private void OnUnitTabClicked()
    {
        ShowTab(true);
        if (!IsOpen) Open();
    }

    private void OnTileTabClicked()
    {
        ShowTab(false);
        if (!IsOpen) Open();
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

        
        List<float> linkedStartPositions = new List<float>();
        if (linkedRectTransforms != null)
        {
            foreach (var rt in linkedRectTransforms)
            {
                if (rt != null) linkedStartPositions.Add(rt.anchoredPosition.y);
                else linkedStartPositions.Add(0);
            }
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = t * (2 - t);

            float currentY = Mathf.Lerp(startY, targetY, t);
            float diffY = currentY - startY;

            Vector2 pos = drawerRect.anchoredPosition;
            pos.y = currentY;
            drawerRect.anchoredPosition = pos;

            
            if (linkedRectTransforms != null)
            {
                for (int i = 0; i < linkedRectTransforms.Count; i++)
                {
                    if (linkedRectTransforms[i] != null)
                    {
                        Vector2 lPos = linkedRectTransforms[i].anchoredPosition;
                        lPos.y = linkedStartPositions[i] + diffY;
                        linkedRectTransforms[i].anchoredPosition = lPos;
                    }
                }
            }

            yield return null;
        }

        Vector2 finalPos = drawerRect.anchoredPosition;
        float finalDiffY = targetY - startY;
        finalPos.y = targetY;
        drawerRect.anchoredPosition = finalPos;

        if (linkedRectTransforms != null)
        {
            for (int i = 0; i < linkedRectTransforms.Count; i++)
            {
                if (linkedRectTransforms[i] != null)
                {
                    Vector2 lPos = linkedRectTransforms[i].anchoredPosition;
                    lPos.y = linkedStartPositions[i] + finalDiffY;
                    linkedRectTransforms[i].anchoredPosition = lPos;
                }
            }
        }
    }
}


