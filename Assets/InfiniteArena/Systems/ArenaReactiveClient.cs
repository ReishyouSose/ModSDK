using Assets.InfiniteArena.Components;
using Assets.InfiniteArena.Other;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.InfiniteArena.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class ArenaReactiveClient : PugSimulationSystemBase
    {

        protected override void OnUpdate()
        {
            var collision = GetPhysicsWorld().CollisionWorld;
            var objDataLookup = SystemAPI.GetComponentLookup<ObjectDataCD>();
            Entities.ForEach((Entity entity, ref ObjectDataCD objData, ref ArenaReactiveCD arena, in DistanceToPlayerCD toPlayer, in LocalTransform local) =>
            {
                if (toPlayer.minDistanceSq < 1)
                {
                    /*NativeList<ColliderCastHit> hits = new(Allocator.Temp);
                    collision.SphereCastAll(local.Position, 1, float3.zero, 0, ref hits, CollisionFilter.Default);
                    foreach (ColliderCastHit hit in hits)
                    {
                        if(objDataLookup.TryGetComponent( hit.Entity,out var info)&&info.objectID==ObjectID.AncientGemstone)
                        {
                            return;
                        }
                    }*/
                }
            })
                .WithName("ArenaAno")
                .WithBurst()
                .Run();
        }
    }
}
