using Unity.Entities;

namespace Assets.TrophyKeeper
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(EnemySpawnerPlatformSystem))]
    public partial class TrophyEntityDropSystem : PugSimulationSystemBase
    {
        private ComponentLookup<DontDropLootCD> dontDropLookup;
        protected override void OnCreate()
        {
            dontDropLookup = SystemAPI.GetComponentLookup<DontDropLootCD>();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            if (!TrophyKeeper.TrophyEntityDrop.Value)
                return;
            var ecb = CreateCommandBuffer();
            var dontDropLookup = this.dontDropLookup;
            Entities.ForEach((in EnemySpawnerPlatformCD spawner) =>
            {
                Entity e = spawner.spawnedEntity;
                if(dontDropLookup.HasComponent(e))
                ecb.RemoveComponent<DontDropLootCD>(e);
            })
                .WithName("TrophyEntityDrop")
                .WithBurst()
                .WithEntityQueryOptions(EntityQueryOptions.IncludeDisabledEntities)
                .Schedule();
            base.OnUpdate();
        }
    }
}
