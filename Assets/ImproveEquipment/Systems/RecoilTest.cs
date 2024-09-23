/*using PlayerEquipment;
using Unity.Entities;

namespace Assets.ImproveEquipment.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class RecoilTestServerSystem : PugSimulationSystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref PlayerAttackCD attack) =>
            {
                attack.recoilForce = 0;
            })
                .WithName("RecoilTest")
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
    }
}*/
