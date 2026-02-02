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

    protected List<Unit> GetEnemies(Unit caster)
    {
        if (caster.CurrentNode == null) return new List<Unit>();

        return caster.CurrentNode.UnitsOnTile
            .Where(u => u != caster && 
                        u.IsPlayerTeam != caster.IsPlayerTeam && 
                        !u.Combat.IsDead)
            .ToList();
    }
    
    protected List<Unit> GetAllies(Unit caster)
    {
        if (caster.CurrentNode == null) return new List<Unit>();

        return caster.CurrentNode.UnitsOnTile
            .Where(u => u.IsPlayerTeam == caster.IsPlayerTeam && 
                        !u.Combat.IsDead)
            .ToList();
    }
}