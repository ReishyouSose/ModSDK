using HarmonyLib;

namespace Assets.LuckyBlock
{
    [HarmonyPatch]
    public static class ChallengePatch
    {
        [HarmonyPatch(typeof(LootTableConverter), nameof(LootTableConverter.Convert))]
        [HarmonyPrefix]
        private static void InsteadLootTables()
        {
            if (!ModConfig.Ins.ChallengeMode.Value)
                return;
            var id = PugMod.API.Authoring.GetObjectID("LuckyBlock:Item");
            foreach (LootTable loot in Manager.mod.LootTable)
            {
                foreach (LootInfo info in loot.lootInfos)
                {
                    info.objectID = id;
                }
            }
        }

        [HarmonyPatch(typeof(DropLootConverter), nameof(DropLootConverter.Convert))]
        [HarmonyPrefix]
        private static void InsteadDropLoot(DropLootAuthoring authoring)
        {
            if (!ModConfig.Ins.ChallengeMode.Value)
                return;
            var id = PugMod.API.Authoring.GetObjectID("LuckyBlock:Item");
            if (authoring.hasCustomLoot)
            {
                int count = authoring.customLoot.Values.Count;
                for (int i = 0; i < count; i++)
                {
                    var loot = authoring.customLoot.Values[i];
                    authoring.customLoot.Values[i] = new()
                    {
                        amount = loot.amount,
                        multiplayerAmountAdditionScaling = loot.multiplayerAmountAdditionScaling,
                        lootDropID = id
                    };
                }
            }

            if (authoring.hasLootDropsOnUse)
            {
                int count = authoring.onUseLootDrops.lootDrops.Count;
                for (int i = 0; i < count; i++)
                {
                    var loot = authoring.onUseLootDrops.lootDrops[i];
                    authoring.onUseLootDrops.lootDrops[i] = new()
                    {
                        lootDropID = id,
                        amount = loot.amount,
                        chance = loot.chance,
                    };
                }
            }

            if (authoring.hasLootDropsOnTakingDamage)
            {
                authoring.lootDropsWhenDamaged.dropsLoot = id;
            }
        }
    }
}
