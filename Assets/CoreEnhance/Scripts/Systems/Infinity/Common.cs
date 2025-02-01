using Assets.CoreEnhance.Scripts.Configs;
using PlayerEquipment;
using Unity.Entities;

namespace Assets.CoreEnhance.Scripts.Systems.Infinity
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class InfinityCommonSystem: PugSimulationSystemBase
    {
        private const int ResetTimer = 60;
        private float timer;
        protected override void OnCreate()
        {
            NeedDatabase();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            Infinity_Minion();
            if (timer < ResetTimer)
            {
                timer += World.Time.DeltaTime;
                return;
            }
            timer = 0;
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
    }
}
