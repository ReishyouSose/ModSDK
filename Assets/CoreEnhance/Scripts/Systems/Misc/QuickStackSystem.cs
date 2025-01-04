using Assets.CoreEnhance.Scripts.Configs;
using CoreLib.Data.Configuration;
using Inventory;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Systems.Misc
{
    public struct QuickStackRpc : IRpcCommand
    {
        public int playerIndex;
        public QuickStackRpc(int playerInex)
        {
            this.playerIndex = playerInex;
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class QuickStackClient : PugSimulationSystemBase
    {
        internal static QuickStackClient Ins { get; private set; }
        private NativeQueue<QuickStackRpc> queue;
        private EntityArchetype archetype;
        protected override void OnCreate()
        {
            Ins = this;
            queue = new(Allocator.Persistent);
            archetype = EntityManager.CreateArchetype(typeof(QuickStackRpc), typeof(SendRpcCommandRequest));
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            while (queue.TryDequeue(out var rpc))
            {
                Entity e = ecb.CreateEntity(archetype);
                ecb.SetComponent(e, rpc);
            }
            base.OnUpdate();
        }
        public static bool SendRequest()
        {
            if (Ins == null)
                return false;
            var player = Manager.main.player;
            if (player == null)
                return false;
            if (!ModConfig.TryGetEnable(EnhanceCategory.Misc, EC_Misc.QuickStack))
                return false;
            Ins.queue.Enqueue(new(player.playerIndex));
            return true;
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class QuickStackServer : PugSimulationSystemBase
    {
        private NativeQueue<QuickStackRpc> queue;
        private EntityQuery playerQuery;
        private EntityQuery containerQuery;
        private InventoryHandlerShared shard;
        protected override void OnCreate()
        {
            queue = new(Allocator.Persistent);
            playerQuery = EntityManager.CreateEntityQuery(typeof(PlayerGhost));
            containerQuery = EntityManager.CreateEntityQuery(typeof(InventoryCD), typeof(MineableCD));
            NeedDatabase();
            base.OnCreate();
        }
        protected override void OnStartRunning()
        {
            shard = new InventoryHandlerShared(ref CheckedStateRef,
                SystemAPI.GetSingleton<PugDatabase.DatabaseBankCD>(),
                SystemAPI.GetSingleton<SkillTalentsTableCD>(),
                SystemAPI.GetSingleton<UpgradeCostsTableCD>(),
                SystemAPI.GetSingleton<InventoryAuxDataSystemDataCD>());
            base.OnStartRunning();
        }
        protected override void OnUpdate()
        {
            if (!ModConfig.TryGetValue(EnhanceCategory.Misc, EC_Misc.QuickStack, out ConfigEntry<int> value))
                return;
            var ecb = CreateCommandBuffer();
            var localQueue = queue;
            shard.Update(ref CheckedStateRef, ecb, SystemAPI.GetSingleton<NetworkTime>());
            Entities.ForEach((Entity e, in QuickStackRpc rpc) =>
            {
                localQueue.Enqueue(rpc);
                ecb.DestroyEntity(e);
            })
                .WithName("Misc_QuickStack")
                .WithAll<ReceiveRpcCommandRequest>()
                .WithBurst()
                .Schedule();

            while (queue.TryDequeue(out var rpc))
            {
                var playerLookup = SystemAPI.GetComponentLookup<PlayerGhost>();
                var posLookup = SystemAPI.GetComponentLookup<LocalTransform>();
                var entities = playerQuery.ToEntityArray(Allocator.Temp);
                var player = entities.First(x => MatchPlayerIndex(playerLookup, x, rpc.playerIndex));
                posLookup.TryGetComponent(player, out var playerPos);
                entities.Dispose();
                entities = containerQuery.ToEntityArray(Allocator.Temp);
                int range = value.Value;
                foreach (var container in entities)
                {
                    posLookup.TryGetComponent(container, out var pos);
                    float dis = Mathf.Sqrt(Vector2.Distance(playerPos.Position.xz, pos.Position.xz));
                    if (dis > range)
                        continue;
                    InventoryUtility.QuickStack(shard, player, container);
                }
                entities.Dispose();
            }

            base.OnUpdate();
        }
        private static bool MatchPlayerIndex(ComponentLookup<PlayerGhost> lookup, Entity e, int index)
        {
            if (lookup.TryGetComponent(e, out var player))
            {
                return player.playerIndex == index;
            }
            return false;
        }
    }
}
