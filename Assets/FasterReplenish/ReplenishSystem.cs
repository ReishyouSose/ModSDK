using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.FasterReplenish
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class ReplenishSystem : PugSimulationSystemBase
    {
        protected override void OnUpdate()
        {
            int timeLimit = FasterReplenishMod.Config.refreshTime.Value;
            if (timeLimit == 0)
            {
                Entities.ForEach((DynamicBuffer<ContainedObjectsBuffer> inventoryBuffer,
                    ref MerchantCD merchantCD, ref ObjectDataCD objectData) =>
                {
                    int count = 0;
                    foreach (var item in inventoryBuffer)
                    {
                        if (item.objectID != ObjectID.None)
                        {
                            count++;
                        }
                    }
                    if (count != merchantCD.previousAmountOfItems)
                    {
                        objectData.amount = 0;
                    }
                })
                    .WithAll<StateInfoCD>()
                    .Schedule();
            }
            else
            {
                int time = math.clamp(timeLimit, 60, 1500);
                Entities.ForEach((ref ObjectDataCD objectData) =>
                {
                    if (objectData.amount > time)
                    {
                        objectData.amount = time;
                    }
                })
                    .WithAll<MerchantCD>()
                    .WithAll<StateInfoCD>()
                    .Schedule();
            }
        }
    }
}
