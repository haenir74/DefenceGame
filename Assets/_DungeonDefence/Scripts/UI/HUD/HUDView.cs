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

    public Button SpeedButton => speedButton;
    public Button SettingsButton => settingsButton;
}
