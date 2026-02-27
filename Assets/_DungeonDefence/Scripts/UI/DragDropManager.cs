using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragPayload
{
    public enum SourceType { Inventory, Grid, Dispatch }

    public SourceType Source;


    public UnitDataSO UnitData;
    public TileDataSO TileData;


    public Unit GridUnit;


    public DispatchSlotUI FromSlot;


    public GridNode OriginalNode;
}

public class DragDropManager : Singleton<DragDropManager>
{
    protected override bool DontDestroy => true;

    [SerializeField] private Image ghostImage;
    [SerializeField] private Canvas rootCanvas;

    public DragPayload CurrentPayload { get; private set; }
    public bool IsDragging => CurrentPayload != null;

    protected override void Awake()
    {
        base.Awake();


        EnsureSceneRequirements();


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

        UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnSceneUnloaded(UnityEngine.SceneManagement.Scene scene)
    {
        CancelDrag();
    }

    private void EnsureSceneRequirements()
    {

        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam.GetComponent<PhysicsRaycaster>() == null)
        {
            mainCam.gameObject.AddComponent<PhysicsRaycaster>();

        }


        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();

        }
    }

    private void CreateDynamicGhostImage()
    {

        GameObject canvasObj = new GameObject("DragDropCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasObj);


        GameObject ghostObj = new GameObject("GhostImage");
        ghostObj.transform.SetParent(canvasObj.transform);
        ghostImage = ghostObj.AddComponent<Image>();
        ghostImage.color = new Color(1, 1, 1, 0.6f);
        ghostImage.raycastTarget = false;

        rootCanvas = canvas;
    }

    private void Update()
    {

        if (IsDragging || (GameManager.Instance != null && (GameManager.Instance.SelectedUnitToPlace != null || GameManager.Instance.SelectedTileToPlace != null)))
        {

            if (!IsDragging)
            {
                if (GameManager.Instance.SelectedUnitToPlace != null)
                {
                    if (InventoryManager.Instance != null && InventoryManager.Instance.GetItemAmount(GameManager.Instance.SelectedUnitToPlace) <= 0)
                    {

                        GameManager.Instance.ClearSelection();
                        return;
                    }
                }
                else if (GameManager.Instance.SelectedTileToPlace != null)
                {
                    if (InventoryManager.Instance != null && InventoryManager.Instance.GetItemAmount(GameManager.Instance.SelectedTileToPlace) <= 0)
                    {

                        GameManager.Instance.ClearSelection();
                        return;
                    }
                }
            }

            UpdateGhostPosition(Input.mousePosition);


            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                if (IsDragging) CancelDrag();
                else GameManager.Instance.ClearSelection();
            }


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


    public void BeginDrag(DragPayload payload, Sprite icon)
    {
        CurrentPayload = payload;

        if (ghostImage != null)
        {
            ghostImage.sprite = icon;
            ghostImage.gameObject.SetActive(true);
            ghostImage.rectTransform.sizeDelta = new Vector2(100, 100);
            ghostImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);


            ghostImage.color = (icon == null) ? Color.clear : new Color(1, 1, 1, 0.7f);
        }
    }


    public void UpdateGhostPosition(Vector2 screenPos)
    {
        if (ghostImage == null) return;


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


    public void EndDrag(bool skipWorldCheck = false)
    {
        if (!IsDragging) return;

        if (!skipWorldCheck)
        {

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


            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {



            }
            else
            {

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


        PlacementManager.Instance.ExecutePlacement(CurrentPayload, targetObj);
    }


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
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
}



