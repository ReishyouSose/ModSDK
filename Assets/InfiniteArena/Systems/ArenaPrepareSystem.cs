using Assets.InfinieArena;
using Assets.InfiniteArena.Components;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

namespace Assets.InfiniteArena.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class ArenaPrepareSystem : PugSimulationSystemBase
    {
        protected override void OnCreate()
        {
            UpdatesInRunGroup();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            return;
            var ecb = CreateCommandBuffer();
            var needTime = InfinieArenaMod.Config.ChargeTime;
            Entities.ForEach((Entity entity, in ObjectDataCD objdata) =>
            {
                if (objdata.objectID != ObjectID.EventTerminal || objdata.variation == 0)
                    return;
                if (objdata.variation == 1)
                {
                    ecb.AddComponent(entity, new DistanceToPlayerCD());
                }
                ecb.AddComponent(entity, new ArenaReactiveCD());
            })
                .WithName("ArenaPrepare")
                .WithNone<ArenaReactiveCD>()
                .WithAll<LocalTransform>()
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
    }
}
