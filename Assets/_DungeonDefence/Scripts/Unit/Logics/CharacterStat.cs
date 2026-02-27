using System.Collections.Generic;

public class CharacterStat
{
    public float BaseValue;
    private readonly List<StatModifier> statModifiers = new List<StatModifier>();
    protected bool isDirty = true;
    protected float lastBaseValue;
    protected float lastCalculatedValue;

    public CharacterStat(float baseValue)
    {
        BaseValue = baseValue;
    }

    public float Value
    {
        get
        {
            if (isDirty || BaseValue != lastBaseValue)
            {
                lastBaseValue = BaseValue;
                lastCalculatedValue = CalculateFinalValue();
                isDirty = false;
            }
            return lastCalculatedValue;
        }
    }

    public void AddModifier(StatModifier mod)
    {
        statModifiers.Add(mod);
        isDirty = true;
    }

    public void RemoveModifier(StatModifier mod)
    {
        statModifiers.Remove(mod);
        isDirty = true;
    }

    public void RemoveAllModifiersFromSource(object source)
    {
        for (int i = statModifiers.Count - 1; i >= 0; i--)
        {
            if (statModifiers[i].Source == source)
            {
                statModifiers.RemoveAt(i);
                isDirty = true;
            }
        }
    }

    private float CalculateFinalValue()
    {
        float finalValue = BaseValue;
        float sumPercentAdd = 0;

        foreach (var mod in statModifiers)
        {
            if (mod.Type == StatModType.FlatAdd) finalValue += mod.Value;
            else if (mod.Type == StatModType.PercentMultiply) sumPercentAdd += mod.Value;
        }

        return finalValue * (1 + sumPercentAdd);
    }
}
