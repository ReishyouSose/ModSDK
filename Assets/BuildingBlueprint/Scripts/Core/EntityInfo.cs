using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts.Core
{
    [Serializable]
    public struct EntityInfo
    {
        public HashSet<EntityCD> Entities;
        public int2 Position;
    }
}
