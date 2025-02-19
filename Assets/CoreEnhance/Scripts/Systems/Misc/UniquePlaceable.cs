using Assets.CoreEnhance.Scripts.Components;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Assets.CoreEnhance.Scripts.Systems.Misc
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class UniquePlaceableSystem : PugSimulationSystemBase
    {
        protected override void OnUpdate()
        {
            NativeHashMap<int, double> uniques = new(UniquePlaceableConverter.Count, Allocator.Temp);
            var current = World.Time.ElapsedTime;
            JobHandle check = Entities.ForEach((ref UniquePlaceableCD unique, in ObjectDataCD objData) =>
            {
                ref double time = ref unique.placeTime;
                if (!unique.init)
                {
                    unique.init = true;
                    time = current;
                }
                int id = (int)objData.objectID;
                uniques[id] = uniques.ContainsKey(id) ? Math.Min(uniques[id], time) : time;
            })
                .WithName("CheckUnqiuePlaceable")
                .WithBurst()
                .ScheduleParallel(Dependency);

            JobHandle destory = Entities.ForEach((ref HealthCD health, in UniquePlaceableCD unique, in ObjectDataCD objData) =>
            {
                if (!uniques.TryGetValue((int)objData.objectID, out double first))
                    return;
                if (unique.placeTime <= first)
                    return;
                health.health = 0;
            })
                .WithName("DestroyExcessUniques")
                .WithBurst()
                .ScheduleParallel(check);

            destory.Complete();
            uniques.Dispose();

            base.OnUpdate();
        }
    }
}