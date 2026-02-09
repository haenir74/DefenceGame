using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveGroup
{
    public UnitDataSO unitData;
    public int count;
    public float spawnInterval;
    public float initialDelay;
}

[CreateAssetMenu(fileName = "WaveData_01", menuName = "DungeonDefence/Wave/Wave Data")]
public class WaveDataSO : ScriptableObject
{
    public int waveIndex;
    [TextArea] public string description;
    
    public List<WaveGroup> groups;

    public int GetTotalEnemyCount()
    {
        int total = 0;
        if (groups != null)
        {
            foreach (var group in groups)
            {
                total += group.count;
            }
        }
        return total;
    }
}