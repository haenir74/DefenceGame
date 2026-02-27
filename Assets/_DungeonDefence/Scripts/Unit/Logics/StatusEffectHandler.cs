using System.Collections.Generic;
using UnityEngine;



public class StatusEffectHandler : MonoBehaviour
{
    private Dictionary<TileEffectDataSO, StatusEffectInstance> activeEffects = new Dictionary<TileEffectDataSO, StatusEffectInstance>();

    public void AddEffect(TileEffectDataSO effectData)
    {
        if (effectData == null) return;

        if (activeEffects.TryGetValue(effectData, out var instance))
        {
            if (effectData.isStackable && instance.CurrentStacks < effectData.maxStacks)
            {
                instance.CurrentStacks++;
            }
            instance.TimeRemaining = effectData.baseDuration;
            effectData.ApplyEffect(GetComponent<Unit>(), instance.CurrentStacks);
        }
        else
        {
            var newInstance = new StatusEffectInstance
            {
                Data = effectData,
                TimeRemaining = effectData.baseDuration,
                CurrentStacks = 1
            };
            activeEffects.Add(effectData, newInstance);
            effectData.ApplyEffect(GetComponent<Unit>(), 1);
        }
    }

    public void RemoveEffect(TileEffectDataSO effectData)
    {
        if (effectData == null) return;

        if (activeEffects.TryGetValue(effectData, out var instance))
        {
            effectData.RemoveEffect(GetComponent<Unit>(), instance.CurrentStacks);
            activeEffects.Remove(effectData);
        }
    }

    private UnitCombat combat;

    private void Start()
    {
        combat = GetComponent<UnitCombat>();
        if (combat != null)
        {
            combat.OnDeath += HandleDeath;
        }
    }

    private void OnDestroy()
    {
        if (combat != null)
        {
            combat.OnDeath -= HandleDeath;
        }
    }

    private void HandleDeath()
    {
        Unit unit = GetComponent<Unit>();
        foreach (var kvp in activeEffects)
        {
            kvp.Key.ExecuteDeathEffect(unit, kvp.Value.CurrentStacks);
        }
    }

    private void Update()
    {
        UpdateEffects(Time.deltaTime);
    }

    private void UpdateEffects(float deltaTime)
    {
        List<TileEffectDataSO> toRemove = new List<TileEffectDataSO>();

        foreach (var kvp in activeEffects)
        {
            kvp.Value.TimeRemaining -= deltaTime;
            if (kvp.Value.TimeRemaining <= 0)
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var effectData in toRemove)
        {
            RemoveEffect(effectData);
        }
    }
}
