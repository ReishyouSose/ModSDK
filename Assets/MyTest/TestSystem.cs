using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.MyTest.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class TestClient : PugSimulationSystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            base.OnUpdate();
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class TestServer : PugSimulationSystemBase
    {
        protected override void OnCreate()
        {
            UpdatesInRunGroup();
            NeedDatabase();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var manager = EntityManager;
            //GetPhysicsWorld
            var collision = GetPhysicsWorld().CollisionWorld;
            var objLookup = SystemAPI.GetComponentLookup<ObjectDataCD>();
            var posLookup = SystemAPI.GetComponentLookup<LocalTransform>();
            Entities.ForEach((Entity entity, in ObjectDataCD objdata, in LocalTransform local) =>
            {
                if (objdata.objectID == ObjectID.EventTerminal && objdata.variation == 1)
                {
                    int2 pos = local.Position.RoundToInt2();
                    NativeList<ColliderCastHit> list = new(Allocator.Temp);
                    collision.SphereCastAll(local.Position, 10, float3.zero, 10, ref list, CollisionFilter.Default);
                    foreach (ColliderCastHit hit in list)
                    {
                        if (objLookup.TryGetComponent(hit.Entity, out var data) && data.objectID == ObjectID.EnemySpawnerPlatform)
                        {
                            if (posLookup.TryGetComponent(hit.Entity, out var position))
                            {
                                Debug.Log(position.Position.RoundToInt2());
                            }
                        }
                    }
                }
            })
                .WithBurst()
                .Run();
        }
    }
}
