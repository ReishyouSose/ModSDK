using PlayerState;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.SoulLinkKeeper
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation, WorldSystemFilterFlags.Default)]
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [UpdateAfter(typeof(UpdateHealthSystemGroup))]
    [UpdateBefore(typeof(SpawnGraveForDeadPlayerSystem))]
    public partial class DeathSpreadSystem : PugSimulationSystemBase
    {
        private NativeQueue<Hash128> queue;
        protected override void OnCreate()
        {
            queue = new(Allocator.Persistent);
            RequireForUpdate<WorldInfoCD>();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            var queue = this.queue;
            var worldInfo = SystemAPI.GetSingleton<WorldInfoCD>();
            bool guest = worldInfo.guestMode;

            // worldInfo.pvpEnabled
            var remove = Entities.ForEach((Entity e, in PlayerStateCD playerStateCD) =>
            {
                if (playerStateCD.HasAnyState(PlayerStateEnum.SpawningFromCore | PlayerStateEnum.Death))
                    ecb.SetComponentEnabled<BlockDeathSpread>(e, false);
            })
                .WithName("RemoveDeathCD")
                .WithAll<PlayerGhost>()
                .WithAll<BlockDeathSpread>()
                .ScheduleParallel(Dependency);

            var check = Entities.ForEach((Entity e,  in HealthCD healthCD, in PlayerGhost playerGhost, in PlayerStateCD playerStateCD) =>
            {
                if (guest && playerGhost.adminPrivileges < 1)
                    return;
                if (healthCD.health <= 0 && !playerStateCD.HasAnyState(PlayerStateEnum.SpawningFromCore | PlayerStateEnum.Death))
                {
                    queue.Enqueue(playerGhost.playerGuid);
                }
            })
                .WithName("DeathSpreadRecord")
                .WithBurst()
                .WithNone<BlockDeathSpread>()
                .ScheduleParallel(remove);

            if (queue.TryDequeue(out var guid))
            {
                queue.Clear();
                Entities.ForEach((Entity e, ref DeathSpreadCD spread, ref HealthCD health, in PlayerGhost playerGhost, in PlayerStateCD playerStateCD) =>
                {
                    if (playerGhost.playerGuid == guid)
                    {
                        spread.Server++;
                        return;
                    }
                    if (!playerStateCD.HasAnyState(PlayerStateEnum.SpawningFromCore | PlayerStateEnum.Death))
                        health.health = 0;
                    ecb.SetComponentEnabled<BlockDeathSpread>(e, true);
                })
                    .WithName("DeathSpreadTigger")
                    .WithBurst()
                    .ScheduleParallel(check);
            }
            base.OnUpdate();
        }
    }

}
