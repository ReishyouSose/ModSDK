using Assets.THCompass.Helper;
using System.Collections.Generic;
using System.Linq;
using static Assets.THCompass.Compasses.CompassData;
using static Assets.THCompass.Compasses.THLootTable;
using static Assets.THCompass.System.CompassLootSystem;

namespace Assets.THCompass.Compasses
{
    public static class CompassLootTable
    {
        public static ObjectID[] PetEggs = new ObjectID[]
        {
            ObjectID.PetBirdEgg,
            ObjectID.PetBunnyEgg,
            ObjectID.PetCatEgg,
            ObjectID.PetDogEgg,
        };
        public static ObjectID[] AnimalsUseful = new ObjectID[]
        {
            /*ObjectID.CritterBeetle,
            ObjectID.CritterCentipede,
            ObjectID.CritterCockroach,
            ObjectID.CritterCrab,
            ObjectID.CritterCrab2,
            ObjectID.CritterGrasshopper,
            ObjectID.CritterLarva,
            ObjectID.CritterNewt,
            ObjectID.CritterScorpion,
            ObjectID.CritterTinySnail,
            ObjectID.CritterWorm,*/
            ObjectID.Wool,
            ObjectID.RolyPolyPlate,
        };
        public static ObjectID[] AnimalsUseless = new ObjectID[]
        {
            ObjectID.Meat,
            ObjectID.Milk,
            ObjectID.KelpDumpling,
        };

        public static ObjectID GetSummoner(BossID bossID)
        {
            return bossID switch
            {
                BossID.Slime or BossID.PoisonSlime or BossID.SlipperySlime
                    or BossID.LavaSlime => ObjectID.SlimeBossSummoningItem,
                BossID.Hive => ObjectID.HiveBossSummoningItem,
                BossID.Devourer => ObjectID.LarvaBossSummoningItem,
                BossID.Shaman => ObjectID.ShamanBossSummoningItem,
                BossID.Bird => ObjectID.LargeShinyGlimmeringObject,
                BossID.Octopus => ObjectID.BaitOctopusBoss,
                BossID.Scarab => ObjectID.Thumper,
                _ => ObjectID.None,
            };
        }
        public enum EquipID
        {
            Pot,// ÌÕ¹øÌ×
            Ranger,// ÓÎÏÀÌ×
            //Tamer,// Âù»ÄÌ×
            Peasant,// Å©ÃñÌ×
            Caveling,// Ñ¨¾ÓÈËÌ×
            Golem,// ÉÚ±øÌ×
            Larva,// Ó×³æÌ×
            Hunter,// ÁÔÈËÌ×
            Mercenary,// ¹ÍÓ¶±øÌ×
            Moldweb,// Ã¹¾úÌ×
            Scholar,// Ñ§ÕßÌ×
            Assassin,// ´Ì¿ÍÌ×
            Paladin,// Ê¥ÆïÌ×
            Ninja,// ÈÌÕßÌ×
            Gemstone,// ±¦Ê¯Ì×
            Cosmos,// ÓîÖæÌ×
            Fish,// µöÓãÌ×
            Mine,// ÍÚ¿óÌ×
            Swimsuit,// Ó¾×°
        }
        public static IEnumerable<ObjectID> GetAreaEquips(EquipID armorID)
        {
            switch (armorID)
            {
                case EquipID.Pot:
                    yield return ObjectID.PotHelm;
                    yield return ObjectID.PotBreastArmor;
                    break;// ÌÕ¹øÌ×
                case EquipID.Ranger:
                    yield return ObjectID.RangerHelm;
                    yield return ObjectID.RangerBreastArmor;
                    yield return ObjectID.RangerPantsArmor;
                    break;// ÓÎÏÀÌ×
                /*case EquipID.Tamer:
                    yield return ObjectID.TamerHelm;
                    yield return ObjectID.TamerBreastArmor;
                    yield return ObjectID.TamerPantsArmor;
                    yield return ObjectID.TamerNecklace;
                    yield return ObjectID.TamerRing;
                    break;// Âù»ÄÌ×*/
                case EquipID.Peasant:
                    yield return ObjectID.PeasantHelm;
                    yield return ObjectID.PeasantBreastArmor;
                    break;// Å©ÃñÌ×
                case EquipID.Caveling:
                    yield return ObjectID.CavelingHelm;
                    yield return ObjectID.CavelingBreastArmor;
                    yield return ObjectID.CavelingPantsArmor;
                    break;// Ñ¨¾ÓÈËÌ×
                case EquipID.Golem:
                    yield return ObjectID.GolemHelm;
                    yield return ObjectID.GolemBreastArmor;
                    yield return ObjectID.GolemPantsArmor;
                    yield return ObjectID.GolemShield;
                    break;// ÉÚ±øÌ×
                case EquipID.Larva:
                    yield return ObjectID.LarvaHelm;
                    yield return ObjectID.LarvaBreastArmor;
                    break;// Ó×³æÌ×
                case EquipID.Hunter:
                    yield return ObjectID.HunterHelm;
                    yield return ObjectID.HunterBreastArmor;
                    break;// ÁÔÈËÌ×
                case EquipID.Mercenary:
                    yield return ObjectID.RamboHelm;
                    yield return ObjectID.RamboBreastArmor;
                    yield return ObjectID.RamboPantsArmor;
                    break;// ¹ÍÓ¶±øÌ×
                case EquipID.Moldweb:
                    yield return ObjectID.MoldwebHelm;
                    yield return ObjectID.MoldwebBreastArmor;
                    yield return ObjectID.MoldwebPantsArmor;
                    yield return ObjectID.MoldVeinNecklace;
                    yield return ObjectID.MoldRing;
                    break;// Ã¹¾úÌ×
                case EquipID.Scholar:
                    yield return ObjectID.ScholarHelm;
                    yield return ObjectID.ScholarArmor;
                    yield return ObjectID.ScholarStaff;
                    break;// Ñ§ÕßÌ×
                case EquipID.Assassin:
                    yield return ObjectID.AssassinHelm;
                    yield return ObjectID.AssassinBreastArmor;
                    break;// ´Ì¿ÍÌ×
                case EquipID.Paladin:
                    yield return ObjectID.DesertHelm;
                    yield return ObjectID.DesertBreastArmor;
                    yield return ObjectID.DesertPantsArmor;
                    break;// Ê¥ÆïÌ×
                case EquipID.Ninja:
                    yield return ObjectID.NinjaHelm;
                    yield return ObjectID.NinjaBreastArmor;
                    yield return ObjectID.NinjaPantsArmor;
                    break;// ÈÌÕßÌ×
                case EquipID.Gemstone:
                    yield return ObjectID.AncientGemstoneBreastArmor;
                    yield return ObjectID.AncientGemstonePantsArmor;
                    break;// ±¦Ê¯Ì×
                case EquipID.Cosmos:
                    yield return ObjectID.AlienTechHelm;
                    yield return ObjectID.AlienTechBreastArmor;
                    yield return ObjectID.AlienTechPantsArmor;
                    break;// ÓîÖæÌ×
                case EquipID.Fish:
                    yield return ObjectID.DiverHelm;
                    yield return ObjectID.DiverArmor;
                    yield return ObjectID.DiverPantsArmor;
                    yield return ObjectID.DiverNecklace;
                    yield return ObjectID.DiverRing;
                    break;// µöÓãÌ×
                case EquipID.Mine:
                    yield return ObjectID.MinerHelm;
                    yield return ObjectID.MinerBreastArmor;
                    yield return ObjectID.MinerPantsArmor;
                    yield return ObjectID.BlackNecklace;
                    yield return ObjectID.BlackRing;
                    break;// ÍÚ¿óÌ×
                case EquipID.Swimsuit:
                    yield return ObjectID.SunglassesHelm;
                    yield return ObjectID.TowelBreastArmor;
                    /*yield return ObjectID.TowerShellNecklace;
                    yield return ObjectID.TowerShell;*/
                    yield return ObjectID.BikiniBreastArmorRed;
                    yield return ObjectID.BikiniBreastArmorGreen;
                    yield return ObjectID.BikiniBreastArmorBlue;
                    yield return ObjectID.BikiniPantsArmorRed;
                    yield return ObjectID.BikiniPantsArmorGreen;
                    yield return ObjectID.BikiniPantsArmorBlue;
                    yield return ObjectID.SwimingPantsArmorRed;
                    yield return ObjectID.SwimingPantsArmorGreen;
                    yield return ObjectID.SwimingPantsArmorBlue;
                    break;// Ó¾×°
            }
            yield return ObjectID.None;
        }
        public static IEnumerable<ObjectID> GetArea(BossID bossID)
        {
            List<EquipID> equips = new();
            void Add(params EquipID[] ids) => equips.AddRange(ids);
            switch (bossID)
            {
                case BossID.Slime:
                    Add(EquipID.Pot, EquipID.Peasant);
                    yield return ObjectID.ExplorerHelm;
                    yield return ObjectID.MushroomHelm;
                    yield return ObjectID.OracleCardGem;
                    yield return ObjectID.HeartBerryNecklace;
                    break;
                case BossID.Hive:
                    Add(EquipID.Peasant, EquipID.Larva);
                    yield return ObjectID.ExplorerHelm;
                    yield return ObjectID.MushroomHelm;
                    yield return ObjectID.SkirmisherHelm;
                    yield return ObjectID.TinAxe;
                    yield return ObjectID.HuntingSpear;
                    yield return ObjectID.CavePouch;
                    yield return ObjectID.GrubEggNecklace;
                    yield return ObjectID.LarvaRing;
                    break;
                case BossID.Shaman:
                    Add(EquipID.Caveling, EquipID.Golem);
                    yield return ObjectID.TrenchcoatBreastArmor;
                    yield return ObjectID.BattleAxe;
                    yield return ObjectID.FireballStaff;
                    yield return ObjectID.RingOfRock;
                    break;
                case BossID.PoisonSlime:
                    Add(EquipID.Moldweb);
                    yield return ObjectID.PlagueDoctorHelm;
                    yield return ObjectID.IvyRing;
                    yield return ObjectID.RemedaisyNecklace;
                    yield return ObjectID.Blowpipe;
                    break;
                case BossID.Bird:
                    Add(EquipID.Hunter, EquipID.Mercenary);
                    yield return ObjectID.TrenchcoatBreastArmor;
                    yield return ObjectID.FarmerHelm;
                    yield return ObjectID.FlintlockMusket;
                    yield return ObjectID.PetalRing;
                    break;
                case BossID.SlipperySlime:
                case BossID.Octopus:
                    Add(EquipID.Scholar, EquipID.Fish, EquipID.Swimsuit);
                    yield return ObjectID.TentacleWhip;
                    yield return ObjectID.BubbleGun;
                    yield return ObjectID.TopazRing;
                    yield return ObjectID.SeptumRing;
                    yield return ObjectID.AncientGuardianRing;
                    yield return ObjectID.CavelingID;
                    yield return ObjectID.WhiteGlassRing;
                    yield return ObjectID.TorcNecklace;
                    yield return ObjectID.AnchorAxe;
                    yield return ObjectID.OracleCardLeader;
                    break;
                case BossID.LavaSlime:
                case BossID.Scarab:
                    Add(EquipID.Assassin, EquipID.Paladin, EquipID.Mine);
                    yield return ObjectID.ThrowingDaggers;
                    yield return ObjectID.ConceiledBlade;
                    yield return ObjectID.BombRing;
                    yield return ObjectID.ScarabMortar;
                    yield return ObjectID.FlameRing;
                    yield return ObjectID.FlameNecklace;
                    break;
                case BossID.Atlantis:
                    Add(EquipID.Ninja, EquipID.Cosmos, EquipID.Gemstone);
                    yield return ObjectID.NatureGemstone;
                    yield return ObjectID.SeaGemstone;
                    yield return ObjectID.DesertGemstone;
                    yield return ObjectID.LaserDrillTool;
                    yield return ObjectID.ShardClub;
                    yield return ObjectID.RazorFlake;
                    break;
            }
            yield return ObjectID.None;
        }
        public static ObjectID GetAreaChest(BossID bossID)
        {
            return bossID switch
            {
                BossID.Slime or BossID.Hive or BossID.Devourer => ObjectID.LockedCopperChest,
                BossID.Shaman => ObjectID.LockedIronChest,
                BossID.Bird or BossID.PoisonSlime => ObjectID.LockedScarletChest,
                BossID.Octopus or BossID.SlipperySlime => ObjectID.LockedOctarineChest,
                BossID.Scarab or BossID.LavaSlime => ObjectID.LockedGalaxiteChest,
                BossID.Atlantis => ObjectID.LockedSolariteChest,
                _ => ObjectID.None,
            };
        }
        public static IEnumerable<ObjectID> GetReinforcement(BossID bossID)
        {
            switch (bossID)
            {
                case BossID.Bird: yield return ObjectID.NatureGemstone; break;
                case BossID.Octopus: yield return ObjectID.SeaGemstone; break;
                case BossID.Scarab: yield return ObjectID.DesertGemstone; break;
            }
        }
        public static IEnumerable<ObjectID> GetUniqueness(BossID bossID)
        {
            switch (bossID)
            {
                case BossID.Slime:
                case BossID.Hive:
                case BossID.Devourer:
                    yield return ObjectID.ParsecPalsDolls;
                    yield return ObjectID.ColossCicada;
                    yield return ObjectID.AmmoniteNecklace;
                    break;
                case BossID.Shaman:
                    yield return ObjectID.LegendarySwordGemstone;
                    break;
                case BossID.PoisonSlime:
                    yield return ObjectID.CavelingDoll;
                    yield return ObjectID.LegendarySwordGemstone;
                    yield return ObjectID.LegendarySwordBlade;
                    yield return ObjectID.CavelingMothersRing;
                    yield return ObjectID.AncientGuardianNecklace;
                    yield return ObjectID.OldSporeMask;
                    yield return ObjectID.MoldCicadaWithoutSickle;
                    break;
                case BossID.Bird:
                    yield return ObjectID.CavelingDoll;
                    yield return ObjectID.LegendarySwordGemstone;
                    yield return ObjectID.LegendarySwordBlade;
                    yield return ObjectID.CavelingMothersRing;
                    yield return ObjectID.AncientGuardianNecklace;
                    yield return ObjectID.OldSporeMask;
                    yield return ObjectID.MoldCicadaWithoutSickle;
                    yield return ObjectID.CrystalCicada;
                    break;
                case BossID.SlipperySlime:
                case BossID.Octopus:
                    yield return ObjectID.LegendaryBowPart1;
                    yield return ObjectID.LegendaryBowPart2;
                    yield return ObjectID.LegendaryBowPart3;
                    yield return ObjectID.LegendaryBowParchment;
                    yield return ObjectID.ConchShellNecklace;
                    yield return ObjectID.SpineRing;
                    yield return ObjectID.OceanHeartNecklace;
                    yield return ObjectID.TurtleShell;
                    yield return ObjectID.TowerShellNecklace;
                    break;
                case BossID.LavaSlime:
                    yield return ObjectID.BindingString;
                    for (int i = 0; i < 8; i++) yield return ObjectID.CrystalMeteorShard;
                    yield return ObjectID.GodsentHelm;
                    yield return ObjectID.GodsentBreastArmor;
                    yield return ObjectID.GodsentPantsArmor;
                    yield return ObjectID.FrozenFlame;
                    yield return ObjectID.WhiteWhistle;
                    break;
                case BossID.Scarab:
                    yield return ObjectID.BindingString;
                    for (int i = 0; i < 8; i++) yield return ObjectID.CrystalMeteorShard;
                    yield return ObjectID.GodsentHelm;
                    yield return ObjectID.GodsentBreastArmor;
                    yield return ObjectID.GodsentPantsArmor;
                    yield return ObjectID.FrozenFlame;
                    yield return ObjectID.WhiteWhistle;
                    yield return ObjectID.AgarthaReport;
                    yield return ObjectID.CrystalTent;
                    break;
                case BossID.Atlantis:
                    yield return ObjectID.ConchShellNecklace;
                    yield return ObjectID.SpineRing;
                    yield return ObjectID.OceanHeartNecklace;
                    yield return ObjectID.TurtleShell;
                    yield return ObjectID.TowerShellNecklace;
                    yield return ObjectID.CrystalCicada;
                    yield return ObjectID.AgarthaReport;
                    yield return ObjectID.CrystalTent;
                    break;
            }
            yield return ObjectID.None;
        }
        public static List<THLootInfo> GetGrandPrize(LootType lootType, BossID bossID, in List<THLootInfo> origin, int mult)
        {
            if (!RollGrandPrize(lootType, mult))
            {
                return origin;
            }
            List<THLootInfo> result = new();
            switch (lootType)
            {
                case LootType.Normal:
                    break;
                case LootType.PetEgg:
                    result.Add(3, 5, 1f, PetEggs);
                    break;
                case LootType.Animal:
                    result.Add(10, 30, 1f, AnimalsUseful);
                    break;
                case LootType.HealthFood:
                    result.Add(10, 30, 1f, ObjectID.GiantMushroom2, ObjectID.AmberLarva2);
                    break;
                case LootType.Summon:
                    if (bossID.ToString().Contains("Slime"))
                    {
                        result.Add(ObjectID.KingSlimeSummoningItem, 1, 3);
                    }
                    result.Add(GetSummoner(bossID), 1, 3);
                    break;
                case LootType.Area:
                    result.Add(GetAreaChest(bossID), 3, 5);
                    break;
                case LootType.Reinforcement:
                    switch (bossID)
                    {
                        case BossID.Bird: result.Add(ObjectID.NatureGemstone, 10, 10); break;
                        case BossID.Octopus: result.Add(ObjectID.SeaGemstone, 10, 10); break;
                        case BossID.Scarab: result.Add(ObjectID.DesertGemstone, 10, 10); break;
                    }
                    break;
                case LootType.Uniqueness:
                    switch (bossID)
                    {
                        case BossID.PoisonSlime:
                        case BossID.Bird:
                            result.Add(ObjectID.LegendarySword);
                            break;
                        case BossID.SlipperySlime:
                        case BossID.Octopus:
                            result.Add(ObjectID.LegendaryBow);
                            break;
                        case BossID.LavaSlime:
                        case BossID.Scarab:
                            switch (RandomHelper.Rng.Next(4))
                            {
                                case 0: result.Add(ObjectID.LegendaryMiningPick); break;
                                case 1: result.Add(ObjectID.OracleDeck); break;
                                case 2: result.Add(ObjectID.CrystalMeteorShardOffhand); break;
                                case 3:
                                    result.Add(1, 1, 1f, ObjectID.GodsentHelm,
                                        ObjectID.GodsentBreastArmor, ObjectID.GodsentPantsArmor);
                                    break;
                            }
                            break;
                        case BossID.Atlantis:
                            break;
                    }
                    break;
            }
            return result.Any() ? result : origin;
        }
    }
}