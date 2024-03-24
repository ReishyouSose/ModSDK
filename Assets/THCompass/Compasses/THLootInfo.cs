using Assets.THCompass.Helper;
using System;
using System.Collections.Generic;
using static Assets.THCompass.Compasses.CompassData;
using static Assets.THCompass.System.CompassLootSystem;

namespace Assets.THCompass.Compasses
{
    public struct THLootTable
    {
        private Dictionary<LootType, List<THLootInfo>> pool;
        public static THLootTable CreateLootTable()
        {
            return new()
            {
                pool = new()
                    {
                        { LootType.Normal, new() {
                            { ObjectID.AncientGemstone, 3, 7, 1f },
                            { ObjectID.MechanicalPart, 3, 7, 1f }, }
                        },
                        { LootType.PetEgg, new() },
                        { LootType.Animal, new() },
                        { LootType.HealthFood, new() {
                            { ObjectID.GiantMushroom2, 1, 3, 0.1f },
                            { ObjectID.AmberLarva2, 1, 3, 0.1f }, }
                        },
                        { LootType.Summon, new() },
                        { LootType.Area, new() },
                        { LootType.Reinforcement, new() },
                        { LootType.Uniqueness, new() },
                    }
            };
        }
        public readonly void AddLootTable(LootType lootType, params THLootInfo[] infos)
        {
            pool[lootType].AddRange(infos);
        }
        public readonly void AddLootTable(LootType lootType, int min, int max, float dropChance, params ObjectID[] ids)
        {
            foreach (ObjectID id in ids)
            {
                if (id != ObjectID.None)
                {
                    pool[lootType].Add(id, min, max, dropChance);
                }
            }
        }
        public readonly void AddLootTable(LootType lootType, float dropChance, IEnumerable<ObjectID> ids, int min = 1, int max = 1)
        {
            foreach (ObjectID id in ids)
            {
                if (id != ObjectID.None)
                {
                    pool[lootType].Add(id, min, max, dropChance);
                }
            }
        }
        public readonly Dictionary<LootType, List<THLootInfo>> GetLootTable() => pool;
        public struct THLootInfo
        {
            public ObjectID itemType;
            public int minDrop;
            public int maxDrop;
            public float dropChance;
            public THLootInfo(ObjectID itemType, int minDrop, int maxDrop, float dropChance)
            {
                this.itemType = itemType;
                this.minDrop = minDrop;
                this.maxDrop = maxDrop;
                this.dropChance = dropChance;
            }
            public THLootInfo(ObjectID itemType, float dropChance)
            {
                this.itemType = itemType;
                minDrop = 1;
                maxDrop = 1;
                this.dropChance = dropChance;
            }
            public readonly int DropCount => RandomHelper.GetRngInt(minDrop, maxDrop);
        }
    }
}
