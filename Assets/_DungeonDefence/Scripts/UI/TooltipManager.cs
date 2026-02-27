using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TooltipManager : Singleton<TooltipManager>
{
    protected override bool DontDestroy => true;

    [Header("UI References")]
    [SerializeField] private RectTransform tooltipRect;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI statText;
    [SerializeField] private TextMeshProUGUI tagText;

    private void Start()
    {
        HideTooltip();
    }

    private void Update()
    {
        if (tooltipRect != null && tooltipRect.gameObject.activeSelf)
        {
            UpdateTooltipPosition();
        }
    }

    public void ShowTooltip(Unit targetUnit)
    {
        if (targetUnit == null || targetUnit.Data == null || targetUnit.Combat == null) return;

        UnitDataSO data = targetUnit.Data;
        UnitCombat combat = targetUnit.Combat;

        // Set Texts
        if (nameText != null) nameText.text = data.unitName;
        if (descriptionText != null) descriptionText.text = data.description;

        // Format Stats
        if (statText != null)
        {
            float currentAtk = combat.AttackPower.Value;
            float baseAtk = data.basePower;

            string atkColorHex = "#FFFFFF"; // default
            if (currentAtk > baseAtk)
            {
                atkColorHex = "green"; // buff
            }
            else if (currentAtk < baseAtk)
            {
                atkColorHex = "red"; // debuff
            }

            statText.text = $"ATK: <color={atkColorHex}>{currentAtk:F1}</color> / HP: {combat.CurrentHp:F0}/{combat.MaxHp:F0}";
        }

        // Format Tags
        if (tagText != null)
        {
            string tagString = data.tags.ToString();
            // Optional: format 'tagString' for better readability if needed
            tagText.text = $"Tags: {tagString}";
        }

        // Show Tooltip
        tooltipRect.gameObject.SetActive(true);
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        // Force layout update to get correct size
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
        
        UpdateTooltipPosition();
    }

    public void HideTooltip()
    {
        if (tooltipRect != null)
        {
            tooltipRect.gameObject.SetActive(false);
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }
    }

    private void UpdateTooltipPosition()
    {
        Vector2 mousePos = Input.mousePosition;

        // Determine pivot based on screen quadrant to avoid clipping
        float pivotX = mousePos.x / Screen.width > 0.5f ? 1.0f : 0.0f;
        float pivotY = mousePos.y / Screen.height > 0.5f ? 1.0f : 0.0f;
        
        // Add a small offset so the tooltip isn't directly right under the cursor
        float offsetX = pivotX == 0.0f ? 15f : -15f;
        float offsetY = pivotY == 0.0f ? 15f : -15f;

        tooltipRect.pivot = new Vector2(pivotX, pivotY);
        tooltipRect.position = mousePos + new Vector2(offsetX, offsetY);
        
        // Additional clamp to ensure it stays within screen bounds (if canvas scalar affects position)
        Vector3[] corners = new Vector3[4];
        tooltipRect.GetWorldCorners(corners);
        
        float width = corners[2].x - corners[0].x;
        float height = corners[1].y - corners[0].y;
        
        Vector3 newPos = tooltipRect.position;
        if (pivotX == 0f && newPos.x + width > Screen.width) newPos.x = Screen.width - width;
        if (pivotX == 1f && newPos.x - width < 0) newPos.x = width;
        if (pivotY == 0f && newPos.y + height > Screen.height) newPos.y = Screen.height - height;
        if (pivotY == 1f && newPos.y - height < 0) newPos.y = height;
        
        tooltipRect.position = newPos;
    }
}
