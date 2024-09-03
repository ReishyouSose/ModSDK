using Assets.THCompass.Component;
using CoreLib.Drops;
using HarmonyLib;
using UnityEngine;

namespace Assets.THCompass.Patchs
{
    [HarmonyPatch]
    public static class SpawnItemOnUsePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(DropsLootOnUseConverter), nameof(DropsLootOnUseConverter.Convert))]
        private static bool Convert(DropsLootOnUseConverter __instance, DropsLootOnUseAuthoring authoring)
        {
            if (authoring.gameObject.TryGetComponent<DropFromBossAuthoring>(out var bossCD))
            {
                authoring.lootTableToSpawn = DropTablesModule.GetLootTableID("THCompass:Loot_"
                    + bossCD.bossID.ToString().Replace(" ", string.Empty));
                authoring.spawnEffects = EffectID.None;
                Debug.Log("Compass Convert Loot " + bossCD.bossID);
            }
            return true;
        }
    }
}
