using Assets.CoreEnhance.Scripts.Cores;
using CoreLib.Data.Configuration;
using Pug.UnityExtensions;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using static Assets.CoreEnhance.Scripts.Helpers.TileHelper;


namespace Assets.CoreEnhance.Scripts.Systems.Misc
{
    public struct OreCD : IComponentData { }

    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [UpdateBefore(typeof(DropLootSystem))]
    [UpdateAfter(typeof(SetEntitiesDestroyedSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
    public partial class ChainMiningSystem : PugSimulationSystemBase
    {
        private NativeList<int2> offset;
        private CollisionWorld collision;
        private TileAccessor tileAccessor;
        private ComponentLookup<KilledByPlayerCD> killLookup;
        private ComponentLookup<PlayerGhost> playerLookup;
        private ComponentLookup<ObjectDataCD> objLookup;
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
            objLookup = SystemAPI.GetComponentLookup<ObjectDataCD>();
            base.OnCreate();
        }
        protected override void OnStartRunning()
        {
            tileAccessor = CreateTileAccessor();
            collision = GetPhysicsWorld().CollisionWorld;
            base.OnStartRunning();
        }
        protected override void OnUpdate()
        {
            if (!EnhanceConfig.TryGetValues(EnhanceCategory.ChainMining, out var values))
                return;
            if (!SystemAPI.TryGetSingletonBuffer<TileDamageBuffer>(out var tileDamageBuffer))
                return;
            if (!SystemAPI.TryGetSingletonBuffer<HealthChangeBuffer>(out var healthChangeBuffer))
                return;
            var tileAccessor = this.tileAccessor;
            var offset = this.offset;
            var killLookup = this.killLookup;
            var playerLookup = this.playerLookup;
            var objLookup = this.objLookup;
            bool adsorption = (values["Adsorption"] as ConfigEntry<bool>).Value;
            bool needPlayer = (values["NeedPlayer"] as ConfigEntry<bool>).Value;
            bool addSkill = (values["GiveExp"] as ConfigEntry<bool>).Value;
            var ecb = CreateCommandBuffer();
            var collision = this.collision;
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

                var origin = local.Position;
                var p = origin.RoundToInt2();
                var tiles = tileAccessor.Get(p, Allocator.Temp);
                TryGetResource(tiles, out bool ore, out bool wood);
                tiles.Dispose();
                int count = 0;
                if (ore || wood)
                {
                    foreach (var target in offset)
                    {
                        var pos = p + target;
                        tiles = tileAccessor.Get(pos, Allocator.Temp);
                        TryGetResource(tiles, out bool isOre, out bool isWood);
                        if ((isOre && ore) || (isWood && wood))
                        {
                            tileDamageBuffer.Add(new()
                            {
                                pullAnyLootToPlayer = adsorption,
                                causedByEntity = player,
                                damage = 999999,
                                canHitLowColliders = wood,
                                dontHitGroundSlime = true,
                                position = pos
                            });
                            count++;
                        }
                        tiles.Dispose();
                    }
                }
                else if (SpecialOre(objLookup, e))
                {
                    NativeList<ColliderCastHit> hits = new(Allocator.Temp);
                    collision.SphereCastAll(origin, 1.5f, float3.zero, 0, ref hits, CollisionFilter.Default);
                    foreach (var hit in hits)
                    {
                        var entity = hit.Entity;
                        if (SpecialOre(objLookup, entity))
                        {
                            healthChangeBuffer.Add(new()
                            {
                                healthChange = new()
                                {
                                    entity = entity,
                                    amount = -999999,
                                    causedByEntity = player,
                                    pullLootToPlayer = true,
                                }
                            });
                            count++;
                        }
                    }
                    hits.Dispose();
                }
                if (count > 0 && addSkill && player != Entity.Null)
                {
                    var skill = ecb.CreateEntity();
                    ecb.AddComponent(skill, new AddSkillValueCD()
                    {
                        amount = count,
                        skillID = SkillID.Mining,
                        entity = player
                    });
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


        public static bool SpecialOre(ComponentLookup<ObjectDataCD> objLookup, Entity e) =>
            objLookup.TryGetComponent(e, out var objData)
            && objData.objectID is ObjectID.PandoriumCrystal or ObjectID.SmallPandoriumCrystal;
    }
}
