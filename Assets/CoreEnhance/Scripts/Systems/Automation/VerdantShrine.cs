using Assets.CoreEnhance.Scripts.Items;
using PugTilemap;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.CoreEnhance.Scripts.Systems.Automation
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class VerdantShrineSystem : PugSimulationSystemBase
    {
        private ComponentLookup<GrowingCD> growingLookup;
        private TileAccessor tileAccessor;
        protected override void OnCreate()
        {
            growingLookup = SystemAPI.GetComponentLookup<GrowingCD>();
            RequireForUpdate<TileUpdateBuffer>();
            base.OnCreate();
        }
        protected override void OnStartRunning()
        {
            tileAccessor = CreateTileAccessor();
            base.OnStartRunning();
        }
        protected override void OnUpdate()
        {
            float deltaTime = World.Time.DeltaTime;
            var tileLookup = tileAccessor;
            var ecb = CreateCommandBuffer();
            var tileUpdateEntity = SystemAPI.GetSingletonEntity<TileUpdateBuffer>();
            NativeHashMap<int2, VerdantShrineCD> shrines = new(128, Allocator.Temp);
            /*var job = */
            Entities.ForEach((DynamicBuffer<ContainedObjectsBuffer> containers, ref VerdantShrineCD shrine, in LocalTransform trans) =>
            {
                shrine.Nature = containers[0].amount;
                shrine.Sea = containers[1].amount;
                shrine.Desert = containers[2].amount;
                shrines.Add(trans.Position.RoundToInt2(), shrine);
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
                        if (i != 0 || j != 0)
                        {
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
                }
            })
              .WithName("CountVerdantShrine")
              .WithBurst()
              //.ScheduleParallel(Dependency);
              .Run();

            /*Entities.ForEach((Entity e, ref GrowingCD growing, ref PlantCD plant, in LocalTransform trans) =>
            {

            })
                .WithName("VerdantShrineEffect")
                .WithBurst()
                .ScheduleParallel(job);*/
            base.OnUpdate();
        }
        private bool ShrineHovering(VerdantShrineCD shrine, int2 o, int2 t)
        {
            int r = shrine.radiums;
            return t.x >= o.x - r && t.x <= o.x + r && t.y >= o.y - r && t.y <= o.y + r;
        }
        private void DoWater()
        {

            /*if (shrine.timer > 0f)
            {
                shrine.timer -= deltaTime;
                return;
            }
            shrine.timer = 1;
            int r = (int)near.radius;
            for (int i = -r; i <= r; i++)
            {
                for (int j = -r; j <= r; j++)
                {
                    if (i != 0 || j != 0)
                    {
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
            }*/
        }
    }
}
