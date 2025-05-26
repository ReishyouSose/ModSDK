using Assets.CoreEnhance.Scripts.Configs;
using Assets.CoreEnhance.Scripts.Patchs;
using Assets.CoreEnhance.Scripts.Sturcts;
using CoreLib.Data.Configuration;
using PlayerState;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Assets.CoreEnhance.Scripts.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class AccelerateCommonSystem : PugSimulationSystemBase
    {
        private uint tickRate;
        private ComponentLookup<CattleCD> cattleLookup;
        protected override void OnCreate()
        {
            tickRate = (uint)NetworkingManager.GetSimulationTickRateForPlatform();
            cattleLookup = SystemAPI.GetComponentLookup<CattleCD>();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            Accelerate_Merchant();
            Accelerate_SoulOrb(ecb, tickRate);
            Accelerate_Crafting(cattleLookup);
            Accelerate_Casting(tickRate);
            Accelerate_Portal(ecb);
            base.OnUpdate();
        }
        private void Accelerate_Merchant()
        {
            if (!EnhanceConfig.TryGetValue(EnhanceCategory.Accelerate, EC_Accelerate.Merchant, out ConfigEntry<int> value))
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
        private void Accelerate_SoulOrb(EntityCommandBuffer ecb, uint tickRate)
        {
            if (!EnhanceConfig.TryGetValue(EnhanceCategory.Accelerate, EC_Accelerate.Titan, out ConfigEntry<int> value))
                return;
            int maxTime = value.Value;
            var current = GetServerTick();
            bool any = false;
            JobHandle job = Entities.ForEach((Entity e, ref DestroyTimerCD destroy) =>
            {
                ref var timer = ref destroy.timer;
                if (timer.GetRemainingSeconds(current, tickRate) > maxTime)
                {
                    timer.SetTargetTicks(maxTime, tickRate);
                    ecb.AddComponent<ProcessedTagCD>(e);
                    any = true;
                }
            })
                .WithName("Accelerate_SoulOrb")
                .WithAll<SoulOrbCD>()
                .WithNone<ProcessedTagCD>()
                .WithBurst()
                .ScheduleParallel(Dependency);
            job.Complete();
            if (any)
            {
                BossCheckPatch.ShouldCheckImmdiately = true;
            }
        }

        private void Accelerate_Crafting(ComponentLookup<CattleCD> cattleLookup)
        {
            if (!EnhanceConfig.IsEnable(EnhanceCategory.Accelerate, EC_Accelerate.Crafting))
                return;
            bool animals = EnhanceConfig.TryGetValue<bool>(EnhanceCategory.Accelerate,
                EC_Accelerate.Crafting, out var animalConfig, "Animals") && animalConfig.Value;

            Entities.ForEach((Entity e, ref CraftingCD crafting) =>
            {
                if (crafting.currentlyCraftingIndex < 0)
                    return;
                if (crafting.disable != 0)
                    return;
                if (crafting.timeLeftToCraft <= 0)
                    return;
                if (!animals && cattleLookup.HasComponent(e))
                    return;
                crafting.timeLeftToCraft = 0;
            })
                .WithName("Accelerate_Crafting")
                .WithNone<PlayerGhost>()
                .WithBurst()
                .Schedule();
        }
        private void Accelerate_Casting(uint tickRate)
        {
            if (!EnhanceConfig.IsEnable(EnhanceCategory.Accelerate, EC_Accelerate.Casting))
                return;
            Entities.ForEach((ref CastingStateCD casting) =>
            {
                casting.castTimer.SetTargetTicks(0, tickRate);
            })
                .WithName("Accelerate_Casting")
                .WithBurst()
                .Schedule();
        }
        private void Accelerate_Portal(EntityCommandBuffer ecb)
        {
            if (!EnhanceConfig.IsEnable(EnhanceCategory.Accelerate, EC_Accelerate.Portal))
                return;
            Entities.ForEach((Entity e, ref ObjectDataCD objData) =>
            {
                objData.amount = 1200;
                ecb.AddComponent<ProcessedTagCD>(e);
            })
                .WithName("Accelerate_Portal")
                .WithAll<PortalCD>()
                .WithNone<WayPointCD>()
                .WithNone<ProcessedTagCD>()
                .WithBurst()
                .Schedule();

            Entities.ForEach((Entity e, ref ObjectDataCD objData, in WayPointCD wayPoint, in DistanceToPlayerCD dis) =>
            {
                float minDis = dis.minDistanceSq;
                if (minDis > 0 && minDis <= wayPoint.distanceToActivateSQ)
                {
                    objData.amount = 600;
                    ecb.AddComponent<ProcessedTagCD>(e);
                }
            })
                .WithName("Accelerate_WayPoint")
                .WithNone<ProcessedTagCD>()
                .WithBurst()
                .Schedule();
        }
    }
}
