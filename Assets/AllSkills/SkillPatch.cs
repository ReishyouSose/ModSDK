using HarmonyLib;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Assets.AllSkills
{
    [HarmonyPatch]
    public static class SkillPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SaveManager), "GetAvailableTalentPoints")]
        public static bool PreGetAvailableTalentPoints(SaveManager __instance, SkillID skillTreeID, ref int __result)
        {
            int skillValue = Manager.saves.GetSkillValue(skillTreeID);
            int skillLevel = SkillExtensions.GetLevelFromSkill(skillTreeID, skillValue);
            int num = math.min(skillLevel, 60) / 3;
            skillLevel -= 60;
            if (skillLevel > 0)
                num += skillLevel / 2;
            int num2 = 0;
            List<int> skillTalentTreesPoints = Manager.saves.GetSkillTalentTreesPoints(skillTreeID);
            if (skillTalentTreesPoints != null)
            {
                for (int i = 0; i < skillTalentTreesPoints.Count; i++)
                {
                    num2 += skillTalentTreesPoints[i];
                }
            }
            __result = num - num2;
            return false;
        }

        [HarmonyPatch(typeof(PetExtensions), nameof(PetExtensions.GetTotalTalentPoints))]
        [HarmonyPrefix]
        private static bool GetTotalTalentPoints(int currentXp, ref int __result)
        {
            int level = PetExtensions.GetLevelFromXP(currentXp);
            __result = math.max(0, level - 1);
            return false;
        }
    }
}
