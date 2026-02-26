using UnityEngine;

[CreateAssetMenu(fileName = "ResourceEffect", menuName = "DungeonDefence/Effects/Resource")]
public class ResourceTileEffect : TileEffectDataSO
{
    public int goldPerSecond = 0;
    public int goldOnWaveClear = 0;
    public bool onlyDispatched = true;

    public override void OnUpdate(Unit unit)
    {
        if (goldPerSecond > 0 && IsValid(unit))
        {
            // EconomyManager takes int for AddCurrency, so we might need a buffer or just add small amounts
            // Let's use a simple per-frame probability or a timer-based approach if needed, 
            // but for now let's assume goldPerSecond is handled by EconomyManager if it supports float, 
            // otherwise we'll just add 1 Gold every 1/goldPerSecond seconds.
            
            // Simpler: 5G/sec -> 1G every 0.2s.
            // For now, let's just use a static timer if we want precision, or just use Time.deltaTime.
            
            // Actually, EconomyManager.AddCurrency(type, amount) takes int.
            // We'll use a timer buffer in the unit or the effect? 
            // Effects are SOs, so they shouldn't store state.
            // I'll skip the per-second gold for now or implement it as a constant rate in EconomyManager.
            
            // Wait, I can just use a simple accumulation in the unit if I really want to.
            // But let's keep it simple: 
            if (Random.value < Time.deltaTime * goldPerSecond) 
            {
                 EconomyManager.Instance.AddCurrency(CurrencyType.Gold, 1);
            }
        }
    }

    public override void OnWaveClear(Unit unit)
    {
        if (goldOnWaveClear > 0 && IsValid(unit))
        {
            EconomyManager.Instance.AddCurrency(CurrencyType.Gold, goldOnWaveClear);
            Debug.Log($"[ResourceEffect] Wave clear bonus: {goldOnWaveClear}G from {unit.Data.Name}");
        }
    }

    private bool IsValid(Unit unit)
    {
        if (unit == null || unit.IsDead) return false;
        if (onlyDispatched && !unit.IsDispatched) return false;
        return true;
    }
}
