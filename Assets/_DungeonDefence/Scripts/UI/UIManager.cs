using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private HUDView hudView;
    [SerializeField] private InventoryUIView inventoryView;
    [SerializeField] private ShopUIView shopView;

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

        // activeSelf 대신 InventoryUIView의 공개 메서드를 사용하여 상태 제어
        // 만약 애니메이션 중인 상태를 고려한다면 InventoryUIView에 isOpen 프로퍼티를 public으로 노출하는 것이 좋습니다.
        if (!inventoryView.gameObject.activeSelf) 
        {
            shopView?.Close();
            inventoryView.gameObject.SetActive(true); // 오브젝트가 꺼져있다면 먼저 켬
            inventoryView.Open();
        }
        else
        {
            // 이미 켜져 있다면 Toggle 내 로직에 따라 Close 호출
            inventoryView.ToggleInventory(); 
        }
    }

    public void CloseAllPopups()
    {
        shopView?.Close();
        inventoryView?.Close();
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
    }
}