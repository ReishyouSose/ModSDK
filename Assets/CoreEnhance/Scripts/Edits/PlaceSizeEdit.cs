using Unity.Entities;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Edits
{
    public static class PlaceSizeEdit
    {
        internal static void EditResizeableTool(Entity entity, GameObject authoring, EntityManager entityManager)
        {
            if (authoring.TryGetComponent<EntityMonoBehaviourData>(out var data)
                && data.ObjectInfo.objectType is ObjectType.Hoe or ObjectType.Shovel)
            {
                if (data.objectInfo.prefabTileSize.x == 1)
                    return;
                data.objectInfo.prefabTileSize = new(7, 7);
            }
        }
    }
}
