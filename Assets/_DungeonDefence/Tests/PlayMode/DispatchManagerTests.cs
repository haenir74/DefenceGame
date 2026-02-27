using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DungeonDefence.Tests.PlayMode
{
    public class DispatchManagerTests
    {
        private GameObject bootstrapperObj;

        [SetUp]
        public void Setup()
        {
            // 의존성 초기화를 위해 Bootstrapper를 통한 매니저 셋업
            bootstrapperObj = new GameObject("TestBootstrapper");
            bootstrapperObj.AddComponent<GameBootstrapper>();
        }

        [TearDown]
        public void Teardown()
        {
            if (bootstrapperObj != null)
                Object.Destroy(bootstrapperObj);

            // DispatchManager 상태 초기화가 필요한 경우 실행
            if (DispatchManager.Instance != null && DispatchManager.Instance.DispatchSlots.Count > 0)
            {
                for (int i = DispatchManager.Instance.DispatchSlots.Count - 1; i >= 0; i--)
                {
                    DispatchManager.Instance.RequestRecall(i, false);
                }
            }
        }

        [UnityTest]
        public IEnumerator DispatchManager_StateCommands_TriggersEventsProperly()
        {
            // Arrange: 1프레임 대기 후 Start()가 불리면서 매니저들이 구성됨
            yield return null;

            bool isStateChangedTriggered = false;

            // 상태 변경 검증을 위한 이벤트 구독 (UI 순수 분리 체크용)
            DispatchManager.Instance.OnDispatchStateChanged += () => { isStateChangedTriggered = true; };

            // 테스트 목적용 Unit & UnitData 셋업
            GameObject unitObj = new GameObject("TestMockUnit");
            Unit mockUnit = unitObj.AddComponent<Unit>();

            UnitDataSO mockData = ScriptableObject.CreateInstance<UnitDataSO>();
            mockData.category = UnitCategory.Normal;
            mockData.dispatchEfficiency = 1.0f;
            mockData.isPlayerTeam = true;

            mockUnit.Initialize(mockData, null);

            int targetSlotIndex = 0;

            // Act 1: RequestAssignUnit 호출 (커맨드 패턴 검증 수행)
            bool assignSuccess = DispatchManager.Instance.RequestAssignUnit(targetSlotIndex, mockUnit);

            // DispatchManager가 즉시 처리하는 경우 yield가 필수적이지 않으나, 안전을 위해 1프레임 대기할 수 있음.
            yield return null;

            // Assert 1: Assign 수행결과 및 상태 변경 알림 이벤트 작동 여부
            Assert.IsTrue(assignSuccess, "RequestAssignUnit returned false.");
            Assert.IsTrue(isStateChangedTriggered, "OnDispatchStateChanged was not triggered after Assignment.");
            Assert.AreEqual(1, DispatchManager.Instance.DispatchSlots.Count, "DispatchSlots count should be 1.");
            Assert.IsNotNull(DispatchManager.Instance.DispatchSlots[targetSlotIndex].Unit, "Assigned unit is null.");

            // Reset Flag
            isStateChangedTriggered = false;

            // Act 2: RequestRecall 호출
            DispatchManager.Instance.RequestRecall(targetSlotIndex, false); // 아이템 인벤토리 반환(false)
            yield return null;

            // Assert 2: Recall 수행결과 및 상태 변경 이벤트 작동
            Assert.IsTrue(isStateChangedTriggered, "OnDispatchStateChanged was not triggered after Recall.");
            Assert.AreEqual(0, DispatchManager.Instance.DispatchSlots.Count, "DispatchSlots count did not decrease after Recall.");

            // Cleanup
            Object.Destroy(unitObj);
        }
    }
}
