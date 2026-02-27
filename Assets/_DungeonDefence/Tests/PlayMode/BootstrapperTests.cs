using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DungeonDefence.Tests.PlayMode
{
    public class BootstrapperTests
    {
        [UnityTest]
        public IEnumerator GameBootstrapper_Initializes_AllSingletonManagers()
        {
            // 씬 로드나 다른 매니저 오브젝트들이 구성되어 있다고 가정할 수도 있으나,
            // 게임 부트스트래퍼 컴포넌트 추가를 통한 시뮬레이션
            GameObject bootstrapperObj = new GameObject("TestBootstrapper");
            bootstrapperObj.AddComponent<GameBootstrapper>();

            // 1프레임 대기하여 Start() 또는 Awake() 사이클이 실행되도록 함
            yield return null;

            // Assert: 매니저들이 정상적으로 생성/초기화되었는지 확인
            Assert.IsNotNull(PoolManager.Instance, "PoolManager was not initialized.");
            Assert.IsNotNull(EconomyManager.Instance, "EconomyManager was not initialized.");
            Assert.IsNotNull(GridManager.Instance, "GridManager was not initialized.");
            Assert.IsNotNull(UnitManager.Instance, "UnitManager was not initialized.");
            Assert.IsNotNull(DispatchManager.Instance, "DispatchManager was not initialized.");

            // Cleanup
            Object.Destroy(bootstrapperObj);
        }
    }
}
