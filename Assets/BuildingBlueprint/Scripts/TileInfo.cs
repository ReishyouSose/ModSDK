using System.Collections.Generic;
using Unity.Mathematics;

namespace Assets.BuildingBlueprint.Scripts
{
    public struct TileInfo
    {
        public int2 Position;
        public Dictionary<TileCD, bool> Tiles;
    }
}
