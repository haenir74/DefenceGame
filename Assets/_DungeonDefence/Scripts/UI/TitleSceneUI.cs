using UnityEngine;
using UnityEngine.UI;

public class TitleSceneUI : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        if (startButton != null) startButton.onClick.AddListener(OnStartClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void OnStartClicked()
    {
        if (SceneController.Instance != null)
            SceneController.Instance.LoadLobby();
    }

    private void OnQuitClicked()
    {
        if (SceneController.Instance != null)
            SceneController.Instance.QuitGame();
    }
}



