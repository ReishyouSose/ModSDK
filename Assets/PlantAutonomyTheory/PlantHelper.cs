using PugProperties;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;

namespace Assets.PlantAutonomyTheory
{
    public static class PlantHelper
    {
        public static bool FullyGrown(this GrowingCD grow, Entity entity, ComponentLookup<ObjectPropertiesCD> lookup)
            => lookup.TryGetComponent(entity, out var properties) && grow.HasFinishedGrowing(properties);

        public static bool ArableWet(this TileAccessor tileAccessor, LocalTransform local)
            => tileAccessor.GetTop(local.Position.RoundToInt2()).tileType == PugTilemap.TileType.wateredGround;

        public static bool GoldenPlant(this PlantCD plant, in ObjectDataCD objData)
            => plant.objectToDropWhenHarvested != objData.objectID + 1;
    }
}
