using Assets.CoreEnhance.Scripts.Configs;
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
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
    public partial class ChainMining : PugSimulationSystemBase
    {
        private TileAccessor tileAccessor;
        private ComponentLookup<TileCD> tileLookup;
        private static int2[] check;
        private NativeHashMap<Entity, NativeHashSet<int2>> lasts;
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
            lasts = new(8, Allocator.Persistent);
            base.OnCreate();
        }
        protected override void OnStartRunning()
        {
            tileAccessor = CreateTileAccessor();
            base.OnStartRunning();
        }
        protected override void OnUpdate()
        {
            if (!EnhanceConfig.IsEnable(EnhanceCategory.Misc, EC_Misc.ChainMining))
                return;
            if (!SystemAPI.TryGetSingletonBuffer<TileDamageBuffer>(out var damager))
                return;
            var tileAccessor = this.tileAccessor;
            var ecb = CreateCommandBuffer();
            var collision = GetPhysicsWorld().CollisionWorld;
            var tileLookup = this.tileLookup;
            var lasts = this.lasts;
            Entities.ForEach((Entity e, in HealthCD health, in KilledByPlayerCD killer, in LocalTransform trans) =>
            {
                var player = killer.playerEntity;
                if (health.health <= 0 && player != Entity.Null)
                {
                    var wp = trans.Position;
                    var pos = wp.RoundToInt2();
                    if (lasts.TryGetValue(player, out var last))
                    {
                        if (last.Contains(pos))
                            return;
                    }
                    else
                    {
                        lasts[player] = new(64, Allocator.Persistent);
                    }
                    lasts[player].Clear();
                    lasts[player].Add(pos);
                    NativeHashSet<int2> done = new(32, Allocator.Temp);
                    Chain(tileAccessor, tileLookup, pos, ref done);
                    foreach (var p in done)
                    {
                        lasts[player].Add(p);
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
                }
            })
                .WithName("ChainMining")
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
                switch (tile.tileType)
                {
                    case TileType.ore:
                    case TileType.ancientCrystal:
                        for (int i = 0; i < 4; i++)
                        {
                            var pos = ori + check[i];
                            if (done.Contains(pos))
                                continue;
                            if (IsTarget(tileAccessor, pos))
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
        private static bool IsTarget(TileAccessor tileAccessor, int2 pos)
        {
            NativeArray<TileCD> array = tileAccessor.Get(pos, Allocator.Temp);
            foreach (var tile in array)
            {
                if (tile.tileType is TileType.ore or TileType.ancientCrystal)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
