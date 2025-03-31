using Assets.CoreEnhance.Scripts.Datas;
using Assets.CoreEnhance.Scripts.Items;
using Inventory;
using PlayerState;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace Assets.CoreEnhance.Scripts.Systems.Misc
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(PlayerStateSystemGroup))]
    [UpdateBefore(typeof(UpdatePlayerStateSystem))]
    public partial class QocFinishCastingSystem : PugSimulationSystemBase
    {
        private BiomeLookup biomeLookup;
        private ComponentLookup<QocCastingSpawnSceneCD> spawnSceneLookup;
        private ComponentLookup<LocalTransform> transLookup;
        private ComponentLookup<GodModeCD> godLookup;
        private EntityQuery sceneQuery;
        private EntityQuery arenaQuery;
        protected override void OnCreate()
        {
            spawnSceneLookup = SystemAPI.GetComponentLookup<QocCastingSpawnSceneCD>();
            transLookup = SystemAPI.GetComponentLookup<LocalTransform>();
            godLookup = SystemAPI.GetComponentLookup<GodModeCD>();
            sceneQuery = EntityManager.CreateEntityQuery(typeof(CustomSceneTableCD));
            arenaQuery = EntityManager.CreateEntityQuery(typeof(EventTerminalCD));
            RequireForUpdate<CustomSceneTableCD>();
            RequireForUpdate<BiomeRangesCD>();
            base.OnCreate();
        }
        protected override void OnStartRunning()
        {
            biomeLookup = SystemAPI.TryGetSingleton<BiomeSamplesCD>(out var sample)
                ? new(sample) : new(SystemAPI.GetSingleton<BiomeRangesCD>().Value, Allocator.Persistent);
            base.OnStartRunning();
        }
        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingletonBuffer<InventoryChangeBuffer>(out var invChanger))
                return;
            var sceneQuery = this.sceneQuery;
            var arenaQuery = this.arenaQuery;
            var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
            var spawnSceneLookup = this.spawnSceneLookup;
            var transLookup = this.transLookup;
            var biomeLookup = this.biomeLookup;
            var godLookup = this.godLookup;
            var ecb = CreateCommandBuffer();
            Entities.ForEach((StateUpdateAspect stateUpdateAspect) =>
            {
                ref var playerState = ref stateUpdateAspect.playerStateCD.ValueRW;
                if (playerState.currentState != PlayerStateEnum.Casting)
                    return;
                ref var casting = ref stateUpdateAspect.castingStateCD.ValueRW;
                if (!casting.castTimer.IsTimerElapsed(currentTick))
                    return;
                Entity equipmentPrefab = stateUpdateAspect.equippedObjectCD.ValueRO.equipmentPrefab;
                if (!spawnSceneLookup.TryGetComponent(equipmentPrefab, out var spawnScene))
                    return;
                casting.itemIsInProcessOfBeingUsed = true;
                if (casting.objectData.objectID == QocObjectID.ArenaScanner && !arenaQuery.IsEmpty)
                {
                    playerState.SetNextState(PlayerStateEnum.Walk, false);
                    return;
                }
                var player = stateUpdateAspect.entity;
                bool god = godLookup.IsComponentEnabled(player);
                var pos = transLookup.GetRefRO(player).ValueRO.Position;
                var p = pos.RoundToInt2();
                var rng = PugRandom.GetRng();
                if (!god)
                {
                    var requireBiome = spawnScene.Biome;
                    if (requireBiome != Biome.None && biomeLookup.GetBiome(p) != requireBiome)
                    {
                        playerState.SetNextState(PlayerStateEnum.Walk, false);
                        return;
                    }
                }
                var sceneTable = sceneQuery.GetSingleton<CustomSceneTableCD>();
                ref var scenes = ref sceneTable.Value.Value.scenes;
                int len = scenes.Length;
                var sceneName = spawnScene.SceneNames[rng.NextInt(spawnScene.SceneNames.Length)];
                for (int i = 0; i < len; i++)
                {
                    ref var scene = ref scenes[i];
                    if (!scene.sceneName.ToString().Equals(sceneName.ToString()))
                        continue;
                    var r = (int)scene.radius;
                    p = new(p.x + rng.NextInt(-r, r + 1), p.y + rng.NextInt(-r, r + 1));
                    if (!god && spawnScene.LimitRange)
                    {
                        float dis = 0;
                        while (dis < spawnScene.MinRadiums || dis > spawnScene.MaxRadiums)
                        {
                            p = new(p.x + rng.NextInt(-r, r + 1), p.y + rng.NextInt(-r, r + 1));
                            dis = math.lengthsq(p);
                        }
                    }
                    Entity name = ecb.CreateEntity();
                    ecb.AddComponent(name, new CustomSceneCD { name = new(sceneName) });

                    Entity block = ecb.CreateEntity();
                    ecb.AddComponent(block, new BlockedSpawnAreaCD(p, scene.radius));

                    Entity spawn = ecb.CreateEntity();
                    ecb.AddComponent(spawn, LocalTransform.FromPosition((p + scene.boundsSize / 2).ToFloat3()));
                    ecb.AddComponent(spawn, new SpawnCustomSceneCD
                    {
                        name = sceneName,
                        seed = PugRandom.GetSeed()
                    });
                    invChanger.Add(new()
                    {
                        inventoryChangeData = Create.ConsumeEntityAt(stateUpdateAspect.entity,
                            stateUpdateAspect.equippedObjectCD.ValueRO.equippedSlotIndex,
                            1, true, !god && !spawnScene.Consume),
                        playerEntity = stateUpdateAspect.entity
                    });
                    break;
                }
                playerState.SetNextState(PlayerStateEnum.Walk, false);
            })
                .WithName("QocFinishCasting")
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
    }
}
