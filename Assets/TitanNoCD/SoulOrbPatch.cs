using CoreLib.Util.Extensions;
using HarmonyLib;
using System;

namespace Assets.TitanNoCD
{
    [HarmonyPatch]
    public static class SoulOrbPatch
    {
        public static ModConfig Config => TitanNoCDMod.Config;
        [HarmonyPatch(typeof(DestroyTimerConverter), nameof(DestroyTimerConverter.Convert))]
        [HarmonyPrefix]
        private static void DecreaseDestroyTimer(DestroyTimerAuthoring authoring)
        {
            ObjectID id = authoring.GetEntityObjectID();
            string idName = id.ToString();
            if (idName.EndsWith("SoulOrb"))
            {
                authoring.duration = Math.Clamp(Config.SoulOrbDuration.Value, 5, 300);
            }
        }

        [HarmonyPatch(typeof(BossSpawnSystem), "OnUpdate")]
        [HarmonyPrefix]
        private static void ModifyTime(ref float ___systemTimer)
        {
            float max = Math.Clamp(Config.DetectionInterval.Value, 0, 60);
            if (___systemTimer > max)
                ___systemTimer = max;
        }
    }
}
