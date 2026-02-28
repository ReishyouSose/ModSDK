using Assets.CoreEnhance.Scripts.Components;
using PugWorldGen;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Systems.Misc
{
#pragma warning disable CS0618 // 类型或成员已过时
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(SpawnDungeonAndSceneSystem))]
    public partial class ReportSpawnedSceneSystem : PugSimulationSystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            Entities.ForEach((Entity e, in CustomSceneCD scene, in LocalTransform local) =>
            {
                Debug.Log($"{scene.name} {local.Position}");
                if (!local.Position.Equals(float3.zero))
                    ecb.AddComponent(e, new Translation()
                    {
                        Value = local.Position
                    });
                ecb.AddComponent<ProcessedTagCD>(e);
            })
                .WithName("ReportSpawnedScene")
                .WithNone<ProcessedTagCD>()
                .Run();
            base.OnUpdate();
        }
    }
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SerializationSystemGroup))]
    [UpdateAfter(typeof(DeserializeComponentsSystem))]
    public partial class CheckSpawnedSceneSystem : PugSimulationSystemBase
    {
        private ComponentLookup<LocalTransform> localLookup;
        private ComponentLookup<Translation> transLookup;
        protected override void OnCreate()
        {
            localLookup = SystemAPI.GetComponentLookup<LocalTransform>();
            transLookup = SystemAPI.GetComponentLookup<Translation>();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            var localLookup = this.localLookup;
            var transLookup = this.transLookup;
            Entities.ForEach((Entity e, in CustomSceneCD scene) =>
            {
                if(localLookup.TryGetComponent(e, out var local))
                {
                    Debug.Log($"{scene.name} local:{local.Position}");
                }
                else if(transLookup.TryGetComponent(e, out var trans))
                {
                    Debug.Log($"{scene.name} trans:{local.Position}");
                }
                else
                {
                    Debug.Log($"{scene.name} no localTrans");
                }
                ecb.AddComponent<ProcessedTagCD>(e);
            })
                .WithName("CheckSpawnedScene")
                .WithNone<ProcessedTagCD>()
                .Run();
            base.OnUpdate();
        }
    }
#pragma warning restore CS0618 // 类型或成员已过时
}
