using UnityEngine;

/// <summary>
/// 타일 위에서 발생하는 이벤트를 처리하는 ScriptableObject 기반 추상 클래스.
/// 구체 구현체를 만들어 TileDataSO의 tileEffect 필드에 연결하면 된다.
/// </summary>
public abstract class TileEffectDataSO : ScriptableObject
{
    [Header("Basic Info")]
    public string effectName;
    [TextArea] public string description;

    /// <summary>유닛이 이 타일에 처음 진입했을 때 호출.</summary>
    public virtual void OnEnter(Unit unit) { }

    /// <summary>유닛이 이 타일 위에 있는 동안 매 프레임 호출.</summary>
    public virtual void OnUpdate(Unit unit) { }

    /// <summary>유닛이 이 타일에서 나갈 때 호출 (다른 타일로 이동).</summary>
    public virtual void OnExit(Unit unit) { }

    /// <summary>유닛이 이 타일 위에서 사망할 때 호출.</summary>
    public virtual void OnDeath(Unit unit) { }
}
