using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class SkillDataSO : ScriptableObject
{
    public string skillName;
    [TextArea] public string description;
    public Sprite icon;

    
    public abstract void Cast(Unit caster, Unit target);

    
    public virtual void OnUnitDie(Unit owner) { }

    
    public virtual void OnUnitUpdate(Unit owner) { }

    
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


