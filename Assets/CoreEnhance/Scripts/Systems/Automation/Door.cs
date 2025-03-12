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
                if (min > 3)
                    return;
                ref var variation = ref objData.variation;
                bool open = variation % 2 == 1;
                if (open && min > 1.5f || !open && min <= 1.5f)
                {
                    variation = PugDatabase.GetObjectInfo(objData.objectID, variation).variationToToggleTo;
                }
            })
                .WithName("Automation_Door")
                .WithAll<ChangeVariationTriggerCD>()
                .WithAll<DoorCD>()
                .WithBurst()
                .Run();
            base.OnUpdate();
        }
        public static void AddDistanceCD(Entity e, GameObject authoringData, EntityManager manager)
        {
            if (authoringData.TryGetComponent<DoorAuthoring>(out _))
                manager.AddComponentData(e, new DistanceToPlayerCD());
        }
    }
}
