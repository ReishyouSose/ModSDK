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
        private NativeQueue<ConfigDataRPC> send;
        private EntityArchetype dataArchetype;

        protected override void OnCreate()
        {
            UpdatesInRunGroup();
            send = new NativeQueue<ConfigDataRPC>(Allocator.Persistent);
            dataArchetype = EntityManager.CreateArchetype(typeof(ConfigDataRPC), typeof(SendRpcCommandRequest));
            base.OnCreate();
        }
        public void SendConfigChange(ConfigEntryBase config)
        {
            if (ModConfigMenu.TryConvertConfig(config, out var data))
            {
                send.Enqueue(new ConfigDataRPC(data));
                Debug.Log("Send " + data);
            }
        }
        protected override void OnUpdate()
        {
            EntityCommandBuffer ecb = CreateCommandBuffer();

            while (send.TryDequeue(out ConfigDataRPC sender))
            {
                Entity e = ecb.CreateEntity(dataArchetype);
                ecb.SetComponent(e, sender);
            }

            var queue = ModConfigMenu.Receive;
            Entities.ForEach((Entity rpcEntity, in ConfigDataRPC rpc) =>
            {
                queue.Enqueue(rpc);
                ecb.DestroyEntity(rpcEntity);
            })
                .WithAll<ReceiveRpcCommandRequest>()
                .WithBurst()
                .Schedule();
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class ConfigSyncServer : PugSimulationSystemBase
    {
        private NativeQueue<ConfigDataRPC> dataQueue;
        private NativeQueue<SendRpcCommandRequest> playerQueue;
        private EntityArchetype archetype;
        private struct ConfigDatasSended : IComponentData { }
        protected override void OnCreate()
        {
            dataQueue = new NativeQueue<ConfigDataRPC>(Allocator.Persistent);
            playerQueue = new NativeQueue<SendRpcCommandRequest>(Allocator.Persistent);
            archetype = EntityManager.CreateArchetype(typeof(ConfigDataRPC), typeof(SendRpcCommandRequest));
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            EntityCommandBuffer ecb = CreateCommandBuffer();
            var queue = dataQueue;
            Entities.ForEach((Entity rpcEntity, in ConfigDataRPC rpc) =>
            {
                queue.Enqueue(rpc);
                ecb.DestroyEntity(rpcEntity);
            })
                .WithAll<ReceiveRpcCommandRequest>()
                .WithBurst()
                .Schedule();

            while (dataQueue.TryDequeue(out ConfigDataRPC component))
            {
                Entity e = ecb.CreateEntity(archetype);
                ecb.SetComponent(e, component);
            }

            var send = playerQueue;
            Entities.ForEach((Entity e, in PlayerGhost player) =>
            {
                send.Enqueue(new() { TargetConnection = player.connection });
                ecb.AddComponent<ConfigDatasSended>(e);
            })
                .WithNone<ConfigDatasSended>()
                .WithBurst()
                .Schedule();

            while (playerQueue.TryDequeue(out SendRpcCommandRequest sender))
            {
                Debug.LogWarning("Send All Config Data");
                foreach (var configFile in ConfigFile.AllConfigFilesReadOnly)
                {
                    if (configFile.ConfigFilePath.StartsWith("CoreLib"))
                        continue;
                    foreach (var (_, entry) in configFile.Entries)
                    {
                        if (!entry.Scope.ShouldSync)
                            continue;
                        if (ModConfigMenu.TryConvertConfig(entry, out var data))
                        {
                            var e = ecb.CreateEntity(archetype);
                            ecb.SetComponent(e, new ConfigDataRPC(data));
                            ecb.SetComponent(e, sender);
                        }
                    }
                }
            }
        }
    }
}
