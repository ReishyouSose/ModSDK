using PugTilemap;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

namespace Assets.THCompass.Helper
{
    public static class TileHelper
    {
        public static int2 ExpandingSpiral(int count, out int dir)
        {
            int2 pos = new(0, 1);
            int dis = 1;
            dir = 0;
            while (count > 0)
            {
                switch (dir)
                {
                    case 0: if (pos.x + 1 > dis) dir++; break;
                    case 1: if (pos.y - 1 < -dis) dir++; ; break;
                    case 2: if (pos.x - 1 < -dis) dir++; break;
                }
                switch (dir)
                {
                    case 0: pos.x++; break;
                    case 1: pos.y--; break;
                    case 2: pos.x--; break;
                    case 3:
                        pos.y++;
                        if (pos.y > dis)
                        {
                            dis++;
                            dir = 0;
                        }
                        break;
                }
                count--;
            }
            return pos;
        }

        public static bool GroundCanPlace(float3 worldPos, bool allowWater)
        {
            int2 pos = EntityMonoBehaviour.ToRenderFromWorld(worldPos.RoundToInt2());
            TileType tileType = Manager.multiMap.GetTileLayerLookup()
                .GetTopTileAndCheckWater(pos, out bool hasWater).tileType;
            if (allowWater && hasWater) return true;
            return tileType is TileType.ground or TileType.bridge or TileType.rug or
                TileType.groundSlime or TileType.dugUpGround or TileType.floor;
        }
        public static bool HasEntity(float3 pos, EntityManager manager, out NativeList<ColliderCastHit> results)
        {
            CollisionWorld collisionWorld = PhysicsManager.GetCollisionWorld();
            PhysicsManager physicsManager = Manager.physics;
            results = new(Allocator.Temp);
            PhysicsCollider collider = physicsManager.GetBoxCollider(new(0, 0.5f, 0), new(0.1f, 0.2f, 0.1f), 0xffffffff);
            ColliderCastInput input = PhysicsManager.GetColliderCastInput(pos, pos, collider);
            if (collisionWorld.CastCollider(input, ref results))
            {
                foreach (ColliderCastHit hit in results)
                {
                    Entity entity = hit.Entity;
                    if (manager.HasComponent<HealthCD>(entity) || manager.HasComponent<IndestructibleCD>(entity))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static bool CanPlace(float3 worldPos, EntityManager manager, bool allowWater)
        {
            return GroundCanPlace(worldPos, allowWater) && !HasEntity(worldPos, manager, out _);
        }
        public static bool FindNearestSpace(in int maxRange, in bool large, in bool allowWater, in EntityManager manager, Vector3 origin, out float3 pos, out int dir)
        {
            float3 ori = origin.RountToFloat3();
            pos = new();
            dir = 0;
            for (int i = 0; i < math.pow(maxRange, 2) - 1; i++)
            {
                pos = ori + ExpandingSpiral(i, out dir).ToFloat3();
                bool canplace = CanPlace(pos, manager, allowWater);
                if (large)
                {
                    if (canplace && CanPlace(ori + ExpandingSpiral(++i, out _).ToFloat3(), manager, allowWater))
                    {
                        return true;
                    }
                }
                else if (canplace) return true;
            }
            return false;
        }
        public static ObjectInfo GetTileEntity(TileCD tile)
        {
            return PugDatabase.TryGetTileItemInfo(tile.tileType, tile.tileset);
        }

    }
}
