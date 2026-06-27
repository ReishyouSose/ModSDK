using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts.Core
{
    [Serializable]
    public struct SerializeTileInfo
    {
        public int2 Position;
        public List<TileCD> Tiles;
    }
}
