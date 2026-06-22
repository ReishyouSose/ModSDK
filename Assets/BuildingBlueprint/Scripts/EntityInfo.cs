using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts
{
    [Serializable]
    public struct EntityInfo
    {
        public HashSet<EntityCD> Entities;
        public Vector3 Position;
    }

    [Serializable]
    public struct EntityCD
    {
        public ObjectID ObjectID;
        public int X;
        public int Y;
        public int Variation;
        public Vector3 Position;
        public Vector3 Direction;
    }
}
