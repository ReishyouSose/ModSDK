using Assets.BuildingBlueprint.Scripts.Components;
using Pug.UnityExtensions;
using PugTilemap;
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
        public static ObjectDataCD TileToObject(TileCD tile, in TileWithTilesetToObjectDataMapCD tileSetMap)
        {
            TileType type = tile.tileType;
            switch (tile.tileType)
            {
                case TileType.ground:
                    type = TileType.wall;
                    break;
                case TileType.roofHole:
                    return new()
                    {
                        objectID = ObjectID.RoofingTool,
                        variation = 0
                    };
                case TileType.water:
                    return new()
                    {
                        objectID = ObjectID.Bucket,
                        variation = tile.tileset + 1
                    };
                case TileType.dugUpGround:
                    return new()
                    {
                        objectID = ObjectID.WoodHoe
                    };
            }
            return PugDatabase.TryGetTileItemInfo(type, (Tileset)tile.tileset, tileSetMap);
        }
    }
}
