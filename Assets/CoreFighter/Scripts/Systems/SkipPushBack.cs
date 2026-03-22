using Assets.CoreFighter.Scripts.Cores;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.CoreFighter.Scripts.Systems
{
    [UpdateAfter(typeof(PhysicsWorldHistory))]
    [UpdateBefore(typeof(PushbackSystem))]
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderFirst = true)]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation, WorldSystemFilterFlags.Default)]
    public partial class SkipPushBackSystem : PugSimulationSystemBase
    {
        protected override void OnUpdate()
        {
            if (!FighterConfig.IsEnable(FighterCategory.ImmunePushBack))
                return;
            Entities.ForEach((Entity e, ref ReceivedPushbackCD pushBack) =>
            {
                pushBack.enabled = false;
            })
                .WithName("SkipPushBack")
                .WithAll<PlayerGhost>()
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
    }
}
