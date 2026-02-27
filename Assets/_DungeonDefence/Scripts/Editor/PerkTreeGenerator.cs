using UnityEngine;
using UnityEditor;
using System.IO;

public class PerkTreeGenerator
{
    private const string PerksPath = "Assets/Resources/Data/Perks";

    [MenuItem("Tools/Generate Perk Tree")]
    public static void GeneratePerkTree()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Data"))
            AssetDatabase.CreateFolder("Assets/Resources", "Data");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Data/Perks"))
            AssetDatabase.CreateFolder("Assets/Resources/Data", "Perks");

        // 1. Core Health (1, 2, 3)
        var coreHp1 = CreatePerk("CoreHP_1", "Core Health 1", "Increases Base Core Health by 100.", PerkType.CoreHealthIncrease, numericValue: 100, cost: 5);
        var coreHp2 = CreatePerk("CoreHP_2", "Core Health 2", "Increases Base Core Health by 100.", PerkType.CoreHealthIncrease, numericValue: 100, cost: 10, prereqs: new[] { coreHp1 });
        var coreHp3 = CreatePerk("CoreHP_3", "Core Health 3", "Increases Base Core Health by 100.", PerkType.CoreHealthIncrease, numericValue: 100, cost: 15, prereqs: new[] { coreHp2 });

        // 2. Starting Gold (1, 2, 3)
        var startGold1 = CreatePerk("StartGold_1", "Starting Gold 1", "Grants 100 extra starting gold.", PerkType.StartingGoldIncrease, numericValue: 100, cost: 5);
        var startGold2 = CreatePerk("StartGold_2", "Starting Gold 2", "Grants 150 extra starting gold.", PerkType.StartingGoldIncrease, numericValue: 150, cost: 10, prereqs: new[] { startGold1 });
        var startGold3 = CreatePerk("StartGold_3", "Starting Gold 3", "Grants 200 extra starting gold.", PerkType.StartingGoldIncrease, numericValue: 200, cost: 15, prereqs: new[] { startGold2 });

        // 3. Shop Slot (1, 2, 3, 4)
        var shopSlot1 = CreatePerk("ShopSlot_1", "Shop Slot 1", "Unlocks +1 Shop Slot.", PerkType.ShopSlotUnlock, numericValue: 1, cost: 10);
        var shopSlot2 = CreatePerk("ShopSlot_2", "Shop Slot 2", "Unlocks +1 Shop Slot.", PerkType.ShopSlotUnlock, numericValue: 1, cost: 20, prereqs: new[] { shopSlot1 });
        var shopSlot3 = CreatePerk("ShopSlot_3", "Shop Slot 3", "Unlocks +1 Shop Slot.", PerkType.ShopSlotUnlock, numericValue: 1, cost: 30, prereqs: new[] { shopSlot2 });
        var shopSlot4 = CreatePerk("ShopSlot_4", "Shop Slot 4", "Unlocks +1 Shop Slot.", PerkType.ShopSlotUnlock, numericValue: 1, cost: 40, prereqs: new[] { shopSlot3 });

        // 4. Ally Health Increase (1, 2, 3)
        var allyHp1 = CreatePerk("AllyHP_1", "Ally Health 1", "Allies gain +50 max HP.", PerkType.AllyHealthIncrease, numericValue: 50, cost: 5);
        var allyHp2 = CreatePerk("AllyHP_2", "Ally Health 2", "Allies gain +50 max HP.", PerkType.AllyHealthIncrease, numericValue: 50, cost: 10, prereqs: new[] { allyHp1 });
        var allyHp3 = CreatePerk("AllyHP_3", "Ally Health 3", "Allies gain +50 max HP.", PerkType.AllyHealthIncrease, numericValue: 50, cost: 15, prereqs: new[] { allyHp2 });

        // 5. Ally Attack Increase (1, 2, 3)
        var allyAtk1 = CreatePerk("AllyAtk_1", "Ally Attack 1", "Allies gain +5 Attack.", PerkType.AllyAttackIncrease, numericValue: 5, cost: 5);
        var allyAtk2 = CreatePerk("AllyAtk_2", "Ally Attack 2", "Allies gain +5 Attack.", PerkType.AllyAttackIncrease, numericValue: 5, cost: 10, prereqs: new[] { allyAtk1 });
        var allyAtk3 = CreatePerk("AllyAtk_3", "Ally Attack 3", "Allies gain +5 Attack.", PerkType.AllyAttackIncrease, numericValue: 5, cost: 15, prereqs: new[] { allyAtk2 });

        // 6. Starting Units (Imp, Goblin, Succubus - Linked to Tier 1 nodes arbitrarily)
        var startUnitImp = CreatePerk("Unit_Imp", "Start with Imp", "Start every game with an Imp deployed.", PerkType.StartingUnitAddition, stringValue: "Imp", cost: 20, prereqs: new[] { coreHp1 });
        var startUnitGoblin = CreatePerk("Unit_Goblin", "Start with Goblin", "Start every game with a Goblin deployed.", PerkType.StartingUnitAddition, stringValue: "Goblin", cost: 20, prereqs: new[] { startGold1 });
        var startUnitSuccubus = CreatePerk("Unit_Succubus", "Start with Succubus", "Start every game with a Succubus deployed.", PerkType.StartingUnitAddition, stringValue: "Succubus", cost: 30, prereqs: new[] { startUnitGoblin });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Perk Tree Generation Complete! Saved to " + PerksPath);
    }

    private static PerkDataSO CreatePerk(string id, string name, string desc, PerkType type, float numericValue = 0f, string stringValue = "", int cost = 10, PerkDataSO[] prereqs = null)
    {
        string path = $"{PerksPath}/{id}.asset";
        PerkDataSO perk = AssetDatabase.LoadAssetAtPath<PerkDataSO>(path);

        if (perk == null)
        {
            perk = ScriptableObject.CreateInstance<PerkDataSO>();
            AssetDatabase.CreateAsset(perk, path);
        }

        perk.PerkID = id;
        perk.DisplayName = name;
        perk.Description = desc;
        perk.Type = type;
        perk.NumericValue = numericValue;
        perk.StringValue = stringValue;
        perk.Cost = cost;
        perk.Prerequisites = prereqs ?? new PerkDataSO[0];

        EditorUtility.SetDirty(perk);
        return perk;
    }
}
