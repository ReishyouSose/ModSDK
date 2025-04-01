using Assets.CoreEnhance.Scripts.Configs;
using Assets.CoreEnhance.Scripts.Helpers;
using CoreLib.Util.Extensions;
using Unity.Entities;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Systems.Automation
{
    public struct AutoDoorCD : IComponentData { }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial class AutoDoorSystem : PugSimulationSystemBase
    {
        protected override void OnUpdate()
        {
            if (!EnhanceConfig.IsEnable(EnhanceCategory.Automation, EC_Automation.Door))
                return;

            Entities.ForEach((ref ObjectDataCD objData, in DistanceToPlayerCD dis) =>
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
                .WithAll<AutoDoorCD>()
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
        internal static void MarkDoor(Entity e, GameObject authoringData, EntityManager manager)
        {
            if (authoringData.HasComponent<DoorAuthoring>()
                || authoringData.HasComponent<FenceGateAuthoring>())
            {
                Debug.Log("[CoreEnhance] AutoDoor: Mark " + authoringData.GetEntityObjectID());
                manager.AddComponentData(e, new DistanceToPlayerCD());
                manager.AddComponentData(e, new AutoDoorCD());
            }
        }
    }
}
