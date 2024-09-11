using Assets.THCompass.DataStruct;
using CoreLib.Drops;
using System.Collections.Generic;

namespace Assets.THCompass.Compasses
{
    public static class NonDedicateDrop
    {
        public const float Common = 0.4f;
        public const float Egg = 0.3f;
        public const float Animal = 0.15f;
        public const float Health = 0.04f;
        public const float Biome = 0.04f;
        public const float Boss = 0.04f;
        public const float Boulder = 0.01f;
        public const float Portal = 0.01f;
        public const float Unique = 0.01f;
        public static LootTableID AddNewDrop(this LootTableID lt, ObjectID id, int min, int max,
            float weight, bool mustDrop = false)
        {
            DropTablesModule.AddNewDrop(lt, new(id, min, max, weight, mustDrop));
            return lt;
        }
        public static void AddNewDropRange(this LootTableID lt, int min, int max,
            float weight, bool mustDrop = false, params ObjectID[] ids)
        {
            foreach (var id in ids)
            {
                lt.AddNewDrop(id, min, max, weight, mustDrop);
            }
        }
        public static LootTableID AddCommon(this LootTableID lt)
        {
            lt.AddNewDropRange(3, 7, Common, true, ObjectID.AncientGemstone, ObjectID.MechanicalPart);
            return lt;
        }
        public static LootTableID AddPetEggs(this LootTableID lt)
        {
            lt.AddNewDropRange(1, 3, Egg, false, ObjectID.PetBirdEgg, ObjectID.PetBunnyEgg,
                ObjectID.PetCatEgg, ObjectID.PetDogEgg, ObjectID.PetMothEgg, ObjectID.PetTardigradeEgg);
            return lt;
        }
        public static LootTableID AddAnimals(this LootTableID lt)
        {
            lt.AddNewDropRange(1, 3, Animal, false, ObjectID.Wool, ObjectID.RolyPolyPlate,
                ObjectID.Meat, ObjectID.Milk, ObjectID.KelpDumpling, ObjectID.Egg);
            return lt;
        }
        public static LootTableID AddHealthFood(this LootTableID lt)
        {
            List<ObjectID> result = new();
            if (CheckBoss(lt, BossID.PoisonSlime, BossID.Bird, BossID.HydraNature))
            {
                result.Add(ObjectID.FruitBasket);
            }
            if (CheckBoss(lt, BossID.LavaSlime, BossID.Scarab, BossID.HydraDesert))
            {
                result.Add(ObjectID.LiquidMetal);
            }
            lt.AddNewDropRange(1, 1, Health, false, result.ToArray());
            return lt;
        }
        public static LootTableID AddBoulder(this LootTableID lt)
        {
            List<ObjectID> boulder = new();
            if (lt.CheckBoss(BossID.Slime, BossID.Hive))
            {
                boulder.Add(ObjectID.CopperOreBoulder);
                boulder.Add(ObjectID.TinOreBoulder);
            }
            else if (lt.CheckBoss(BossID.Devourer, BossID.Shaman))
            {
                boulder.Add(ObjectID.IronOreBoulder);
                boulder.Add(ObjectID.GoldOreBoulder);
            }
            else if (lt.CheckBoss(BossID.PoisonSlime, BossID.Bird))
            {
                boulder.Add(ObjectID.ScarletOreBoulder);
            }
            else if (lt.CheckBoss(BossID.SlipperySlime, BossID.Octopus))
            {
                boulder.Add(ObjectID.OctarineOreBoulder);
            }
            else if (lt.CheckBoss(BossID.LavaSlime, BossID.Scarab))
            {
                boulder.Add(ObjectID.GalaxiteOreBoulder);
            }
            else if (lt.CheckBoss(BossID.Atlantis))
            {
                boulder.Add(ObjectID.SolariteOreBoulder);
            }
            else if (lt.CheckBoss(BossID.HydraNature))
            {
                boulder.Add(ObjectID.ScarletOreBoulder);
                boulder.Add(ObjectID.SolariteOreBoulder);
            }
            else if (lt.CheckBoss(BossID.HydraSea))
            {
                boulder.Add(ObjectID.OctarineOreBoulder);
                boulder.Add(ObjectID.SolariteOreBoulder);
            }
            else if (lt.CheckBoss(BossID.HydraDesert))
            {
                boulder.Add(ObjectID.GalaxiteOreBoulder);
                boulder.Add(ObjectID.SolariteOreBoulder);
            }
            else if (lt.CheckBoss(BossID.WallSlime))
            {
                boulder.Add(ObjectID.PandoriumOreBoulder);
            }
            else if (lt.CheckBoss(BossID.CoreCommander))
            {
                boulder.AddRange(new ObjectID[]
                {ObjectID.CopperOreBoulder, ObjectID.TinOreBoulder, ObjectID.IronOreBoulder,
                ObjectID.GoldOreBoulder, ObjectID.ScarletOreBoulder, ObjectID.OctarineOreBoulder,
                ObjectID.GalaxiteOreBoulder, ObjectID.SolariteOreBoulder, ObjectID.PandoriumOreBoulder});
            }
            lt.AddNewDropRange(1, 1, Boulder, false, boulder.ToArray());
            return lt;
        }
        public static LootTableID AddPortal(this LootTableID lt)
        {
            lt.AddNewDrop(ObjectID.WayPoint, 1, 1, Portal, false);
            return lt;
        }
        public static LootTableID AddNonDedicateDrop(this LootTableID lt)
        {
            lt.AddCommon().AddPetEggs().AddAnimals().AddHealthFood().AddBoulder().AddPortal();
            return lt;
        }
        public static bool CheckBoss(this LootTableID lt, params BossID[] bossIDs)
        {
            var ltID = CompassLoader.CompassLootByID;
            foreach (var bossID in bossIDs)
            {
                if (ltID.TryGetValue(bossID, out var bossLT) && bossLT == lt)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
