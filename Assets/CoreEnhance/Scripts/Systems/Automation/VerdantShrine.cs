using Assets.CoreEnhance.Scripts.Buffers;
using Assets.CoreEnhance.Scripts.Items;
using PugTilemap;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.CoreEnhance.Scripts.Systems.Automation
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class VerdantShrineSystem : PugSimulationSystemBase
    {
        private TileAccessor tileAccessor;
        protected override void OnCreate()
        {
            NeedTileUpdateBuffer();
            RequireForUpdate<VerdantShrineBuffer>();
            NeedDatabase();
            base.OnCreate();
        }
        protected override void OnStartRunning()
        {
            tileAccessor = CreateTileAccessor();
            base.OnStartRunning();
        }
        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingletonBuffer<VerdantShrineBuffer>(out var verdantShrineBuffer))
                return;
            verdantShrineBuffer.Clear();
            float deltaTime = World.Time.DeltaTime;
            var tileLookup = tileAccessor;
            var ecb = CreateCommandBuffer();
            var tileUpdateEntity = SystemAPI.GetSingletonEntity<TileUpdateBuffer>();
            Entities.ForEach((Entity e, ref VerdantShrineCD shrine,
                in DynamicBuffer<ContainedObjectsBuffer> containers, in LocalTransform trans) =>
            {
                shrine.Nature = containers[0].amount;
                shrine.Sea = containers[1].amount;
                shrine.Desert = containers[2].amount;
                verdantShrineBuffer.Add(new VerdantShrineBuffer()
                {
                    radiums = shrine.radiums,
                    Nature = shrine.Nature,
                    Sea = shrine.Sea,
                    Desert = shrine.Desert,
                    trans = trans
                });
                ref float timer = ref shrine.timer;
                if (timer > 0)
                {
                    timer -= deltaTime;
                    return;
                }
                timer = 1;
                for (int i = -shrine.radiums + 1; i <= shrine.radiums; i++)
                {
                    for (int j = -shrine.radiums + 1; j <= shrine.radiums; j++)
                    {
                        if (i is 0 or 1 && j is 0 or 1)
                            continue;
                        int2 pos = (trans.Position + new float3(i, 0f, j)).RoundToInt2();
                        if (tileLookup.GetType(pos, TileType.dugUpGround, out TileCD tileCD) && !tileLookup.HasType(pos, TileType.wateredGround))
                        {
                            ecb.AppendToBuffer(tileUpdateEntity, new TileUpdateBuffer
                            {
                                command = TileUpdateBuffer.Command.Add,
                                position = pos,
                                tile = new TileCD
                                {
                                    tileset = tileCD.tileset,
                                    tileType = TileType.wateredGround
                                }
                            });
                        }
                    }
                }
            })
                .WithName("VerdantShrine_Count")
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
    }
}
