using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResultUI : MonoBehaviour
{
    
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI killText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI perkPointText;

    
    [SerializeField] private Button restartButton;
    [SerializeField] private Button titleButton;

    private void Awake()
    {
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
        if (titleButton != null) titleButton.onClick.AddListener(OnTitleClicked);
    }

    private void Start()
    {
        if (MetaManager.Instance != null && MetaManager.Instance.LastRun.waves > 0)
        {
            var result = MetaManager.Instance.LastRun;
            Show(result.isVictory, result.waves, result.kills, result.gold);
        }
    }

    public void Show(bool isVictory, int waves, int kills, int gold)
    {
        gameObject.SetActive(true);

        if (titleText != null)
            titleText.text = isVictory ? "VICTORY!" : "GAME OVER";

        if (waveText != null) waveText.text = $"Waves Cleared: {waves}";
        if (killText != null) killText.text = $"Enemies Defeated: {kills}";
        if (goldText != null) goldText.text = $"Total Gold Earned: {gold}";

        if (scoreText != null) scoreText.text = $"Final Score: {MetaManager.Instance.LastRun.score}";
        if (perkPointText != null) perkPointText.text = $"Earned Perk Points: +{MetaManager.Instance.LastRun.earnedPoints}";
    }

    private void OnRestartClicked()
    {
        if (SceneController.Instance != null)
            SceneController.Instance.LoadLobby();
    }

    private void OnTitleClicked()
    {
        if (SceneController.Instance != null)
            SceneController.Instance.LoadTitle();
    }
}



