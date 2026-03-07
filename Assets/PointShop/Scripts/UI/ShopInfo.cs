using System.Collections.Generic;
using Unity.Entities;

namespace Assets.PointShop.Scripts
{
    public class ShopInfo
    {
        internal static ShopInfo Ins { get; private set; }
        private readonly Dictionary<Zone, List<ObjectID>> Shops;
        private readonly Dictionary<Zone, bool> allow;
        private EntityQuery query;
        private bool entityQueryLoaded;
        public ShopInfo()
        {
            Ins = this;
            Shops = new();
            allow = new();
            int max = (int)Zone.MAX;
            for (int i = 0; i < max; i++)
            {
                Zone zone = (Zone)i;
                allow[zone] = false;
            }
            Shops[Zone.Dirt] = Dirt();
            Shops[Zone.Clay] = Clay();
            Shops[Zone.LarvaHive] = LarvaHive();
            Shops[Zone.Stone] = Stone();
            Shops[Zone.Nature] = Nature();
            Shops[Zone.Mold] = Mold();
            Shops[Zone.Sea] = Sea();
            Shops[Zone.City] = City();
            Shops[Zone.Desert] = Desert();
            Shops[Zone.Lava] = Lava();
            Shops[Zone.Oasis] = Oasis();
            Shops[Zone.Crystal] = Crystal();
            Shops[Zone.Alien] = Alien();
            Shops[Zone.Passage] = Passage();
            Shops[Zone.Excavation] = Excavation();
        }
        public bool TryGetShopItem(Zone zone, out List<ObjectID> shop)
        {
            if (!Shops.TryGetValue(zone, out shop))
                return false;
            return allow.TryGetValue(zone, out bool defeat) && defeat;
        }

        public void CheckDefeat()
        {
            var player = Manager.main.player;
            if (player == null)
                return;
            if (!entityQueryLoaded)
            {
                query = Manager.ecs.GetClientEntityQuery(new ComponentType[] { typeof(BossDefeatedInfo) });
                entityQueryLoaded = true;
            }
            if (!query.TryGetSingleton(out BossDefeatedInfo info))
                return;
            if (info.Slime)
                allow[Zone.Dirt] = true;
            if (info.Devourer)
                allow[Zone.Clay] = true;
            if (info.LarvaHive)
                allow[Zone.LarvaHive] = true;
            if (info.Shaman)
                allow[Zone.Stone] = true;
            if (info.Bird)
                allow[Zone.Nature] = true;
            if (info.PoisonSlime)
                allow[Zone.Mold] = true;
            if (info.Octopus)
                allow[Zone.Sea] = true;
            if (info.SlipperySlime)
                allow[Zone.City] = true;
            if (info.Scarab)
                allow[Zone.Desert] = true;
            if (info.LavaSlime)
                allow[Zone.Lava] = true;
            if (info.HydraDesert)
                allow[Zone.Crystal] = true;
            if (info.CoreCommander)
                allow[Zone.Alien] = true;
            if (info.Cicada)
                allow[Zone.Oasis] = true;
            if (info.WallSlime)
                allow[Zone.Passage] = true;
            if (info.Robot)
                allow[Zone.Excavation] = true;
        }
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