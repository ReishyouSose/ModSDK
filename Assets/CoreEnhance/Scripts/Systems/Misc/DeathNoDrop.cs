using Assets.CoreEnhance.Scripts.Components;
using Assets.CoreEnhance.Scripts.Cores;
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
        private ComponentLookup<InitialMoveInventoryFromCD> moveLookup;
        protected override void OnCreate()
        {
            moveLookup = SystemAPI.GetComponentLookup<InitialMoveInventoryFromCD>();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            if (!EnhanceConfig.IsEnable(EnhanceCategory.DeathNoDrop))
                return;
            var ecb = CreateCommandBuffer();
            var moveLookup = this.moveLookup;
            Entities.ForEach((Entity e) =>
            {
                if (moveLookup.HasComponent(e))
                {
                    moveLookup.SetComponentEnabled(e, false);
                    moveLookup.GetRefRW(e).ValueRW.entityFrom = Entity.Null;
                    ecb.AddComponent<ProcessedTagCD>(e);
                }
            })
                .WithName("Misc_DeathNoDrop")
                .WithAll<PlayerGraveCD>()
                .WithNone<ProcessedTagCD>()
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
    }
}
