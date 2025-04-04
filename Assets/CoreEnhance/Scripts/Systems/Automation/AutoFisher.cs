using Assets.CoreEnhance.Scripts.Configs;
using Assets.CoreEnhance.Scripts.Helpers;
using Assets.CoreEnhance.Scripts.Items;
using PugTilemap;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

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
            bool requireBiome = EnhanceConfig.IsEnable(EnhanceCategory.Automation, EC_Automation.Fish);
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
                ref var biome = ref af.biome;
                if (!af.init)
                {
                    af.init = true;
                    biome = biomeLookup.GetBiome(pos);
                    Tileset tileSet = (Tileset)tileAccessor.GetTop(pos).tileset;
                    af.biome = CheckBiome(WaterTilesetToAreaLevel(tileSet));
                    af.biomeIsMatch = biome == af.biome;
                    FishingInfoData info = fishingTable.GetFishingInfoFromWaterTileset(tileSet);
                    af.require = FishingTable.GetSkillRequiredForWater(tileSet);
                    if (info.lootTableID == LootTableID.Empty || tileSet == Tileset.Dirt)
                    {
                        info = fishingTable.GetFishingInfoFromBiome(biome);
                        af.require = FishingTable.GetSkillRequiredForBiome(biome);
                    }
                    af.fishes = info.fishLootTableID;
                    af.items = info.lootTableID;
                }
                if (requireBiome && !af.biomeIsMatch)
                    return;
                ObjectID id = inv[0].objectID;
                if (id != af.rod)
                {
                    af.rod = id;
                    af.enable = false;
                    af.timer = 0;
                    af.wait = 10;
                    var rodEntity = PugDatabase.GetPrimaryPrefabEntity(id, database);
                    if (!conditionLookup.TryGetBuffer(rodEntity, out var conditions))
                        return;
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
                    LootTableID lt = rng.NextInt(6) == 0 ? af.items : af.fishes;
                    using var drops = PugDatabase.GetRandomLoot(lt,
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
        private static Biome CheckBiome(AreaLevel level) => level switch
        {
            AreaLevel.Slime or AreaLevel.StartArea => Biome.Slime,
            AreaLevel.Clay or AreaLevel.LarvaHive => Biome.Larva,
            AreaLevel.Stone => Biome.Stone,
            AreaLevel.Nature or AreaLevel.Mold => Biome.Nature,
            AreaLevel.Sea or AreaLevel.City => Biome.Sea,
            AreaLevel.Desert or AreaLevel.Lava => Biome.Desert,
            AreaLevel.Crystal => Biome.Crystal,
            AreaLevel.Passage => Biome.Passage,
            _ => Biome.Slime,
        };
        //fishing loot determind - Pug.Other.Fising line:598
    }
}
