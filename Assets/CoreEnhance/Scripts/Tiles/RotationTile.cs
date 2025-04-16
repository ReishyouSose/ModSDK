using Pug.ECS.Hybrid;
using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Tiles
{
    [RequireComponent(typeof(EntityMonoBehaviour))]
    public class RotationTile : MonoBehaviour, IGraphicalSpawn
    {
        public Action<float3, int> ExtraRotEvent;
        public void Spawn(Entity entity, EntityManager manager)
        {
            float3 dir = float3.zero;
            if (manager.HasComponent<DirectionCD>(entity))
            {
                dir = manager.GetComponentData<DirectionCD>(entity).direction;
            }
            else if (manager.HasComponent<DirectionBasedOnVariationCD>(entity))
            {
                dir = manager.GetComponentData<DirectionBasedOnVariationCD>(entity).direction.ToFloat3();
            }

            if (dir.Equals(float3.zero))
            {
                Debug.Log("No DirectionCD or BasedOnVariCD");
                return;
            }

            int variation = DirectionBasedOnVariationCD.GetVariationFromDirection(dir.RoundToInt2());
            int index = (variation + 2) % 4;
            foreach (var sprite in GetComponent<EntityMonoBehaviour>().spriteObjects)
            {
                sprite.ApplyVisualChange();
                sprite.SetVariantByIndex(index);
            }
            ExtraRotEvent?.Invoke(dir, index);
        }
    }
}
