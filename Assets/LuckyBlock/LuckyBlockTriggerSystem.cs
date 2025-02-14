using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.LuckyBlock
{
    public struct TriggerLBCD : IRpcCommand
    {
        public Entity entity;
        public TriggerLBCD(Entity entity)
        {
            this.entity = entity;
        }
    }

    public struct DropedLBCD : IComponentData { }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class TriggerLBClient : PugSimulationSystemBase
    {
        private static TriggerLBClient instance;
        private NativeQueue<TriggerLBCD> queue;
        private EntityArchetype archetype;
        protected override void OnCreate()
        {
            instance = this;
            queue = new(Allocator.Persistent);
            archetype = EntityManager.CreateArchetype(typeof(TriggerLBCD), typeof(SendRpcCommandRequest));
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            while (queue.TryDequeue(out var lb))
            {
                Entity e = ecb.CreateEntity(archetype);
                ecb.SetComponent(e, lb);
            }
            base.OnUpdate();
        }
        public static void Trigger(Entity lb)
        {
            instance.queue.Enqueue(new(lb));
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class TriggerLBServer : PugSimulationSystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            var healthLookup = SystemAPI.GetComponentLookup<HealthCD>();
            Entities.ForEach((Entity e, in TriggerLBCD lb) =>
            {
                healthLookup.GetRefRW(lb.entity).ValueRW.health = 0;
                ecb.DestroyEntity(e);
            })
                .WithName("TriggerLuckyBlock")
                .WithAll<ReceiveRpcCommandRequest>()
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
    }
}
