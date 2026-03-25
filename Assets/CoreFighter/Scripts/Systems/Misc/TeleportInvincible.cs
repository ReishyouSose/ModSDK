using Assets.CoreFighter.Scripts.Cores;
using PlayerState;
using Unity.Entities;

namespace Assets.CoreFighter.Scripts.Systems.Misc
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(PlayerStateSystemGroup))]
    public partial class TeleportInvincibleSystem : PugSimulationSystemBase
    {
        private uint tickRate;
        protected override void OnCreate()
        {
            tickRate = (uint)PlatformConfiguration.Instance.SessionConfiguration.SimulationTickRate;
            RequireForUpdate<ConditionsTableCD>();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            if (!FighterConfig.TryGetValue<bool>(FighterCategory.MapMarkerTeleport, out var value))
                return;
            if (!value.Value)
                return;
            var world = World;
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
                    conditionID = ConditionID.ImmuneToDamageAfterRespawn,
                    value = 1,
                    duration = 0
                }, c1, condition, tick, tickRate, c2);
            })
                .WithName("TeleportInvincible")
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
    }
}
