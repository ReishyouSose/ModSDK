using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts
{
    public struct TileInfo
    {
        public Vector2Int Position;
        public Dictionary<TileCD, bool> Tiles;
    }

    [Serializable]
    public struct SerializeTileInfo
    {
        public Vector2Int Position;
        public List<TileCD> Tiles;
    }
}
