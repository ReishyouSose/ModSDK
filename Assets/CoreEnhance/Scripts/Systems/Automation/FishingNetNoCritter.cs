using Assets.CoreEnhance.Scripts.Cores;
using PugMod;
using Unity.Entities;

namespace Assets.CoreEnhance.Scripts.Systems.Automation
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class FishingNetNoCritterSystem : PugSimulationSystemBase
    {
        private ObjectID holoWorm;
        protected override void OnCreate()
        {
            holoWorm = API.Authoring.GetObjectID("CoreEnhance_HoloWorm");
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            if (!EnhanceConfig.IsEnable(EnhanceCategory.FishingNetNoCritter))
                return;
            var holoWorm = this.holoWorm;
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
                            objectID = holoWorm,
                            amount = 1,
                        }
                    };
                }
            })
                .WithName("FishingNetNoCritter")
                .WithBurst()
                .WithAll<FishingCD>()
                .WithEntityQueryOptions(EntityQueryOptions.IncludeDisabledEntities)
                .Schedule();
        }
    }
}
