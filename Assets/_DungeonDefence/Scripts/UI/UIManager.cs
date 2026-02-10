using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private HUDView hudView;
    [SerializeField] private InventoryUIView inventoryView;

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
            EconomyManager.Instance.OnGoldChanged += (gold) => 
            {
                currentGold = gold;
                hudView.UpdateResources(currentGold, currentPop, maxPop);
            };
        }

        if (UnitManager.Instance != null)
        {
            UnitManager.Instance.OnUnitCountChanged += (ally, enemy) => 
            {
                currentPop = ally;
                hudView.UpdateResources(currentGold, currentPop, maxPop);
            };
            
            UnitManager.Instance.OnCoreHpChanged += (cur, max) => 
                hudView.UpdateCoreInfo(cur, max);
        }

        // 코어 체력
        if (UnitManager.Instance != null)
        {
            UnitManager.Instance.OnCoreHpChanged += (cur, max) => 
                hudView.UpdateCoreInfo(cur, max);
        }

        // 웨이브 정보
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveInfoChanged += (wave, rem, total) => 
                hudView.UpdateWaveInfo(wave, rem, total);
        }

        // 시스템 버튼
        if (hudView != null)
        {
            hudView.SpeedButton?.onClick.AddListener(ToggleGameSpeed);
            // hudView.SettingsButton?.onClick.AddListener(OnSettingsClick);
        }

        // 인벤토리 버튼
        if (hudView != null && hudView.BagButton != null)
        {
            hudView.BagButton.onClick.AddListener(() => 
            {
                inventoryView.ToggleInventory();
            });
        }

        // 전투 시작 버튼
        if (hudView != null && hudView.StartWaveButton != null)
        {
            hudView.StartWaveButton.onClick.RemoveAllListeners();
            hudView.StartWaveButton.onClick.AddListener(() => 
            {
                Debug.Log("[UI] 전투 시작 버튼 클릭됨");
                GameManager.Instance.StartBattlePhase();
            });
        }
    }

    private void InitializeUI()
    {
        if (hudView == null) return;
        if (EconomyManager.Instance) currentGold = EconomyManager.Instance.CurrentGold;
        
        hudView.UpdateResources(currentGold, currentPop, maxPop);
        hudView.UpdateWaveInfo(1, 0, 0); 
        hudView.UpdateSpeed(timeScale);
    }

    private void ToggleGameSpeed()
    {
        timeScale = (timeScale >= 1.5f) ? 1.0f : 2.0f;
        Time.timeScale = timeScale;
        hudView?.UpdateSpeed(timeScale);
    }

    // 페이즈 전환
    
    public void SwitchToMaintenancePhase()
    {
        hudView?.SetPhaseUI(false); 
        // inventoryView.CloseInventory(); 
    }

    public void SwitchToBattlePhase()
    {
        hudView?.SetPhaseUI(true);
        inventoryView?.CloseInventory();
    }
}