using Assets.THCompass.Component;
using CoreLib.Drops;
using HarmonyLib;

namespace Assets.THCompass.Patchs
{
    [HarmonyPatch]
    public static class SpawnItemOnUsePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(DropLootConverter), nameof(DropLootConverter.Convert))]
        private static void Convert(DropLootAuthoring authoring)
        {
            if (authoring.TryGetComponent<DropFromBossAuthoring>(out var bossCD))
            {
                authoring.onUseLootDrops.lootTableID = DropTablesModule.GetLootTableID("THCompass:Loot_"
                    + bossCD.bossID.ToString().Replace(" ", string.Empty));
            }
        }
    }
}
