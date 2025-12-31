using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileAction : ScriptableObject
{
    // 액션 실행 함수
    // node: 액션이 발생한 타일 노드
    // instigator: 액션을 유발한 주체 (플레이어 매니저, 적 유닛 등) - null일 수도 있음
    public abstract void Execute(Node node, GameObject instigator = null);
}