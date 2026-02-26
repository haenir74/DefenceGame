using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopUnlockPool", menuName = "DungeonDefence/Shop/Unlock Pool")]
public class ShopUnlockPoolSO : ScriptableObject
{
    
    
    public List<UnitDataSO> unitCandidates = new List<UnitDataSO>();
    public List<TileDataSO> tileCandidates = new List<TileDataSO>();

    
    public List<ITradable> GetRandomCandidates(int count, List<ITradable> alreadyUnlocked)
    {
        List<ITradable> pool = new List<ITradable>();
        foreach (var u in unitCandidates)
            if (u != null && !alreadyUnlocked.Contains(u)) pool.Add(u);
        foreach (var t in tileCandidates)
            if (t != null && !alreadyUnlocked.Contains(t)) pool.Add(t);

        
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        return pool.Count > count ? pool.GetRange(0, count) : pool;
    }
}



