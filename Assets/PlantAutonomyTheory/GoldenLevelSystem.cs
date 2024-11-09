using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Assets.PlantAutonomyTheory
{
    public struct GoldPlantRPC : IRpcCommand
    {
        public int user;
        public int goldLevel;
    }
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class GoldenLevelClient : PugSimulationSystemBase
    {
        private int goldLevel = -1;
        private NativeQueue<GoldPlantRPC> rpcQueue;
        private EntityArchetype rpcArchetype;
        protected override void OnCreate()
        {
            UpdatesInRunGroup();
            rpcQueue = new NativeQueue<GoldPlantRPC>(Allocator.Persistent);
            rpcArchetype = EntityManager.CreateArchetype(typeof(GoldPlantRPC), typeof(SendRpcCommandRequest));
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            base.OnUpdate();
            var player = Manager.main.player;
            if (player == null)
                return;

            var talent = Manager.saves.GetSkillTalentTreesPoints(SkillID.Gardening);
            int gl = (talent != null && talent.Count > 6) ? talent[6] : 0;

            if (gl != goldLevel)
            {
                rpcQueue.Enqueue(new()
                {
                    goldLevel = gl,
                    user = player.playerName.GetHashCode(),
                });
                goldLevel = gl;
            }
            EntityCommandBuffer ecb = CreateCommandBuffer();
            while (rpcQueue.TryDequeue(out var component))
            {
                Entity e = ecb.CreateEntity(rpcArchetype);
                ecb.SetComponent(e, component);
                ecb.AddComponent(e, new SendRpcCommandRequest());
            }
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class GoldenLevelServer : PugSimulationSystemBase
    {
        private Dictionary<int, int> playersGold = new();
        internal static int max;
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            NativeHashMap<int, int> newInfo = new(4, Allocator.Temp);
            Entities.ForEach((Entity e, in GoldPlantRPC rpc) =>
            {
                newInfo.Add(rpc.user, rpc.goldLevel);
                ecb.DestroyEntity(e);
            })
                .WithAll<ReceiveRpcCommandRequest>()
                .WithBurst()
                .Run();

            if (newInfo.Count > 0)
            {
                if (playersGold.Count == 0)
                {
                    Init(newInfo);
                }
                else
                {
                    var oldInfo = GetOldInfo();
                    playersGold = new();
                    foreach (var player in Manager.main.allPlayers)
                    {
                        int user = player.playerName.GetHashCode();
                        int value = newInfo.TryGetValue(user, out var skill) ? skill : oldInfo.TryGetValue(user, out skill) ? skill : 0;
                        playersGold[user] = value;
                    }
                    oldInfo.Dispose();
                }
                newInfo.Dispose();
                max = playersGold.Max(x => x.Value);
                Debug.Log("Current Infos: \n" + string.Join("\n", playersGold));
                Debug.Log(max);
            }
        }
        private NativeHashMap<int, int> GetOldInfo()
        {
            NativeHashMap<int, int> result = new(playersGold.Count, Allocator.Temp);
            foreach (var (user, level) in playersGold)
            {
                result[user] = level;
            }
            return result;
        }
        private void Init(NativeHashMap<int, int> newInfo)
        {
            using var infos = newInfo.GetEnumerator();
            while (infos.MoveNext())
            {
                var info = infos.Current;
                playersGold[info.Key] = info.Value;
            }
        }
    }
}
