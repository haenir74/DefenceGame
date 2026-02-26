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
            
        }
    }

    private bool IsValid(Unit unit)
    {
        if (unit == null || unit.IsDead) return false;
        if (onlyDispatched && !unit.IsDispatched) return false;
        return true;
    }
}



