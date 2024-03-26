using Assets.THCompass.DataStruct;
using Assets.THCompass.DropManager.Condition;
using Assets.THCompass.DropManager.Rule;
using Assets.THCompass.Helper;
using Assets.THCompass.System;
using System;
using System.Collections.Generic;
using Unity.Collections;

namespace Assets.THCompass.Compasses
{
    public static class CompassLoader
    {
        public static Dictionary<BossID, Compass> CompassByID;
        private static Dictionary<LootType, List<DropRule>> commonLoot;
        private static Dictionary<LootType, List<DropRule>> grandLoot;
        private static HashSet<ObjectID> canStack;
        public static void Load()
        {
            commonLoot = new();
            grandLoot = new();
            canStack = new();
            CompassByID = new()
            {
                { BossID.Slime, new Slime() },
                { BossID.Hive, new Hive() },
                { BossID.Devourer, new Devourer() },
                { BossID.Shaman, new Shaman() },
                { BossID.PoisonSlime, new PoisonSlime() },
                { BossID.Bird, new Bird() },
                { BossID.SlipperySlime, new SlipperySlime() },
                { BossID.Octopus, new Octopus() },
                { BossID.LavaSlime, new LavaSlime() },
                { BossID.Scarab, new Scarab( )},
                { BossID.Atlantis, new Atlantis() },
            };
            foreach (LootType lt in Enum.GetValues(typeof(LootType)))
            {
                commonLoot[lt] = new();
                grandLoot[lt] = new();
            }
            AddNormal(commonLoot[LootType.Normal]);
            AddPetEgg(commonLoot[LootType.PetEgg], grandLoot[LootType.PetEgg]);
            AddAnimal(commonLoot[LootType.Animal], grandLoot[LootType.Animal]);
            AddHealthFood(commonLoot[LootType.HealthFood]);
            AddSummon(commonLoot[LootType.Summon], grandLoot[LootType.Summon]);
            AddReinforcement(commonLoot[LootType.Reinforcement], grandLoot[LootType.Reinforcement]);
            AddUnique(commonLoot[LootType.Uniqueness], grandLoot[LootType.Uniqueness]);
            DropCondition ten = new IsTenTimes();
            DropCondition notTen = new IsTenTimes().ReverseCondition();
            foreach (Compass cps in CompassByID.Values)
            {
                List<DropRule> common = new(), grand = new();
                cps.RegisterUniqueDrop(common, grand);
                MatchBoss cd = new(cps.BossID);
                commonLoot[LootType.Uniqueness].AddRange(common.WithCondition(cd, notTen));
                commonLoot[LootType.Uniqueness].AddRange(grand.WithCondition(cd, ten));
            }
        }
        private static void AddNormal(List<DropRule> loot)
        {
            loot.AddRange(Drop.CommonMany(3, 7, 1, ObjectID.AncientGemstone, ObjectID.MechanicalPart));
        }
        private static void AddPetEgg(List<DropRule> common, List<DropRule> grand)
        {
            ObjectID[] eggs = new ObjectID[4] {ObjectID.PetBirdEgg,
                ObjectID.PetBunnyEgg, ObjectID.PetCatEgg, ObjectID.PetDogEgg};
            common.AddRange(Drop.CommonMany(1, 3, 0.1f, eggs));
            grand.AddRange(Drop.CommonMany(3, 5, 1, eggs));
        }
        private static void AddAnimal(List<DropRule> common, List<DropRule> grand)
        {
            ObjectID[] animal = new ObjectID[5] { ObjectID.Wool, ObjectID.RolyPolyPlate,
                ObjectID.Meat, ObjectID.Milk, ObjectID.KelpDumpling };
            common.AddRange(Drop.CommonMany(1, 3, 0.1f, animal[..2]));
            common.AddRange(Drop.CommonMany(1, 3, 0.15f, animal[2..]));
            grand.AddRange(Drop.CommonMany(10, 30, 1, animal));
        }
        private static void AddHealthFood(List<DropRule> loot)
        {
            loot.AddRange(Drop.CommonMany(1, 3, 0.1f, ObjectID.GiantMushroom2, ObjectID.AmberLarva2));
        }
        private static void AddSummon(List<DropRule> common, List<DropRule> grand)
        {
            DropCondition slime = new BlongsToSlime();
            common.Add(Drop.Common(ObjectID.SlimeBossSummoningItem, 1, 1, 5, 100).WithCondition(slime));
            grand.Add(Drop.Common(ObjectID.SlimeBossSummoningItem, 1, 3, 1).WithCondition(slime));
            grand.Add(Drop.Common(ObjectID.KingSlimeSummoningItem, 1, 3, 1).WithCondition(slime));
        }
        private static void AddReinforcement(List<DropRule> common, List<DropRule> grand)
        {
            void Add(ObjectID gem, DropCondition boss)
            {
                common.Add(Drop.Common(gem, 1, 2, 0.1f).WithCondition(boss));
                grand.Add(Drop.Common(gem, 10, 10).WithCondition(boss));
            }
            Add(ObjectID.NatureGemstone, new MatchBoss(BossID.Bird));
            Add(ObjectID.SeaGemstone, new MatchBoss(BossID.Octopus));
            Add(ObjectID.DesertGemstone, new MatchBoss(BossID.Scarab));
        }
        private static void AddUnique(List<DropRule> common, List<DropRule> grand)
        {
            common.AddRange(Drop.CommonMany(1, 1, 0.01f, ObjectID.ParsecPalsDolls, ObjectID.ColossCicada,
                ObjectID.AmmoniteNecklace).WithCondition(new MatchArea(AreaType.Dirt)));
            grand.Add(Drop.Common(ObjectID.LegendarySword).WithCondition(new MatchArea(AreaType.Nature)));
            grand.Add(Drop.Common(ObjectID.LegendaryBow)
                .WithCondition(new MatchArea(AreaType.Sea), new MatchBoss(BossID.Atlantis).ReverseCondition()));
            SelectMany godsent = Drop.SelectMany(3, 3, 1, 1, 1, ObjectID.GodsentHelm,
                ObjectID.GodsentBreastArmor, ObjectID.GodsentPantsArmor);
            grand.Add(new OneOf(Drop.Common(ObjectID.LegendaryMiningPick), Drop.Common(ObjectID.OracleDeck),
                Drop.Common(ObjectID.CrystalMeteorShardOffhand), godsent)
                .WithCondition(new MatchArea(AreaType.Desert)));
        }
        private static bool RollGrand(LootType lootType, Unity.Mathematics.Random rng, int bonus)
        {
            return rng.NextBool((lootType switch
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
            }) * bonus);
        }
        public static NativeList<ObjectDataCD> GetLoots(CompassLootRPC rpc)
        {
            int time = 1, bonus = 1;
            if (rpc.ten)
            {
                time = 10;
                bonus = 2;
            }
            List<DropInfo> result = new();
            foreach (LootType lt in Enum.GetValues(typeof(LootType)))
            {
                for (int i = 0; i < time; i++)
                {
                    DropSource source = new(CompassByID[rpc.bossID], rpc.ten, i);
                    List<DropRule> rules = RollGrand(lt, PugRandom.GetRng(), bonus) ? grandLoot[lt] : commonLoot[lt];
                    foreach (DropRule rule in rules)
                    {
                        result.AddRange(rule.Drop(source));
                    }
                }
            }
            Dictionary<ObjectID, int> combine = new();
            foreach (DropInfo info in result)
            {
                ObjectID id = info.itemID;
                int stack = info.stack;
                if (combine.ContainsKey(id))
                {
                    combine[id] += stack;
                }
                else combine[id] = stack;
            }
            NativeList<ObjectDataCD> items = new(Allocator.Temp);
            foreach (var (type, stack) in combine)
            {
                int amount = stack;
                while (amount > 999)
                {
                    items.Add(ItemHelper.NewItem(type, 999));
                    amount -= 999;
                }
                if (!canStack.Contains(type))
                {
                    ObjectInfo objInfo = PugDatabase.GetObjectInfo(type);
                    if (objInfo != null && objInfo.isStackable) canStack.Add(type);
                }
                while (!canStack.Contains(type) && amount > 1)
                {
                    items.Add(ItemHelper.NewItem(type, 1));
                    amount--;
                }
                items.Add(ItemHelper.NewItem(type, amount));
            };
            return items;
        }
    }
}
