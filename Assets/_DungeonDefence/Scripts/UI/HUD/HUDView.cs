using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDView : MonoBehaviour
{
    [Header("Core Info")]
    [SerializeField] private Slider coreHpSlider;
    [SerializeField] private TextMeshProUGUI coreHpText;

    [Header("Battle Status")]
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI enemyCountText;

    [Header("Resources")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI populationText;

    [Header("System")]
    [SerializeField] private Button speedButton;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private Button settingsButton;

    [Header("Phase UI Containers")]
    [SerializeField] private GameObject maintenancePanel;
    [SerializeField] private GameObject battlePanel;

    [Header("Maintenance Controls")]
    [SerializeField] private Button bagButton;
    [SerializeField] private Button startWaveButton;

    public Button SpeedButton => speedButton;
    public Button SettingsButton => settingsButton;
    public Button BagButton => bagButton;
    public Button StartWaveButton => startWaveButton;

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

        // 전투 시작하면 인벤토리는 닫아야 할 수도 있음 (Manager에서 처리)
    }
}
