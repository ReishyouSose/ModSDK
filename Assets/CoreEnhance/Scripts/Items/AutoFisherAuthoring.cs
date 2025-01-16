using PugConversion;
using PugTilemap;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Items
{
    public class AutoFisherAuthoring : MonoBehaviour
    {
    }

    public struct AutoFisherCD : IComponentData
    {
        public bool init;
        public LootTableID fishes;
        public LootTableID items;
        public Biome biome;
        public AreaLevel level;
        public float timer;

        public static void Init(ref AutoFisherCD af, TileAccessor tileAccessor, BiomeLookup biomeLookup, LocalTransform trans)
        {
            if (!af.init)
            {
                af.init = true;
                int2 pos = trans.Position.xz.RoundToInt2();
                af.level = WaterTilesetToAreaLevel((Tileset)tileAccessor.GetTop(pos).tileset);
                (af.fishes, af.items) = af.level switch
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
                af.biome = biomeLookup.GetBiome(pos);
            }
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
        public readonly bool CheckRodLevel(DynamicBuffer<ContainedObjectsBuffer> containers,
            out float efficiency, out int chance)
        {
            efficiency = 0;
            chance = containers[2].objectID == ObjectID.None ? 0 : 3;
            ObjectID rod = containers[0].objectID;
            if (rod == ObjectID.None)
                return false;
            efficiency = rod switch
            {
                ObjectID.WoodFishingRod => 0.4f,
                ObjectID.TinFishingRod => 0.6f,
                ObjectID.IronFishingRod => 0.8f,
                ObjectID.ScarletFishingRod => 1f,
                ObjectID.OctarineFishingRod => 1.2f,
                ObjectID.GalaxiteFishingRod => 1.4f,
                ObjectID.SolariteFishingRod => 1.6f,
                _ => 0f
            };
            bool allow = level switch
            {
                AreaLevel.Slime or AreaLevel.StartArea => true,
                AreaLevel.Clay or AreaLevel.LarvaHive => efficiency >= 0.6f,
                AreaLevel.Stone => efficiency >= 0.8f,
                AreaLevel.Nature or AreaLevel.Mold => efficiency >= 1f,
                AreaLevel.Sea or AreaLevel.City => efficiency >= 1.2f,
                AreaLevel.Desert or AreaLevel.Lava => efficiency >= 1.4f,
                AreaLevel.Crystal or AreaLevel.Passage or AreaLevel.Obsidian => efficiency >= 1.6f,
                _ => false,
            };
            float additive = 1;
            for (int i = 3; i < 8; i++)
            {
                if (containers[i].objectID != ObjectID.None)
                    additive++;
            }
            float bait = containers[1].objectID != ObjectID.None ? 1 : 0.1f;
            efficiency = efficiency * additive * bait;
            return allow;
        }
    }
    public class AutoFisherConverter : SingleAuthoringComponentConverter<AutoFisherAuthoring>
    {
        protected override void Convert(AutoFisherAuthoring authoring)
        {
            AddComponentData(new AutoFisherCD());
            AddComponentData(new RandomCD() { Value = PugRandom.GetRng() });
        }
    }
}
