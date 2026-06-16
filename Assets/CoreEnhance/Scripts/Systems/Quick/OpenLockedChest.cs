using Assets.CoreEnhance.Scripts.Cores;
using Inventory;
using Pug.UnityExtensions;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace Assets.CoreEnhance.Scripts.Systems.Quick
{
    public struct QuickOpenLockedChestRPC : IRpcCommand
    {
        public Entity player;
    }
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class QuickOpenLockedChestClient : PugSimulationSystemBase
    {
        private static QuickOpenLockedChestClient ins;
        private NativeQueue<Entity> queue;
        private EntityArchetype archetype;
        protected override void OnCreate()
        {
            ins = this;
            queue = new(Allocator.Persistent);
            archetype = EntityManager.CreateArchetype(typeof(SendRpcCommandRequest));
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            if (!EnhanceConfig.IsEnable(EnhanceCategory.OpenLockedChest))
                return;
            var ecb = CreateCommandBuffer();
            while (queue.TryDequeue(out var player))
            {
                var e = ecb.CreateEntity(archetype);
                ecb.AddComponent(e, new QuickOpenLockedChestRPC() { player = player });
            }
            base.OnUpdate();
        }
        public static void Trigger(PlayerController player)
        {
            if (EnhanceConfig.IsEnable(EnhanceCategory.OpenLockedChest))
                ins.queue.Enqueue(player.entity);
        }
    }
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class QuickOpenLockedChestServer : PugSimulationSystemBase
    {
        private NativeQueue<Entity> queue;
        private ComponentLookup<LocalTransform> transLookup;
        private BufferLookup<ContainedObjectsBuffer> containerLookup;
        protected override void OnCreate()
        {
            queue = new(Allocator.Persistent);
            transLookup = SystemAPI.GetComponentLookup<LocalTransform>();
            containerLookup = SystemAPI.GetBufferLookup<ContainedObjectsBuffer>();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            if (!EnhanceConfig.IsEnable(EnhanceCategory.OpenLockedChest))
                return;
            var queue = this.queue;
            var ecb = CreateCommandBuffer();
            Entities.ForEach((Entity e, in QuickOpenLockedChestRPC rpc) =>
            {
                queue.Enqueue(rpc.player);
                ecb.DestroyEntity(e);
            })
                .WithName("ReceiveQuickOpenLockedChestRequest")
                .WithAll<ReceiveRpcCommandRequest>()
                .WithBurst()
                .Schedule();

            if (!SystemAPI.TryGetSingletonBuffer<InventoryChangeBuffer>(out var invChangeBuffer))
                return;
            while (queue.TryDequeue(out var player))
            {
                transLookup.TryGetComponent(player, out var trans);
                var ori = trans.Position.RoundToInt2();
                containerLookup.TryGetBuffer(player, out var inv);
                Entities.ForEach((DynamicBuffer<ContainedObjectsBuffer> slot,
                    in ChangeVariationWhenContainingObjectCD require, in LocalTransform local) =>
                {
                    if (slot[0].objectID != ObjectID.None)
                        return;
                    var pos = local.Position.RoundToInt2();
                    if (math.abs(pos.x - ori.x) > 10)
                        return;
                    if (math.abs(pos.y - ori.y) > 10)
                        return;
                    ObjectID target = require.objectID;
                    for (int i = 0; i < inv.Length; i++)
                    {
                        if (inv[i].objectID == target)
                        {
                            slot[0] = new()
                            {
                                objectData = new()
                                {
                                    objectID = target,
                                    amount = 1
                                }
                            };
                            invChangeBuffer.Add(new()
                            {
                                inventoryChangeData = Create.ConsumeEntityAt(player, i, 1, false, false),
                                playerEntity = player
                            });
                            return;
                        }
                    }
                })
                    .WithName("QuickOpenLockedChest")
                    .WithBurst()
                    .Schedule();
            }
            base.OnUpdate();
        }
    }
}
