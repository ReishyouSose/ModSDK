using Assets.CoreEnhance.Scripts.Helpers;
using Assets.CoreEnhance.Scripts.Items;
using Inventory;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.CoreEnhance.Scripts.Systems.Automation
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class AutoFisherSystem : PugSimulationSystemBase
    {
        private BiomeLookup biomeLookup;
        private BufferLookup<ContainedObjectsBuffer> containerLookup;
        private ComponentLookup<ObjectDataCD> objLookup;
        private float timer;
        protected override void OnCreate()
        {
            containerLookup = SystemAPI.GetBufferLookup<ContainedObjectsBuffer>();
            objLookup = SystemAPI.GetComponentLookup<ObjectDataCD>();
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

            if (!SystemAPI.TryGetSingletonEntity<AutoFisherTerminalCD>(out var terminal))
                return;
            containerLookup.TryGetBuffer(terminal, out var containers);
            var objData = objLookup.GetRefRW(terminal);
            var tileAccessor = CreateTileAccessor();
            var biomeLookup = this.biomeLookup;
            var localDatabase = database;
            var localLootBack = lootBank;
            Entities.ForEach((DynamicBuffer<ContainedObjectsBuffer> requires, ref AutoFisherCD af,
                ref RandomCD random, in LocalTransform trans) =>
            {
                int2 pos = trans.Position.xz.RoundToInt2();
                var biome = biomeLookup.GetBiome(pos);
                AutoFisherCD.Init(ref af, tileAccessor, pos);
                if (!af.CheckLevel(requires, biome, out float efficiency, out int chance))
                    return;
                ref var rng = ref random.Value;
                af.timer += 1;/* efficiency;*/
                while (af.timer > 3)
                {
                    af.timer -= 3;
                    /*if (rng.NextInt(10 - chance) >= 5)
                        continue;*/
                    using var drops = PugDatabase.GetRandomLoot(rng.NextInt(6) == 0 ? af.items : af.fishes,
                        1, 1, ref rng, localLootBack, localDatabase, trans.Position, biome);
                    ItemHelper.PutItemToContainer(containers, drops[0].objectID, drops[0].amount);
                    objData.ValueRW.amount++;
                }
            })
                .WithName("AutoFisher_Catch")
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
    }
}
