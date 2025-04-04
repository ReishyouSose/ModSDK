using Assets.CoreEnhance.Scripts.Patchs;
using Inventory;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Physics;
using static Assets.CoreEnhance.Scripts.Helpers.ContainerHelper;

namespace Assets.CoreEnhance.Scripts.Systems.Quick
{
    public enum ExtraChestAction
    {
        PutAll,
        TakeAll,
        QuickStack,
        Replenish,
        Split
    }
    public struct ExtraChestActionRpc : IRpcCommand
    {
        public Entity player;
        public Entity chest;
        public ExtraChestAction action;
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(InventorySystemGroup))]
    public partial class ExtraChestActionClient : PugSimulationSystemBase
    {
        private NativeQueue<ExtraChestActionRpc> queue;
        private EntityArchetype archetype;
        private static ExtraChestActionClient ins;
        protected override void OnCreate()
        {
            ins = this;
            queue = new(Allocator.Persistent);
            archetype = EntityManager.CreateArchetype(typeof(ExtraChestActionRpc), typeof(SendRpcCommandRequest));
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            while (queue.TryDequeue(out var rpc))
            {
                var ecb = CreateCommandBuffer();
                var e = ecb.CreateEntity(archetype);
                ecb.SetComponent(e, rpc);
            }
            base.OnUpdate();
        }
        public static void Trigger()
        {
            ExtraChestAction action = Manager.ui.currentSelectedUIElement.GetComponent<ExtraChestButton>().action;
            var player = Manager.main.player;
            ins.queue.Enqueue(new()
            {
                action = action,
                player = player.entity,
                chest = player.activeInventoryHandler.inventoryEntity
            });
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(InventorySystemGroup))]
    public partial class ExtraChestActionServer : PugSimulationSystemBase
    {
        private InventoryHandlerShared inventoryHandlerShared;
        private ComponentLookup<PickUpItemCD> pickupLookup;
        private ComponentLookup<DirectionCD> dirLookup;
        private ComponentLookup<InventoryAutoTransferEnabledCD> autoTransferLookup;
        private CollisionWorld collision;
        protected override void OnCreate()
        {
            pickupLookup = SystemAPI.GetComponentLookup<PickUpItemCD>();
            dirLookup = SystemAPI.GetComponentLookup<DirectionCD>();
            autoTransferLookup = SystemAPI.GetComponentLookup<InventoryAutoTransferEnabledCD>();
            RequireForUpdate<PugDatabase.DatabaseBankCD>();
            NeedDatabase();
            base.OnCreate();
        }
        protected override void OnStartRunning()
        {
            inventoryHandlerShared = new InventoryHandlerShared(ref CheckedStateRef,
                SystemAPI.GetSingleton<PugDatabase.DatabaseBankCD>(),
                SystemAPI.GetSingleton<SkillTalentsTableCD>(),
                SystemAPI.GetSingleton<UpgradeCostsTableCD>(),
                SystemAPI.GetSingleton<InventoryAuxDataSystemDataCD>());
            collision = GetPhysicsWorld().CollisionWorld;
            base.OnStartRunning();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            var shared = inventoryHandlerShared;
            var pickupLookup = this.pickupLookup;
            var dirLookup = this.dirLookup;
            var autoTransferLookup = this.autoTransferLookup;
            var collision = this.collision;
            var tick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
            var databaseCD = SystemAPI.GetSingleton<PugDatabase.DatabaseBankCD>();
            Entities.ForEach((Entity e, in ExtraChestActionRpc rpc) =>
            {
                switch (rpc.action)
                {
                    case ExtraChestAction.PutAll:
                        PutAll(shared, rpc.player, rpc.chest);
                        break;
                    case ExtraChestAction.TakeAll:
                        TakeAll(shared, rpc.player, rpc.chest);
                        break;
                    case ExtraChestAction.QuickStack:
                        QuickStack(shared, rpc.player, rpc.chest);
                        break;
                    case ExtraChestAction.Replenish:
                        Replenish(shared, rpc.player, rpc.chest);
                        break;
                    case ExtraChestAction.Split:
                        SplitStacks(shared, rpc.chest);
                        break;
                }
                ecb.DestroyEntity(e);
            })
                .WithName("ExtraChestAction")
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
    }
}