using Assets.BuildingBlueprint.Scripts.Core;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.BuildingBlueprint.Scripts.Components
{
    public struct BlueprintStateChangeRpc : IRpcCommand
    {
        public BlueprintUIAction Action;
        public int Value;
        public Entity Player;
    }
}
