using Assets.CoreEnhance.Scripts.Cores;
using Assets.CoreEnhance.Scripts.Helpers;
using PugScan;
using Unity.Entities;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Systems.Infinity
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(RunSimulationSystemGroup))]
    public partial class InfinityBossScanSystem : PugSimulationSystemBase
    {
        private struct BossScannedCD : IComponentData { }
        private struct CircleMoveMark : IComponentData { }

        private ComponentLookup<RoamingPathCD> roamingLookup;
        private ComponentLookup<CircleMoveMark> circleLookup;
        protected override void OnCreate()
        {
            RequireForUpdate<KilledEnemiesBuffer>();
            roamingLookup = SystemAPI.GetComponentLookup<RoamingPathCD>();
            circleLookup = SystemAPI.GetComponentLookup<CircleMoveMark>();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            if (!EnhanceConfig.TryGetValue<bool>(EnhanceCategory.BossScan, out var roaming))
                return;
            if (!SystemAPI.TryGetSingletonBuffer<KilledEnemiesBuffer>(out var killed))
                return;
            var ecb = CreateCommandBuffer();
            bool roamingOnly = roaming.Value;
            var roamingLookup = this.roamingLookup;
            var circleLookup = this.circleLookup;
            Entities.ForEach((Entity e, in ObjectDataCD objData) =>
            {
                if (roamingOnly && (!roamingLookup.HasComponent(e) || circleLookup.HasComponent(e)))
                    return;
                ecb.AddComponent<BossScannedCD>(e);
                var id = objData.objectID;
                foreach (var boss in killed)
                {
                    if (boss.objectData.objectID != id)
                        continue;
                    var scan = ecb.CreateEntity();
                    ecb.AddComponent(scan, new ScanRequestCD()
                    {
                        objectToScan = new ObjectDataCD
                        {
                            objectID = id,
                        },
                        sendResponse = true,
                        typeOfRequest = PugScanType.Scan,
                    });
                    break;
                }
            })
                .WithName("InfinityBossScan")
                .WithBurst()
                .WithEntityQueryOptions(EntityQueryOptions.IncludeDisabledEntities)
                .WithNone<BossScannedCD>()
                .WithAll<BossCD>()
                .WithAll<EnemyCD>()
                .Schedule();

            base.OnUpdate();
        }

        internal static void MarkCircleMoveBoss(Entity e, GameObject authoringData, EntityManager manager)
        {
            switch (authoringData.GetEntityObjectID(out _))
            {
                case ObjectID.BossLarva:
                case ObjectID.WallBoss:
                    manager.AddComponentData(e, new CircleMoveMark());
                    return;
            }
        }
    }
}
