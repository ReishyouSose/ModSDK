using Assets.CoreEnhance.Scripts.Component;
using Assets.CoreEnhance.Scripts.Configs;
using Assets.CoreEnhance.Scripts.Patchs;
using CoreLib.Data.Configuration;
using PlayerState;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.CoreEnhance.Scripts.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class AccelerateSystem : PugSimulationSystemBase
    {
        private uint simulationTickRateForPlatform;
        protected override void OnCreate()
        {
            simulationTickRateForPlatform = (uint)NetworkingManager.GetSimulationTickRateForPlatform();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            var tickRate = simulationTickRateForPlatform;
            Accelerate_Merchant();
            Accelerate_SoulOrb(ecb,tickRate);
            Accelerate_Crafting();
            Accelerate_Casting(tickRate);
            base.OnUpdate();
        }
        private void Accelerate_Merchant()
        {
            if (!ModConfig.TryGetValue(EnhanceCategory.Accelerate, EC_Accelerate.Merchant, out ConfigEntry<int> value))
                return;
            int timeLimit = value.Value;
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
                    .WithName("Accelerate_Merchant_Immediate")
                    .WithAll<StateInfoCD>()
                    .WithBurst()
                    .Schedule();
            }
            else
            {
                int time = math.max(180, timeLimit);
                Entities.ForEach((ref ObjectDataCD objectData) =>
                {
                    if (objectData.amount > time)
                    {
                        objectData.amount = time;
                    }
                })
                    .WithName("Accelerate_Merchant_Reduce")
                    .WithAll<MerchantCD>()
                    .WithAll<StateInfoCD>()
                    .WithBurst()
                    .Schedule();
            }
        }
        private void Accelerate_SoulOrb(EntityCommandBuffer ecb,uint tickRate)
        {
            if (!ModConfig.TryGetValue(EnhanceCategory.Accelerate, EC_Accelerate.Titan, out ConfigEntry<int> value))
                return;
            int maxTime = value.Value;
            var current = GetServerTick();
            Entities.ForEach((Entity e, ref DestroyTimerCD destroy) =>
            {
                ref var timer = ref destroy.timer;
                if (timer.GetRemainingSeconds(current, tickRate) > maxTime)
                {
                    timer.SetTargetTicks(maxTime, tickRate);
                    ecb.AddComponent<SoulOrbCutCD>(e);
                    BossCheckPatch.ShouldCheckImmdiately = true;
                }
            })
                .WithName("Accelerate_SoulOrb")
                .WithAll<SoulOrbCD>()
                .WithNone<SoulOrbCutCD>()
                .WithBurst()
                .Schedule();
        }

        private void Accelerate_Crafting()
        {
            if (!ModConfig.TryGetEnable(EnhanceCategory.Accelerate, EC_Accelerate.Crafting))
                return;
            Entities.ForEach((ref CraftingCD crafting) =>
            {
                if (crafting.disable != 0)
                    return;
                crafting.timeLeftToCraft = 0;
            })
                .WithName("Accelerate_Crafting")
                .WithBurst()
                .Schedule();
        }
        private void Accelerate_Casting(uint tickRate)
        {
            if (!ModConfig.TryGetEnable(EnhanceCategory.Accelerate, EC_Accelerate.Casting))
                return;
            Entities.ForEach((ref CastingStateCD casting) =>
            {
                casting.castTimer.SetTargetTicks(0, tickRate);
            })
                .WithName("Accelerate_Casting")
                .WithBurst()
                .Schedule();
        }
    }
}
