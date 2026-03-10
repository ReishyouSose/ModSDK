using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Assets.PointShop.Scripts
{
    public class ShopInfo
    {
        internal static ShopInfo Ins { get; private set; }
        private readonly Dictionary<Zone, List<ObjectID>> Shops;
        private readonly Dictionary<Zone, ObjectID> boss;
        public ShopInfo()
        {
            Ins = this;
            Shops = new()
            {
                [Zone.Dirt] = Dirt(),
                [Zone.Clay] = Clay(),
                [Zone.LarvaHive] = LarvaHive(),
                [Zone.Stone] = Stone(),
                [Zone.Nature] = Nature(),
                [Zone.Mold] = Mold(),
                [Zone.Sea] = Sea(),
                [Zone.City] = City(),
                [Zone.Desert] = Desert(),
                [Zone.Lava] = Lava(),
                [Zone.Oasis] = Oasis(),
                [Zone.Crystal] = Crystal(),
                [Zone.Alien] = Alien(),
                [Zone.Passage] = Passage(),
                [Zone.Excavation] = Excavation()
            };
            boss = new()
            {
                [Zone.Dirt] = ObjectID.SlimeBoss,
                [Zone.Clay] = ObjectID.BossLarva,
                [Zone.LarvaHive] = ObjectID.LarvaHiveBoss,
                [Zone.Stone] = ObjectID.ShamanBoss,
                [Zone.Nature] = ObjectID.BirdBoss,
                [Zone.Mold] = ObjectID.PoisonSlimeBoss,
                [Zone.Sea] = ObjectID.OctopusBoss,
                [Zone.City] = ObjectID.SlipperySlimeBoss,
                [Zone.Desert] = ObjectID.ScarabBoss,
                [Zone.Lava] = ObjectID.LavaSlimeBoss,
                [Zone.Oasis] = ObjectID.GiantCicadaBoss,
                [Zone.Crystal] = ObjectID.HydraBossDesert,
                [Zone.Alien] = ObjectID.CoreBoss,
                [Zone.Passage] = ObjectID.WallBoss,
                [Zone.Excavation] = ObjectID.RobotBoss
            };
        }
        public bool TryGetShopItem(Zone zone, out List<ObjectID> shop)
        {
            return Shops.TryGetValue(zone, out shop);
        }
        public ObjectID GetBoss(Zone zone) => boss[zone];
        private List<ObjectID> Dirt()
        {
            return new()
            {
                ObjectID.ParsecPalsDolls,
                ObjectID.ColossCicada,
                ObjectID.AmmoniteNecklace
            };
        }
        private List<ObjectID> Clay()
        {
            return new()
            {
                ObjectID.ParsecPalsDolls,
                ObjectID.ColossCicada,
                ObjectID.AmmoniteNecklace
            };
        }
        private List<ObjectID> LarvaHive()
        {
            return new()
            {
                ObjectID.ParsecPalsDolls,
                ObjectID.ColossCicada,
                ObjectID.AmmoniteNecklace
            };
        }
        private List<ObjectID> Stone()
        {
            return new()
            {
                ObjectID.LegendarySwordGemstone
            };
        }
        private List<ObjectID> Nature()
        {
            return new()
            {
                ObjectID.CavelingDoll,
                ObjectID.LegendarySwordGemstone,
                ObjectID.LegendarySwordBlade,
                ObjectID.CavelingMothersRing,
                ObjectID.AncientGuardianNecklace,
                ObjectID.OldSporeMask,
                ObjectID.MoldCicadaWithoutSickle,
                ObjectID.CrystalCicada,
            };
        }
        private List<ObjectID> Mold()
        {
            return new()
            {
                ObjectID.LegendarySwordGemstone,
                ObjectID.CavelingDoll,
                ObjectID.LegendarySwordBlade,
                ObjectID.CavelingMothersRing,
                ObjectID.AncientGuardianNecklace,
                ObjectID.OldSporeMask,
                ObjectID.MoldCicadaWithoutSickle,
            };
        }
        private List<ObjectID> Sea()
        {
            return new()
            {
                 ObjectID.LegendaryBowPart1,
                 ObjectID.LegendaryBowPart2,
                 ObjectID.LegendaryBowPart3,
                 ObjectID.LegendaryBowParchment,
                 ObjectID.ConchShellNecklace,
                 ObjectID.SpineRing,
                 ObjectID.OceanHeartNecklace,
                 ObjectID.TurtleShell,
                 ObjectID.TowerShellNecklace
            };
        }
        private List<ObjectID> City()
        {
            return new()
            {
                 ObjectID.LegendaryBowPart1,
                 ObjectID.LegendaryBowPart2,
                 ObjectID.LegendaryBowPart3,
                 ObjectID.LegendaryBowParchment,
                 ObjectID.ConchShellNecklace,
                 ObjectID.SpineRing,
                 ObjectID.OceanHeartNecklace,
                 ObjectID.TurtleShell,
                 ObjectID.TowerShellNecklace,
            };
        }
        private List<ObjectID> Desert()
        {
            return new()
            {
                ObjectID.BindingString,
                ObjectID.CavelingProphetMask,
                ObjectID.GodsentHelm,
                ObjectID.GodsentBreastArmor,
                ObjectID.GodsentPantsArmor,
                ObjectID.FrozenFlame,
                ObjectID.WhiteWhistle,
                ObjectID.AgarthaReport,
                ObjectID.CrystalTent,
            };
        }
        private List<ObjectID> Lava()
        {
            return new()
            {
                ObjectID.BindingString,
                ObjectID.CavelingProphetMask,
                ObjectID.GodsentHelm,
                ObjectID.GodsentBreastArmor,
                ObjectID.GodsentPantsArmor,
                ObjectID.FrozenFlame,
                ObjectID.WhiteWhistle,
            };
        }
        private List<ObjectID> Crystal()
        {
            return new()
            {
                ObjectID.LegendarySwordGemstone,
                ObjectID.CavelingDoll,
                ObjectID.LegendarySwordBlade,
                ObjectID.CavelingMothersRing,
                ObjectID.AncientGuardianNecklace,
                ObjectID.OldSporeMask,
                ObjectID.MoldCicadaWithoutSickle,
                ObjectID.AgarthaReport,
                ObjectID.CrystalTent,
                ObjectID.CrystalCicada,
                ObjectID.AlienChest,
            };
        }
        private List<ObjectID> Alien()
        {
            return new()
            {
                ObjectID.AlienChest,
            };
        }

        private List<ObjectID> Oasis()
        {
            return new()
            {

            };
        }
        private List<ObjectID> Passage()
        {
            return new()
            {

            };
        }
        private List<ObjectID> Excavation()
        {
            return new()
            {

            };
        }
    }
}