using Assets.CoreFighter.Scripts.Cores;
using PlayerState;
using Unity.Entities;

namespace Assets.CoreFighter.Scripts.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(ConditionEffectsUpdateSystemGroup), OrderLast = true)]
    [UpdateBefore(typeof(ConditionsSystem))]
    public partial class InfinityExplosiveSystem : PugSimulationSystemBase
    {
        private float timer;
        private uint tickRate;
        protected override void OnCreate()
        {
            tickRate = (uint)PlatformConfiguration.Instance.SessionConfiguration.SimulationTickRate;
            RequireForUpdate<ConditionsTableCD>();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            if (!FighterConfig.IsEnable(FighterCategory.Explosive))
                return;
            if (timer < 1)
            {
                timer += World.Time.DeltaTime;
                return;
            }
            timer = 0;
            var condition = SystemAPI.GetSingleton<ConditionsTableCD>();
            var tick = GetServerTick();
            var tickRate = this.tickRate;
            Entities.ForEach((in DynamicBuffer<ConditionsBuffer> c1,
                in PlayerStateCD state, in DynamicBuffer<SummarizedConditionsBuffer> c2) =>
            {
                if (!state.HasAnyState(PlayerStateEnum.Teleporting))
                    return;

                EntityUtility.AddOrRefreshCondition(new ConditionData()
                {
                    conditionID = ConditionID.ChanceToNotConsumeExplosives,
                    value = 100,
                    duration = 0
                }, c1, condition, tick, tickRate, c2);
            })
                .WithName("InfinityExplosive")
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
    }
}
