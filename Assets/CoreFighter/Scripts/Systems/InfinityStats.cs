using Assets.CoreFighter.Scripts.Cores;
using Unity.Entities;

namespace Assets.CoreFighter.Scripts.Systems.Infinity
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class InfinityStatsSystem : PugSimulationSystemBase
    {
        private float timer;
        protected override void OnUpdate()
        {
            if (timer > 0)
            {
                timer -= SystemAPI.Time.DeltaTime;
                return;
            }
            timer = 0.2f;
            Mana();
            Hunger();
            base.OnUpdate();
        }
        private void Mana()
        {
            if (!FighterConfig.IsEnable(FighterCategory.Mana))
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
        }
        private void Hunger()
        {
            if (!FighterConfig.IsEnable(FighterCategory.Hunger))
                return;
            Entities.ForEach((ref HungerCD hunger) =>
            {
                if (hunger.hunger < HungerCD.MAX_HUNGER)
                {
                    hunger.hunger = HungerCD.MAX_HUNGER;
                }
            })
                .WithName("Infinity_Hunger")
                .WithBurst()
                .Schedule();
        }
    }
}
