using HarmonyLib;

namespace Assets.CoreEnhance.Scripts.Patchs
{
    [HarmonyPatch]
    public static class BossCheckPatch
    {
        internal static bool ShouldCheckImmdiately;

        [HarmonyPatch(typeof(BossSpawnSystem), "OnUpdate")]
        [HarmonyPrefix]
        private static void ModifyTime(ref float ___systemTimer)
        {
            if (!ShouldCheckImmdiately)
                return;
            ShouldCheckImmdiately = false;
            ___systemTimer = 7;
        }
    }
}
