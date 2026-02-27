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

        // Initialize PerkTreeManager for this scene
        if (PerkTreeManager.Instance != null)
        {
            PerkTreeManager.Instance.Initialize();
        }

        if (startGameButton != null)
        {
            startGameButton.onClick.RemoveAllListeners();
            startGameButton.onClick.AddListener(OnStartGameClicked);
        }
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackClicked);
        }
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
        Debug.Log("[LobbySceneUI] Start Game Button Clicked. Loading Game Scene...");
        if (SceneController.Instance != null)
            SceneController.Instance.LoadGame();
    }

    private void OnBackClicked()
    {
        Debug.Log("[LobbySceneUI] Back Button Clicked. Loading Title Scene...");
        if (SceneController.Instance != null)
            SceneController.Instance.LoadTitle();
    }
}



