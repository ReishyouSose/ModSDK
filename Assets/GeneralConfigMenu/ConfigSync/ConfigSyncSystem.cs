using CoreLib.Data.Configuration;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Assets.GeneralConfigMenu.ConfigSync
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class ConfigSyncClient : PugSimulationSystemBase
    {
        private NativeQueue<ConfigChangeRPC> rpcQueue;
        private EntityArchetype rpcArchetype;

        protected override void OnCreate()
        {
            UpdatesInRunGroup();
            rpcQueue = new NativeQueue<ConfigChangeRPC>(Allocator.Persistent);
            rpcArchetype = EntityManager.CreateArchetype(typeof(ConfigChangeRPC), typeof(SendRpcCommandRequest));

            base.OnCreate();
        }
        public void SendConfigChange(string mod, string file, ConfigDefinition def, string value)
        {
            try
            {
                rpcQueue.Enqueue(new ConfigChangeRPC(mod, file, def, value));
            }
            catch
            {
                Debug.Log("Too long config bytes");
            }
        }
        protected override void OnUpdate()
        {
            EntityCommandBuffer ecb = CreateCommandBuffer();

            while (rpcQueue.TryDequeue(out ConfigChangeRPC component))
            {
                Entity e = ecb.CreateEntity(rpcArchetype);
                ecb.SetComponent(e, component);
                ecb.AddComponent(e, new SendRpcCommandRequest());
            }

            Entities.ForEach((Entity rpcEntity, in ConfigChangeRPC rpc) =>
            {
                rpc.TryChangeConfig();
                ecb.DestroyEntity(rpcEntity);
            })
                .WithAll<ReceiveRpcCommandRequest>()
                .WithoutBurst()
                .Run();
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class ConfigSyncServer : PugSimulationSystemBase
    {
        private NativeQueue<ConfigChangeRPC> rpcQueue;
        private EntityArchetype rpcArchetype;

        protected override void OnCreate()
        {
            UpdatesInRunGroup();
            rpcQueue = new NativeQueue<ConfigChangeRPC>(Allocator.Persistent);
            rpcArchetype = EntityManager.CreateArchetype(typeof(ConfigChangeRPC), typeof(SendRpcCommandRequest));

            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            EntityCommandBuffer ecb = CreateCommandBuffer();
            Entities.ForEach((Entity rpcEntity, in ConfigChangeRPC rpc) =>
            {
                rpcQueue.Enqueue(rpc);
                ecb.DestroyEntity(rpcEntity);
            })
                .WithAll<ReceiveRpcCommandRequest>()
                .WithoutBurst()
                .Run();

            while (rpcQueue.TryDequeue(out ConfigChangeRPC component))
            {
                Entity e = ecb.CreateEntity(rpcArchetype);
                ecb.SetComponent(e, component);
                ecb.AddComponent(e, new SendRpcCommandRequest());
            }
        }
    }
}
