using Assets.CoreEnhance.Scripts.Configs;
using Unity.Entities;

namespace Assets.CoreEnhance.Scripts.Systems.Infinity
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class InfinityCommonSystem : PugSimulationSystemBase
    {
        private const int ResetTimer = 1;
        private float timer;
        protected override void OnCreate()
        {
            NeedDatabase();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            if (timer < ResetTimer)
            {
                timer += World.Time.DeltaTime;
                return;
            }
            timer = 0;
            Infinity_Minion();
            Infinity_Mana();
            Infinity_Boulder();
            base.OnUpdate();
        }
        private void Infinity_Boulder()
        {
            if (!ModConfig.IsEnable(EnhanceCategory.Infinity, EC_Infinity.Boulder))
                return;
            Entities.ForEach((ref HealthCD heal, in ObjectDataCD objdata) =>
            {
                heal.health = heal.maxHealth;
            })
                .WithName("Infinity_Boulder")
                .WithAll<RequiresDrillCD>()
                .WithAll<DontDropSelfCD>()
                .WithBurst()
                .Schedule();
        }
        private void Infinity_Minion()
        {
            if (!ModConfig.IsEnable(EnhanceCategory.Infinity, EC_Infinity.Minion))
                return;
            Entities.ForEach((ref MinionCD minion) =>
            {
                if (minion.hasStartedLifeSpanTimer)
                    minion.lifespanTimer = minion.lifespan;
            })
                .WithName("Infinity_Minion")
                .WithBurst()
                .Schedule();
        }
        private void Infinity_Mana()
        {
            if (!ModConfig.IsEnable(EnhanceCategory.Infinity, EC_Infinity.Mana))
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
    }
}
