using Assets.CoreEnhance.Scripts.Configs;
using CoreLib.Data.Configuration;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using static Assets.CoreEnhance.Scripts.Helpers.TileHelper;

namespace Assets.CoreEnhance.Scripts.Systems.Misc
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [UpdateBefore(typeof(DropLootSystem))]
    [UpdateAfter(typeof(SetEntitiesDestroyedSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
    public partial class ChainMiningSystem : PugSimulationSystemBase
    {
        private NativeList<int2> offset;
        private TileAccessor tileAccessor;
        private ComponentLookup<KilledByPlayerCD> killLookup;
        private ComponentLookup<PlayerGhost> playerLookup;
        protected override void OnCreate()
        {
            offset = new(4, Allocator.Persistent)
            {
                new(1, 0),
                new(-1, 0),
                new(0, 1),
                new(0, -1)
            };
            killLookup = SystemAPI.GetComponentLookup<KilledByPlayerCD>();
            playerLookup = SystemAPI.GetComponentLookup<PlayerGhost>();
            base.OnCreate();
        }
        protected override void OnStartRunning()
        {
            tileAccessor = CreateTileAccessor();
            base.OnStartRunning();
        }
        protected override void OnUpdate()
        {
            if (!EnhanceConfig.TryGetValues(EnhanceCategory.Misc, EC_Misc.ChainMining, out var values))
                return;
            if (!SystemAPI.TryGetSingletonBuffer<TileDamageBuffer>(out var buffer))
                return;
            var tileAccessor = this.tileAccessor;
            var offset = this.offset;
            var killLookup = this.killLookup;
            var playerLookup = this.playerLookup;
            bool adsorption = (values["Adsorption"] as ConfigEntry<bool>).Value;
            bool needPlayer = (values["NeedPlayer"] as ConfigEntry<bool>).Value;
            Entities.ForEach((Entity e, in LocalTransform local) =>
            {
                var player = Entity.Null;
                if (killLookup.TryGetComponent(e, out var killer))
                    player = killer.playerEntity;
                if (needPlayer)
                {
                    if (player == Entity.Null)
                        return;
                    if (!playerLookup.HasComponent(player))
                        return;
                }

                var p = local.Position.RoundToInt2();
                var tiles = tileAccessor.Get(p, Allocator.Temp);
                TryGetResource(tiles, out bool ore, out bool wood);
                tiles.Dispose();
                if (ore || wood)
                {
                    foreach (var target in offset)
                    {
                        var pos = p + target;
                        tiles = tileAccessor.Get(pos, Allocator.Temp);
                        TryGetResource(tiles, out bool isOre, out bool isWood);
                        if ((isOre && ore) || (isWood && wood))
                        {
                            buffer.Add(new()
                            {
                                pullAnyLootToPlayer = adsorption,
                                causedByEntity = player,
                                damage = 999999,
                                canHitLowColliders = wood,
                                dontHitGroundSlime = true,
                                position = pos
                            });
                        }
                        tiles.Dispose();
                    }
                }
            })
                .WithName("ChainMining")
                .WithBurst()
                .WithAll<MineableCD>()
                .WithAll<EntityDestroyedCD>()
                .WithDisabled<StartDroppingLootCD>()
                .Schedule();
            base.OnUpdate();
        }
    }
}
