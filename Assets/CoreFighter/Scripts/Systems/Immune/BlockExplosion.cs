using Assets.CoreFighter.Scripts.Cores;
using Unity.Entities;
using Unity.Physics;

namespace Assets.CoreFighter.Scripts.Systems.Immune
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation, WorldSystemFilterFlags.Default)]
    [UpdateInGroup(typeof(BeforePredictedFixedStepSimulationSystemGroup), OrderFirst = true)]
    [UpdateAfter(typeof(SummarizeConditionsSystem))]
    public partial class BlockExplosionSystem : PugSimulationSystemBase
    {
        protected override void OnUpdate()
        {
            if (!FighterConfig.IsEnable(FighterCategory.Explosion))
                return;
            Entities.ForEach((DynamicBuffer<SummarizedConditionEffectsBuffer> effects) =>
            {
                effects[(int)ConditionEffect.ReducedDamageFromExplosions] = new SummarizedConditionEffectsBuffer
                {
                    value = 100
                };
            })
                .WithName("BlockExplosion")
                .WithBurst()
                .WithAll<PlayerGhost>()
                .Schedule();
            base.OnUpdate();
        }
    }
}
