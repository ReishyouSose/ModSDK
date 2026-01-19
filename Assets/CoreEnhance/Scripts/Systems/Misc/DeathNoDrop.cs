using Assets.CoreEnhance.Scripts.Components;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.CoreEnhance.Scripts.Systems.Misc
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateAfter(typeof(UpdateHealthSystemGroup))]
    [UpdateBefore(typeof(InitMoveInventorySystem))]
    public partial class DeathNoDropSystem : PugSimulationSystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            Misc_DeathNoDrop(ecb);
            base.OnUpdate();
        }
        private void Misc_DeathNoDrop(EntityCommandBuffer ecb)
        {
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
                .Schedule();
        }
    }
}
