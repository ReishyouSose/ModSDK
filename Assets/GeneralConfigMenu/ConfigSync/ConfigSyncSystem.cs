using Assets.GeneralConfigMenu.Scripts;
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
        private NativeQueue<ConfigDataRPC> send, receive;
        private NativeQueue<JoinRequest> join;
        private EntityArchetype dataArchetype, joinArchetype;
        private bool joinSended;

        protected override void OnCreate()
        {
            UpdatesInRunGroup();
            send = new NativeQueue<ConfigDataRPC>(Allocator.Persistent);
            receive = new NativeQueue<ConfigDataRPC>(Allocator.Persistent);
            join = new NativeQueue<JoinRequest>(Allocator.Persistent);
            dataArchetype = EntityManager.CreateArchetype(typeof(ConfigDataRPC), typeof(SendRpcCommandRequest));
            joinArchetype = EntityManager.CreateArchetype(typeof(JoinRequest), typeof(SendRpcCommandRequest));
            base.OnCreate();
        }
        public void SendConfigChange(ConfigEntryBase config)
        {
            if (ConfigManager.TryConvertConfig(config, out var data))
            {
                send.Enqueue(new ConfigDataRPC(data));
                Debug.Log("Send " + data);
            }
        }
        public void JoinRequest()
        {
            if (joinSended)
                return;
            joinSended = true;
            join.Enqueue(new JoinRequest());
        }
        protected override void OnUpdate()
        {
            EntityCommandBuffer ecb = CreateCommandBuffer();

            while (send.TryDequeue(out ConfigDataRPC sender))
            {
                Entity e = ecb.CreateEntity(dataArchetype);
                ecb.SetComponent(e, sender);
            }

            while (join.TryDequeue(out JoinRequest joiner))
            {
                Debug.Log("Try request config data");
                Entity e = ecb.CreateEntity(joinArchetype);
                ecb.AddComponent(e, joiner);
            }

            var queue = receive;
            Entities.ForEach((Entity rpcEntity, in ConfigDataRPC rpc) =>
            {
                queue.Enqueue(rpc);
                ecb.DestroyEntity(rpcEntity);
            })
                .WithAll<ReceiveRpcCommandRequest>()
                .WithBurst()
                .Schedule();

            var ins = ConfigManager.Instance;
            if (ins == null || !ins.Loaded)
                return;
            if (Manager.main.player == null)
            {
                if (joinSended)
                    joinSended = false;
                return;
            }
            while (receive.TryDequeue(out ConfigDataRPC reader))
            {
                reader.TryChangeConfig();
            }
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class ConfigSyncServer : PugSimulationSystemBase
    {
        private NativeQueue<ConfigDataRPC> rpcQueue;
        private EntityArchetype rpcArchetype;

        protected override void OnCreate()
        {
            rpcQueue = new NativeQueue<ConfigDataRPC>(Allocator.Persistent);
            rpcArchetype = EntityManager.CreateArchetype(typeof(ConfigDataRPC), typeof(SendRpcCommandRequest));
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            EntityCommandBuffer ecb = CreateCommandBuffer();
            var queue = rpcQueue;
            Entities.ForEach((Entity rpcEntity, in ConfigDataRPC rpc) =>
            {
                queue.Enqueue(rpc);
                ecb.DestroyEntity(rpcEntity);
            })
                .WithAll<ReceiveRpcCommandRequest>()
                .WithBurst()
                .Run();

            while (rpcQueue.TryDequeue(out ConfigDataRPC component))
            {
                Entity e = ecb.CreateEntity(rpcArchetype);
                ecb.SetComponent(e, component);
            }
        }
    }
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class JoinSyncServer : PugSimulationSystemBase
    {
        private NativeQueue<SendRpcCommandRequest> rpcQueue;
        protected override void OnCreate()
        {
            UpdatesInRunGroup();
            rpcQueue = new NativeQueue<SendRpcCommandRequest>(Allocator.Persistent);
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var queue = rpcQueue;
            var ecb = CreateCommandBuffer();
            Entities.ForEach((Entity e, in ReceiveRpcCommandRequest req) =>
            {
                queue.Enqueue(new() { TargetConnection = req.SourceConnection });
                ecb.DestroyEntity(e);
            })
                .WithAll<JoinRequest>()
                .WithBurst()
                .Run();

            while (rpcQueue.TryDequeue(out SendRpcCommandRequest sender))
            {
                foreach (var configFile in ConfigFile.AllConfigFilesReadOnly)
                {
                    if (configFile.ConfigFilePath.StartsWith("CoreLib"))
                        continue;
                    foreach (var (_, entry) in configFile.Entries)
                    {
                        if (!entry.Scope.ShouldSync)
                            continue;
                        if (ConfigManager.TryConvertConfig(entry, out var data))
                        {
                            var e = ecb.CreateEntity();
                            ecb.AddComponent(e, new ConfigDataRPC(data, -1));
                            ecb.AddComponent(e, sender);
                        }
                    }
                }
            }
        }
    }
}
