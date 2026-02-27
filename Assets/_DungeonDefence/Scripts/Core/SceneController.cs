using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneController : Singleton<SceneController>
{
    protected override bool DontDestroy => true;

    public const string TITLE_SCENE = "01_TitleScene";
    public const string LOBBY_SCENE = "02_LobbyScene";
    public const string GAME_SCENE = "03_GameScene";
    public const string RESULT_SCENE = "04_ResultScene";

    public void LoadTitle() => LoadScene(TITLE_SCENE);
    public void LoadLobby() => LoadScene(LOBBY_SCENE);
    public void LoadGame() => LoadScene(GAME_SCENE);
    public void LoadResult() => LoadScene(RESULT_SCENE);

    private void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}



