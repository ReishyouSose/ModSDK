using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.CoreEnhance.Scripts.Systems.Misc
{
    [GhostComponent]
    public struct ClearAllDropItemCD : IComponentData { }

    public struct ClearDropItemRPC : IRpcCommand { }
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class ClearDropItemRPCClient : PugSimulationSystemBase
    {
        private static ClearDropItemRPCClient ins;
        private NativeQueue<Entity> queue;
        private EntityArchetype archetype;
        protected override void OnCreate()
        {
            ins = this;
            queue = new(Allocator.Persistent);
            archetype = EntityManager.CreateArchetype(typeof(ClearDropItemRPC), typeof(SendRpcCommandRequest));
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            while (queue.TryDequeue(out _))
            {
                ecb.CreateEntity(archetype);
            }
            base.OnUpdate();
        }
        public static void Clear(PlayerController p)
        {
            if (p.adminPrivileges > 0)
                ins.queue.Enqueue(p.entity);
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class ClearDropItemRPCServer : PugSimulationSystemBase
    {
        private EntityArchetype archetype;
        protected override void OnCreate()
        {
            archetype = EntityManager.CreateArchetype(typeof(ClearAllDropItemCD));
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            var archetype = this.archetype;
            Entities.ForEach((Entity e, in ClearDropItemRPC rpc) =>
            {
                ecb.DestroyEntity(e);
                ecb.CreateEntity(archetype);
            })
                .WithName("ReceiveClearDropItemRPC")
                .WithBurst()
                .WithAll<ReceiveRpcCommandRequest>()
                .Schedule();
            base.OnUpdate();
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [UpdateAfter(typeof(PickUpItemSystem))]
    [UpdateBefore(typeof(EndPredictedSimulationSystemGroup))]
    public partial class ClearAllDropItemSystem : PugSimulationSystemBase
    {
        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingletonEntity<ClearAllDropItemCD>(out var clear))
                return;
            var ecb = CreateCommandBuffer();
            ecb.DestroyEntity(clear);
            var job = Entities.ForEach((Entity e, in PickUpItemCD pick) =>
            {
                if (pick.state != PickUpItemState.None)
                    return;
                ecb.DestroyEntity(e);
            })
                .WithName("ClearAllDropItems")
                .WithEntityQueryOptions(EntityQueryOptions.IncludeDisabledEntities)
                .WithBurst()
                .ScheduleParallel(Dependency);
            base.OnUpdate();
        }
    }
}
