using Unity.Entities;
using Unity.NetCode;

namespace Assets.AllSkills
{
    [UpdateBefore(typeof(SaveSkillsSystem))]
    [UpdateInGroup(typeof(RunSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class UpdateSkillsSystem : PugSimulationSystemBase
    {
        protected override void OnUpdate()
        {
            foreach (var skillBuffer in SystemAPI.Query<DynamicBuffer<SkillBuffer>>().WithAll<GhostOwnerIsLocal>().WithChangeFilter<SkillBuffer>())
            {
                for (int i = 0; i < skillBuffer.Length; i++)
                {
                    SkillID skillID = (SkillID)i;
                    int value = skillBuffer[i].Value;
                    int skillValue = Manager.saves.GetSkillValue(skillID);

                    if (skillValue != value)
                    {
                        int levelFromSkill = SkillExtensions.GetLevelFromSkill(skillID, skillValue);
                        int maxSkillLevel = SkillExtensions.GetMaxSkillLevel(skillID);

                        if (value <= skillValue || levelFromSkill < maxSkillLevel)
                        {
                            Manager.saves.SetSkillValue(skillID, value);
                            ConditionData conditionDataForSkill = SkillExtensions.GetConditionDataForSkill(skillID, skillValue);
                            ConditionData conditionDataForSkill2 = SkillExtensions.GetConditionDataForSkill(skillID, value);

                            if (conditionDataForSkill.value != conditionDataForSkill2.value &&
                                Manager.main.player != null &&
                                conditionDataForSkill2.value - conditionDataForSkill.value > 0)
                            {
                                int levelFromSkill2 = SkillExtensions.GetLevelFromSkill(skillID, value);
                                bool isMajorLevel = levelFromSkill2 % (levelFromSkill2 > 60 ? 2 : 3) == 0;

                                if (levelFromSkill != 0 || levelFromSkill2 != 3)
                                {
                                    Manager.main.player.SpawnSkillIncreasePopup(skillID, !isMajorLevel);
                                }

                                if (isMajorLevel)
                                {
                                    Manager.main.player.SpawnNewSkillPopup(skillID);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
