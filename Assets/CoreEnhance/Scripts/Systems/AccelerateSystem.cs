using Assets.CoreEnhance.Scripts.Configs;
using CoreLib.Data.Configuration;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

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
            Accelerate_Merchant();
            Accelerate_SoulOrb();
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
                    .Schedule();
            }
            else
            {
                int time = math.max(60, timeLimit);
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
                    .Schedule();
            }
        }
        private void Accelerate_SoulOrb()
        {
            if (!ModConfig.TryGetValue(EnhanceCategory.Accelerate, EC_Accelerate.Titan, out ConfigEntry<int> value))
                return;
            int maxTime = value.Value;
            var current = GetServerTick();
            uint tickRate = simulationTickRateForPlatform;
            Entities.ForEach((ref DestroyTimerCD destroy) =>
            {
                ref var timer = ref destroy.timer;
                if (timer.GetRemainingSeconds(current, tickRate) > maxTime)
                {
                    timer.SetTargetTicks(maxTime, tickRate);
                    Debug.Log("Set tick");
                }
            })
                .WithName("Accelerate_SoulOrb")
                .WithAll<SoulOrbCD>()
                .WithBurst()
                .Run();
        }
    }
}
