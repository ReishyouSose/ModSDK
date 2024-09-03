//using HarmonyLib;
//using Unity.Entities;

//namespace Assets.AllSkills
//{
//    [HarmonyPatch]
//    public class PopTextPatch
//    {
//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(SaveSkillsSystem), "OnUpdate")]
//        public bool OnUpdate(SaveSkillsSystem __instance, ref SystemState state)
//        {
//            foreach (DynamicBuffer<SkillBuffer> dynamicBuffer in SaveSkillsSystem.IFE_1641582815_0.Query(__instance.__query_1641582815_0, __instance.__TypeHandle.__IFE_1641582815_0_TypeHandle, ref state))
//            {
//                for (int i = 0; i < dynamicBuffer.Length; i++)
//                {
//                    SkillID skillID = (SkillID)i;
//                    int value = dynamicBuffer[i].Value;
//                    int skillValue = Manager.saves.GetSkillValue(skillID);
//                    if (skillValue != dynamicBuffer[i].Value)
//                    {
//                        int levelFromSkill = SkillExtensions.GetLevelFromSkill(skillID, skillValue);
//                        int maxSkillLevel = SkillExtensions.GetMaxSkillLevel(skillID);
//                        if (value <= skillValue || levelFromSkill < maxSkillLevel)
//                        {
//                            Manager.saves.SetSkillValue(skillID, value);
//                            ConditionData conditionDataForSkill = SkillExtensions.GetConditionDataForSkill(skillID, skillValue);
//                            ConditionData conditionDataForSkill2 = SkillExtensions.GetConditionDataForSkill(skillID, value);
//                            if (conditionDataForSkill.value != conditionDataForSkill2.value && !(Manager.main.player == null) && conditionDataForSkill2.value - conditionDataForSkill.value > 0)
//                            {
//                                int levelFromSkill2 = SkillExtensions.GetLevelFromSkill(skillID, dynamicBuffer[i].Value);
//                                bool flag;
//                                if (levelFromSkill2 <= 60)
//                                {
//                                    flag = levelFromSkill2 % 3 == 0;
//                                }
//                                else
//                                    flag = levelFromSkill2 % 2 == 0;
//                                //bool flag = levelFromSkill2 % 5 == 0;
//                                if (levelFromSkill != 0 || levelFromSkill2 != 3)
//                                {
//                                    Manager.main.player.SpawnSkillIncreasePopup(skillID, !flag);
//                                }
//                                if (flag)
//                                {
//                                    Manager.main.player.SpawnNewSkillPopup(skillID);
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//            return false;
//        }
//    }
//}
