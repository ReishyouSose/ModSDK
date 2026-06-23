using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts
{
    [Serializable]
    public class BuildingInfo
    {
        public string Name;
        public List<EntityInfo> EntityInfos;
        public List<SerializeTileInfo> TileInfos;
        public Vector2Int Size;
    }
}
