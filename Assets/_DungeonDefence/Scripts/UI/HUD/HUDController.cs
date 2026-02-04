using UnityEngine;
using TMPro; // TextMeshPro 사용 권장 (또는 UnityEngine.UI)

public class HUDController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI unitCountText;
    // [SerializeField] private TextMeshProUGUI lifeText; // 필요 시 추가

    private void Start()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnGoldChanged += UpdateGoldUI;
            UpdateGoldUI(EconomyManager.Instance.CurrentGold); // 초기값 설정
        }
        if (UnitManager.Instance != null)
        {
            UnitManager.Instance.OnUnitSpawned += _ => UpdateUnitCount();
            UnitManager.Instance.OnUnitDied += _ => UpdateUnitCount();
            UpdateUnitCount();
        }
    }

    private void UpdateGoldUI(int amount)
    {
        if (goldText != null) 
            goldText.text = $"Gold: {amount}";
    }

    private void UpdateUnitCount()
    {
        if (unitCountText != null && UnitManager.Instance != null)
        {
            int count = UnitManager.Instance.GetAllUnits().Count;
            unitCountText.text = $"Units: {count}";
        }
    }

    private void OnDestroy()
    {
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnGoldChanged -= UpdateGoldUI;
            
        if (UnitManager.Instance != null)
        {
            UnitManager.Instance.OnUnitSpawned -= _ => UpdateUnitCount();
            UnitManager.Instance.OnUnitDied -= _ => UpdateUnitCount();
        }
    }
}