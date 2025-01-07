using Assets.CoreEnhance.Scripts.Items;
using PugTilemap;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace Assets.CoreEnhance.Scripts.Systems.Misc
{
    public struct AutoFisherExpRpc : IRpcCommand
    {
        public Entity AutoFisher;
        public Entity Player;
        public AutoFisherExpRpc(Entity autoFisher, Entity player)
        {
            AutoFisher = autoFisher;
            Player = player;
        }
    }
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class AutoFisherClient : PugSimulationSystemBase
    {
        private NativeQueue<AutoFisherExpRpc> queue;
        private EntityArchetype archetype;
        private static AutoFisherClient ins;
        protected override void OnCreate()
        {
            ins = this;
            queue = new(Allocator.Persistent);
            archetype = EntityManager.CreateArchetype(typeof(AutoFisherExpRpc), typeof(SendRpcCommandRequest));
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            while (queue.TryDequeue(out var exp))
            {
                var e = ecb.CreateEntity(archetype);
                ecb.SetComponent(e, exp);
            }
            base.OnUpdate();
        }
        public static void ReceiveExp(Entity autoFisher, Entity player)
        {
            ins.queue.Enqueue(new(autoFisher, player));
        }
    }
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class AutoFisherServer : PugSimulationSystemBase
    {
        private NativeHashMap<int, bool> stackable;
        private BiomeLookup biomeLookup;
        private ComponentLookup<ObjectDataCD> objDataLookup;
        protected override void OnCreate()
        {
            stackable = new(1024, Allocator.Persistent);
            NeedDatabase();
            NeedLootBank();
            base.OnCreate();
        }
        protected override void OnStartRunning()
        {
            objDataLookup = SystemAPI.GetComponentLookup<ObjectDataCD>();
            biomeLookup = SystemAPI.TryGetSingleton<BiomeSamplesCD>(out var sample)
                ? new(sample) : new(SystemAPI.GetSingleton<BiomeRangesCD>().Value, Allocator.Persistent);
            base.OnStartRunning();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            var lookup = objDataLookup;
            Entities.ForEach((Entity e, in AutoFisherExpRpc exp) =>
            {
                ref var objData = ref lookup.GetRefRW(exp.AutoFisher).ValueRW;
                PlayerController.AddSkill(exp.Player, SkillID.Fishing, objData.amount, ecb, true);
                objData.amount = 0;
                ecb.DestroyEntity(e);
            })
                .WithName("AutoFisher_ReceiveExp")
                .WithAll<ReceiveRpcCommandRequest>()
                .WithBurst()
                .Schedule();

            var tileAccessor = CreateTileAccessor();
            var biomeLookup = this.biomeLookup;
            var localDatabase = database;
            var localLootBack = lootBank;
            var localStackable = stackable;
            var delta = SystemAPI.Time.DeltaTime;
            Entities.ForEach((DynamicBuffer<ContainedObjectsBuffer> containers, ref ObjectDataCD objData,
                ref AutoFisherCD af, ref RandomCD random, in InventoryCD inv, in LocalTransform trans) =>
            {
                ref var rng = ref random.Value;
                if (rng.NextFloat(10) > af.timer)
                {
                    af.timer += delta;
                    return;
                }
                af.timer = 0;
                if (!af.init)
                {
                    af.init = true;
                    int2 pos = trans.Position.xz.RoundToInt2();
                    AreaLevel areaLevel = WaterTilesetToAreaLevel((Tileset)tileAccessor.GetTop(pos).tileset);
                    (af.fishes, af.items) = areaLevel switch
                    {
                        AreaLevel.Passage => (LootTableID.PassageFishes, LootTableID.PassageFishingLoot),
                        AreaLevel.Crystal => (LootTableID.CrystalFishes, LootTableID.CrystalFishingLoot),
                        AreaLevel.Lava => (LootTableID.LavaFishes, LootTableID.LavaFishingLoot),
                        AreaLevel.Desert => (LootTableID.DesertFishes, LootTableID.DesertFishingLoot),
                        AreaLevel.Sea => (LootTableID.SeaFishes, LootTableID.SeaFishingLoot),
                        AreaLevel.Mold => (LootTableID.MoldFishes, LootTableID.MoldFishingLoot),
                        AreaLevel.Nature => (LootTableID.NatureFishes, LootTableID.NatureFishingLoot),
                        AreaLevel.Stone => (LootTableID.StoneFishes, LootTableID.StoneFishingLoot),
                        AreaLevel.LarvaHive => (LootTableID.LarvaFishes, LootTableID.LarvaFishingLoot),
                        _ => (LootTableID.DirtFishes, LootTableID.DirtFishingLoot)
                    };
                    Biome biome = biomeLookup.GetBiome(pos);
                }
                var drops = PugDatabase.GetRandomLoot(rng.NextBool() ? af.fishes : af.items, 1, 1,
                      ref rng, localLootBack, localDatabase, trans.Position, af.biome);
                int length = inv.size;
                int count = drops.Length;
                for (int j = 0; j < count; j++)
                {
                    var item = drops[j];
                    objData.amount++;
                    for (int i = 0; i < length; i++)
                    {
                        var data = containers[i].objectData;
                        ObjectID id = data.objectID;
                        if (id == item.objectID)
                        {
                            if (!localStackable.TryGetValue((int)id, out bool stack))
                            {
                                ref var info = ref PugDatabase.GetEntityObjectInfo(id, localDatabase);
                                localStackable.Add((int)id, stack = info.isStackable);
                            }
                            if (stack)
                            {
                                int amount = data.amount + item.amount;
                                if (amount > 9999)
                                {
                                    containers[i] = CreateItem(id, 9999);
                                    drops[j] = new()
                                    {
                                        objectID = id,
                                        amount = amount - 9999
                                    };
                                    continue;
                                }
                                else
                                {
                                    containers[i] = CreateItem(id, amount);
                                    break;
                                }
                            }
                            else
                                continue;
                        }
                        else if (id == ObjectID.None)
                        {
                            containers[i] = CreateItem(item.objectID, item.amount);
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                drops.Dispose();
            })
                .WithName("AutoFisher_Catch")
                .WithBurst()
                .Schedule();

            base.OnUpdate();
        }
        private static ContainedObjectsBuffer CreateItem(ObjectID objID, int amount, int variation = 0)
        {
            return new()
            {
                objectData = new()
                {
                    objectID = objID,
                    amount = amount,
                    variation = variation,
                }
            };
        }
        private static AreaLevel WaterTilesetToAreaLevel(Tileset tileset)
        {
            if (tileset <= Tileset.Desert)
            {
                switch (tileset)
                {
                    case Tileset.Dirt:
                        return AreaLevel.Slime;
                    case Tileset.Stone:
                        return AreaLevel.Stone;
                    case Tileset.Obsidian:
                    case Tileset.Extras:
                    case Tileset.BaseBuildingWood:
                    case Tileset.BaseBuildingStone:
                        break;
                    case Tileset.Lava:
                        return AreaLevel.Lava;
                    case Tileset.LarvaHive:
                        return AreaLevel.Clay;
                    case Tileset.Nature:
                        return AreaLevel.Nature;
                    case Tileset.Mold:
                        return AreaLevel.Mold;
                    case Tileset.Sea:
                        return AreaLevel.Sea;
                    default:
                        if (tileset == Tileset.Desert)
                        {
                            return AreaLevel.Desert;
                        }
                        break;
                }
            }
            else
            {
                if (tileset == Tileset.Crystal)
                {
                    return AreaLevel.Crystal;
                }
                if (tileset == Tileset.Passage)
                {
                    return AreaLevel.Passage;
                }
            }
            return AreaLevel.Slime;
        }
    }
}
