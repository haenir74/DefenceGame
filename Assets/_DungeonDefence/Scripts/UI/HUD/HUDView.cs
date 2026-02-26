using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDView : MonoBehaviour
{
    
    [SerializeField] private Slider coreHpSlider;
    [SerializeField] private TextMeshProUGUI coreHpText;

    
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI enemyCountText;

    
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI populationText;

    
    [SerializeField] private Button speedButton;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private Button settingsButton;

    
    [SerializeField] private GameObject maintenancePanel;
    [SerializeField] private GameObject battlePanel;

    
    [SerializeField] private Button bagButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button startWaveButton;
    [SerializeField] private Button dispatchButton;

    public Button SpeedButton => speedButton;
    public Button SettingsButton => settingsButton;
    public Button BagButton => bagButton;
    public Button ShopButton => shopButton;
    public Button StartWaveButton => startWaveButton;
    public Button DispatchButton => dispatchButton;
    public RectTransform MaintenancePanelRect => maintenancePanel != null ? maintenancePanel.GetComponent<RectTransform>() : null;

    public void UpdateCoreInfo(float current, float max)
    {
        if (coreHpSlider != null)
        {
            coreHpSlider.maxValue = max;
            coreHpSlider.value = current;
        }
        if (coreHpText != null)
        {
            coreHpText.text = $"{current:F0} / {max:F0}";
        }
    }

    public void UpdateResources(int gold, int currentPop, int maxPop)
    {
        if (goldText != null) goldText.text = $"{gold}";
        if (populationText != null) populationText.text = $"{currentPop}/{maxPop}";
    }

    public void UpdateWaveInfo(int waveIndex, int remaining, int total)
    {
        if (waveText != null)
            waveText.text = $"WAVE {waveIndex}";

        if (enemyCountText != null)
            enemyCountText.text = $"{remaining} / {total}";
    }

    public void UpdateSpeed(float speed)
    {
        if (speedText != null) speedText.text = $"x{speed}";
    }

    public void SetPhaseUI(bool isBattlePhase)
    {
        if (maintenancePanel != null) maintenancePanel.SetActive(!isBattlePhase);
        if (battlePanel != null) battlePanel.SetActive(isBattlePhase);
    }
}



