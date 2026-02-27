using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DungeonDefence.Tests.PlayMode
{
    public class UnitManagerTests
    {
        private GameObject testUnitObj;
        private Unit testUnit;

        [SetUp]
        public void Setup()
        {
            testUnitObj = new GameObject("TestUnit");
            testUnit = testUnitObj.AddComponent<Unit>();
            // Unit 컴포넌트 내부적으로 UnitDataSO가 필요할 수 있으나, 
            // 이벤트 트리거만 테스트하기 위해서는 UnitManager 내부의 Register/Unregister가
            // 호출될 수 있도록 수동 또는 가상의 데이터 주입 등을 사용합니다.
            // 테스트의 무결성을 위해 MonoBehaviour 기반 초기화는 Awake, Start 등에서 일어납니다.
        }

        [TearDown]
        public void Teardown()
        {
            if (testUnitObj != null)
                Object.Destroy(testUnitObj);

            // 싱글톤 상태를 정리(필요시)
            // if (UnitManager.Instance != null) UnitManager.Instance.Reset();
        }

        [UnityTest]
        public IEnumerator UnitSpawnAndDespawn_TriggersEvents_And_UpdatesTagRegistry()
        {
            bool isSpawnEventTriggered = false;
            bool isDespawnEventTriggered = false;

            UnitManager.Instance.OnUnitSpawned += (unit) => { isSpawnEventTriggered = true; };
            UnitManager.Instance.OnUnitDespawned += (unit) => { isDespawnEventTriggered = true; };

            // UnitDataSO를 ScriptableObject.CreateInstance로 생성하여 가짜 데이터로 주입
            UnitDataSO mockData = ScriptableObject.CreateInstance<UnitDataSO>();
            mockData.tags = UnitTag.Spider; // 스파이더 태그를 가상으로 설정
            mockData.category = UnitCategory.Normal;

            // 리플렉션을 사용하거나, UnitManager.SpawnUnit 등의 API를 모킹하여 등록
            testUnit.Initialize(mockData, null);

            // 1. 유닛 직접 레지스터 (Unit의 Start에서 호출된다면 Start 대기, 그렇지 않으면 수동 호출)
            UnitManager.Instance.RegisterUnit(testUnit);

            yield return null;

            // Assert: 이벤트가 트리거되었는지 확인
            Assert.IsTrue(isSpawnEventTriggered, "OnUnitSpawned event was not triggered.");

            // Assert: 태그 레지스트리 (GetUnitsByTag) Count가 1이 되었는지 확인
            var spiders = UnitManager.Instance.GetUnitsByTag(UnitTag.Spider);
            Assert.IsNotNull(spiders, "Tag registry returned null.");
            Assert.AreEqual(1, spiders.Count, $"Expected 1 Spider unit, but found {spiders.Count}");

            // 2. 유닛 등록 해제 (Despawn)
            UnitManager.Instance.UnregisterUnit(testUnit);

            yield return null;

            // Assert: Despawn 이벤트가 트리거되었는지 확인
            Assert.IsTrue(isDespawnEventTriggered, "OnUnitDespawned event was not triggered.");

            // Assert: 캐싱된 태그 콜렉션의 count 감소 확인
            var spidersAfterDespawn = UnitManager.Instance.GetUnitsByTag(UnitTag.Spider);
            int countAfter = (spidersAfterDespawn != null) ? spidersAfterDespawn.Count : 0;
            Assert.AreEqual(0, countAfter, "Target tag collection count did not decrease after unit despawn.");
        }
    }
}
