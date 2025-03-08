using Assets.CoreEnhance.Scripts.Components;
using Assets.CoreEnhance.Scripts.Items;
using Assets.CoreEnhance.Scripts.Systems.Misc;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;

namespace Assets.CoreEnhance.Scripts.Systems.Automation
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class AutoFisherTerminalClient : PugSimulationSystemBase
    {
        private static AutoFisherTerminalClient ins;
        private NativeQueue<OpenTerminalCD> queue;
        private EntityArchetype archetype;
        protected override void OnCreate()
        {
            ins = this;
            queue = new(Allocator.Persistent);
            archetype = EntityManager.CreateArchetype(typeof(OpenTerminalCD), typeof(SendRpcCommandRequest));
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            while (queue.TryDequeue(out _))
            {
                ecb.CreateEntity(archetype);
            }
            base.OnUpdate();
        }
        public static void OpenTerminal()
        {
            var skill = SkillID.Fishing;
            int skillValue = Manager.saves.GetSkillValue(skill);
            int num = (int)math.floor(SkillExtensions.GetLevelFromSkill(skill, skillValue) / 5f);
            if (num >= 100)
                return;
            ins.queue.Enqueue(new(Manager.main.player.entity, TerminalType.AutoFisher));
        }
    }

    [UpdateAfter(typeof(UniquePlaceableSystem))]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class AutoFisherTerminalServer : PugSimulationSystemBase
    {
        private NativeQueue<OpenTerminalRPC> queue;
        private int timer;
        protected override void OnCreate()
        {
            queue = new(Allocator.Persistent);
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            if (timer < 180)
            {
                timer++;
                return;
            }
            timer = 0;
            var ecb = CreateCommandBuffer();
            /*var queue = this.queue;
            Entities.ForEach((Entity e, in OpenTerminalRPC rpc) =>
            {
                if ((TerminalType)rpc.TerminalType == TerminalType.AutoFisher)
                {
                    queue.Enqueue(rpc);
                    ecb.DestroyEntity(e);
                }
            })
                .WithName("AutoFisherTerminal_CheckOpen")
                .WithAll<ReceiveRpcCommandRequest>()
                .WithBurst()
                .Schedule();

            while (queue.TryDequeue(out var open))
            {
                if (SystemAPI.TryGetSingletonEntity<AutoFisherTerminalCD>(out var terminal))
                {
                    ecb.AddComponent(terminal, new OpenTerminalCD()
                    {
                        Player = open.Player
                    });
                }
            }*/

            Entities.ForEach((Entity e, ref ObjectDataCD objData, in DistanceToPlayerCD dis) =>
            {
                PlayerController.AddSkill(dis.closestPlayer, SkillID.Fishing, objData.amount - 1, ecb, true);
                objData.amount = 1;
            })
                .WithName("AutoFisherTerminal_CollectLoots")
                .WithAll<AutoFisherTerminalCD>()
                .WithBurst()
                .Schedule();

            base.OnUpdate();
        }
    }
}
