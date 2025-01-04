using Assets.CoreEnhance.Scripts.Component;
using Assets.CoreEnhance.Scripts.Configs;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Systems.Misc
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateAfter(typeof(UpdateHealthSystemGroup))]
    [UpdateBefore(typeof(InitMoveInventorySystem))]
    public partial class DeathNoDropSyetem : PugSimulationSystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            Misc_DeathNoDrop(ecb);
            base.OnUpdate();
        }
        private void Misc_DeathNoDrop(EntityCommandBuffer ecb)
        {
            if (!ModConfig.TryGetEnable(EnhanceCategory.Misc, EC_Misc.DeathNoDrop))
                return;
            var lookup = SystemAPI.GetComponentLookup<InitialMoveInventoryFromCD>();
            Entities.ForEach((Entity e) =>
            {
                if (lookup.HasComponent(e))
                {
                    lookup.SetComponentEnabled(e, false);
                    lookup.GetRefRW(e).ValueRW.entityFrom = Entity.Null;
                    ecb.AddComponent<ProcessedTagCD>(e);
                }
            })
                .WithName("Misc_DeathNoDrop")
                .WithAll<PlayerGraveCD>()
                .WithNone<ProcessedTagCD>()
                .WithBurst()
                .Run();


        }
    }
}
