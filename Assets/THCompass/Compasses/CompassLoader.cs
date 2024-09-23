using Assets.THCompass.DataStruct;
using Assets.THCompass.DropManager.Condition;
using Assets.THCompass.DropManager.Rule;
using Assets.THCompass.Helper;
using Assets.THCompass.System;
using CoreLib.Drops;
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Assets.THCompass.Compasses
{
    public static class CompassLoader
    {
        public static Dictionary<BossID, Compass> CompassByID;
        internal static Dictionary<BossID, LootTableID> CompassLootByID;
        internal static Dictionary<LootTableID, BossID> BossLootByID;
        internal static Dictionary<ObjectID, BossID> BossIDByObjID;
        private static Dictionary<LootType, List<DropRule>> commonLoot;
        private static Dictionary<LootType, List<DropRule>> grandLoot;
        private static HashSet<ObjectID> canStack;
        public static void Load()
        {
            commonLoot = new();
            grandLoot = new();
            canStack = new();
            BossIDByObjID = new()
            {
                {ObjectID.SlimeBoss, BossID.Slime},
                {ObjectID.LarvaHiveBoss, BossID.Hive},
                {ObjectID.BossLarva, BossID.Devourer},
                {ObjectID.ShamanBoss,BossID.Shaman},
                {ObjectID.PoisonSlimeBoss,BossID.PoisonSlime},
                {ObjectID.BirdBoss, BossID.Bird },
                {ObjectID.SlipperySlimeBoss, BossID.SlipperySlime},
                {ObjectID.OctopusBoss,BossID.Octopus },
                {ObjectID.LavaSlimeBoss,BossID.LavaSlime},
                {ObjectID.ScarabBoss, BossID.Scarab },
                {ObjectID.SnakeBossSegment, BossID.Atlantis },
                {ObjectID.HydraBossNature, BossID.HydraNature},
                {ObjectID.HydraBossSea, BossID.HydraSea},
                {ObjectID.HydraBossDesert, BossID.HydraDesert},
                {ObjectID.WallBoss, BossID.WallSlime},
                {ObjectID.CoreBoss, BossID.CoreCommander },
            };
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
                { BossID.HydraNature, new HydraNature( )},
                { BossID.HydraSea, new HydraSea( )},
                { BossID.HydraDesert, new HydraDesert( )},
                { BossID.WallSlime, new WallSlime( )},
                { BossID.CoreCommander, new CoreCommander( )},
            };
            BossLootByID = new()
            {
                { LootTableID.SlimeBoss, BossID.Slime },
                { LootTableID.HiveBoss, BossID.Hive },
                { LootTableID.Bosslarva, BossID.Devourer },
                { LootTableID.ShamanBoss, BossID.Shaman },
                { LootTableID.PoisonSlimeBoss, BossID.PoisonSlime },
                { LootTableID.BirdBoss, BossID.Bird },
                { LootTableID.SlipperySlimeBoss, BossID.SlipperySlime },
                { LootTableID.OctopusBoss, BossID.Octopus },
                { LootTableID.LavaSlimeBoss, BossID.LavaSlime },
                { LootTableID.ScarabBoss, BossID.Scarab },
                { LootTableID.AtlanteanWormBoss, BossID.Atlantis },
                { LootTableID.HydraBossNature, BossID.HydraNature },
                { LootTableID.HydraBossSea, BossID.HydraSea },
                { LootTableID.HydraBossDesert, BossID.HydraDesert },
                { LootTableID.WallBoss, BossID.WallSlime },
                { LootTableID.CoreCommander, BossID.CoreCommander }
            };

            RegisterLoot();
            //AddCompassLoot();
            /*foreach (LootType lt in Enum.GetValues(typeof(LootType)))
            {
                commonLoot[lt] = new();
                grandLoot[lt] = new();
            }
            AddNormal(commonLoot[LootType.Normal]);
            AddPetEgg(commonLoot[LootType.PetEgg], grandLoot[LootType.PetEgg]);
            AddAnimal(commonLoot[LootType.Animal], grandLoot[LootType.Animal]);
            AddHealthFood(commonLoot[LootType.HealthFood]);
            AddSummon(commonLoot[LootType.Summon], grandLoot[LootType.Summon]);
            AddAreaChest(commonLoot[LootType.Area], grandLoot[LootType.Area]);
            AddReinforcement(commonLoot[LootType.Reinforcement], grandLoot[LootType.Reinforcement]);
            AddLegend(grandLoot[LootType.Uniqueness]);
            foreach (Compass cps in CompassByID.Values)
            {
                ObjectID summoner = cps.BossSummoner;
                if (summoner > ObjectID.None)
                {
                    MatchBoss boss = new(cps.BossID);
                    commonLoot[LootType.Summon].Add(Drop.Common(summoner, 1, 1, 0.05f).WithCondition(boss));
                    grandLoot[LootType.Summon].Add(Drop.Common(summoner).WithCondition(boss));
                }
            }*/
        }
        private static void RegisterLoot()
        {
            CompassLootByID = new();
            const string PreName = "THCompass:Loot_";
            var config = THCompassMain.config;
            int min = config.MinRoll, max = config.MaxRoll;
            if (min <= 0)
                min = 1;
            if (max <= 0)
                max = 1;
            if (min > max)
                min = max;
            Debug.Log("minRoll: " + min);
            Debug.Log("maxRoll: " + max);
            foreach (var cps in CompassByID.Values)
            {
                LootTableID lt = DropTablesModule.AddLootTable(PreName + cps.BossID, min, max, false);
                CompassLootByID[cps.BossID] = lt;
            }
            foreach (var cps in CompassByID.Values)
            {
                LootTableID lt = DropTablesModule.GetLootTableID(PreName + cps.BossID);
                lt.AddNonDedicateDrop();
                ObjectID chest = cps.Area switch
                {
                    AreaType.Dirt => ObjectID.LockedCopperChest,
                    AreaType.Stone => ObjectID.LockedIronChest,
                    AreaType.Nature => ObjectID.LockedScarletChest,
                    AreaType.Sea => ObjectID.LockedOctarineChest,
                    AreaType.Desert => ObjectID.LockedGalaxiteChest,
                    AreaType.Shimmer => ObjectID.LockedSolariteChest,
                    AreaType.Passage => ObjectID.None,
                    _ => ObjectID.None
                };
                if (chest > ObjectID.None)
                {
                    lt.AddNewDrop(chest, 1, 3, NonDedicateDrop.Biome, false);
                }
                if (cps.BossSummoner > ObjectID.None)
                {
                    lt.AddNewDrop(cps.BossSummoner, 1, 2, NonDedicateDrop.Boss, false);
                }
                else if (cps.BelongsToSlime)
                {
                    lt.AddNewDrop(ObjectID.SlimeBossSummoningItem, 1, 2, NonDedicateDrop.Boss, false);
                }
                var uniques = cps.GetUniques();
                NonDedicateDrop.AddNewDropRange(lt, 1, 1, NonDedicateDrop.Unique, false, uniques);
            }
        }

        private static void AddCompassLoot()
        {
            return;
            static void Add(LootTableID lt, BossID id)
            {
                DropTableInfo info = new("THCompass:Compass_" + id, 1, 3, 0.1f, false);
                DropTablesModule.AddNewDrop(lt, info);
            }
            Add(LootTableID.SlimeBoss, BossID.Slime);
            Add(LootTableID.HiveBoss, BossID.Hive);
            Add(LootTableID.Bosslarva, BossID.Devourer);
            Add(LootTableID.ShamanBoss, BossID.Shaman);
            Add(LootTableID.PoisonSlimeBoss, BossID.PoisonSlime);
            Add(LootTableID.BirdBoss, BossID.Bird);
            Add(LootTableID.SlipperySlimeBoss, BossID.SlipperySlime);
            Add(LootTableID.ScarabBoss, BossID.Scarab);
            Add(LootTableID.LavaSlimeBoss, BossID.LavaSlime);
            Add(LootTableID.AtlanteanWormBoss, BossID.Atlantis);
            Add(LootTableID.HydraBossNature, BossID.HydraNature);
            Add(LootTableID.HydraBossSea, BossID.HydraSea);
            Add(LootTableID.HydraBossDesert, BossID.HydraDesert);
            Add(LootTableID.WallBoss, BossID.WallSlime);
            Add(LootTableID.CoreCommander, BossID.CoreCommander);
        }

        #region 上个版本的
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
            DropCondition slime = Drop.BelongsToSlime;
            common.Add(Drop.Common(ObjectID.SlimeBossSummoningItem, 1, 1, 5, 100).WithCondition(slime));
            grand.Add(Drop.Common(ObjectID.SlimeBossSummoningItem, 1, 3, 1).WithCondition(slime));
            grand.Add(Drop.Common(ObjectID.KingSlimeSummoningItem, 1, 3, 1).WithCondition(slime));
        }
        private static void AddAreaChest(List<DropRule> common, List<DropRule> grand)
        {
            void Add(MatchArea area, ObjectID lockedChest)
            {
                common.Add(Drop.Common(lockedChest, 1, 1, 0.1f).WithCondition(area));
                grand.Add(Drop.Common(lockedChest, 3, 7).WithCondition(area));
            }
            Add(Drop.BelongsToDirt, ObjectID.LockedCopperChest);
            Add(Drop.BelongsToStone, ObjectID.LockedIronChest);
            Add(Drop.BelongsToNature, ObjectID.LockedScarletChest);
            Add(Drop.BelongsToSea, ObjectID.LockedOctarineChest);
            Add(Drop.BelongsToDesert, ObjectID.LockedGalaxiteChest);
            Add(Drop.BelongsToShimmer, ObjectID.LockedSolariteChest);
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
        private static void AddLegend(List<DropRule> grand)
        {
            grand.Add(Drop.Common(ObjectID.LegendarySword).WithCondition(Drop.BelongsToNature));
            grand.Add(Drop.Common(ObjectID.LegendaryBow).WithCondition(Drop.BelongsToSea));
            SelectMany godsent = Drop.SelectMany(3, 3, 1, 1, 1, ObjectID.GodsentHelm,
                ObjectID.GodsentBreastArmor, ObjectID.GodsentPantsArmor);
            grand.Add(new OneOf(Drop.Common(ObjectID.LegendaryMiningPick), Drop.Common(ObjectID.OracleDeck),
                Drop.Common(ObjectID.CrystalMeteorShardOffhand), godsent)
                .WithCondition(Drop.BelongsToDesert));
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
                LootType.Area => 0.01f,
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
            Compass cps = CompassByID[rpc.bossID];
            foreach (LootType lt in Enum.GetValues(typeof(LootType)))
            {
                for (int i = 0; i < time; i++)
                {
                    DropSource source = new(cps, rpc.ten, i);
                    foreach (DropRule dr in commonLoot[lt])
                    {
                        result.AddRange(dr.Drop(source));
                    }
                    if (RollGrand(lt, PugRandom.GetRng(), bonus))
                    {
                        foreach (DropRule dr in grandLoot[lt])
                        {
                            result.AddRange(dr.Drop(source));
                        }
                    }
                    if (lt == LootType.Uniqueness)
                    {
                        foreach (DropRule dr in cps.loots)
                        {
                            result.AddRange(dr.Drop(source));
                        }
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
                else
                    combine[id] = stack;
            }
            NativeList<ObjectDataCD> items = new(Allocator.Temp);
            foreach (var (type, stack) in combine)
            {
                int amount = stack;
                while (amount > 9999)
                {
                    items.Add(ItemHelper.NewItem(type, 9999));
                    amount -= 9999;
                }
                if (!canStack.Contains(type))
                {
                    ObjectInfo objInfo = PugDatabase.GetObjectInfo(type);
                    if (objInfo?.isStackable == true)
                        canStack.Add(type);
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
        #endregion

    }
}
