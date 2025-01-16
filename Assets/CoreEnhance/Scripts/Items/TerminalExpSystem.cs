using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.NetCode;

namespace Assets.CoreEnhance.Scripts.Items
{
    public struct ReceiveExpRpc : IRpcCommand
    {
        public Entity Source;
        public Entity Player;
        public byte Skill;
        public ReceiveExpRpc(Entity source, Entity player, SkillID skill)
        {
            Source = source;
            Player = player;
            Skill = (byte)skill;
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class ReceiveExpClient : PugSimulationSystemBase
    {
        private NativeQueue<ReceiveExpRpc> queue;
        private EntityArchetype archetype;
        private static ReceiveExpClient ins;
        protected override void OnCreate()
        {
            ins = this;
            queue = new(Allocator.Persistent);
            archetype = EntityManager.CreateArchetype(typeof(ReceiveExpRpc), typeof(SendRpcCommandRequest));
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            while (queue.TryDequeue(out var exp))
            {
                var e = ecb.CreateEntity(archetype);
                ecb.SetComponent(e, exp);
            }
            base.OnUpdate();
        }
        public static void ReceiveExp(Entity autoFisher, Entity player, SkillID skill)
        {
            ins.queue.Enqueue(new(autoFisher, player, skill));
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class ReceiveExpServer : PugSimulationSystemBase
    {
        private ComponentLookup<ObjectDataCD> objDataLookup;
        protected override void OnCreate()
        {
            objDataLookup = SystemAPI.GetComponentLookup<ObjectDataCD>();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            var objDataLookup = this.objDataLookup;
            Entities.ForEach((Entity e, in ReceiveExpRpc exp) =>
            {
                ref var objData = ref objDataLookup.GetRefRW(exp.Source).ValueRW;
                PlayerController.AddSkill(exp.Player, (SkillID)exp.Skill, objData.amount - 1, ecb, true);
                objData.amount = 1;
                ecb.DestroyEntity(e);
            })
                .WithName("Terminal_ReceiveExp")
                .WithAll<ReceiveRpcCommandRequest>()
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
    }
}
