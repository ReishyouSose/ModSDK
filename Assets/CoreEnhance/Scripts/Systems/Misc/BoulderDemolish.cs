using Assets.CoreEnhance.Scripts.Items;
using Assets.CoreEnhance.Scripts.Sturcts;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Systems.Misc
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class BoulderDemolishSystem : PugSimulationSystemBase
    {
        private EntityQuery queue;
        private ComponentLookup<LocalTransform> transLookup;
        private ComponentLookup<HealthCD> healthLookup;
        private ComponentLookup<DropsLootWhenDamagedCD> lootLookup;
        protected override void OnCreate()
        {
            queue = EntityManager.CreateEntityQuery(typeof(RequiresDrillCD));
            transLookup = SystemAPI.GetComponentLookup<LocalTransform>();
            healthLookup = SystemAPI.GetComponentLookup<HealthCD>();
            lootLookup = SystemAPI.GetComponentLookup<DropsLootWhenDamagedCD>();
            NeedDatabase();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingletonBuffer<HealthChangeBuffer>(out var healthChangeBuffer))
            {
                Debug.Log("No buffer");
                return;
            }
            var boulders = queue.ToEntityArray(Allocator.Temp);
            var transLookup = this.transLookup;
            var healthLookup = this.healthLookup;
            var lootLookup = this.lootLookup;
            var database = this.database;
            var ecb = CreateCommandBuffer();
            var job = Entities.ForEach((Entity entity, ref HealthCD health,
                in KilledByPlayerCD killer, in LocalTransform trans) =>
            {
                if (health.health > 0)
                    return;
                foreach (var e in boulders)
                {
                    if (!transLookup.TryGetComponent(e, out var local))
                        continue;
                    if (!lootLookup.TryGetComponent(e, out var loot))
                        continue;
                    if (!healthLookup.TryGetComponent(e, out var boulder))
                        continue;
                    var pos = local.Position.xz;
                    var ori = trans.Position.xz;
                    if (math.distancesq(pos, ori) > 2)
                        continue;
                    int count = (int)math.round((float)boulder.health / loot.damageToDealToDropLoot);
                    EntityUtility.CreateAndDropItem(loot.dropsLoot, 0, math.min(count, 1800),
                        local.Position, killer.playerEntity, database, ecb);
                    ecb.DestroyEntity(e);
                }
                ecb.AddComponent<ProcessedTagCD>(entity);
            })
                .WithName("BoulderDemolish")
                .WithAll<BoulderDemolishCD>()
                .WithNone<ProcessedTagCD>()
                .WithBurst()
                .ScheduleParallel(Dependency);
            job.Complete();
            boulders.Dispose();
            base.OnUpdate();
        }
    }
}
