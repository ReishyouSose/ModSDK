using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts.Core
{
    public struct TilesInfo
    {
        public int2 Position;
        public Dictionary<TileCD, bool> Tiles;
    }
}
