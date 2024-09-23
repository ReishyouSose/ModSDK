using Assets.THCompass.Compasses;
using Assets.THCompass.DataStruct;
using Assets.THCompass.Helper;
using HarmonyLib;
using UnityEngine;

namespace Assets.THCompass.Patchs
{
    [HarmonyPatch]
    public class ModifyLootConvertPatch
    {
        [HarmonyPatch(typeof(LootTableConverter), nameof(LootTableConverter.Convert))]
        [HarmonyPrefix]
        private static void AddCustomLootTables()
        {
            var lootList = Manager.mod.LootTable;
            var config = THCompassMain.config;
            int min = config.MinDrop, max = config.MaxDrop;
            if (min <= 0)
                min = 1;
            if (max <= 0)
                return;
            if (min > max)
                min = max;
            Debug.Log("min: " + min);
            Debug.Log("max: " + max);
            foreach (LootTable loot in lootList)
            {
                LootTableID ltID = loot.id;
                if (CompassLoader.BossLootByID.TryGetValue(ltID, out BossID boss))
                {
                    foreach (LootInfo info in loot.lootInfos)
                    {
                        if (info.objectID is ObjectID.AncientGemstone or ObjectID.MechanicalPart)
                        {
                            Debug.Log("Modify " + ltID + " 's" + info.objectID + "weight = " + info.weight + " to compass");
                            info.objectID = ItemHelper.GetItemID("Compass_" + boss);
                            info.amount = new() { min = min, max = max };
                        }
                    }
                }
            }
        }
    }
}
