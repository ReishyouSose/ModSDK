using Assets.CoreEnhance.Scripts.Configs;
using Unity.Entities;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Systems.Automation
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial class AutoDoorSystem : PugSimulationSystemBase
    {
        protected override void OnUpdate()
        {
            if (!EnhanceConfig.IsEnable(EnhanceCategory.Automation, EC_Automation.Door))
                return;

            Entities.ForEach((DynamicBuffer<AdaptiveEntityBuffer> adaptive,
                ref ObjectDataCD objData, in DistanceToPlayerCD dis) =>
            {
                var min = dis.minDistanceSq;
                if (min > 2)
                    return;
                ref var variation = ref objData.variation;
                foreach (var condition in adaptive)
                {
                    if (condition.adaptiveCondition.variation != variation)
                        continue;
                    if (min > 1.5f)
                        variation = PugDatabase.GetObjectInfo(objData.objectID, variation).variationToToggleTo;
                    return;
                }
                if (min <= 1.5f)
                {
                    variation = PugDatabase.GetObjectInfo(objData.objectID, variation).variationToToggleTo;
                }
            })
                .WithName("Automation_Door")
                .WithAll<ChangeVariationTriggerCD>()
                .WithAll<DoorCD>()
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
        public static void AddDistanceCD(Entity e, GameObject authoringData, EntityManager manager)
        {
            if (authoringData.TryGetComponent<DoorAuthoring>(out _))
                manager.AddComponentData(e, new DistanceToPlayerCD());
        }
    }
}
