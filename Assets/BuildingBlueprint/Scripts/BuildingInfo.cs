using Unity.Mathematics;

namespace Assets.BuildingBlueprint.Scripts
{
    public struct BuildingInfo
    {
        public ObjectID ObjectID;
        public int X;
        public int Y;
        public int Variation;
        public float3 Position;
        public DirectionCD Direction;
    }
}
