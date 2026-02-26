using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbySceneUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI perkPointText;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button backButton;

    private void Start()
    {
        UpdateUI();
        if (startGameButton != null) startGameButton.onClick.AddListener(OnStartGameClicked);
        if (backButton != null) backButton.onClick.AddListener(OnBackClicked);
    }

    private void UpdateUI()
    {
        if (MetaManager.Instance != null && perkPointText != null)
        {
            perkPointText.text = $"Perk Points: {MetaManager.Instance.PerkPoints}";
        }
    }

    private void OnStartGameClicked()
    {
        if (SceneController.Instance != null)
            SceneController.Instance.LoadGame();
    }

    private void OnBackClicked()
    {
        if (SceneController.Instance != null)
            SceneController.Instance.LoadTitle();
    }
}



