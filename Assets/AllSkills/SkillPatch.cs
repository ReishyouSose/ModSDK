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
            if (skillLevel > 0) num += skillLevel / 2;
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
        /*[HarmonyPostfix]
        [HarmonyPatch(typeof(SaveManager), "GetAvailableTalentPoints")]
        private static void PostGetAvailableTalentPoints(SaveManager __instance, SkillID skillTreeID, ref int __result)
        {
            __result = math.max(__result, 0);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SaveManager), "GetSkillTalentTreesPoints")]
        private static void PostGetSkillTalentTreesPoints(SaveManager __instance, SkillID skillTreeID, ref List<int> __result)
        {
            int skillValue = Manager.saves.GetSkillValue(skillTreeID);
            int level = SkillExtensions.GetLevelFromSkill(skillTreeID, skillValue);
            if (level == 100)
            {
                __result = new List<int>() { 5, 5, 5, 5, 5, 5, 5, 5 };
            }
        }*/
    }
}
