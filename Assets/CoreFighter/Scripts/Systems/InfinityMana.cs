using Assets.CoreFighter.Scripts.Configs;
using Unity.Entities;

namespace Assets.CoreFighter.Scripts.Systems.Infinity
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class InfinityManaSystem : PugSimulationSystemBase
    {
        private const int ResetTimer = 1;
        private float timer;
        protected override void OnUpdate()
        {
            if (timer < ResetTimer)
            {
                timer += World.Time.DeltaTime;
                return;
            }
            timer = 0;
            if (!FighterConfig.IsEnable(FighterCategory.Infinity, FC_Infinity.Mana))
                return;
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
            base.OnUpdate();
        }
    }
}
