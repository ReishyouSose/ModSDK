using Assets.CoreEnhance.Scripts.Configs;
using PlayerEquipment;
using Unity.Entities;

namespace Assets.CoreEnhance.Scripts.Systems.Infinity
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
    [UpdateBefore(typeof(ChangeDurabilitySystem))]
    [UpdateInGroup(typeof(EndPredictedSimulationSystemGroup))]
    public partial class Durability : PugSimulationSystemBase
    {
        private ComponentLookup<ReduceDurabilityOfEquippedTriggerCD> heldLookup;
        private ComponentLookup<ReduceDurabilityOfAllEquipmentTriggerCD> allLookup;
        protected override void OnCreate()
        {
            heldLookup = SystemAPI.GetComponentLookup<ReduceDurabilityOfEquippedTriggerCD>();
            allLookup = SystemAPI.GetComponentLookup<ReduceDurabilityOfAllEquipmentTriggerCD>();
            NeedDatabase();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            if (!ModConfig.IsEnable(EnhanceCategory.Infinity, EC_Infinity.Durability))
                return;
            var held = heldLookup;
            var all = allLookup;
            Entities.ForEach((Entity player) =>
            {
                if (held.HasComponent(player))
                    held.SetComponentEnabled(player, false);
                if (all.HasComponent(player))
                    all.SetComponentEnabled(player, false);
            })
                .WithName("Infinity_Durability")
                .WithAll<PlayerGhost>()
                .WithBurst()
                .Schedule();

            base.OnUpdate();
        }

    }
}
