using UnityEngine;
using UnityEngine.UI;
using Panex.Inventory;

public class InventoryUIView : MonoBehaviour
{
    [Header("Container")]
    [SerializeField] private RectTransform containerRect;
    [SerializeField] private float slideDuration = 0.3f;
    
    [Header("Tabs")]
    [SerializeField] private Button unitTabButton;
    [SerializeField] private Button tileTabButton;
    [SerializeField] private GameObject unitListObj;
    [SerializeField] private GameObject tileListObj;

    [Header("Close")]
    [SerializeField] private Button closeButton;

    private bool isOpen = false;
    private float hiddenY = -200f;
    private float shownY = 0f;

    private void Start()
    {
        isOpen = false;
        ShowUnitTab();

        unitTabButton.onClick.AddListener(ShowUnitTab);
        tileTabButton.onClick.AddListener(ShowTileTab);
        if (closeButton != null) closeButton.onClick.AddListener(CloseInventory);

        if(containerRect) containerRect.gameObject.SetActive(false);
    }

    public void ToggleInventory()
    {
        if (isOpen) CloseInventory();
        else OpenInventory();
    }

    public void OpenInventory()
    {
        isOpen = true;
        if(containerRect) 
        {
            containerRect.gameObject.SetActive(true);
        }
    }

    public void CloseInventory()
    {
        isOpen = false;
        if(containerRect) 
        {
            containerRect.gameObject.SetActive(false);
        }
    }

    private void ShowUnitTab()
    {
        unitListObj.SetActive(true);
        tileListObj.SetActive(false);
    }

    private void ShowTileTab()
    {
        unitListObj.SetActive(false);
        tileListObj.SetActive(true);
    }
}