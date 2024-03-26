using Assets.THCompass.Compasses;
using Assets.THCompass.DataStruct;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace Assets.THCompass.System
{
    public struct CompassLootRPC : IRpcCommand
    {
        public BossID bossID;
        public bool ten;
        public float3 pos;
        public int dir;
        public readonly ObjectID ChestID => bossID switch
        {
            BossID.Slime => ObjectID.CopperChest,
            BossID.Hive => ObjectID.CopperChest,
            BossID.Devourer => ObjectID.CopperChest,
            BossID.Shaman => ObjectID.IronChest,
            BossID.PoisonSlime => ObjectID.ScarletChest,
            BossID.Bird => ObjectID.ScarletChest,
            BossID.SlipperySlime => ObjectID.OctarineChest,
            BossID.Octopus => ObjectID.OctarineChest,
            BossID.LavaSlime => ObjectID.GalaxiteChest,
            BossID.Scarab => ObjectID.GalaxiteChest,
            BossID.Atlantis => ObjectID.SolariteChest,
            _ => ObjectID.None
        };

        public readonly ObjectID BossChestID => bossID switch
        {
            BossID.Slime => ObjectID.GlurchChest,
            BossID.Hive => ObjectID.HivemotherChest,
            BossID.Devourer => ObjectID.GhormChest,
            BossID.Shaman => ObjectID.BossChest,
            BossID.PoisonSlime => ObjectID.IvyChest,
            BossID.Bird => ObjectID.EasterChest,
            BossID.SlipperySlime => ObjectID.MorphaChest,
            BossID.Octopus => ObjectID.OctopusBossChest,
            BossID.LavaSlime => ObjectID.LavaSlimeBossChest,
            BossID.Scarab => ObjectID.BossChest,
            BossID.Atlantis => ObjectID.AtlantianWormChest,
            _ => ObjectID.None
        };
    }

    [UpdateInGroup(typeof(RunSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class ClientCompassLootSystem : PugSimulationSystemBase
    {
        private NativeQueue<CompassLootRPC> rpcQueue;
        private EntityArchetype rpcArchetype;

        protected override void OnCreate()
        {
            UpdatesInRunGroup();
            rpcQueue = new NativeQueue<CompassLootRPC>(Allocator.Persistent);
            rpcArchetype = EntityManager.CreateArchetype(typeof(CompassLootRPC), typeof(SendRpcCommandRequest));

            base.OnCreate();
        }

        public void CompassLoot(BossID bossID, bool ten, float3 pos, int dir)
        {
            rpcQueue.Enqueue(new CompassLootRPC
            {
                bossID = bossID,
                ten = ten,
                pos = pos,
                dir = dir
            });
        }
        protected override void OnUpdate()
        {
            EntityCommandBuffer ecb = CreateCommandBuffer();

            while (rpcQueue.TryDequeue(out CompassLootRPC component))
            {
                Entity e = ecb.CreateEntity(rpcArchetype);
                ecb.SetComponent(e, component);
                ecb.AddComponent(e, new SendRpcCommandRequest());
            }
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class ServerCompassLootSystem : PugSimulationSystemBase
    {
        protected override void OnCreate()
        {
            UpdatesInRunGroup();
            NeedDatabase();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            var databaseLocal = database;
            var cobl = GetBufferLookup<ContainedObjectsBuffer>(true);
            Entities.ForEach((Entity rpcEntity, in CompassLootRPC rpc, in ReceiveRpcCommandRequest req) =>
            {
                var rng = PugRandom.GetRng();
                using NativeList<ObjectDataCD> items = CompassLoader.GetLoots(rpc, rng);
                int dir = rpc.dir;
                float3 offset = new();
                ObjectID chestID = rpc.ChestID;
                if (rpc.ten)
                {
                    offset = new(dir == 2 ? -1 : 0, 0, dir == 1 ? -1 : 0);
                    chestID = rpc.BossChestID;
                }
                Entity chest = EntityUtility.CreateEntityWithItems(ecb, rpc.pos + offset, chestID, 1, items, databaseLocal, cobl);
                ecb.SetComponent<DirectionCD>(chest, new()
                {
                    direction = dir switch
                    {
                        1 => new(-1, 0, 0),
                        2 => new(0, 0, 1),
                        3 => new(1, 0, 0),
                        _ => new(0, 0, -1)
                    }
                });
                ecb.DestroyEntity(rpcEntity);
            })
                .WithoutBurst()
                .Schedule();

            base.OnUpdate();
        }
    }
}
