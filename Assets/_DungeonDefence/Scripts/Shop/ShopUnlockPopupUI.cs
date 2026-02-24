using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// 웨이브 클리어 후 3개의 해금 후보를 카드 형태로 보여주고
/// 플레이어가 하나를 선택하면 해금하는 팝업 UI.
/// </summary>
public class ShopUnlockPopupUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cardContainer;
    [SerializeField] private GameObject cardPrefab;  // ShopSlotUI 또는 간단한 카드 프리팹
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

        // 기존 카드 제거
        foreach (Transform child in cardContainer)
            Destroy(child.gameObject);

        if (candidates == null || candidates.Count == 0)
        {
            // 후보 없으면 바로 종료
            Skip();
            return;
        }

        if (titleText != null)
            titleText.text = "웨이브 클리어!\n새 유닛/타일을 해금하세요.";

        foreach (var candidate in candidates)
        {
            GameObject card = Instantiate(cardPrefab, cardContainer);
            SetupCard(card, candidate);
        }
    }

    private void SetupCard(GameObject card, ITradable item)
    {
        // ShopSlotUI 컴포넌트로 카드 초기화 시도
        var slotUI = card.GetComponent<ShopSlotUI>();
        if (slotUI != null)
        {
            if (item is UnitDataSO unit) slotUI.Initialize(unit);
            else if (item is TileDataSO tile) slotUI.Initialize(tile);

            slotUI.OnBuyClick += (_) => SelectUnlock(item);
            return;
        }

        // 폴백: 간단한 버튼 + 텍스트
        var nameText = card.GetComponentInChildren<TextMeshProUGUI>();
        if (nameText != null) nameText.text = item.Name;

        var btn = card.GetComponentInChildren<Button>();
        if (btn != null) btn.onClick.AddListener(() => SelectUnlock(item));
    }

    private void SelectUnlock(ITradable item)
    {
        ShopManager.Instance.UnlockItem(item);
        Debug.Log($"<color=lime>[ShopUnlock] {item.Name} 해금 완료!</color>");
        Hide();
        OnUnlockConfirmed?.Invoke();
    }

    private void Skip()
    {
        Debug.Log("[ShopUnlock] 해금 선택 스킵");
        Hide();
        OnUnlockConfirmed?.Invoke();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
