using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private HUDView hudView;
    [SerializeField] private InventoryUIView inventoryView;
    [SerializeField] private ShopUIView shopView;
    [SerializeField] private DispatchPanelUI dispatchPanel;

    private int currentGold = 0;
    private int currentPop = 0;
    private int maxPop = 20;

    private float timeScale = 1.0f;

    private void Start()
    {
        ConnectEvents();
        InitializeUI();
    }

    private void ConnectEvents()
    {
        // 자원
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnCurrencyChanged += (type, amount) =>
            {
                if (type == CurrencyType.Gold)
                {
                    currentGold = amount;
                    hudView?.UpdateResources(currentGold, currentPop, maxPop);
                    if (shopView != null && shopView.gameObject.activeSelf)
                    {
                        shopView.UpdateGoldText(currentGold);
                    }
                }
            };
        }

        // 코어 체력
        if (UnitManager.Instance != null)
        {
            UnitManager.Instance.OnUnitCountChanged += (ally, enemy) =>
            {
                currentPop = ally;
                hudView?.UpdateResources(currentGold, currentPop, maxPop);
            };

            UnitManager.Instance.OnCoreHpChanged += (cur, max) =>
                hudView?.UpdateCoreInfo(cur, max);
        }

        // 웨이브 정보
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveInfoChanged += (wave, rem, total) =>
                hudView?.UpdateWaveInfo(wave, rem, total);
        }

        // 시스템 버튼
        if (hudView != null)
        {
            hudView.SpeedButton?.onClick.AddListener(ToggleGameSpeed);
            hudView.StartWaveButton?.onClick.AddListener(() =>
            {
                Debug.Log("[UI] 전투 시작 요청");
                CloseAllPopups();
                GameManager.Instance.StartBattlePhase();
            });
            hudView.BagButton?.onClick.AddListener(ToggleInventory);
            hudView.ShopButton?.onClick.AddListener(ToggleShop);
            hudView.DispatchButton?.onClick.AddListener(ToggleDispatchPanel);
        }

        // 상점
        if (shopView != null)
        {
            shopView.OnCloseRequested += CloseShop;
            shopView.OnRerollRequested += () =>
            {
                if (ShopManager.Instance != null)
                    ShopManager.Instance.RerollShop();
            };
        }
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.OnShopRefreshed += () =>
            {
                if (shopView != null && shopView.gameObject.activeSelf)
                    shopView.RefreshShop();
            };
        }

        // 인벤토리 상태 동기화 (파견 패널 등)
        if (inventoryView != null)
        {
            inventoryView.OnOpenStatusChanged += (isOpenStatus) =>
            {
                if (dispatchPanel != null)
                    dispatchPanel.gameObject.SetActive(isOpenStatus);
            };
        }
    }

    private void InitializeUI()
    {
        if (hudView != null)
        {
            if (EconomyManager.Instance) currentGold = EconomyManager.Instance.GetCurrencyAmount(CurrencyType.Gold);
            hudView.UpdateResources(currentGold, currentPop, maxPop);
            hudView.UpdateWaveInfo(GameManager.Instance.CurrentWave, 0, 0);
            hudView.UpdateSpeed(timeScale);

            // 초기 코어 정보 설정
            if (UnitManager.Instance != null)
            {
                var core = UnitManager.Instance.GetAllUnits().Find(u => u.Data != null && u.Data.category == UnitCategory.Core);
                if (core != null)
                {
                    hudView.UpdateCoreInfo(core.Combat.CurrentHp, core.Data.maxHp);
                }
            }
        }

        if (inventoryView != null && hudView != null)
        {
            inventoryView.AddLinkedRectTransform(hudView.MaintenancePanelRect);
        }

        CloseAllPopups();
    }

    private void ToggleGameSpeed()
    {
        timeScale = (timeScale >= 1.5f) ? 1.0f : 2.0f;
        Time.timeScale = timeScale;
        hudView?.UpdateSpeed(timeScale);
    }

    public void ToggleShop()
    {
        if (shopView == null) return;

        if (!shopView.gameObject.activeSelf)
        {
            inventoryView?.Close();
            shopView.Open();
            shopView.UpdateGoldText(currentGold);
            shopView.RefreshShop();
        }
        else
        {
            shopView.Close();
        }
    }

    public void OpenShop()
    {
        inventoryView?.Close();
        if (shopView != null)
        {
            shopView.Open();
            shopView.UpdateGoldText(currentGold);
            shopView.RefreshShop();
        }
    }

    public void CloseShop()
    {
        shopView?.Close();
    }

    public void ToggleInventory()
    {
        if (inventoryView == null) return;

        if (!inventoryView.IsOpen)
        {
            shopView?.Close();
            inventoryView.Open();
        }
        else
        {
            inventoryView.Close();
        }
    }

    public void CloseAllPopups()
    {
        shopView?.Close();
        inventoryView?.Close();
        dispatchPanel?.gameObject.SetActive(false);
    }

    // 페이즈 전환    
    public void SwitchToMaintenancePhase()
    {
        hudView?.SetPhaseUI(false);
        hudView?.UpdateWaveInfo(GameManager.Instance.CurrentWave, 0, 0);
        CloseAllPopups();
    }

    public void SwitchToBattlePhase()
    {
        hudView?.SetPhaseUI(true);
        CloseAllPopups();
        dispatchPanel?.gameObject.SetActive(false);
    }

    public void ToggleDispatchPanel()
    {
        if (dispatchPanel == null) return;
        bool isOpen = dispatchPanel.gameObject.activeSelf;
        if (!isOpen)
        {
            shopView?.Close();
            inventoryView?.Close();
            dispatchPanel.gameObject.SetActive(true);
        }
        else
        {
            dispatchPanel.gameObject.SetActive(false);
        }
    }
}

