using Unity.Entities;
using Unity.NetCode;

namespace Assets.BuildingBlueprint.Scripts.Components
{
    [GhostComponent]
    public struct ItemInteractBlockStateCD : IComponentData
    {
        [GhostField]
        public bool State;
    }
}
