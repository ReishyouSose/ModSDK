using Assets.THCompass.Compasses;
using Assets.THCompass.Helper;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using static Assets.THCompass.Compasses.CompassData;
using static Assets.THCompass.Compasses.CompassLootTable;
using static Assets.THCompass.Compasses.THLootTable;
using static Assets.THCompass.Helper.ItemHelper;

namespace Assets.THCompass.System
{
    public static class CompassLootSystem
    {
        public static void Add(this List<THLootInfo> list, ObjectID itemType, int min = 1, int max = 1, float dropChance = 1f)
        {
            list.Add(new(itemType, min, max, dropChance));
        }
        public static void Add(this List<THLootInfo> list, int min, int max, float dropChance, params ObjectID[] ids)
        {
            foreach (ObjectID id in ids)
            {
                list.Add(id, min, max, dropChance);
            }
        }
        public static NativeList<ObjectDataCD> GetLoots(BossID bossID, bool ten)
        {
            THLootTable table = THLootTable.CreateLootTable();
            table.AddLootTable(LootType.Animal, 3, 5, 0.1f, AnimalsUseful);
            table.AddLootTable(LootType.Animal, 3, 5, 0.15f, AnimalsUseless);
            table.AddLootTable(LootType.Summon, 1, 1, 0.05f, GetSummoner(bossID));
            table.AddLootTable(LootType.Area, 1, 3, 0.1f, GetAreaChest(bossID));
            table.AddLootTable(LootType.Reinforcement, 0.1f, GetReinforcement(bossID), 1, 2);
            table.AddLootTable(LootType.Uniqueness, 0.01f, GetUniqueness(bossID));
            Dictionary<ObjectID, int> result = new();
            int time = 1, mult = 1;
            if (ten)
            {
                time = 10;
                mult = 2;
            }
            foreach (var (type, list) in table.GetLootTable())
            {
                for (int i = 0; i < time; i++)
                {
                    foreach (THLootInfo info in GetGrandPrize(type, bossID, list, mult))
                    {
                        if (info.dropChance.GetRngBool())
                        {
                            if (result.ContainsKey(info.itemType))
                            {
                                result[info.itemType] += info.DropCount;
                            }
                            else result[info.itemType] = info.DropCount;
                        }
                    }
                }
            }
            NativeList<ObjectDataCD> items = new(Allocator.Temp);
            foreach (var (type, stack) in result)
            {
                int amount = stack;
                while (amount > 999)
                {
                    items.Add(NewItem(type, 999));
                    amount -= 999;
                }

                if (!canstack.Contains(type))
                {
                    ObjectInfo info = PugDatabase.GetObjectInfo(type);
                    if (info != null && info.isStackable) canstack.Add(type);
                }
                while (!canstack.Contains(type) && amount > 1)
                {
                    items.Add(NewItem(type, 1));
                    amount--;
                }
                items.Add(NewItem(type, amount));
            };
            return items;
        }
    }
    public struct CompassLootRPC : IRpcCommand
    {
        public BossID bossID;
        public bool ten;
        public float3 pos;
        public int dir;
        public readonly ObjectID ChestID => chest[bossID];
        public readonly ObjectID BossChestID => bossChest[bossID];
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
                using NativeList<ObjectDataCD> items = CompassLootSystem.GetLoots(rpc.bossID, rpc.ten);
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
