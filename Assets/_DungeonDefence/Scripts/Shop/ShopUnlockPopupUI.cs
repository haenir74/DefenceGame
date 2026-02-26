using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ShopUnlockPopupUI : MonoBehaviour
{
    
    [SerializeField] private Transform cardContainer;
    [SerializeField] private GameObject cardPrefab;  
    [SerializeField] private Button skipButton;
    [SerializeField] private TextMeshProUGUI titleText;

    public event Action OnUnlockConfirmed;

    private void Awake()
    {
        if (skipButton != null)
            skipButton.onClick.AddListener(Skip);
    }

    public void Show(List<ITradable> candidates)
    {
        gameObject.SetActive(true);

        
        foreach (Transform child in cardContainer)
            Destroy(child.gameObject);

        if (candidates == null || candidates.Count == 0)
        {
            
            Skip();
            return;
        }

        if (titleText != null)
            titleText.text = "?⑥씠釉??대━??\n???좊떅/??쇱쓣 ?닿툑?섏꽭??";

        foreach (var candidate in candidates)
        {
            GameObject card = Instantiate(cardPrefab, cardContainer);
            SetupCard(card, candidate);
        }
    }

    private void SetupCard(GameObject card, ITradable item)
    {
        
        var slotUI = card.GetComponent<ShopSlotUI>();
        if (slotUI != null)
        {
            if (item is UnitDataSO unit) slotUI.Initialize(unit);
            else if (item is TileDataSO tile) slotUI.Initialize(tile);

            slotUI.OnBuyClick += (_) => SelectUnlock(item);
            return;
        }

        
        var nameText = card.GetComponentInChildren<TextMeshProUGUI>();
        if (nameText != null) nameText.text = item.Name;

        var btn = card.GetComponentInChildren<Button>();
        if (btn != null) btn.onClick.AddListener(() => SelectUnlock(item));
    }

    private void SelectUnlock(ITradable item)
    {
        ShopManager.Instance.UnlockItem(item);
        
        Hide();
        OnUnlockConfirmed?.Invoke();
    }

    private void Skip()
    {
        
        Hide();
        OnUnlockConfirmed?.Invoke();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}



