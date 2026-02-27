using UnityEngine;

/// <summary>
/// A centralized bootstrapper to explicitly define the initialization order of all Singelton Managers.
/// Prevents race conditions and avoids scattered Awake/Start/Coroutine waiting times.
/// Must be set with highest execution order.
/// </summary>
public class GameBootstrapper : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void RuntimeInit()
    {
        // Add a game object with GameBootstrapper if it does not exist, but let's just make it a user component.
    }

    private void Start()
    {
        // Explicitly initialize managers in the correct order of data flow

        if (MetaManager.Instance != null) MetaManager.Instance.Initialize();
        if (EconomyManager.Instance != null) EconomyManager.Instance.Initialize();
        if (InventoryManager.Instance != null) InventoryManager.Instance.Initialize();

        // Critical Core Systems
        if (GridManager.Instance != null) GridManager.Instance.Initialize();
        if (UnitManager.Instance != null) UnitManager.Instance.Initialize();

        // Logic Systems
        if (WaveManager.Instance != null) WaveManager.Instance.Initialize();
        if (ShopManager.Instance != null) ShopManager.Instance.Initialize();
        if (DispatchManager.Instance != null) DispatchManager.Instance.Initialize();

        // UI and Visual
        if (CameraManager.Instance != null) CameraManager.Instance.Initialize();
        if (PerkTreeManager.Instance != null) PerkTreeManager.Instance.Initialize();
        if (TooltipManager.Instance != null) TooltipManager.Instance.Initialize();
        if (UIManager.Instance != null) UIManager.Instance.Initialize();

        // Game orchestrator initializes last
        if (GameManager.Instance != null) GameManager.Instance.Initialize();

        // If we are in the boot scene, transition to title after initialization
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("Boot"))
        {
            SceneController.Instance?.LoadTitle();
        }
    }
}
