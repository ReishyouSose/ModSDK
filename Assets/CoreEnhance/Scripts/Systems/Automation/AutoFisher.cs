using Assets.CoreEnhance.Scripts.Configs;
using Assets.CoreEnhance.Scripts.Helpers;
using Assets.CoreEnhance.Scripts.Items;
using PugTilemap;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.CoreEnhance.Scripts.Systems.Automation
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class AutoFisherSystem : PugSimulationSystemBase
    {
        private BiomeLookup biomeLookup;
        private BufferLookup<ContainedObjectsBuffer> containerLookup;
        private BufferLookup<GivesConditionsWhenEquippedBuffer> conditionLookup;
        private TileAccessor tileAccessor;
        private float timer;
        protected override void OnCreate()
        {
            containerLookup = SystemAPI.GetBufferLookup<ContainedObjectsBuffer>();
            conditionLookup = SystemAPI.GetBufferLookup<GivesConditionsWhenEquippedBuffer>();
            NeedDatabase();
            NeedLootBank();
            RequireForUpdate<BiomeRangesCD>();
            base.OnCreate();
        }
        protected override void OnStartRunning()
        {
            biomeLookup = SystemAPI.TryGetSingleton<BiomeSamplesCD>(out var sample)
                ? new(sample) : new(SystemAPI.GetSingleton<BiomeRangesCD>().Value, Allocator.Persistent);
            tileAccessor = CreateTileAccessor();
            base.OnStartRunning();
        }
        protected override void OnUpdate()
        {
            var delta = SystemAPI.Time.DeltaTime;
            if (timer < 1)
            {
                timer += delta;
                return;
            }
            timer = 0;

            if (!SystemAPI.TryGetSingletonEntity<AutoFisherTerminalCD>(out var terminal))
                return;
            if (!SystemAPI.TryGetSingleton<FishingTableCD>(out var fishingTable))
                return;
            float expChance = EnhanceConfig.TryGetValue<int>(EnhanceCategory.Automation,
                EC_Automation.GiveExp, out var exp, "Fishing") ? exp.Value : 0;
            containerLookup.TryGetBuffer(terminal, out var containers);
            var tileAccessor = this.tileAccessor;
            var biomeLookup = this.biomeLookup;
            var conditionLookup = this.conditionLookup;
            var database = base.database;
            var lootBack = lootBank;
            var ecb = CreateCommandBuffer();
            Entities.ForEach((DynamicBuffer<ContainedObjectsBuffer> inv, ref AutoFisherCD af,
               in DistanceToPlayerCD dis, in LocalTransform trans) =>
            {
                int2 pos = trans.Position.xz.RoundToInt2();
                if (!af.init)
                {
                    var biome = biomeLookup.GetBiome(pos);
                    Tileset tileSet = (Tileset)tileAccessor.GetTop(pos).tileset;
                    FishingInfoData info = fishingTable.GetFishingInfoFromWaterTileset(tileSet);
                    af.require = FishingTable.GetSkillRequiredForWater(tileSet);
                    if (info.lootTableID == LootTableID.Empty || tileSet == Tileset.Dirt)
                    {
                        info = fishingTable.GetFishingInfoFromBiome(biome);
                        af.require = FishingTable.GetSkillRequiredForBiome(biome);
                    }
                    af.fishes = info.fishLootTableID;
                    af.items = info.lootTableID;
                    af.init = true;
                }
                ObjectID id = inv[0].objectID;
                if (id != af.rod)
                {
                    af.rod = id;
                    var rodEntity = PugDatabase.GetPrimaryPrefabEntity(id, database);
                    if (!conditionLookup.TryGetBuffer(rodEntity, out var conditions))
                        return;
                    af.enable = false;
                    foreach (var condition in conditions)
                    {
                        var c = condition.equipmentCondition;
                        if (c.id == ConditionID.IncreasedFishing && c.value > af.require)
                        {
                            af.enable = true;
                            break;
                        }
                    }
                }
                if (!af.enable)
                    return;
                af.timer++;
                if (af.timer >= af.wait)
                {
                    af.timer = 0;
                    var rng = PugRandom.GetRng();
                    af.wait = rng.NextInt(3, 11);
                    var biome = biomeLookup.GetBiome(pos);
                    using var drops = PugDatabase.GetRandomLoot(rng.NextInt(6) == 0 ? af.items : af.fishes,
                        1, 1, ref rng, lootBack, database, trans.Position, biome);
                    ItemHelper.PutItemToContainer(containers, drops[0].objectID, drops[0].amount);
                    if (rng.NextInt(100) < expChance)
                        PlayerController.AddSkill(dis.closestPlayer, SkillID.Fishing, 1, ecb, true);
                }
            })
                .WithName("AutoFisher_Catch")
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
        //fishing loot determind - Pug.Other.Fising line:598
    }
}
