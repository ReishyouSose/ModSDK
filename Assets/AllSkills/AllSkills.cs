using HarmonyLib;
using PugMod;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.AllSkills
{
    [HarmonyPatch]
    public class AllSkills : IMod
    {
        public void EarlyInit()
        {
        }

        public void Init()
        {
            BurstDisabler.DisableBurstForSystem<SaveSkillsSystem>();
        }

        public void ModObjectLoaded(Object obj)
        {
        }

        public void Shutdown()
        {
        }

        public void Update()
        {
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.GetAvailableTalentPoints))]
        public static bool PreGetAvailableTalentPoints(SaveManager __instance, SkillID skillTreeID, ref int __result)
        {
            int skillValue = __instance.GetSkillValue(skillTreeID);
            int skillLevel = SkillExtensions.GetLevelFromSkill(skillTreeID, skillValue);
            int num = math.min(skillLevel, 60) / 3 + math.max(skillLevel - 60, 0) / 2;
            List<int> skillTalentTreesPoints = __instance.GetSkillTalentTreesPoints(skillTreeID);
            __result = num - (skillTalentTreesPoints?.Sum() ?? 0);
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

        [HarmonyPatch(typeof(SaveSkillsSystem), nameof(SaveSkillsSystem.OnUpdate))]
        [HarmonyPrefix]
        private static bool SaveSkillsSystem_Update(ref SystemState state)
        {
            return false;
        }
    }
}
