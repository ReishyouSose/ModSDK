using Assets.CoreEnhance.Scripts.Helpers;
using Assets.CoreEnhance.Scripts.Items;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Assets.CoreEnhance.Scripts.Systems.Misc
{

    [UpdateAfter(typeof(UniquePlaceableSystem))]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class AutoFisherServer : PugSimulationSystemBase
    {
        private BiomeLookup biomeLookup;
        private float timer;
        protected override void OnCreate()
        {
            NeedDatabase();
            NeedLootBank();
            RequireForUpdate<BiomeRangesCD>();
            base.OnCreate();
        }
        protected override void OnStartRunning()
        {
            biomeLookup = SystemAPI.TryGetSingleton<BiomeSamplesCD>(out var sample)
                ? new(sample) : new(SystemAPI.GetSingleton<BiomeRangesCD>().Value, Allocator.Persistent);
            base.OnStartRunning();
        }
        protected override void OnUpdate()
        {
            var delta = SystemAPI.Time.DeltaTime;
            if (timer < 1)
            {
                timer += delta;
                return;
            }
            timer = 0;

            var tileAccessor = CreateTileAccessor();
            var biomeLookup = this.biomeLookup;
            var localDatabase = database;
            var localLootBack = lootBank;
            Entities.ForEach((DynamicBuffer<ContainedObjectsBuffer> containers, ref AutoFisherCD af,
                ref RandomCD random, in LocalTransform trans) =>
            {
                if (!af.CheckRodLevel(containers, out float efficiency, out int chance))
                    return;
                ref var rng = ref random.Value;
                af.timer += efficiency;
                while (af.timer > 3)
                {
                    af.timer -= 3;
                    if (rng.NextInt(10) >= 5 + chance)
                        continue;
                    AutoFisherCD.Init(ref af, tileAccessor, biomeLookup, trans);
                    using var drops = PugDatabase.GetRandomLoot(rng.NextInt(6) == 0 ? af.items : af.fishes,
                        1, 1, ref rng, localLootBack, localDatabase, trans.Position, af.biome);
                    int count = containers.Length;
                    for (int i = 9; i < count; i++)
                    {
                        if (containers[i].objectData.objectID != ObjectID.None)
                            continue;
                        var item = drops[0];
                        containers[i] = ItemHelper.CreateItem(item.objectID, item.amount);
                        break;
                    }
                }
            })
                .WithName("AutoFisher_Catch")
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
    }
}
