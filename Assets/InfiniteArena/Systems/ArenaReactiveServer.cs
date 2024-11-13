using Assets.InfinieArena;
using Assets.InfiniteArena.Components;
using Assets.InfiniteArena.Other;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using static Assets.InfiniteArena.Other.ArenaRecord;

namespace Assets.InfiniteArena.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class ArenaReactiveServer : PugSimulationSystemBase
    {
        protected override void OnCreate()
        {
            UpdatesInRunGroup();
            NeedDatabase();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            return;
            var deltaTime = World.Time.DeltaTime;
            var config = InfinieArenaMod.Config;
            var chargeTime = config.ChargeTime;
            var checkInterval = config.CheckInterval;
            var ecb = CreateCommandBuffer();
            var databaseLocal = database;
            var collision = GetPhysicsWorld().CollisionWorld;
            var objLookup = SystemAPI.GetComponentLookup<ObjectDataCD>();
            var posLookup = SystemAPI.GetComponentLookup<LocalTransform>();
            var arenaLookup = SystemAPI.GetComponentLookup<EventTerminalCD>();
            var spawnerLookup = SystemAPI.GetComponentLookup<EnemySpawnerPlatformCD>();
            Entities.ForEach((Entity entity, ref ArenaReactiveCD arena, ref ObjectDataCD objData, in DistanceToPlayerCD toPlayer, in LocalTransform local) =>
            {
                float dis = toPlayer.minDistanceSq;
                if (arenaLookup.HasComponent(entity))
                {
                    if (dis > 100 || arena.checkCompleted)
                        return;
                    arena.checkCompleted = true;
                    arena.style = objData.variation - 2;
                    MiscHelper.CombatInfo("CheckCompleted", local.Position);
                    int2 center = local.Position.RoundToInt2();
                    var map = ArenaDatas[arena.style].GetMark();
                    NativeList<ColliderCastHit> hits = new(Allocator.Temp);
                    collision.SphereCastAll(local.Position, 10, float3.zero, 0, ref hits, CollisionFilter.Default);
                    NativeList<ArenaData> datas = new(Allocator.Temp);
                    Entity chest = Entity.Null;
                    foreach (ColliderCastHit hit in hits)
                    {
                        if (!objLookup.TryGetComponent(hit.Entity, out var hitData))
                            continue;
                        ObjectID objID = hitData.objectID;
                        if (objID == ObjectID.AlienChest)
                        {
                            ecb.SetComponent(hit.Entity, new HealthCD()
                            {
                                health = 0,
                                maxHealth = 2
                            });
                            continue;
                        }
                        if (objID == ObjectID.EnemySpawnerPlatform)
                        {
                            if (!posLookup.TryGetComponent(hit.Entity, out var pos))
                                continue;
                            if (!spawnerLookup.TryGetComponent(hit.Entity, out var spawner))
                                continue;
                            if (spawner.enemyToSpawn != ObjectID.None)
                                continue;
                            ArenaData data = new()
                            {
                                objID = objID,
                                offset = pos.Position.RoundToInt2() - center
                            };
                            if (!map.TryGetValue(data, out bool mark))
                                continue;
                            ecb.SetComponent(hit.Entity, new EnemySpawnerPlatformCD()
                            {
                                enemyToSpawn = mark ? ObjectID.OrbitalTurret : ObjectID.Mimite
                            });
                        }
                    }
                    return;
                }
                ref bool completed = ref arena.chargeCompleted;
                ref int amount = ref objData.amount;
                if (!completed && ++amount >= chargeTime)
                    completed = true;

                ref float time = ref arena.waitTime;
                time += deltaTime;
                if (time < checkInterval)
                    return;
                time = 0;

                float3 position = local.Position;
                if (!completed)
                {
                    if (dis < 100)
                    {
                        float wait = (chargeTime - amount) / 1200f;
                        int minute = (int)wait;
                        int second = (int)math.round(math.lerp(0, 60, wait - minute));
                        MiscHelper.CombatInfo("WaitCharge", position, "\n" + minute.ToString("D2") + ":" + second.ToString("D2"));
                    }
                    return;
                }

                if (dis > 100)
                {
                    time = 0;
                    return;
                }

                if (dis < 1)
                {
                    ref int style = ref arena.style;
                    style = ++style % ArenaReactiveCD.StyleMax;
                    MiscHelper.CombatInfo("SelectStyle", local.Position, style + 1);
                }
                else
                {
                    int index = arena.style;
                    int2 center = local.Position.RoundToInt2();
                    ObjectID objID = objData.objectID;

                    NativeList<ColliderCastHit> hits = new(Allocator.Temp);
                    collision.SphereCastAll(local.Position, 10, float3.zero, 0, ref hits, CollisionFilter.Default);
                    NativeList<ArenaData> datas = new(Allocator.Temp);
                    Entity chest = Entity.Null;
                    foreach (ColliderCastHit hit in hits)
                    {
                        if (!objLookup.TryGetComponent(hit.Entity, out var hitData))
                            continue;
                        if (hitData.objectID is ObjectID.ElectricalWire or ObjectID.EnemySpawnerPlatform)
                        {
                            if (!posLookup.TryGetComponent(hit.Entity, out var pos))
                                continue;
                            datas.Add(new()
                            {
                                objID = hitData.objectID,
                                offset = pos.Position.RoundToInt2() - center,
                            });
                        }
                    }
                    hits.Dispose();

                    var map = ArenaDatas[index].GetMatch();
                    foreach (var data in datas)
                    {
                        if (map.TryGetValue(data, out var mark))
                        {
                            map[data] = true;
                        }
                    }
                    bool match = true;
                    int capacity = 5;
                    bool pop = false;
                    foreach (var data in map)
                    {
                        if (!data.Value)
                        {
                            match = false;
                            if (capacity > 0)
                            {
                                float3 target = (center + data.Key.offset).ToFloat3();
                                bool wire = data.Key.objID == ObjectID.ElectricalWire;
                                var puff = wire ? PuffID.SmallAncientEnergy : PuffID.SmallEnergyExplosion;
                                Manager.effects.PlayPuff(puff, target - Manager.camera.RenderOrigo.ToFloat3());
                                if (!pop)
                                {
                                    MiscHelper.CombatInfo("MapCheck", (center - new int2(0, 1)).ToFloat3(), (index + 1));
                                    pop = true;
                                }
                                capacity--;
                            }
                            else
                                break;
                        }
                    }
                    map.Clear();
                    map = null;
                    datas.Dispose();

                    if (match)
                    {
                        Entity re = EntityUtility.CreateEntity(ecb, local.Position, objID, 1, databaseLocal, index + 2);
                        ecb.AddComponent(re, new ArenaReactiveCD() { style = index });
                        ecb.DestroyEntity(entity);
                    }
                }
            })
                .WithName("ArenaReactive")
                .WithBurst()
                .Schedule();
        }
    }
}
