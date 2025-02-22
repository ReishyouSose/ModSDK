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
        private TileAccessor tileAccessor;
        private ComponentLookup<TileCD> tileLookup;
        private ComponentLookup<LocalTransform> transLookup;
        private static int2[] check;
        protected override void OnCreate()
        {
            tileLookup = SystemAPI.GetComponentLookup<TileCD>();
            transLookup = SystemAPI.GetComponentLookup<LocalTransform>();
            check = new int2[4]
            {
                new(-1, 0),
                new(1, 0),
                new(0, 1),
                new(0, -1),
            };
            RequireForUpdate<TileCD>();
            RequireForUpdate<KilledByPlayerCD>();
            base.OnCreate();
        }
        protected override void OnStartRunning()
        {
            tileAccessor = CreateTileAccessor();
            base.OnStartRunning();
        }
        protected override void OnUpdate()
        {
            if (!ModConfig.IsEnable(EnhanceCategory.Misc, EC_Misc.ChainMining))
                return;
            if (!SystemAPI.TryGetSingletonBuffer<TileDamageBuffer>(out var damager))
                return;
            var tileAccessor = this.tileAccessor;
            var ecb = CreateCommandBuffer();
            var collision = GetPhysicsWorld().CollisionWorld;
            var tileLookup = this.tileLookup;
            var transLookup = this.transLookup;
            Entities.ForEach((Entity e, in HealthCD health, in KilledByPlayerCD killer, in LocalTransform trans) =>
            {
                if (health.health <= 0 && killer.playerEntity != Entity.Null)
                {
                    var pos = trans.Position.RoundToInt2();
                    NativeHashSet<int2> done = new(32, Allocator.Temp);
                    Chain(tileAccessor, tileLookup, pos, ref done);
                    foreach (var p in done)
                    {
                        damager.Add(new()
                        {
                            bypassMaxDamagePerHit = true,
                            bypassDamageReduction = true,
                            causedByEntity = killer.playerEntity,
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
    }
}
