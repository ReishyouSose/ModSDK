using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

namespace Assets.LuckyBlock
{
    public struct LuckyBlockRpc : IRpcCommand
    {
        public Entity entity;
        public LuckyBlockRpc(Entity e)
        {
            entity = e;
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class LuckyBlockClient : PugSimulationSystemBase
    {
        private static LuckyBlockClient instance;
        private NativeQueue<LuckyBlockRpc> queue;
        private EntityArchetype archetype;
        protected override void OnCreate()
        {
            instance = this;
            queue = new(Allocator.Persistent);
            archetype = EntityManager.CreateArchetype(typeof(LuckyBlockRpc), typeof(SendRpcCommandRequest));
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            while (queue.TryDequeue(out LuckyBlockRpc rpc))
            {
                var e = ecb.CreateEntity(archetype);
                ecb.SetComponent(e, rpc);
            }
            base.OnUpdate();
        }
        public static void TriggerLuckyBlock(Entity luckyBlock)
        {
            instance.queue.Enqueue(new(luckyBlock));
        }
    }


    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class LuckyBlockServer : PugSimulationSystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            var posLookup = SystemAPI.GetComponentLookup<LocalTransform>();
            var healthLookup = SystemAPI.GetComponentLookup<HealthCD>();
            Entities.ForEach((Entity e, in LuckyBlockRpc rpc) =>
            {
                healthLookup.GetRefRW(e).ValueRW.health = 0;
                ecb.AddComponent<DontDropSelfCD>(e);
                //var pos = posLookup.GetRefRO(e).ValueRO.Position;
            })
                .WithName("LuckyBlock")
                .WithAll<ReceiveRpcCommandRequest>()
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
    }
}
