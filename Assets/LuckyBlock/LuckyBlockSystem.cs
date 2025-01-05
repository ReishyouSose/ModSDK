using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.LuckyBlock
{
    [UpdateBefore(typeof(UpdateHealthFromBufferSystem))]
    [UpdateInGroup(typeof(UpdateHealthSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class LuckyBlockSystem : PugSimulationSystemBase
    {
        private NativeQueue<float3> queue;
        private CustomScenesDataTable sceneData;
        private Array objID;
        private Unity.Mathematics.Random rng;
        protected override void OnCreate()
        {
            queue = new NativeQueue<float3>(Allocator.Persistent);
            sceneData = Resources.Load<CustomScenesDataTable>("Scenes/CustomScenesDataTable");
            objID = Enum.GetValues(typeof(ObjectID));
            rng = PugRandom.GetRng();
            NeedDatabase();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            var localQueue = queue;
            Entities.ForEach((Entity e, in HealthCD health, in LocalTransform trans) =>
            {
                if (health.health != 0)
                    return;
                localQueue.Enqueue(trans.Position);
                ecb.SetComponentEnabled<LuckyBlockCD>(e, false);
            })
                .WithName("LuckyBlock")
                .WithAll<LuckyBlockCD>()
                .WithBurst()
                .Schedule();

            while (queue.TryDequeue(out float3 pos))
            {
                int index = rng.NextInt(objID.Length);
                ObjectID id = (ObjectID)objID.GetValue(index);
                bool stackable = PugDatabase.GetEntityObjectInfo(id, database).isStackable;
                int amount = stackable ? rng.NextInt(1, 9999) : 1;
                Entity e = EntityUtility.CreateEntity(ecb, id, 1, database);
                ecb.SetComponent(e, new LocalTransform()
                {
                    Position = pos
                });
                Debug.Log("Lucky Random");
            }

            base.OnUpdate();
        }
    }
}
