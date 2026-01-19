using Unity.Entities;

namespace Assets.CoreFighter.Scripts.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(ConditionEffectsUpdateSystemGroup), OrderLast = true)]
    [UpdateBefore(typeof(ConditionsSystem))]
    public partial class InfinityExplosiveSystem : PugSimulationSystemBase
    {
        private float timer;
        protected override void OnUpdate()
        {
            if (timer < 1)
            {
                timer += World.Time.DeltaTime;
                return;
            }
            timer = 0;
            Entities.ForEach((Entity e, DynamicBuffer<NewConditionsBuffer> buffer) =>
            {
                buffer.Add(new()
                {
                    conditionData = new()
                    {
                        conditionID = ConditionID.ChanceToNotConsumeExplosives,
                        value = 100,
                        duration = 1.1f
                    }
                });
            })
                .WithName("InfinityExplosive")
                .WithBurst()
                .WithAll<PlayerGhost>()
                .Schedule();
            base.OnUpdate();
        }
    }
}
