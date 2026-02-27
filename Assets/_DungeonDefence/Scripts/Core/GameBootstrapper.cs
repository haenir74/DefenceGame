using UnityEngine;

/// <summary>
/// A centralized bootstrapper to explicitly define the initialization order of all Singelton Managers.
/// Prevents race conditions and avoids scattered Awake/Start/Coroutine waiting times.
/// Must be set with highest execution order.
/// </summary>
public class GameBootstrapper : MonoBehaviour
{
    private static bool initialized = false;

    private void Start()
    {
        if (initialized) return;

        Debug.Log("[GameBootstrapper] Starting global system initialization...");

        // Use try-catch to ensure we at least try to move to the next scene even if a manager fails
        try
        {
            if (MetaManager.Instance != null) MetaManager.Instance.Initialize();
            if (EconomyManager.Instance != null) EconomyManager.Instance.Initialize();
            if (InventoryManager.Instance != null) InventoryManager.Instance.Initialize();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameBootstrapper] Error during manager initialization: {e.Message}\n{e.StackTrace}");
        }

        initialized = true;

        // Check scene and transition
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene.Contains("Boot"))
        {
            StartCoroutine(TransitionToTitle());
        }
    }

    private System.Collections.IEnumerator TransitionToTitle()
    {
        // Give time for everything to settle
        yield return new WaitForSeconds(0.5f);

        Debug.Log("[GameBootstrapper] Attempting transition to Title Scene...");

        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadTitle();
        }
        else
        {
            Debug.LogWarning("[GameBootstrapper] SceneController instance not found, using direct SceneManager load.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("01_TitleScene");
        }
    }
}
