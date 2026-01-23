using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Effects/Damage Effect")]
public class DamageEffectSO : TileEffectSO
{
    public float damageAmount = 10f;

    public override void ExecuteEnter(Unit unit)
    {
        unit.TakeDamage(damageAmount);
        Debug.Log($"{unit.name}에게 {damageAmount} 데미지 적용!");
    }

    public override void ExecuteExit(Unit unit) { }
}