using Assets.CoreEnhance.Helpers;
using Unity.Entities;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Systems.Automation
{
    public struct AutoDoorCD : IComponentData { }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial class AutoDoorSystem : PugSimulationSystemBase
    {
        protected override void OnCreate()
        {
            NeedDatabase();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var database = this.database;
            Entities.ForEach((ref ObjectDataCD objData, in DistanceToPlayerCD dis) =>
            {
                var min = dis.minDistanceSq;
                if (min > 3)
                    return;
                ref var variation = ref objData.variation;
                bool open = variation % 2 == 1;
                if (open && min > 1.5f || !open && min <= 1.5f)
                {
                    variation = PugDatabase.GetEntityObjectInfo(objData.objectID, database, variation).variationToToggleTo;
                }
            })
                .WithName("Automation_Door")
                .WithAll<AutoDoorCD>()
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
        public static void MarkDoor(Entity e, GameObject authoringData, EntityManager manager)
        {
            if (authoringData.HasComponent<DoorAuthoring>()
                || authoringData.HasComponent<FenceGateAuthoring>())
            {
                manager.AddComponentData(e, new DistanceToPlayerCD());
                manager.AddComponentData(e, new AutoDoorCD());
            }
        }
    }
}
