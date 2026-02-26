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
    public enum SourceType { Inventory, Grid, Dispatch }

    public SourceType Source;

    // 인벤토리에서 드래그할 때
    public UnitDataSO UnitData;
    public TileDataSO TileData;

    // 그리드에서 드래그할 때
    public Unit GridUnit;

    // 파견 슬롯에서 드래그할 때
    public DispatchSlotUI FromSlot;

    // [NEW] 취소 시 원래 위치로 되돌리기 위해 원본 위치를 저장합니다.
    public GridNode OriginalNode;
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

        // [FIX] 3D 오브젝트 드래그를 위해 필수적인 컴포넌트 체크
        EnsureSceneRequirements();

        // [FIX] 고스트 이미지가 할당되지 않았다면 동적으로 생성
        if (ghostImage == null)
        {
            CreateDynamicGhostImage();
        }

        if (ghostImage != null)
        {
            ghostImage.gameObject.SetActive(false);
            ghostImage.raycastTarget = false;
        }

        if (rootCanvas == null && ghostImage != null)
        {
            rootCanvas = ghostImage.GetComponentInParent<Canvas>()?.rootCanvas;
        }
    }

    private void EnsureSceneRequirements()
    {
        // 1. 카메라에 PhysicsRaycaster가 있어야 3D 물체 클릭/드래그가 가능함
        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam.GetComponent<PhysicsRaycaster>() == null)
        {
            mainCam.gameObject.AddComponent<PhysicsRaycaster>();
            Debug.Log("[DragDrop] Main Camera에 PhysicsRaycaster를 추가했습니다.");
        }

        // 2. EventSystem이 있어야 UI 및 PhysicsRaycaster 이벤트가 집계됨
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
            Debug.Log("[DragDrop] EventSystem을 자동으로 생성했습니다.");
        }
    }


    private void CreateDynamicGhostImage()
    {
        // 1. 전용 캔버스 생성 (항상 최상단에 보이도록)
        GameObject canvasObj = new GameObject("DragDropCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // 최상단
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasObj);

        // 2. 고스트 이미지 생성
        GameObject ghostObj = new GameObject("GhostImage");
        ghostObj.transform.SetParent(canvasObj.transform);
        ghostImage = ghostObj.AddComponent<Image>();
        ghostImage.color = new Color(1, 1, 1, 0.6f); // 약간 투명하게
        ghostImage.raycastTarget = false;

        rootCanvas = canvas;
    }

    private void Update()
    {
        // 드래그 중이거나, 선택 모드(GameManager 에 선택된 것이 있을 때)일 경우 고스트 이미지 갱신
        if (IsDragging || (GameManager.Instance != null && (GameManager.Instance.SelectedUnitToPlace != null || GameManager.Instance.SelectedTileToPlace != null)))
        {
            // [FIX] 선택 모드일 때 인벤토리 잔량 실시간 체크
            if (!IsDragging)
            {
                if (GameManager.Instance.SelectedUnitToPlace != null)
                {
                    if (InventoryManager.Instance != null && InventoryManager.Instance.GetItemAmount(GameManager.Instance.SelectedUnitToPlace) <= 0)
                    {
                        Debug.Log("[DragDrop] Selected unit out of stock. Clearing selection.");
                        GameManager.Instance.ClearSelection();
                        return;
                    }
                }
                else if (GameManager.Instance.SelectedTileToPlace != null)
                {
                    if (InventoryManager.Instance != null && InventoryManager.Instance.GetItemAmount(GameManager.Instance.SelectedTileToPlace) <= 0)
                    {
                        Debug.Log("[DragDrop] Selected tile out of stock. Clearing selection.");
                        GameManager.Instance.ClearSelection();
                        return;
                    }
                }
            }

            UpdateGhostPosition(Input.mousePosition);

            // [FIX] 우클릭 또는 Esc로 드래그/선택 취소
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                if (IsDragging) CancelDrag();
                else GameManager.Instance.ClearSelection();
            }

            // [FIX] 선택 모드일 때 아이콘 강제 활성화 (클릭 배치용)
            if (!IsDragging)
            {
                if (GameManager.Instance.SelectedUnitToPlace != null)
                {
                    if (!ghostImage.gameObject.activeSelf)
                        ShowGhostForSelection(GameManager.Instance.SelectedUnitToPlace.icon);
                }
                else if (GameManager.Instance.SelectedTileToPlace != null)
                {
                    if (!ghostImage.gameObject.activeSelf)
                        ShowGhostForSelection(GameManager.Instance.SelectedTileToPlace.icon);
                }
            }
        }
        else
        {
            // 아무것도 안 하는 중이면 숨김
            if (ghostImage.gameObject.activeSelf && !IsDragging)
            {
                ghostImage.gameObject.SetActive(false);
            }
        }
    }

    private void ShowGhostForSelection(Sprite icon)
    {
        if (ghostImage == null) return;
        ghostImage.sprite = icon;
        ghostImage.gameObject.SetActive(true);
        ghostImage.rectTransform.sizeDelta = new Vector2(100, 100);
        ghostImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        ghostImage.color = new Color(1, 1, 1, 0.7f);
    }

    /// <summary>드래그 시작</summary>
    public void BeginDrag(DragPayload payload, Sprite icon)
    {
        CurrentPayload = payload;

        if (ghostImage != null)
        {
            ghostImage.sprite = icon;
            ghostImage.gameObject.SetActive(true);
            ghostImage.rectTransform.sizeDelta = new Vector2(100, 100); // 기본 크기
            ghostImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);

            // 아이콘이 없으면 투명하게
            ghostImage.color = (icon == null) ? Color.clear : new Color(1, 1, 1, 0.7f);
        }
    }

    /// <summary>매 프레임 고스트 이미지 위치 갱신</summary>
    public void UpdateGhostPosition(Vector2 screenPos)
    {
        if (ghostImage == null) return;

        // [FIX] PlacementManager를 통해 그리드 스내핑 좌표를 가져옴
        Vector3 targetScreenPos = screenPos;
        if (PlacementManager.Instance != null)
        {
            targetScreenPos = PlacementManager.Instance.GetGhostPosition(screenPos);
        }

        RectTransform parentRect = ghostImage.transform.parent as RectTransform;
        if (parentRect == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            targetScreenPos,
            (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? rootCanvas.worldCamera : null,
            out Vector2 localPos))
        {
            ghostImage.rectTransform.anchoredPosition = localPos;
        }
    }

    /// <summary>드래그 성공 종료 (UI에서 수동 호출하거나 OnEndDrag에서 호출됨)</summary>
    public void EndDrag(bool skipWorldCheck = false)
    {
        if (!IsDragging) return;

        if (!skipWorldCheck)
        {
            // [FIX] PlacementManager를 통해 통합 드롭 로직 실행
            ResolveWorldDrop();
        }
        else
        {
            CleanupDrag();
        }
    }

    private void ResolveWorldDrop()
    {
        if (PlacementManager.Instance == null)
        {
            CleanupDrag();
            return;
        }

        GameObject targetObj = null;
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

            // UI를 먼저 체크 (EventSystem)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                // UI 드롭은 보통 IDropHandler가 이미 처리했겠지만, 
                // 만약 여기까지 왔다면 유효한 UI 타겟을 찾지 못한 것.
                // PlacementManager가 Fallback 처리하도록 함.
            }
            else
            {
                // 월드 레이캐스트
                RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
                InputController ic = FindObjectOfType<InputController>();
                LayerMask tLayer = (ic != null) ? ic.tileLayer : LayerMask.GetMask("Ground");

                foreach (var hit in hits)
                {
                    if (((1 << hit.collider.gameObject.layer) & tLayer) != 0)
                    {
                        targetObj = hit.collider.gameObject;
                        break;
                    }
                }
            }
        }

        // 통합 배치 로직 실행 (targetObj가 null이면 Fallback Recall 실행됨)
        PlacementManager.Instance.ExecutePlacement(CurrentPayload, targetObj);
    }

    /// <summary>드래그 취소 (데이터 처리 없이 종료)</summary>
    public void CancelDrag()
    {
        if (IsDragging && PlacementManager.Instance != null)
        {
            PlacementManager.Instance.CancelPlacement(CurrentPayload);
        }
        CleanupDrag();
    }

    private void CleanupDrag()
    {
        CurrentPayload = null;

        if (ghostImage != null)
        {
            ghostImage.gameObject.SetActive(false);
        }
        Debug.Log("[DragDrop] Drag session cleaned up");
    }

}


