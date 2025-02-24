using Assets.CoreEnhance.Scripts.Configs;
using Assets.CoreEnhance.Scripts.Sturcts;
using PugTilemap;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace Assets.CoreEnhance.Scripts.Systems.Misc
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [UpdateAfter(typeof(TileDamageSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class ChainMining : PugSimulationSystemBase
    {
        private BiomeLookup biomeLookup;
        private TileAccessor tileAccessor;
        private ComponentLookup<TileCD> tileLookup;
        private static int2[] check;
        private NativeHashMap<Entity, int> counts;
        protected override void OnCreate()
        {
            tileLookup = SystemAPI.GetComponentLookup<TileCD>();
            check = new int2[4]
            {
                new(-1, 0),
                new(1, 0),
                new(0, 1),
                new(0, -1),
            };
            counts = new(8, Allocator.Persistent);
            NeedDatabase();
            base.OnCreate();
        }
        protected override void OnStartRunning()
        {
            tileAccessor = CreateTileAccessor();
            biomeLookup = SystemAPI.TryGetSingleton<BiomeSamplesCD>(out var sample)
                ? new(sample) : new(SystemAPI.GetSingleton<BiomeRangesCD>().Value, Allocator.Persistent);
            base.OnStartRunning();
        }
        protected override void OnUpdate()
        {
            if (!ModConfig.TryGetValue<int>(EnhanceCategory.Misc, EC_Misc.ChainMining, out var value))
                return;
            if (!SystemAPI.TryGetSingletonBuffer<TileDamageBuffer>(out var damager))
                return;
            var tileAccessor = this.tileAccessor;
            var ecb = CreateCommandBuffer();
            var collision = GetPhysicsWorld().CollisionWorld;
            var tileLookup = this.tileLookup;
            var biomeLookup = this.biomeLookup;
            var counts = this.counts;
            var need = value.Value;
            var database = this.database;
            Entities.ForEach((Entity e, in HealthCD health, in KilledByPlayerCD killer, in TileCD tile, in LocalTransform trans) =>
            {
                var player = killer.playerEntity;
                if (health.health <= 0 && player != Entity.Null)
                {
                    var pos = trans.Position.RoundToInt2();
                    NativeHashSet<int2> done = new(32, Allocator.Temp);
                    Chain(tileAccessor, tileLookup, pos, ref done);
                    int count = done.Count;
                    if (counts.ContainsKey(player))
                    {
                        counts[player] += count;
                    }
                    else
                        counts.Add(player, count);
                    while (counts[player] >= need)
                    {
                        counts[player] -= need;
                        ObjectID id = BiomeAndTilesetToChest(biomeLookup.GetBiome(pos), (Tileset)tile.tileset);
                        EntityUtility.CreateAndDropItem(id, 0, 1, trans.Position, player, database, ecb);
                    }
                    foreach (var p in done)
                    {
                        damager.Add(new()
                        {
                            bypassMaxDamagePerHit = true,
                            bypassDamageReduction = true,
                            causedByEntity = player,
                            damage = 114514,
                            canHitGround = true,
                            position = p,
                            pullAnyLootToPlayer = true,
                            dontHitGroundSlime = true,
                            dontHitBridges = true,
                        });
                    }
                    done.Dispose();
                    ecb.AddComponent<ProcessedTagCD>(e);
                }
            })
                .WithName("ChainMining")
                .WithNone<ProcessedTagCD>()
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
        private static void Chain(TileAccessor tileAccessor, ComponentLookup<TileCD> tileLookup,
            int2 ori, ref NativeHashSet<int2> done)
        {
            NativeArray<TileCD> array = tileAccessor.Get(ori, Allocator.Temp);
            foreach (var tile in array)
            {
                var type = tile.tileType;
                switch (type)
                {
                    case TileType.ore:
                    case TileType.ancientCrystal:
                        for (int i = 0; i < 4; i++)
                        {
                            var pos = ori + check[i];
                            if (done.Contains(pos))
                                continue;
                            if (IsTarget(tileAccessor, pos, type))
                            {
                                done.Add(pos);
                                Chain(tileAccessor, tileLookup, pos, ref done);
                            }
                        }
                        break;
                }
            }
            array.Dispose();
        }
        private static bool IsTarget(TileAccessor tileAccessor, int2 pos, TileType type)
        {
            NativeArray<TileCD> array = tileAccessor.Get(pos, Allocator.Temp);
            foreach (var tile in array)
            {
                if (tile.tileType == type)
                {
                    return true;
                }
            }
            return false;
        }
        private static ObjectID BiomeAndTilesetToChest(Biome biome, Tileset tileset)
        {
            switch (biome)
            {
                case Biome.Slime:
                    if (tileset == Tileset.Dirt || tileset == Tileset.Turf || tileset == Tileset.Sand)
                    {
                        return ObjectID.LockedCopperChest;
                    }

                    break;
                case Biome.Larva:
                    if (tileset == Tileset.Clay || tileset == Tileset.Sand)
                    {
                        return ObjectID.LockedCopperChest;
                    }

                    break;
                case Biome.Stone:
                    if (tileset == Tileset.Stone || tileset == Tileset.Sand)
                    {
                        return ObjectID.LockedIronChest;
                    }

                    break;
                case Biome.Nature:
                    switch (tileset)
                    {
                        case Tileset.Stone:
                        case Tileset.Nature:
                            return ObjectID.LockedScarletChest;
                        case Tileset.Crystal:
                            return ObjectID.LockedSolariteChest;
                    }

                    break;
                case Biome.Sea:
                    switch (tileset)
                    {
                        case Tileset.Sea:
                            return ObjectID.LockedOctarineChest;
                        case Tileset.Crystal:
                            return ObjectID.LockedSolariteChest;
                    }

                    break;
                case Biome.Desert:
                    switch (tileset)
                    {
                        case Tileset.Desert:
                            return ObjectID.LockedGalaxiteChest;
                        case Tileset.Crystal:
                            return ObjectID.LockedSolariteChest;
                    }

                    break;
                case Biome.Crystal:
                    if (tileset == Tileset.Crystal)
                    {
                        return ObjectID.LockedSolariteChest;
                    }

                    break;
            }

            return ObjectID.None;
        }
    }
}
