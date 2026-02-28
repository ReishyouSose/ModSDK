using Assets.CoreEnhance.Scripts.Cores;
using HarmonyLib;
using Inventory;
using Pug.UnityExtensions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace Assets.CoreEnhance.Scripts.Patchs
{
    [HarmonyPatch(typeof(InventoryUtility))]
    public static class InventoryUtilityPatch
    {
        [HarmonyPatch(nameof(InventoryUtility.Fish)), HarmonyTranspiler]
        private static List<CodeInstruction> Fish(IEnumerable<CodeInstruction> codes)
        {
            var list = codes.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                var code = list[i];
                if (code.opcode != OpCodes.Ldfld)
                    continue;
                if (!code.operand.ToString().Contains("fishLootTableID"))
                    continue;
                list[i] = new(OpCodes.Call, AccessTools.Method(typeof(InventoryUtilityPatch), nameof(RollFishLootTableID)));
                break;
            }
            return list;
        }
        private static LootTableID RollFishLootTableID(FishingInfoData infoData)
        {
            if (!EnhanceConfig.TryGetValue<float>(EnhanceCategory.FishingNetCanGetItem, out var value))
                return infoData.fishLootTableID;
            return PugRandom.GetRng().NextFloat() < value.Value ? infoData.lootTableID : infoData.fishLootTableID;
        }
    }
}
