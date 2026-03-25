using Assets.CoreFighter.Scripts.Cores;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.CoreFighter.Scripts.Systems.Equip
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation, WorldSystemFilterFlags.Default)]
    [UpdateInGroup(typeof(BeforePredictedFixedStepSimulationSystemGroup), OrderFirst = true)]
    [UpdateAfter(typeof(SummarizeConditionsSystem))]
    public partial class EnableAllPresetSystem : PugSimulationSystemBase
    {
        public struct BonusRecord : IEquatable<BonusRecord>
        {
            public int BonusID;
            public int RequirePieces;

            public readonly bool Equals(BonusRecord other)
                => BonusID == other.BonusID && RequirePieces == other.RequirePieces;

            public readonly override int GetHashCode()
                => (BonusID * 397) ^ RequirePieces;
        }

        private NativeParallelHashMap<int, SetBonusID> objectIDToSetBonus;
        private NativeParallelMultiHashMap<int, SetBonusData> setBonusesLookUp;
        private ComponentLookup<DurabilityCD> durabilityLookup;
        private ComponentLookup<LevelCD> levelLookup;
        private BufferLookup<LevelEntitiesBuffer> levelEntitiesLookup;
        private BufferLookup<GivesConditionsWhenEquippedBuffer> equippedStatsLookup;

        protected override void OnCreate()
        {
            NeedDatabase();
            RequireForUpdate<ConditionsTableCD>();
            RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();

            SetBonusesTable setBonusesTable = Resources.Load<SetBonusesTable>("SetBonusesTable");
            if (setBonusesTable == null)
            {
                Debug.LogError("Could not find SetBonusesTable asset, conditions are disabled.");
                Enabled = false;
                return;
            }
            setBonusesTable.UpdateSetBonusDatas();
            objectIDToSetBonus = new NativeParallelHashMap<int, SetBonusID>(setBonusesTable.setBonuses.Count * 2 * 3, Allocator.Persistent);
            setBonusesLookUp = new NativeParallelMultiHashMap<int, SetBonusData>(setBonusesTable.setBonuses.Count * 2, Allocator.Persistent);
            foreach (SetBonusInfo setBonusInfo in setBonusesTable.setBonuses)
            {
                foreach (ObjectID objectID in setBonusInfo.availablePieces)
                {
                    objectIDToSetBonus.Add((int)objectID, setBonusInfo.setBonusID);
                }
                foreach (SetBonusData setBonusData in setBonusInfo.setBonusDatas)
                {
                    setBonusesLookUp.Add((int)setBonusInfo.setBonusID, setBonusData);
                }
            }

            PetInfosTable table = PetInfosTable.GetTable();
            if (table == null)
            {
                Debug.LogError("Could not find PetInfosTable asset, conditions are disabled.");
                Enabled = false;
                return;
            }

            durabilityLookup = SystemAPI.GetComponentLookup<DurabilityCD>();
            levelLookup = SystemAPI.GetComponentLookup<LevelCD>();
            levelEntitiesLookup = SystemAPI.GetBufferLookup<LevelEntitiesBuffer>();
            equippedStatsLookup = SystemAPI.GetBufferLookup<GivesConditionsWhenEquippedBuffer>();
            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            if (objectIDToSetBonus.IsCreated)
                objectIDToSetBonus.Dispose();
            if (setBonusesLookUp.IsCreated)
                setBonusesLookUp.Dispose();
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            if (!FighterConfig.TryGetValue<bool>(FighterCategory.EnableAllPreset, out var bonusSet))
                return;
            bool allowBonus = bonusSet.Value;

            var database = this.database;
            var durabilityLookup = this.durabilityLookup;
            var levelLookup = this.levelLookup;
            var levelEntitiesLookup = this.levelEntitiesLookup;
            var equippedStatsLookup = this.equippedStatsLookup;
            var ConditionsTable = SystemAPI.GetSingleton<ConditionsTableCD>().Value;
            var ObjectIDToSetBonus = objectIDToSetBonus;
            var SetBonusesLookUp = setBonusesLookUp;

            Entities.ForEach((Entity e, DynamicBuffer<SummarizedConditionEffectsBuffer> effects,
                DynamicBuffer<SummarizedConditionsBuffer> conditions, DynamicBuffer<ContainedObjectsBuffer> container,
                in ActiveEquipmentPresetCD active, in DynamicBuffer<EquipmentPresetsBuffer> preset) =>
            {
                var equipped = new NativeList<ObjectDataCD>(16, Allocator.Temp);
                var bonusRequireCount = new NativeParallelHashMap<int, int>(16, Allocator.Temp);
                var processedEquip = new NativeParallelHashSet<int>(16, Allocator.Temp);
                var processedBonus = new NativeParallelHashSet<BonusRecord>(16, Allocator.Temp);
                var sort = new NativeList<int>(3, Allocator.Temp) { 0, 1, 2 };
                int current = active.Value;
                sort.RemoveAt(current);
                sort.Add(current);

                for (int i = 2; i >= 0; i--)
                {
                    int index = sort[i];
                    EquipmentCD equip = preset[index].equipment;
                    equipped.Clear();
                    equipped.Add(container[equip.helmSlotIndex].objectData);
                    equipped.Add(container[equip.breastSlotIndex].objectData);
                    equipped.Add(container[equip.pantsSlotIndex].objectData);
                    equipped.Add(container[equip.necklaceSlotIndex].objectData);
                    equipped.Add(container[equip.ring1SlotIndex].objectData);
                    equipped.Add(container[equip.ring2SlotIndex].objectData);
                    equipped.Add(container[equip.offHandIndex].objectData);
                    bool apply = index != current;

                    foreach (ObjectDataCD objData in equipped)
                    {
                        ObjectID objID = objData.objectID;
                        Entity primary = PugDatabase.GetPrimaryPrefabEntity(objID, database, 0);
                        //有耐久组件但耐久归零
                        if (durabilityLookup.TryGetComponent(primary, out DurabilityCD durabilityCD)
                            && objData.amount <= 0)
                            continue;
                        //装备必须是有等级的
                        if (!levelLookup.TryGetComponent(primary, out LevelCD levelCD))
                            continue;
                        if (!levelEntitiesLookup.TryGetBuffer(primary, out DynamicBuffer<LevelEntitiesBuffer> levels))
                            continue;
                        int variation = objData.variation;
                        int level = variation > 0 ? math.min(variation, LevelScaling.GetMaxLevel()) : levelCD.level;
                        // 检查索引有效性
                        if (level < 0 || level > levels.Length)
                            continue;
                        Entity entity = levels[level].entity;
                        if (!equippedStatsLookup.HasBuffer(entity))
                            continue;
                        DynamicBuffer<GivesConditionsWhenEquippedBuffer> equippedStats = equippedStatsLookup[entity];
                        bool reinForced = durabilityCD.IsReinforced(objData.amount);
                        if (apply)
                        {
                            for (int j = 0; j < equippedStats.Length; j++)
                            {
                                EquipmentCondition equipmentCondition = equippedStats[j].equipmentCondition;
                                var condition = ConditionsTable.Value.infos[(int)equipmentCondition.id];
                                int value = equipmentCondition.value;
                                if (!condition.isUnique && reinForced && condition.effect != ConditionEffect.MaxMinions)
                                {
                                    int sign = math.sign(value);
                                    if (sign != 0)
                                    {
                                        float percent = math.abs(value) * 0.15f;
                                        int delta = (int)math.round(math.max(1f, percent));
                                        delta *= sign;
                                        if (condition.isNegative)
                                        {
                                            delta = -delta;
                                        }

                                        value += delta;
                                    }
                                }
                                AddConditionToSummarizedBuffers(equipmentCondition.id, value, ConditionsTable, conditions, effects);
                            }
                        }

                        int id = (int)objID;
                        if (ObjectIDToSetBonus.TryGetValue(id, out SetBonusID setBonusID) &&
                            !processedEquip.Contains(id))
                        {
                            int bonusID = (int)setBonusID;
                            bonusRequireCount.TryGetValue(bonusID, out int count);
                            bonusRequireCount[bonusID] = count + 1;
                            processedEquip.Add(id);
                        }
                    }

                    if (allowBonus)
                    {
                        foreach (var kv in bonusRequireCount)
                        {
                            int key = kv.Key;
                            int count = kv.Value;
                            foreach (SetBonusData setBonusData in SetBonusesLookUp.GetValuesForKey(key))
                            {
                                int require = setBonusData.requiredPieces;
                                BonusRecord record = new()
                                {
                                    BonusID = key,
                                    RequirePieces = require
                                };
                                if (processedBonus.Contains(record))
                                    continue;
                                if (count < require)
                                    continue;
                                if (apply)
                                    AddConditionToSummarizedBuffers(setBonusData.conditionData, ConditionsTable, conditions, effects);
                                processedBonus.Add(record);
                            }
                        }
                    }

                    bonusRequireCount.Clear();
                    processedEquip.Clear();
                    equipped.Clear();
                }

                equipped.Dispose();
                bonusRequireCount.Dispose();
                processedEquip.Dispose();
                processedBonus.Dispose();
                sort.Dispose();
            })
                .WithName("EnableAllPreset")
                .WithBurst()
                .ScheduleParallel(Dependency);
            base.OnUpdate();
        }

        private static void AddConditionToSummarizedBuffers(ConditionID id, int value,
            BlobAssetReference<ConditionsTableBlob> ConditionsTable,
            DynamicBuffer<SummarizedConditionsBuffer> sumConditionsBuffer,
            DynamicBuffer<SummarizedConditionEffectsBuffer> sumConditionEffectsBuffer)
        {
            if (id < ConditionID.None || id >= (ConditionID)ConditionsTable.Value.infos.Length)
            {
                Debug.LogError(string.Format("Condition id {0} is out of bounds for conditions table of length {1}.",
                    (int)id, ConditionsTable.Value.infos.Length));
                return;
            }

            int effect = (int)ConditionsTable.Value.infos[(int)id].effect;

            sumConditionsBuffer[(int)id] = new SummarizedConditionsBuffer
            {
                value = sumConditionsBuffer[(int)id].value + value
            };

            sumConditionEffectsBuffer[effect] = new SummarizedConditionEffectsBuffer
            {
                value = sumConditionEffectsBuffer[effect].value + value
            };
        }

        private static void AddConditionToSummarizedBuffers(ConditionData data,
            BlobAssetReference<ConditionsTableBlob> ConditionsTable,
            DynamicBuffer<SummarizedConditionsBuffer> sumConditionsBuffer,
            DynamicBuffer<SummarizedConditionEffectsBuffer> sumConditionEffectsBuffer)
        {
            AddConditionToSummarizedBuffers(data.conditionID, data.value, ConditionsTable,
                sumConditionsBuffer, sumConditionEffectsBuffer);
        }
    }
}