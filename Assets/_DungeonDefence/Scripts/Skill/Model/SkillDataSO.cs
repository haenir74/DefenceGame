using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class SkillDataSO : ScriptableObject
{
    public string skillName;
    [TextArea] public string description;
    public Sprite icon;

    // 스킬 실행 로직 (각 스킬이 오버라이드)
    public abstract void Cast(Unit caster, Unit target);

    /// <summary>유닛 사망 시 호출 (자폭, 분열 등 사망 시 발동 효과용)</summary>
    public virtual void OnUnitDie(Unit owner) { }

    /// <summary>유닛의 매 Update마다 호출 (지속 효과 등)</summary>
    public virtual void OnUnitUpdate(Unit owner) { }

    /// <summary>유닛이 적을 처치했을 때 호출 (처치 시 보너스 등)</summary>
    public virtual void OnUnitKill(Unit owner, Unit victim) { }

    protected List<Unit> GetEnemies(Unit caster)
    {
        if (caster.CurrentNode == null) return new List<Unit>();

        var unitsOnTile = UnitManager.Instance.GetUnitsOnNode(caster.CurrentNode);

        return unitsOnTile
            .Where(u => u != caster && 
                        u.IsPlayerTeam != caster.IsPlayerTeam && 
                        !u.Combat.IsDead)
            .ToList();
    }
    
    protected List<Unit> GetAllies(Unit caster)
    {
        if (caster.CurrentNode == null) return new List<Unit>();

        return UnitManager.Instance.GetUnitsOnNode(caster.CurrentNode)
            .Where(u => u.IsPlayerTeam == caster.IsPlayerTeam && 
                        !u.Combat.IsDead)
            .ToList();
    }
}