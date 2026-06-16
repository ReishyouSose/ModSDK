using Assets.CoreEnhance.Scripts.Cores;
using Pug.UnityExtensions;
using PugTilemap;
using Unity.Entities;
using Unity.Transforms;

namespace Assets.CoreEnhance.Scripts.Systems.Accelerate
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation, WorldSystemFilterFlags.Default)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(PlantsGrowingSystem))]
    public partial class AcceleratePlantsSystem : PugSimulationSystemBase
    {
        private TileAccessor tileAccessor;
        private ComponentLookup<GrowTimerCD> timerLookup;
        protected override void OnCreate()
        {
            timerLookup = SystemAPI.GetComponentLookup<GrowTimerCD>();
            base.OnCreate();
        }
        protected override void OnStartRunning()
        {
            tileAccessor = CreateTileAccessor();
            base.OnStartRunning();
        }
        protected override void OnUpdate()
        {
            if (!EnhanceConfig.IsEnable(EnhanceCategory.Plant))
                return;
            var tileAccessor = this.tileAccessor;
            var timerLookup = this.timerLookup;
            Entities.ForEach((in GrowTimerRefCD entityRef, in LocalTransform trans) =>
            {
                if (!tileAccessor.HasType(trans.Position.RoundToInt2(), TileType.wateredGround))
                    return;
                var optional = timerLookup.GetRefRWOptional(entityRef.GrowTimerEntity);
                if (!optional.IsValid)
                    return;
                ref var timer = ref optional.ValueRW;
                timer.Value = timer.StageTime;
            })
                .WithName("Accelerate_Plant")
                .WithBurst()
                .WithNone<HasFinishedGrowingCD>()
                .WithEntityQueryOptions(EntityQueryOptions.IncludeDisabledEntities)
                .Schedule();
            base.OnUpdate();
        }
    }
}
