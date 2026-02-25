using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 드래그 앤 드롭으로 전달되는 데이터 컨테이너.
/// 인벤토리 유닛, 그리드 유닛 등 드래그 가능한 모든 항목을 표현.
/// </summary>
public class DragPayload
{
    public enum SourceType { Inventory, GridUnit, DispatchSlot }

    public SourceType Source;

    // 인벤토리에서 드래그할 때
    public UnitDataSO UnitData;

    // 그리드에서 드래그할 때
    public Unit GridUnit;

    // 파견 슬롯에서 드래그할 때
    public DispatchSlotUI FromSlot;
}

/// <summary>
/// 드래그 앤 드롭 세션을 관리하는 싱글톤.
/// 드래그 중인 페이로드와 임시 비주얼(고스트 이미지)을 관리한다.
/// </summary>
public class DragDropManager : Singleton<DragDropManager>
{
    [Header("Ghost Image")]
    [SerializeField] private Image ghostImage;
    [SerializeField] private Canvas rootCanvas;

    public DragPayload CurrentPayload { get; private set; }
    public bool IsDragging => CurrentPayload != null;

    protected override void Awake()
    {
        base.Awake();
        if (ghostImage != null) ghostImage.gameObject.SetActive(false);
    }

    /// <summary>드래그 시작</summary>
    public void BeginDrag(DragPayload payload, Sprite icon)
    {
        CurrentPayload = payload;

        if (ghostImage != null && icon != null)
        {
            ghostImage.sprite = icon;
            ghostImage.gameObject.SetActive(true);
        }
    }

    /// <summary>매 프레임 고스트 이미지 위치 갱신 (드래그 중인 UI 요소에서 호출)</summary>
    public void UpdateGhostPosition(Vector2 screenPos)
    {
        if (ghostImage == null || !IsDragging) return;

        if (rootCanvas == null) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            screenPos,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera,
            out Vector2 localPos
        );
        ghostImage.rectTransform.anchoredPosition = localPos;
    }

    /// <summary>드래그 종료 (성공/실패 무관)</summary>
    public void EndDrag()
    {
        CurrentPayload = null;
        if (ghostImage != null) ghostImage.gameObject.SetActive(false);
    }
}
