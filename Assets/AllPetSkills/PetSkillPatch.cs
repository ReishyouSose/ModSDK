using HarmonyLib;
using System;

[HarmonyPatch]
public static class PetSkillPatch
{
    [HarmonyPatch(typeof(PetExtensions), nameof(PetExtensions.GetAvailableTalentPoints))]
    [HarmonyPrefix]
    private static bool GetTotalTalentPoints(int currentXp, ref int __result)
    {
        int level = PetExtensions.GetLevelFromXP(currentXp);
        __result = Math.Max(0, level - 1);
        return false;
    }
}
