using Unity.Entities;
using Unity.NetCode;

namespace Assets.BuildingBlueprint.Scripts.Components
{
    public struct TileTargetStateBuffer : IBufferElementData
    {
        [GhostField]
        public bool State;
    }
}
