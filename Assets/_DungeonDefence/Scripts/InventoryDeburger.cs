using UnityEngine;

public class InventoryDebugger : MonoBehaviour
{
    [Header("Test Data")]
    public UnitDataSO testUnit; // 인스펙터에서 테스트할 유닛 데이터 할당
    public TileDataSO testTile; // 인스펙터에서 테스트할 타일 데이터 할당

    void Update()
    {
        // 1. 유닛 추가 테스트 (U 키)
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (testUnit != null)
            {
                Debug.Log($"[Test] 유닛 추가 시도: {testUnit.Name}");
                InventoryManager.Instance.AddItem(testUnit, 1);
            }
            else Debug.LogWarning("[Test] 테스트할 UnitDataSO가 할당되지 않았습니다.");
        }

        // 2. 타일 추가 테스트 (T 키)
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (testTile != null)
            {
                // 타일은 한 번에 5개씩 줘봅시다 (스택 테스트용)
                Debug.Log($"[Test] 타일 추가 시도: {testTile.Name} x 5");
                InventoryManager.Instance.AddItem(testTile, 5);
            }
            else Debug.LogWarning("[Test] 테스트할 TileDataSO가 할당되지 않았습니다.");
        }

        // 3. 소모 테스트 (BackSpace 키)
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            Debug.Log("[Test] 아이템 소모 시도");
            InventoryManager.Instance.TryConsumeItem(testTile, 1);
        }
    }
}