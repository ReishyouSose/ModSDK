using Assets.CoreEnhance.Scripts.Configs;
using CoreLib.Data.Configuration;
using Unity.Entities;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class InfinityClient : PugSimulationSystemBase
    {
        private bool added;
        protected override void OnUpdate()
        {
            var player = Manager.main.player;
            if (player == null)
            {
                added = false;
                return;
            }
            if (ModConfig.TryGetEnable(EnhanceCategory.Infinity, EC_Infinity.Durability))
            {
                if (added)
                    return;
                added = true;
                ConditionData cd = new()
                {
                    conditionID = ConditionID.EquipmentDurabilityLastsLonger,
                    value = 100
                };
                player.playerCommandSystem.SetSkillTalentCondition(player.entity, cd);
                cd.conditionID = ConditionID.ToolDurabilityLastsLonger;
                player.playerCommandSystem.SetSkillTalentCondition(player.entity, cd);
            }
            else
            {
                if (!added)
                    return;
                added = false;
                var tree = Manager.saves.GetSkillTalentTreesPoints(SkillID.Crafting);
                int count = tree.Count;

                ConditionData cd = new()
                {
                    conditionID = ConditionID.ToolDurabilityLastsLonger,
                    value = count > 2 ? tree[1] : 0
                };
                player.playerCommandSystem.SetSkillTalentCondition(player.entity, cd);

                cd = new()
                {
                    conditionID = ConditionID.EquipmentDurabilityLastsLonger,
                    value = count > 3 ? tree[2] : 0
                };
                player.playerCommandSystem.SetSkillTalentCondition(player.entity, cd);
            }
            base.OnUpdate();
        }
    }
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class InfinityServer : PugSimulationSystemBase
    {
        private const int ResetTimer = 60;
        private int timer;
        protected override void OnUpdate()
        {
            if (ModConfig.TryGetValue(EnhanceCategory.Infinity, EC_Infinity.Arena, out ConfigEntry<int> value))
            {
                int coin = value.Value;
                Entities.ForEach((Entity e) =>
                {

                })
                    .WithBurst()
                    .Schedule();
            }
            if (--timer > 0)
                return;
            timer = ResetTimer;
            var ecb = CreateCommandBuffer();
            if (ModConfig.TryGetEnable(EnhanceCategory.Infinity, EC_Infinity.Boulder))
            {
                Entities.ForEach((ref HealthCD heal, in ObjectDataCD objdata) =>
                {
                    heal.health = heal.maxHealth;
                })
                    .WithAll<RequiresDrillCD>()
                    .WithAll<DontDropSelfCD>()
                    .WithBurst()
                    .Schedule();
            }
            base.OnUpdate();
        }
    }
}
