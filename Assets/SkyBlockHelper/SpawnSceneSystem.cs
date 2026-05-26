using Pug.UnityExtensions;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.SkyBlockHelper
{
    public struct SpawnSceneCommand
    {
        public FixedString32Bytes Name;
        public int X;
        public int Y;
    }
    public partial class SpawnSceneSystem : PugSimulationSystemBase
    {
        private static SpawnSceneSystem ins;
        private NativeQueue<SpawnSceneCommand> query;
        public static void SpawnScene(string name, int x, int y)
        {
            ins.query.Enqueue(new()
            {
                Name = name,
                X = x,
                Y = y
            });
        }
        protected override void OnCreate()
        {
            ins = this;
            query = new NativeQueue<SpawnSceneCommand>(Allocator.Persistent);
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            var scenes = SkyBlockHelper.sceneData;
            while (query.TryDequeue(out var info))
            {
                var name = info.Name;
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
