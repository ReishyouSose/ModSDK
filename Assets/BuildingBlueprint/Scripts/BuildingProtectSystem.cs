using CoreLib.Util.Extension;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts
{
    public struct SwitchProtectStateRpc : IRpcCommand
    {
        public bool State;
        public Entity Player;
    }

    [GhostComponent]
    public struct ProtectStateCD : IComponentData
    {
        [GhostField]
        public bool State;

        public bool RemovePreviousFrame;
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class BuildingProtectClient : PugSimulationSystemBase
    {
        private static BuildingProtectClient ins;
        private EntityArchetype archetype;
        private NativeQueue<bool> queue;
        protected override void OnCreate()
        {
            ins = this;
            queue = new(Allocator.Persistent);
            archetype = EntityManager.CreateArchetype(typeof(SwitchProtectStateRpc), typeof(SendRpcCommandRequest));
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            while (queue.TryDequeue(out bool state))
            {
                var e = ecb.CreateEntity(archetype);
                ecb.SetComponent(e, new SwitchProtectStateRpc() { State = state, Player = Manager.main.player.entity });
            }
            base.OnUpdate();
        }
        public static void SwitchState(bool state)
        {
            if (Manager.main.player)
                ins.queue.Enqueue(state);
        }
    }
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class BuildingProtectServer : PugSimulationSystemBase
    {
        private ComponentLookup<ImmunityZoneCD> immuneLookup;
        protected override void OnCreate()
        {
            immuneLookup = SystemAPI.GetComponentLookup<ImmunityZoneCD>();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            Entities.ForEach((Entity e, in SwitchProtectStateRpc rpc) =>
            {
                ecb.DestroyEntity(e);
                ecb.SetComponent(rpc.Player, new ProtectStateCD() { State = rpc.State });
            })
                .WithName("SwitchProtectState")
                .WithAll<ReceiveRpcCommandRequest>()
                .WithBurst()
                .Schedule();

            var immuneLookup = this.immuneLookup;
            Entities.ForEach((Entity e, ref ProtectStateCD protect) =>
            {
                if (protect.State)
                {
                    if (!immuneLookup.HasComponent(e))
                        ecb.AddComponent(e, new ImmunityZoneCD()
                        {
                            useRectangularBounds = true,
                            rectangularWidth = 10,
                            rectangularHeight = 10,
                        });
                }
                else
                {
                    if (protect.RemovePreviousFrame)
                    {
                        ecb.RemoveComponent<ImmunityZoneCD>(e);
                        protect.RemovePreviousFrame = false;
                    }
                    var optional = immuneLookup.GetRefRWOptional(e);
                    if (optional.IsValid)
                    {
                        optional.ValueRW.removeImmunityZone = true;
                        protect.RemovePreviousFrame = true;
                    }
                }
            })
                .WithName("BuildingProtect")
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
        internal static void AddProtectStateToPlayer(Entity e, GameObject authoringData, EntityManager manager)
        {
            if (authoringData.GetEntityObjectID() == ObjectID.Player)
                manager.AddComponent<ProtectStateCD>(e);
        }
    }
}
