using Unity.Entities;

namespace Assets.CoreEnhance.Scripts.Systems.Misc
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class QuickConsumableSystem : PugSimulationSystemBase
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
}
