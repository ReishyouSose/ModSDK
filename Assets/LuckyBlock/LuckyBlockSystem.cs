using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.LuckyBlock
{

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
        private List<ObjectID> equips, npcs, foods, misc;
        private ComponentLookup<BossCD> bossLookup;
        private ComponentLookup<HealthCD> healthLookup;
        private ComponentLookup<MerchantCD> merchantLookup;
        protected override void OnCreate()
        {
            lbID = PugMod.API.Authoring.GetObjectID("LuckyBlock:Item");
            spawn = new(Allocator.Persistent);
            drop = new(Allocator.Persistent);
            sceneData = Resources.Load<CustomScenesDataTable>("Scenes/CustomScenesDataTable");
            objID = Enum.GetValues(typeof(ObjectID));
            rng = PugRandom.GetRng();
            bossLookup = SystemAPI.GetComponentLookup<BossCD>();
            healthLookup = SystemAPI.GetComponentLookup<HealthCD>();
            merchantLookup = SystemAPI.GetComponentLookup<MerchantCD>();
            NeedDatabase();
            base.OnCreate();
        }
        protected override void OnStartRunning()
        {
            LoadData();
            base.OnStartRunning();
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
                int scene = config.SceneChance.Value;
                if (scene > 0 && rng.NextInt(100) < scene)
                {
                    RandonScene(ecb, pos);
                    continue;
                }
                RandonSpawn(ecb, pos);
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
        /* private bool RandonSpawn(EntityCommandBuffer ecb, float3 pos)
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
                     {
                         if (PugDatabase.GetObjectInfo(id).icon == null)
                             return false;
                     }
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
                 //case ObjectType.Eatable:
                 //{
                 //    int amount = rng.NextInt(1, 10);
                 //    Entity e = CreateAndDropItem(id, 0, amount, pos, Entity.Null, database, ecb);
                 //    ecb.AddComponent(e,new )
                 //}
                 //break;
                 case ObjectType.PlayerType:
                 case ObjectType.NonObtainable:
                     return false;
                 default:
                     return false;
             }
             return true;
         }*/
        private void RandonSpawn(EntityCommandBuffer ecb, float3 pos)
        {
            var config = ModConfig.Ins;
            float equip = config.Equip.Value;
            if (equip <= 0)
                equip = 1;
            float npc = config.NPC.Value;
            if (npc <= 0)
                npc = 1;
            float food = config.Food.Value;
            if (food <= 0)
                food = 1;
            float misc = config.Misc.Value;
            if (misc <= 0)
                misc = 1;
            float weight = equip + npc + food + misc;
            NativeArray<float> split = new(4, Allocator.Temp);
            split[0] = equips.Count * equip / weight;
            split[1] = split[0] + npcs.Count * npc / weight;
            split[2] = split[1] + foods.Count * food / weight;
            split[3] = split[2] + this.misc.Count * misc / weight;
            float r = rng.NextFloat(split[3]);
            for (int i = 0; i < split.Length; i++)
            {
                if (r < split[i])
                {
                    Debug.Log("Mode " + i);
                    ObjectID id;
                    int amount = rng.NextInt(1, config.MaxStack.Value + 1);
                    switch (i)
                    {
                        case 0:
                            id = equips[rng.NextInt(equips.Count)];
                            amount = PugDatabase.GetObjectInfo(id).initialAmount;
                            break;
                        case 1:
                            Entity e;
                            while (true)
                            {
                                id = npcs[rng.NextInt(npcs.Count)];
                                e = PugDatabase.GetPrimaryPrefabEntity(id, database);
                                if (!merchantLookup.HasComponent(e))
                                {
                                    break;
                                }
                                else
                                {
                                    npcs.Remove(id);
                                }
                            }
                            amount = rng.NextInt(1, 11);
                            bool boss = bossLookup.HasComponent(e);
                            if (config.NoMultiBoss.Value && boss)
                            {
                                amount = 1;
                            }
                            for (int j = 0; j <= amount; j++)
                            {
                                e = EntityUtility.CreateEntity(ecb, id, 1, database);
                                float3 randomPos = new(rng.NextFloat() * 2, 0, rng.NextFloat() * 2);
                                randomPos.x *= rng.NextBool() ? 1 : -1;
                                randomPos.z *= rng.NextBool() ? 1 : -1;
                                ecb.SetComponent(e, LocalTransform.FromPosition(pos + randomPos));
                            }
                            return;
                        case 2:
                            id = foods[rng.NextInt(foods.Count)];
                            break;
                        case 3:
                            id = this.misc[rng.NextInt(this.misc.Count)];
                            break;
                        default:
                            return;
                    }
                    CreateAndDropItem(id, 0, amount, pos, Entity.Null, database, ecb);
                    return;
                }
            }
        }
        public static Entity CreateAndDropItem(ObjectID objectID, int variation, int amount, float3 position, Entity pullTowardsEntity, BlobAssetReference<PugDatabase.PugDatabaseBank> databaseLocal, EntityCommandBuffer ecb)
        {
            ContainedObjectsBuffer containedObject = new()
            {
                objectData = new ObjectDataCD
                {
                    objectID = objectID,
                    amount = amount,
                    variation = variation
                }
            };
            return EntityUtility.DropNewEntity(ecb, containedObject, position, databaseLocal, pullTowardsEntity);
        }
        private void LoadData()
        {
            equips = new();
            npcs = new();
            foods = new();
            misc = new();
            for (int index = 0; index < objID.Length; index++)
            {
                ObjectID id = (ObjectID)objID.GetValue(index);
                if (!PugDatabase.HasObject(id))
                    continue;
                var info = PugDatabase.GetObjectInfo(id);
                var tags = info.tags;
                switch (info.objectType)
                {
                    case ObjectType.Creature:
                        npcs.Add(id);
                        continue;
                    case ObjectType.Eatable:
                        if (tags.Contains(ObjectCategoryTag.CookingIngredient))
                            foods.Add(id);
                        continue;
                    case ObjectType.NonUsable:
                    case ObjectType.NonObtainable:
                    case ObjectType.PlayerType:
                        continue;
                }
                if (tags.Contains(ObjectCategoryTag.CanBeUpgraded))
                {
                    equips.Add(id);
                    continue;
                }
                misc.Add(id);
            }
            StringBuilder builder = new();
            builder.Append("[LuckyBlock] Load Data").AppendLine()
                .Append("Equip ").Append(equips.Count).AppendLine()
                .Append("NPC ").Append(npcs.Count).AppendLine()
                .Append("Food ").Append(foods.Count).AppendLine()
                .Append("Misc ").Append(misc.Count);
            Debug.Log(builder);
        }
    }
}
