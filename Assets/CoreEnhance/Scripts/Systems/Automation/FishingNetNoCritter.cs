using Assets.CoreEnhance.Scripts.Cores;
using Unity.Entities;

namespace Assets.CoreEnhance.Scripts.Systems.Automation
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class FishingNetNoCritterSystem : PugSimulationSystemBase
    {
        protected override void OnUpdate()
        {
            if (!EnhanceConfig.IsEnable(EnhanceCategory.FishingNetNoCritter))
                return;
            Entities.ForEach((Entity e, DynamicBuffer<ContainedObjectsBuffer> container) =>
            {
                int length = container.Length;
                for (int i = 0; i < length; i++)
                {
                    if (container[i].objectID != ObjectID.None)
                        continue;
                    container[i] = new()
                    {
                        objectData = new()
                        {
                            objectID = ObjectID.CritterBeetle,
                            amount = 1,
                        }
                    };
                }
            })
                .WithName("FishingNetNoCritter")
                .WithBurst()
                .WithAll<FishingCD>()
                .Schedule();
        }
    }
}
