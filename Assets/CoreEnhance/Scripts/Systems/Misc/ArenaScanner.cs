using Assets.CoreEnhance.Scripts.Helpers;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace Assets.CoreEnhance.Scripts.Systems.Misc
{
    public struct LocalArenaRpc : IRpcCommand
    {
        public float2 Pos;
        public LocalArenaRpc(float2 pos) => Pos = pos;
    }
    public struct ArenaScannerSwitchRpc : IRpcCommand
    {
        public int Player;
        public bool Open;
        public ArenaScannerSwitchRpc(int player, bool open)
        {
            Player = player;
            Open = open;
        }
    }
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class ArenaScannerClient : PugSimulationSystemBase
    {
        public static ArenaScannerClient Ins;
        public NativeHashSet<float2> arenas;
        private NativeQueue<float2> queue;
        private NativeQueue<ArenaScannerSwitchRpc> switcher;
        private EntityArchetype archetype;
        protected override void OnCreate()
        {
            Ins = this;
            arenas = new(10, Allocator.Persistent);
            queue = new(Allocator.Persistent);
            switcher = new(Allocator.Persistent);
            archetype = EntityManager.CreateArchetype(typeof(ArenaScannerSwitchRpc), typeof(SendRpcCommandRequest));
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var queue = this.queue;
            var ecb = CreateCommandBuffer();
            Entities.ForEach((Entity e, in LocalArenaRpc rpc) =>
            {
                queue.Enqueue(rpc.Pos);
                ecb.DestroyEntity(e);
            })
                .WithName("ArenaScanner_ReceivePos")
                .WithAll<ReceiveRpcCommandRequest>()
                .WithBurst()
                .Schedule();

            while (queue.TryDequeue(out var rpc))
            {
                arenas.Add(rpc);
            }
            while(switcher.TryDequeue(out var rpc))
            {
                Entity e = ecb.CreateEntity(archetype);
                ecb.SetComponent(e, rpc);
            }

            base.OnUpdate();
        }
        public static void SiwtchFunction(bool open)
        {
            Ins.switcher.Enqueue(new(Manager.main.player.playerIndex, open));
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class ArenaScannerServer : PugSimulationSystemBase
    {
        private NativeQueue<ArenaScannerSwitchRpc> switcher;
        private EntityArchetype archetype;
        private float timer;
        private NativeHashMap<int, bool> openMap;
        private NativeHashSet<float2> arenas;
        protected override void OnCreate()
        {
            switcher = new(Allocator.Persistent);
            archetype = EntityManager.CreateArchetype(typeof(LocalArenaRpc), typeof(SendRpcCommandRequest));
            openMap = new(16, Allocator.Persistent);
            arenas = new(10, Allocator.Persistent);
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            var switcher = this.switcher;
            var openMap = this.openMap;
            Entities.ForEach((Entity e, in ArenaScannerSwitchRpc rpc) =>
            {
                switcher.Enqueue(rpc);
                ecb.DestroyEntity(e);
            })
                .WithName("ArenaScanner_Switch")
                .WithAll<ReceiveRpcCommandRequest>()
                .WithBurst()
                .Schedule();

            while (switcher.TryDequeue(out var rpc))
            {
                openMap[rpc.Player] = rpc.Open;
            }

            foreach (var info in openMap)
            {
                if (!info.Value)
                    return;
            }
            if (timer < 1)
            {
                timer += World.Time.DeltaTime;
                return;
            }
            timer = 0;

            var archetype = this.archetype;
            var arenas = this.arenas;
            Entities.ForEach((in LocalTransform trans) =>
            {
                var pos = trans.Position.xz;
                if (pos.HasNaN())
                    return;
                if (arenas.Contains(pos))
                    return;
                arenas.Add(pos);
                Entity e = ecb.CreateEntity(archetype);
                ecb.SetComponent(e, new LocalArenaRpc(pos));
            })
                .WithName("ArenaScanner_SendPos")
                .WithAll<EventTerminalCD>()
                .WithBurst()
                .Schedule();

            base.OnCreate();
        }
    }
}
