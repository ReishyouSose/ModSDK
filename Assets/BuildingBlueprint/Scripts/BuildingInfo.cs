using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts
{
    [Serializable]
    public class BuildingInfo
    {
        public string Name;
        public List<EntityInfo> EntityInfos;
        public List<TileInfo> TileInfos;
        public Vector2Int Size;
    }
}
