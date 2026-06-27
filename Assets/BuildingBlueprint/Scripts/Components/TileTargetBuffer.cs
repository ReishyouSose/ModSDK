using PugTilemap;
using Unity.Entities;

namespace Assets.BuildingBlueprint.Scripts.Components
{
    public struct TileTargetBuffer : IBufferElementData
    {
        public TileType TileType;
    }
}
