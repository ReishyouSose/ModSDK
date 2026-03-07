using Inventory;
using Pug.UnityExtensions;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.CoreEnhance.Scripts.Systems.Misc
{
    public struct ClearDropItemRPC : IRpcCommand
    {
        public Entity Player;
    }
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class ClearDropItemClient : PugSimulationSystemBase
    {
        private static ClearDropItemClient ins;
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
            while (queue.TryDequeue(out var player))
            {
                var e = ecb.CreateEntity(archetype);
                ecb.SetComponent(e, new ClearDropItemRPC()
                {
                    Player = player
                });
            }
            base.OnUpdate();
        }
        public static void Clear(Entity player)
        {
            ins.queue.Enqueue(player);
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class ClearDropItemServer : PugSimulationSystemBase
    {
        private NativeQueue<Entity> queue;
        protected override void OnCreate()
        {
            queue = new(Allocator.Persistent);
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var queue = this.queue;
            var ecb = CreateCommandBuffer();
            Entities.ForEach((Entity e) =>
            {
                queue.Enqueue(e);
                ecb.DestroyEntity(e);
            })
                .WithName("ReceiveClearDropItemRPC")
                .WithBurst()
                .WithAll<ReceiveRpcCommandRequest>()
                .WithAll<ClearDropItemRPC>()
                .Schedule();
            while(queue.TryDequeue(out var player))
            {
                Entities.ForEach((Entity e) =>
                {
                    ecb.DestroyEntity(e);
                })
                    .WithName("ClearDropItem")
                    .WithBurst()
                    .WithAll<PickUpItemCD>()
                    .Schedule();
            }
            base.OnUpdate();
        }
    }
}
