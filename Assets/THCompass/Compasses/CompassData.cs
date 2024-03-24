using Assets.THCompass.Helper;
using System.Collections.Generic;

namespace Assets.THCompass.Compasses
{
    public static class CompassData
    {
        public enum BossID
        {
            None,
            Slime,
            Hive,
            Devourer,
            Shaman,
            PoisonSlime,
            Bird,
            SlipperySlime,
            Octopus,
            LavaSlime,
            Scarab,
            Atlantis,
        }
        public static Dictionary<LootTableID, BossID> bossList = new()
        {
            {LootTableID.SlimeBoss, BossID.Slime},
            {LootTableID.HiveBoss, BossID.Hive},
            {LootTableID.Bosslarva, BossID.Devourer},
            {LootTableID.ShamanBoss, BossID.Shaman},
            {LootTableID.PoisonSlimeBoss, BossID.PoisonSlime},
            {LootTableID.BirdBoss, BossID.Bird},
            {LootTableID.SlipperySlimeBoss, BossID.SlipperySlime},
            {LootTableID.OctopusBoss, BossID.Octopus},
            {LootTableID.LavaSlimeBoss, BossID.LavaSlime},
            {LootTableID.ScarabBoss, BossID.Scarab},
            {LootTableID.AtlanteanWormBoss, BossID.Atlantis}
        };
        internal static Dictionary<BossID, ObjectID> chest = new()
        {
            {BossID.Slime, ObjectID.CopperChest},
            {BossID.Hive, ObjectID.CopperChest},
            {BossID.Devourer, ObjectID.CopperChest},
            {BossID.Shaman, ObjectID.IronChest},
            {BossID.PoisonSlime, ObjectID.ScarletChest},
            {BossID.Bird, ObjectID.ScarletChest},
            {BossID.SlipperySlime, ObjectID.OctarineChest},
            {BossID.Octopus, ObjectID.OctarineChest},
            {BossID.LavaSlime, ObjectID.GalaxiteChest},
            {BossID.Scarab, ObjectID.GalaxiteChest},
            {BossID.Atlantis, ObjectID.SolariteChest},
        };

        internal static Dictionary<BossID, ObjectID> bossChest = new()
        {
            {BossID.Slime, ObjectID.GlurchChest},
            {BossID.Hive, ObjectID.HivemotherChest},
            {BossID.Devourer, ObjectID.GhormChest},
            {BossID.Shaman, ObjectID.BossChest},
            {BossID.PoisonSlime, ObjectID.IvyChest},
            {BossID.Bird, ObjectID.EasterChest},
            {BossID.SlipperySlime, ObjectID.MorphaChest},
            {BossID.Octopus, ObjectID.OctopusBossChest},
            {BossID.LavaSlime, ObjectID.LavaSlimeBossChest},
            {BossID.Scarab, ObjectID.BossChest},
            {BossID.Atlantis, ObjectID.AtlantianWormChest},
        };

        internal static Dictionary<ObjectID, BossID> bossCps = new()
        {
            { ObjectID.GlurchChest, BossID.Slime },
            { ObjectID.HivemotherHalloweenChest, BossID.Hive },
            { ObjectID.HivemotherChest, BossID.Hive },
            { ObjectID.GhormChest, BossID.Devourer },
            { ObjectID.InventoryAncientChest, BossID.Shaman },
            { ObjectID.IvyChest, BossID.PoisonSlime },
            { ObjectID.EasterChest, BossID.Bird },
            { ObjectID.MorphaChest, BossID.SlipperySlime },
            { ObjectID.OctopusBossChest, BossID.Octopus },
            { ObjectID.LavaSlimeBossChest, BossID.LavaSlime },
            { ObjectID.BossChest, BossID.Scarab },
            { ObjectID.AtlantianWormChest, BossID.Atlantis },
        };

        public enum LootType
        {
            Normal,
            PetEgg,// 宠物蛋
            Animal,// 动物素材
            HealthFood,// 血上限素材
            Summon,// Boss召唤物
            Area,// 区域限定
            Reinforcement,// 强化石
            Uniqueness,// 区域唯一性素材
        }
        public static bool RollGrandPrize(LootType lootType, int mult)
        {
            return ((lootType switch
            {
                LootType.Normal => 0,
                LootType.PetEgg => 0.005f,
                LootType.Animal => 0.005f,
                LootType.HealthFood => 0,
                LootType.Summon => 0.01f,
                LootType.Area => 0f,
                LootType.Reinforcement => 0.001f,
                LootType.Uniqueness => 0.0005f,
                _ => 0
            }) * mult).GetRngBool();
        }
    }
}
