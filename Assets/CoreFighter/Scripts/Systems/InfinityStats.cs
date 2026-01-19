using Unity.Entities;

namespace Assets.CoreFighter.Scripts.Systems.Infinity
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class InfinityStatsSystem : PugSimulationSystemBase
    {
        protected override void OnUpdate()
        {
            Mana();
            base.OnUpdate();
        }
        private void Mana()
        {
            Entities.ForEach((ref ManaCD mana) =>
            {
                if (mana.mana != mana.maxMana)
                {
                    mana.mana = mana.maxMana;
                }
            })
                .WithName("Infinity_Mana")
                .WithBurst()
                .Schedule();
        }
    }
}
