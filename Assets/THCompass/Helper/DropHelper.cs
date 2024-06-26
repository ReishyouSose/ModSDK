﻿using Assets.THCompass.DataStruct;
using Assets.THCompass.DropManager.Condition;
using Assets.THCompass.DropManager.Rule;
using System.Collections.Generic;
using System.Linq;

namespace Assets.THCompass.Helper
{
    public static class Drop
    {
        public static IsTenTimes IsTenTime = new();
        public static IsTenTimes NotTenTime = new IsTenTimes().ReverseCondition() as IsTenTimes;
        public static MatchArea BelongsToDirt = new(AreaType.Dirt);
        public static MatchArea BelongsToStone = new(AreaType.Stone);
        public static MatchArea BelongsToNature = new(AreaType.Nature);
        public static MatchArea BelongsToSea = new(AreaType.Sea);
        public static MatchArea BelongsToDesert = new(AreaType.Desert);
        public static MatchArea BelongsToShimmer = new(AreaType.Shimmer);
        public static BlongsToSlime BelongsToSlime = new();
        public static Common Common(ObjectID itemID, int min, int max, float x)
        {
            Common dr = new(itemID)
            {
                minDrop = min,
                maxDrop = max,
            };
            dr.SetDropChance(x);
            return dr;
        }
        public static Common Common(ObjectID itemID, int min, int max, float x, int y) => Common(itemID, min, max, x / y);
        public static Common Common(ObjectID itemID, int min = 1, int max = 1, int x = 1) => Common(itemID, min, max, 1, x);
        public static IEnumerable<Common> CommonMany(int min, int max, float x, params ObjectID[] ids)
        {
            foreach (ObjectID itemID in ids)
            {
                yield return Common(itemID, min, max, x);
            }
        }
        public static OneOf OneOf(int min, int max, float x, params ObjectID[] ids)
        {
            OneOf dr = new(ids)
            {
                minDrop = min,
                maxDrop = max,
            };
            dr.SetDropChance(x);
            return dr;
        }
        public static OneOf OneOf(int min, int max, float x, int y, params ObjectID[] ids) => OneOf(min, max, x / y, ids);
        public static OneOf OneOf(int min, int max, int x, params ObjectID[] ids) => OneOf(min, max, 1, x, ids);
        public static OneOf OneOf(params ObjectID[] ids) => OneOf(1, 1, 1, ids);
        public static SelectMany SelectMany(int selectMin, int selectMax, int min, int max, float x, params ObjectID[] ids)
        {
            SelectMany dr = new(CommonMany(1,1,1,ids).ToArray())
            {
                minDrop = min,
                maxDrop = max,
                selectMin = selectMin,
                selectMax = selectMax,
            };
            dr.SetDropChance(x);
            return dr;
        }
        public static SelectMany SelectMany(int selectMin, int selectMax, int min, int max, float x, int y,
            params ObjectID[] ids) => SelectMany(selectMin, selectMax, min, max, x / y, ids);
        public static SelectMany SelectMany(int selectMin = 1, int selectMax = 1, int min = 1, int max = 1, int x = 1,
            params ObjectID[] ids) => SelectMany(selectMin, selectMax, min, max, 1, x, ids);
        public static IEnumerable<DropRule> WithCondition(this IEnumerable<DropRule> origin, params DropCondition[] cds)
        {
            foreach (DropRule rule in origin)
            {
                rule.WithCondition(cds);
            }
            return origin;
        }
        public static void AddUniqueRange(this List<DropRule> loots, params ObjectID[] ids)
        {
            loots.AddRange(CommonMany(1, 1, 0.01f, ids).WithCondition(NotTenTime));
            loots.Add(OneOf(ids).WithCondition(IsTenTime));
        }
    }
}
