using Assets.CoreEnhance.Scripts.Component;
using Assets.CoreEnhance.Scripts.Configs;
using CoreLib.Data.Configuration;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Assets.CoreEnhance.Scripts.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class InfinityClient : PugSimulationSystemBase
    {
        private bool added;
        protected override void OnUpdate()
        {
            var player = Manager.main.player;
            if (player == null)
            {
                added = false;
                return;
            }
            if (ModConfig.TryGetEnable(EnhanceCategory.Infinity, EC_Infinity.Durability))
            {
                if (added)
                    return;
                added = true;
                ConditionData cd = new()
                {
                    conditionID = ConditionID.EquipmentDurabilityLastsLonger,
                    value = 100
                };
                player.playerCommandSystem.SetSkillTalentCondition(player.entity, cd);
                cd.conditionID = ConditionID.ToolDurabilityLastsLonger;
                player.playerCommandSystem.SetSkillTalentCondition(player.entity, cd);
            }
            else
            {
                if (!added)
                    return;
                added = false;
                var tree = Manager.saves.GetSkillTalentTreesPoints(SkillID.Crafting);
                int count = tree.Count;

                ConditionData cd = new()
                {
                    conditionID = ConditionID.ToolDurabilityLastsLonger,
                    value = count > 2 ? tree[1] : 0
                };
                player.playerCommandSystem.SetSkillTalentCondition(player.entity, cd);

                cd = new()
                {
                    conditionID = ConditionID.EquipmentDurabilityLastsLonger,
                    value = count > 3 ? tree[2] : 0
                };
                player.playerCommandSystem.SetSkillTalentCondition(player.entity, cd);
            }
            base.OnUpdate();
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class InfinityServer : PugSimulationSystemBase
    {
        private const int ResetTimer = 1200;
        private int timer;
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            Infinity_Arena(ecb);
            Infinity_Minion();
            if (--timer > 0)
                return;
            timer = ResetTimer;
            Infinity_Boulder();
            base.OnUpdate();
        }
        private void Infinity_Boulder()
        {
            if (!ModConfig.TryGetEnable(EnhanceCategory.Infinity, EC_Infinity.Boulder))
                return;
            Entities.ForEach((ref HealthCD heal, in ObjectDataCD objdata) =>
            {
                heal.health = heal.maxHealth;
            })
                .WithName("Infinity_Boulder")
                .WithAll<RequiresDrillCD>()
                .WithAll<DontDropSelfCD>()
                .WithBurst()
                .Schedule();
        }
        private void Infinity_Arena(EntityCommandBuffer ecb)
        {
            if (!ModConfig.TryGetValue(EnhanceCategory.Infinity, EC_Infinity.Arena, out ConfigEntry<int> value))
                return;
            Entities.ForEach((Entity entity) =>
            {
                ecb.AddComponent(entity, new ArenaRecordCD());
            })
                .WithName("Infinity_Arena_AddRecord")
                .WithAll<TerminalActiveCD>()
                .WithNone<ArenaRecordCD>()
                .WithBurst()
                .Schedule();

            var collision = GetPhysicsWorld().CollisionWorld;
            var invLookup = SystemAPI.GetBufferLookup<ContainedObjectsBuffer>();
            var disLookup = SystemAPI.GetComponentLookup<DistanceToPlayerCD>();
            var healthLookup = SystemAPI.GetComponentLookup<HealthCD>();
            var objDataLookup = SystemAPI.GetComponentLookup<ObjectDataCD>();
            var count = value.Value;
            Entities.ForEach((Entity entity, ref ArenaRecordCD arena, in LocalTransform local) =>
            {
                if (!disLookup.TryGetComponent(entity, out var dis))
                {
                    ecb.AddComponent(entity, new DistanceToPlayerCD());
                    return;
                }
                if (dis.minDistanceSq < 1)
                {
                    NativeList<ColliderCastHit> hits;
                    float3 pos = local.Position;
                    if (!arena.chest)
                    {
                        hits = new(Allocator.Temp);
                        collision.SphereCastAll(pos, 1, float3.zero, 0, ref hits, CollisionFilter.Default);
                        bool has = false;
                        foreach (var hit in hits)
                        {
                            if (!objDataLookup.TryGetComponent(hit.Entity, out var data))
                                continue;
                            if (data.objectID == ObjectID.AlienChest)
                            {
                                has = true;
                                break;
                            }
                        }
                        hits.Dispose();
                        if (has)
                            return;
                        arena.chest = true;
                    }

                    arena.time++;
                    if (arena.time <= 100)
                        return;
                    if (!invLookup.TryGetBuffer(dis.closestPlayer, out var containers))
                        return;
                    for (int i = 0; i < containers.Length; i++)
                    {
                        var item = containers[i];
                        if (item.objectID != ObjectID.AncientCoin)
                            continue;
                        if (item.amount < count)
                            continue;
                        bool toAir = item.amount == count;
                        if (toAir)
                        {
                            containers[i] = new();
                        }
                        else
                        {
                            containers[i] = new()
                            {
                                objectData = new()
                                {
                                    objectID = item.objectID,
                                    amount = item.amount - count,
                                }
                            };
                        }

                        int index = PugRandom.GetRng().NextInt(5);
                        GetArenaScene(index, out var sceneName, out var radius, out var offset);

                        Entity name = ecb.CreateEntity();
                        ecb.AddComponent(name, new CustomSceneCD { name = sceneName });

                        hits = new(Allocator.Temp);
                        collision.SphereCastAll(pos - offset.ToFloat3(), radius, float3.zero, 0, ref hits, CollisionFilter.Default);
                        foreach (var hit in hits)
                        {
                            var e = hit.Entity;
                            if (!healthLookup.HasComponent(e))
                                continue;
                            if (!objDataLookup.TryGetComponent(e, out var data))
                                continue;
                            if (IsExcept(data.objectID))
                                continue;
                            ecb.DestroyEntity(e);
                        }
                        hits.Dispose();
                        Entity block = ecb.CreateEntity();
                        ecb.AddComponent(block, new BlockedSpawnAreaCD(pos.ToFloat2(), radius));

                        Entity spawn = ecb.CreateEntity();
                        ecb.AddComponent(spawn, LocalTransform.FromPosition(pos - offset.ToFloat3()));
                        ecb.AddComponent(spawn, new SpawnCustomSceneCD
                        {
                            name = sceneName,
                            seed = PugRandom.GetSeed()
                        });
                        ecb.DestroyEntity(entity);
                    }
                }
                else
                    arena.time = 1;
            })
                .WithName("Infinity_Arena_Rebuild")
                .WithNone<EventTerminalCD>()
                .Schedule();
        }
        private void Infinity_Minion()
        {
            if (!ModConfig.TryGetEnable(EnhanceCategory.Infinity, EC_Infinity.Minion))
                return;
            Entities.ForEach((ref MinionCD minion) =>
            {
                if (minion.hasStartedLifeSpanTimer)
                    minion.lifespanTimer = minion.lifespan;
            })
                .WithName("Infinity_Minion")
                .WithBurst()
                .Schedule();
        }
        private static void GetArenaScene(int index, out FixedString32Bytes name, out float radius, out int2 offset)
        {
            int x = 0, y = 0;
            switch (index)
            {
                case 0:
                    name = "EventTerminalCrystal1";
                    radius = 33;
                    x = -1;
                    y = -5;
                    break;
                case 1:
                    name = "EventTerminalCrystal2";
                    radius = 28;
                    break;
                case 2:
                    name = "EventTerminalCrystal3";
                    radius = 32;
                    x = 1;
                    y = -1;
                    break;
                case 3:
                    name = "EventTerminalCrystal4";
                    radius = 28;
                    x = -1;
                    y = 1;
                    break;
                case 4:
                    name = "EventTerminalCrystal5";
                    radius = 32;
                    break;
                default:
                    throw new Exception("Arena index out of range");
            }
            offset = new(x, y);
        }
        private static bool IsExcept(ObjectID id) => id switch
        {
            ObjectID.Portal or ObjectID.WayPoint or ObjectID.Player or ObjectID.PlayerGrave => true,
            _ => false,
        };
    }
}
