/*using Assets.THCompass.Helper;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace Assets.THCompass.System
{
    public struct SpawnRoomRPC : IRpcCommand
    {
        public float3 position;
        public int sceneIndex;
        public readonly void DoSpawn(EntityCommandBuffer ecb)
        {
            var scenes = THCompassMain.SceneData.scenes;
            new SpawnCustomSceneSystem().InvokeMethod("SpawnCustomScene", ecb, position, scenes[sceneIndex], PugRandom.GetRng());
            //if (SpawnRoomPatch.allowScenes.Contains(sceneIndex))
            {
                //var scss = SpawnRoomPatch.ins;
                //scss.InvokeMethod("DecrementSceneCount", scss, sceneIndex);
                //scss.InvokeMethod("SpawnCustomScene", ecb, position, scenes[sceneIndex], PugRandom.GetRng());
                //SpawnRoomPatch.des.ForceEnablePosition(position.ToFloat2(), 32, 0.5f);
            }
            //else Debug.Log(scenes[sceneIndex].sceneName + "已经存在于世界中");
        }
    }

    [UpdateInGroup(typeof(RunSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class ClientSpawnRoomSystem : PugSimulationSystemBase
    {
        private NativeQueue<SpawnRoomRPC> rpcQueue;
        private EntityArchetype rpcArchetype;

        protected override void OnCreate()
        {
            UpdatesInRunGroup();
            rpcQueue = new NativeQueue<SpawnRoomRPC>(Allocator.Persistent);
            rpcArchetype = EntityManager.CreateArchetype(typeof(SpawnRoomRPC), typeof(SendRpcCommandRequest));

            base.OnCreate();
        }

        public void SpawnRoom(float3 position, int sceneIndex)
        {
            rpcQueue.Enqueue(new SpawnRoomRPC
            {
                position = position,
                sceneIndex = sceneIndex
            });
        }
        protected override void OnUpdate()
        {
            EntityCommandBuffer ecb = CreateCommandBuffer();

            while (rpcQueue.TryDequeue(out SpawnRoomRPC component))
            {
                Entity e = ecb.CreateEntity(rpcArchetype);
                ecb.SetComponent(e, component);
                ecb.AddComponent(e, new SendRpcCommandRequest());
            }
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class ServerSpawnRoomSystem : PugSimulationSystemBase
    {
        protected override void OnUpdate()
        {
            bool guestMode = WorldInfo.guestMode;
            var ecb = CreateCommandBuffer();
            var databaseLocal = database;
            Entities.ForEach((Entity rpcEntity, in SpawnRoomRPC rpc, in ReceiveRpcCommandRequest req) =>
            {
                int adminLevel = 0;
                if (SystemAPI.HasComponent<ConnectionAdminLevelCD>(req.SourceConnection))
                    adminLevel = SystemAPI.GetComponent<ConnectionAdminLevelCD>(req.SourceConnection).Value;
                // I recommend to perform these checks, so that clients cannot abuse your mod to do griefing
                if (!guestMode || adminLevel > 0)
                {
                    rpc.DoSpawn(ecb);
                }
                ecb.DestroyEntity(rpcEntity);
            })
                .WithoutBurst()
                .Schedule();

            base.OnUpdate();
        }
    }
}*/
