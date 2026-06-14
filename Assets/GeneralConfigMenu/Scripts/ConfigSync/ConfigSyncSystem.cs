using CoreLib.Data.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts.ConfigSync
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
    public partial class ConfigSyncSystem : PugSimulationSystemBase
    {
        private int sendTimes;
        private List<(Entity, List<(int, string)>)> send;
        private List<ConfigEntryBase> configs;
        private NativeQueue<Entity> syncAll;
        private ComponentLookup<ConfigSyncRpc> syncLookup;
        private ComponentLookup<ConfigEntryRpc> entryLookup;
        private ComponentLookup<ConfigChunkRpc> chunkLookup;
        private ComponentLookup<ReceiveRpcCommandRequest> receiveLookup;
        private ComponentLookup<PlayerGhost> playerLookup;
        private EntityArchetype syncArchetype;
        private EntityArchetype entryArchetype;
        private EntityArchetype chunkArchetype;
        private EntityQuery query;
        private Dictionary<(int syncID, Entity connect), ConfigSyncCollector> collectors;
        private Dictionary<(int, Entity), List<ConfigEntryRpc>> pendingEntries;
        private Dictionary<(int, Entity), List<ConfigChunkRpc>> pendingChunks;

        protected override void OnCreate()
        {
            send = new();
            configs = new();
            if (isServer)
            {
                playerLookup = SystemAPI.GetComponentLookup<PlayerGhost>();
                RequireForUpdate<WorldInfoCD>();
            }
            foreach (var configFile in ConfigFile.AllConfigFilesReadOnly)
            {
                if (configFile.ConfigFilePath.StartsWith("CoreLib"))
                    continue;
                configFile.SaveOnConfigSet = false;
                foreach (var entry in configFile.Entries.Values)
                {
                    if (!entry.Scope.ShouldSync)
                        continue;
                    configs.Add(entry);
                }
            }

            Debug.Log($"Registered {configs.Count} configs");
            syncAll = new(Allocator.Persistent);
            syncArchetype = EntityManager.CreateArchetype(typeof(ConfigSyncRpc), typeof(SendRpcCommandRequest));
            entryArchetype = EntityManager.CreateArchetype(typeof(ConfigEntryRpc), typeof(SendRpcCommandRequest));
            chunkArchetype = EntityManager.CreateArchetype(typeof(ConfigChunkRpc), typeof(SendRpcCommandRequest));
            query = SystemAPI.QueryBuilder().WithAll<ReceiveRpcCommandRequest>().WithAny<ConfigSyncRpc, ConfigEntryRpc, ConfigChunkRpc>().Build();
            syncLookup = SystemAPI.GetComponentLookup<ConfigSyncRpc>();
            entryLookup = SystemAPI.GetComponentLookup<ConfigEntryRpc>();
            chunkLookup = SystemAPI.GetComponentLookup<ConfigChunkRpc>();
            receiveLookup = SystemAPI.GetComponentLookup<ReceiveRpcCommandRequest>();
            collectors = new();
            pendingEntries = new();
            pendingChunks = new();
            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            syncAll.Dispose();
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            EntityCommandBuffer ecb = CreateCommandBuffer();
            bool isServer = this.isServer;
            var send = this.send;
            var configs = this.configs;
            var syncAll = this.syncAll;
            var syncArchetype = this.syncArchetype;
            var entryArchetype = this.entryArchetype;
            var chunkArchetype = this.chunkArchetype;
            var syncLookup = this.syncLookup;
            var entryLookup = this.entryLookup;
            var chunkLookup = this.chunkLookup;
            var receiveLookup = this.receiveLookup;
            var collectors = this.collectors;
            var pendingEntries = this.pendingEntries;
            var pendingChunks = this.pendingChunks;

            using var entities = query.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                ecb.DestroyEntity(entity);
                var source = receiveLookup[entity].SourceConnection;
                if (syncLookup.TryGetComponent(entity, out ConfigSyncRpc sync))
                {
                    var key = (sync.SyncID, source);
                    collectors[key] = new ConfigSyncCollector(sync);
                }
                else if (entryLookup.TryGetComponent(entity, out ConfigEntryRpc entry))
                {
                    var key = (entry.SyncID, source);
                    if (!pendingEntries.TryGetValue(key, out var list))
                        list = pendingEntries[key] = new();
                    list.Add(entry);
                }
                else if (chunkLookup.TryGetComponent(entity, out ConfigChunkRpc chunk))
                {
                    var key = (chunk.SyncID, source);
                    if (!pendingChunks.TryGetValue(key, out var list))
                        list = pendingChunks[key] = new();
                    list.Add(chunk);
                }
            }

            var entryKeys = pendingEntries.Keys.ToArray();
            foreach (var key in entryKeys)
            {
                if (collectors.TryGetValue(key, out var collector))
                {
                    var entries = pendingEntries[key];
                    int count = entries.Count;
                    for (int i = count - 1; i >= 0; i--)
                    {
                        collector.CollectEntry(entries[i]);
                        entries.RemoveAt(i);
                    }
                }
            }
            var chunkKeys = pendingChunks.Keys.ToArray();
            foreach (var key in chunkKeys)
            {
                if (collectors.TryGetValue(key, out var collector))
                {
                    var chunks = pendingChunks[key];
                    int count = chunks.Count;
                    for (int i = count - 1; i >= 0; i--)
                    {
                        if (collector.CollectChunk(chunks[i]))
                            chunks.RemoveAt(i);
                    }
                }
            }

            var collectorKeys = collectors.Keys.ToArray();
            bool guest = false;
            bool adminOnly = false;
            if (isServer)
            {
                if (SystemAPI.TryGetSingleton(out WorldInfoCD worldInfo))
                    guest = worldInfo.guestMode;
                adminOnly = GeneralConfigMenuMod.config.AdminOnly.Value;
            }
            foreach (var key in collectorKeys)
            {
                var collector = collectors[key];
                if (collector.Completed)
                {
                    collectors.Remove(key);
                    pendingEntries.Remove(key);
                    pendingChunks.Remove(key);
                    bool apply = collector.ShouldApply;
                    if (isServer || !apply)
                    {
                        var sync = ecb.CreateEntity(syncArchetype);
                        int syncID = collector.SyncID;
                        Entity connect = isServer && !apply ? key.connect : Entity.Null;
                        var player = collector.Player;
                        ecb.SetComponent(sync, new ConfigSyncRpc(syncID, collector.ConfigCount, player));
                        ecb.SetComponent(sync, new SendRpcCommandRequest() { TargetConnection = connect });
                        int privileges = apply ? playerLookup[player].adminPrivileges : 0;
                        foreach (var (id, change) in collector.Changes)
                        {
                            var entry = ecb.CreateEntity(entryArchetype);
                            var config = configs[id];
                            string name = config.GetFullName();
                            string value = Encoding.UTF8.GetString(change.Value);
                            if (apply)
                            {
                                if (privileges > 0 || (!guest && !adminOnly && config.Scope.accessLevel == ConfigAccessLevel.Server))
                                {
                                    config.SetSerializedValue(value);
                                    value = config.GetSerializedValue();
                                    Debug.Log($"Accepted {name} = {value}");
                                }
                                else
                                    Debug.Log("Rejected " + name);
                            }
                            byte[] bytes = Encoding.UTF8.GetBytes(value);
                            int count = (bytes.Length - 1) / 16 + 1;
                            ecb.SetComponent(entry, new ConfigEntryRpc(syncID, id, count, bytes.Length));
                            ecb.SetComponent(entry, new SendRpcCommandRequest() { TargetConnection = connect });
                            for (int i = 0; i < count; i++)
                            {
                                var chunk = ecb.CreateEntity(chunkArchetype);
                                ecb.SetComponent(chunk, new ConfigChunkRpc(syncID, id, i * 16, bytes));
                                ecb.SetComponent(chunk, new SendRpcCommandRequest() { TargetConnection = connect });
                            }
                        }
                        if (apply && Manager.networking.currentSessionIsDedicatedServer)
                        {
                            var file = configs[collector.Changes.Keys.First()].ConfigFile;
                            file.Save();
                            Debug.Log("Saved " + file.ConfigFilePath);
                        }
                    }
                    else
                    {
                        var menu = ModConfigMenu.Instance;
                        var syncID = collector.SyncID;
                        if (syncID == 0 || syncID == sendTimes)
                            menu.SetWaitState(false);
                        var receive = menu.Receive;
                        foreach (var (id, change) in collector.Changes)
                        {
                            var entry = ecb.CreateEntity(entryArchetype);
                            string value = Encoding.UTF8.GetString(change.Value);
                            receive.Add((id, value));
                        }
                    }
                }
            }

            while (send.Count > 0)
            {
                foreach (var (target, modifies) in send)
                {
                    int syncID = isServer ? 0 : ++sendTimes;
                    ConfigSyncCollector collector = new()
                    {
                        SyncID = syncID,
                        ConfigCount = modifies.Count,
                        Player = isServer ? Entity.Null : Manager.main.player.entity,
                    };
                    collectors.Add((syncID, target), collector);
                    var changes = collector.Changes;
                    foreach (var (id, value) in modifies)
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(value);
                        int count = (bytes.Length - 1) / 16 + 1;
                        var change = changes[id] = new() { ChunkCount = count, Value = bytes };
                    }
                }
                send.Clear();
            }

            if (isServer)
            {
                Entities.ForEach((Entity rpcEntity, in ReceiveRpcCommandRequest receive) =>
                {
                    syncAll.Enqueue(receive.SourceConnection);
                    ecb.DestroyEntity(rpcEntity);
                })
                    .WithBurst()
                    .WithAll<SyncAllConfigRpc>()
                    .Schedule();
            }

            while (syncAll.TryDequeue(out var connect))
            {
                if (isServer)
                {
                    List<(int, string)> changes = new();
                    for (int i = 0; i < configs.Count; i++)
                    {
                        changes.Add(((ushort)i, configs[i].GetSerializedValue()));
                    }
                    send.Add((connect, changes));
                }
                else
                {
                    var e = ecb.CreateEntity();
                    ecb.AddComponent<SyncAllConfigRpc>(e);
                    ecb.AddComponent<SendRpcCommandRequest>(e);
                }
            }
            base.OnUpdate();
        }
        public ConfigEntryBase GetEntryByID(int id) => configs[id];
        public void RequestSyncAll() => syncAll.Enqueue(Entity.Null);

        public List<(int, string)> SendConfigChange()
        {
            List<(int, string)> modifies = new();
            send.Add((Entity.Null, modifies));
            return modifies;
        }
    }
}