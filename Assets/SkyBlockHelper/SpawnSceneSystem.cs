using Pug.UnityExtensions;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

namespace Assets.SkyBlockHelper
{
    public struct SpawnSceneCommand
    {
        public Entity Sender;
        public FixedString32Bytes Name;
        public int X;
        public int Y;
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(CommandReceiveSystemGroup))]
    public partial class SpawnSceneSystem : PugSimulationSystemBase
    {
        private static SpawnSceneSystem ins;
        private NativeQueue<SpawnSceneCommand> query;
        private ComponentLookup<LocalTransform> transLookup;
        public static void SpawnScene(Entity sender, string name, int x, int y)
        {
            ins.query.Enqueue(new()
            {
                Sender = sender,
                Name = name,
                X = x,
                Y = y
            });
        }
        protected override void OnCreate()
        {
            ins = this;
            query = new NativeQueue<SpawnSceneCommand>(Allocator.Persistent);
            transLookup = SystemAPI.GetComponentLookup<LocalTransform>();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            var scenes = SkyBlockHelper.sceneData;
            var transLookup = this.transLookup;
            while (query.TryDequeue(out var info))
            {
                var name = info.Name;
                var sender = info.Sender;
                if (transLookup.TryGetComponent(sender, out var trans))
                {
                    float2 playerPos = trans.Position.RoundToInt2();//player position to tile coordinate
                }
                if (!scenes.TryFindSceneByName(name.ToString(), out var scene))
                {
                    Debug.Log("Can't find scene " + name);
                    continue;
                }
                Entity block = ecb.CreateEntity();
                float2 pos = new(info.X, info.Y);
                ecb.AddComponent(block, new BlockedSpawnAreaCD(pos, scene.radius));

                Entity spawn = ecb.CreateEntity();
                ecb.AddComponent(spawn, LocalTransform.FromPosition((pos - scene.center).ToFloat3()));
                ecb.AddComponent(spawn, new SpawnCustomSceneCD
                {
                    name = name,
                    seed = PugRandom.GetSeed()
                });
            }
            base.OnUpdate();
        }
    }
}
