using Assets.BuildingBlueprint.Scripts.Components;
using Pug.UnityExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts.Core
{
    public static class MiscHelper
    {
        public static Rect GetEntityRect(SelectedEntityBuffer info, BlobAssetReference<PugDatabase.PugDatabaseBank> database)
        {
            var offset = info.GetEntityOffset(database, out var size);
            Vector2 s = size.ToVec2Int();
            var origin = info.Position.ToFloat2() + offset - (float2)s / 2f;
            return new Rect(origin, s);
        }

        public static bool Contains(this Rect self, Rect other)
        {
            return self.xMin <= other.xMin &&
                   self.xMax >= other.xMax &&
                   self.yMin <= other.yMin &&
                   self.yMax >= other.yMax;
        }
    }
}
