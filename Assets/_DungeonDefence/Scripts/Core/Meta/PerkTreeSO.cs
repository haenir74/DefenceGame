using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewPerkTree", menuName = "DungeonDefence/Meta/PerkTree")]
public class PerkTreeSO : ScriptableObject
{
    public List<PerkDataSO> allPerks;
}



