using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 웨이브 클리어 시 선택 가능한 해금 후보 목록을 정의하는 SO.
/// 웨이브마다 별도로 만들거나, 하나의 범용 풀로 사용해도 된다.
/// </summary>
[CreateAssetMenu(fileName = "ShopUnlockPool", menuName = "DungeonDefence/Shop/Unlock Pool")]
public class ShopUnlockPoolSO : ScriptableObject
{
    [Header("해금 후보 풀 (유닛 또는 타일 SO)")]
    [Tooltip("웨이브 클리어 시 이 풀에서 3개를 무작위로 뽑아 선택지로 제공")]
    public List<UnitDataSO> unitCandidates = new List<UnitDataSO>();
    public List<TileDataSO> tileCandidates = new List<TileDataSO>();

    /// <summary>유닛+타일 후보 풀에서 count개를 무작위로 반환 (중복 없음).</summary>
    public List<ITradable> GetRandomCandidates(int count, List<ITradable> alreadyUnlocked)
    {
        List<ITradable> pool = new List<ITradable>();
        foreach (var u in unitCandidates)
            if (u != null && !alreadyUnlocked.Contains(u)) pool.Add(u);
        foreach (var t in tileCandidates)
            if (t != null && !alreadyUnlocked.Contains(t)) pool.Add(t);

        // Fisher-Yates 셔플
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        return pool.Count > count ? pool.GetRange(0, count) : pool;
    }
}
