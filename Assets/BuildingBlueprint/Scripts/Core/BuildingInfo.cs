using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts.Core
{
    [Serializable]
    public class BuildingInfo
    {
        public string Name;
        public List<EntityInfo> EntityInfos;
        public List<SerializeTileInfo> TileInfos;
        public int2 Size;
    }
}
