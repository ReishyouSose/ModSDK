using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

namespace Assets.LuckyBlock
{
    public struct TriggerLBCD : IRpcCommand
    {
        public Entity entity;
        public TriggerLBCD(Entity entity)
        {
            this.entity = entity;
        }
    }

    public struct DropedLBCD : IComponentData { }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class TriggerLBClient : PugSimulationSystemBase
    {
        private static TriggerLBClient instance;
        private NativeQueue<TriggerLBCD> queue;
        private EntityArchetype archetype;
        protected override void OnCreate()
        {
            instance = this;
            queue = new(Allocator.Persistent);
            archetype = EntityManager.CreateArchetype(typeof(TriggerLBCD), typeof(SendRpcCommandRequest));
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            while (queue.TryDequeue(out var lb))
            {
                Entity e = ecb.CreateEntity(archetype);
                ecb.SetComponent(e, lb);
            }
            base.OnUpdate();
        }
        public static void Trigger(Entity lb)
        {
            instance.queue.Enqueue(new(lb));
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class TriggerLBServer : PugSimulationSystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            var healthLookup = SystemAPI.GetComponentLookup<HealthCD>();
            Entities.ForEach((Entity e, in TriggerLBCD lb) =>
            {
                healthLookup.GetRefRW(lb.entity).ValueRW.health = 0;
                ecb.DestroyEntity(e);
            })
                .WithName("TriggerLuckyBlock")
                .WithAll<ReceiveRpcCommandRequest>()
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
    }

    [UpdateBefore(typeof(UpdateHealthFromBufferSystem))]
    [UpdateInGroup(typeof(UpdateHealthSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class LuckyBlockServer : PugSimulationSystemBase
    {
        private NativeQueue<float3> spawn, drop;
        private CustomScenesDataTable sceneData;
        private Array objID;
        private Unity.Mathematics.Random rng;
        private ObjectID lbID;
        protected override void OnCreate()
        {
            lbID = PugMod.API.Authoring.GetObjectID("LuckyBlock:Item");
            spawn = new(Allocator.Persistent);
            drop = new(Allocator.Persistent);
            sceneData = Resources.Load<CustomScenesDataTable>("Scenes/CustomScenesDataTable");
            objID = Enum.GetValues(typeof(ObjectID));
            rng = PugRandom.GetRng();
            NeedDatabase();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            var localQueue = spawn;
            Entities.ForEach((Entity e, in HealthCD health, in LocalTransform trans) =>
            {
                if (health.health != 0)
                    return;
                localQueue.Enqueue(trans.Position);
                ecb.SetComponentEnabled<LuckyBlockCD>(e, false);
            })
                .WithName("LuckyBlock")
                .WithAll<LuckyBlockCD>()
                .WithBurst()
                .Schedule();

            var config = ModConfig.Ins;
            if (config == null)
                return;

            while (spawn.TryDequeue(out float3 pos))
            {
                while (true)
                {
                    if (rng.NextInt(100) < config.SceneChance.Value)
                    {
                        RandonScene(ecb, pos);
                        break;
                    }
                    if (RandonSpawn(ecb, pos))
                        break;
                }
            }

            if (config.ChallengeMode.Value)
            {
                base.OnUpdate();
                return;
            }

            localQueue = drop;
            Entities.ForEach((Entity e, ref RandomCD rng, in HealthCD health, in LocalTransform trans) =>
            {
                if (health.maxHealth >= 2 && health.health == 0)
                {
                    localQueue.Enqueue(trans.Position);
                    ecb.AddComponent<DropedLBCD>(e);
                }
            })
                .WithName("DropLB")
                .WithNone<DropedLBCD>()
                .WithAny<ChanceToDropLootCD>()
                .WithAny<DropsLootFromLootTableCD>()
                .WithBurst()
                .Schedule();

            while (drop.TryDequeue(out float3 pos))
            {
                if (rng.NextInt(100) < config.DropChance.Value)
                {
                    EntityUtility.CreateAndDropItem(lbID, 0, 1, pos, Entity.Null, database, ecb);
                }
            }

            base.OnUpdate();
        }
        private void RandonScene(EntityCommandBuffer ecb, float3 pos)
        {
            var scenes = sceneData.scenes;
            var scene = scenes[rng.NextInt(scenes.Count)];
            Entity name = ecb.CreateEntity();
            ecb.AddComponent(name, new CustomSceneCD { name = scene.sceneName });

            Entity block = ecb.CreateEntity();
            ecb.AddComponent(block, new BlockedSpawnAreaCD(pos.ToFloat2(), scene.radius));

            Entity spawn = ecb.CreateEntity();
            ecb.AddComponent(spawn, LocalTransform.FromPosition(pos));
            ecb.AddComponent(spawn, new SpawnCustomSceneCD
            {
                name = scene.sceneName,
                seed = PugRandom.GetSeed()
            });
        }
        private bool RandonSpawn(EntityCommandBuffer ecb, float3 pos)
        {
            int index = rng.NextInt(objID.Length);
            ObjectID id = (ObjectID)objID.GetValue(index);
            if (!PugDatabase.HasObject(id))
                return false;
            ref var info = ref PugDatabase.GetEntityObjectInfo(id, database);

            switch (info.objectType)
            {
                case ObjectType.NonUsable:
                case ObjectType.Helm:
                case ObjectType.BreastArmor:
                case ObjectType.PantsArmor:
                case ObjectType.Necklace:
                case ObjectType.Ring:
                case ObjectType.Offhand:
                case ObjectType.Bag:
                case ObjectType.Lantern:
                case ObjectType.MeleeWeapon:
                case ObjectType.RangeWeapon:
                case ObjectType.SummoningWeapon:
                case ObjectType.Shovel:
                case ObjectType.Hoe:
                case ObjectType.CastingItem:
                case ObjectType.MiningPick:
                case ObjectType.PaintTool:
                case ObjectType.FishingRod:
                case ObjectType.BugNet:
                case ObjectType.Sledge:
                case ObjectType.RoofingTool:
                case ObjectType.DrillTool:
                case ObjectType.BeamWeapon:
                case ObjectType.PlaceablePrefab:
                case ObjectType.WaterCan:
                case ObjectType.Bucket:
                case ObjectType.Valuable:
                case ObjectType.UniqueCraftingComponent:
                case ObjectType.KeyItem:
                case ObjectType.Instrument:
                case ObjectType.Pet:
                    int amount = info.isStackable ? rng.NextInt(1, 10) : 1;
                    EntityUtility.CreateAndDropItem(id, 0, amount, pos, Entity.Null, database, ecb);
                    break;
                case ObjectType.Critter:
                case ObjectType.Creature:
                    int count = rng.NextInt(10);
                    for (int i = 0; i <= count; i++)
                    {
                        Entity e = EntityUtility.CreateEntity(ecb, id, 1, database);
                        float3 randomPos = new(rng.NextFloat() * 2, 0, rng.NextFloat() * 2);
                        randomPos.x *= rng.NextBool() ? 1 : -1;
                        randomPos.z *= rng.NextBool() ? 1 : -1;
                        ecb.SetComponent(e, LocalTransform.FromPosition(pos + randomPos));
                    }
                    break;
                case ObjectType.Eatable:
                case ObjectType.PlayerType:
                case ObjectType.NonObtainable:
                    return false;
                default:
                    return false;
            }
            return true;
        }
    }
}
