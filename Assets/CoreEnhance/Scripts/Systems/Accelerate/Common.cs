using Assets.CoreEnhance.Scripts.Components;
using Assets.CoreEnhance.Scripts.Patchs;
using PlayerState;
using Pug.Automation;
using Unity.Entities;
using Unity.Jobs;

namespace Assets.CoreEnhance.Scripts.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class AccelerateCommonSystem : PugSimulationSystemBase
    {
        private int tickRate;
        protected override void OnCreate()
        {
            tickRate = PlatformConfiguration.Instance.SessionConfiguration.SimulationTickRate;
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            var utick = (uint)tickRate;
            Accelerate_Merchant();
            Accelerate_SoulOrb(ecb, utick);
            Accelerate_Casting(utick);
            Accelerate_Portal(ecb);
            base.OnUpdate();
        }
        private void Accelerate_Merchant()
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
        private void Accelerate_SoulOrb(EntityCommandBuffer ecb, uint tickRate)
        {
            int maxTime = 5;
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
        private void Accelerate_Casting(uint tickRate)
        {
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
