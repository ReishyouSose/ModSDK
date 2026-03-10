using Inventory;
using PugMod;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

namespace Assets.PointShop.Scripts
{
    public struct PointShopRPC : IRpcCommand
    {
        public Entity Player;
        public ObjectID ObjectID;
        public ObjectID Boss;
        public int Price;
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class PointShopClient : PugSimulationSystemBase
    {
        private static PointShopClient ins;
        private NativeQueue<PointShopRPC> queue;
        private EntityArchetype archetype;
        protected override void OnCreate()
        {
            ins = this;
            queue = new NativeQueue<PointShopRPC>(Allocator.Persistent);
            archetype = EntityManager.CreateArchetype(typeof(PointShopRPC), typeof(SendRpcCommandRequest));
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            while (queue.TryDequeue(out var rpc))
            {
                var ecb = CreateCommandBuffer();
                Entity e = ecb.CreateEntity(archetype);
                ecb.SetComponent(e, rpc);
            }
            base.OnUpdate();
        }
        public static void TryBuyItem(Entity player, ObjectID item, ObjectID boss, int price)
        {
            ins.queue.Enqueue(new()
            {
                Player = player,
                ObjectID = item,
                Boss = boss,
                Price = price
            });
        }
    }
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class PointShopServer : PugSimulationSystemBase
    {
        private ComponentLookup<LocalTransform> transLookup;
        private BufferLookup<ContainedObjectsBuffer> containedLookup;
        private ObjectID coin;
        protected override void OnCreate()
        {
            NeedDatabase();
            coin =API.Authoring.GetObjectID("PointShop_Currency");
            RequireForUpdate<KilledEnemiesBuffer>();
            RequireForUpdate<InventoryChangeBuffer>();
            transLookup = SystemAPI.GetComponentLookup<LocalTransform>();
            containedLookup = SystemAPI.GetBufferLookup<ContainedObjectsBuffer>();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingletonBuffer<InventoryChangeBuffer>(out var inv))
                return;
            if (!SystemAPI.TryGetSingletonBuffer<KilledEnemiesBuffer>(out var killeds))
                return;
            var ecb = CreateCommandBuffer();
            var transLookup = this.transLookup;
            var containedLookup = this.containedLookup;
            var currentcy = coin;
            var database = this.database;
            Entities.ForEach((Entity e, in PointShopRPC rpc) =>
            {
                ecb.DestroyEntity(e);
                var player = rpc.Player;
                var boss = rpc.Boss;
                var price = rpc.Price;
                bool defeated = false;
                foreach (var killed in killeds)
                {
                    if (killed.objectData.objectID == boss)
                    {
                        defeated = true;
                        break;
                    }
                }
                if (!defeated)
                    return;
                if (!InventoryUtility.HasObject(containedLookup, player, currentcy, price))
                    return;
                inv.Add(new()
                {
                    inventoryChangeData = Create.ConsumeObjectType(player, currentcy, price),
                    playerEntity = player
                });
                EntityUtility.CreateAndDropItem(rpc.ObjectID, 0, 1, transLookup[player].Position,
                    player, database, ecb);
            })
                .WithName("PointShopUpdate")
                .WithAll<ReceiveRpcCommandRequest>()
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
    }
}
