using Assets.CoreFighter.Scripts.Cores;
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
        private ComponentLookup<LevelCD> levelLookup;
        private ComponentLookup<DurabilityCD> durabilityLookup;
        private BufferLookup<LevelEntitiesBuffer> levelEntitiesLookup;
        private NativeParallelHashMap<int, SetBonusID> objectIDToSetBonus;
        private NativeParallelMultiHashMap<int, SetBonusData> setBonusesLookUp;
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
            if (!SystemAPI.TryGetSingleton<ConditionsTableCD>(out var conditionsTableCD))
                return;
            bool allowBonus = bonusSet.Value;
            var database = this.database;
            var levelLookup = this.levelLookup;
            var setBonusesLookUp = this.setBonusesLookUp;
            var durabilityLookup = this.durabilityLookup;
            var conditionsTable = conditionsTableCD.Value;
            var objectIDToSetBonus = this.objectIDToSetBonus;
            var levelEntitiesLookup = this.levelEntitiesLookup;
            var equippedStatsLookup = this.equippedStatsLookup;
            Entities.ForEach((Entity e, DynamicBuffer<SummarizedConditionEffectsBuffer> effects,
                DynamicBuffer<SummarizedConditionsBuffer> conditions, DynamicBuffer<ContainedObjectsBuffer> container,
                in ActiveEquipmentPresetCD active, in DynamicBuffer<EquipmentPresetsBuffer> preset) =>
            {
                var equipped = new NativeList<ObjectDataCD>(16, Allocator.Temp);
                var bonusSetCount = new NativeParallelHashMap<int, int>(16, Allocator.Temp);
                var processedEquip = new NativeParallelHashSet<int>(16, Allocator.Temp);
                int current = active.Value;

                for (int i = preset.Length - 1; i >= 0; i--)
                {
                    if (i == current)
                        continue;
                    EquipmentCD equip = preset[i].equipment;
                    equipped.Clear();
                    equipped.Add(container[equip.helmSlotIndex].objectData);
                    equipped.Add(container[equip.breastSlotIndex].objectData);
                    equipped.Add(container[equip.pantsSlotIndex].objectData);
                    equipped.Add(container[equip.necklaceSlotIndex].objectData);
                    equipped.Add(container[equip.ring1SlotIndex].objectData);
                    equipped.Add(container[equip.ring2SlotIndex].objectData);
                    equipped.Add(container[equip.offHandIndex].objectData);
                    foreach (ObjectDataCD objData in equipped)
                    {
                        ObjectID objID = objData.objectID;
                        Entity primary = PugDatabase.GetPrimaryPrefabEntity(objID, database, 0);
                        //有耐久组件但耐久归零
                        bool hasDura = durabilityLookup.TryGetComponent(primary, out DurabilityCD durabilityCD);
                        if (hasDura && objData.amount <= 0)
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
                        bool reinForced = hasDura && durabilityCD.IsReinforced(objData.amount);
                        for (int j = 0; j < equippedStats.Length; j++)
                        {
                            EquipmentCondition equipmentCondition = equippedStats[j].equipmentCondition;
                            var condition = conditionsTable.Value.infos[(int)equipmentCondition.id];
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
                            AddConditionToSummarizedBuffers(equipmentCondition.id, value, conditionsTable, conditions, effects);
                        }

                        int id = (int)objID;
                        if (objectIDToSetBonus.TryGetValue(id, out SetBonusID setBonusID) &&
                            !processedEquip.Contains(id))
                        {
                            int bonusID = (int)setBonusID;
                            bonusSetCount.TryGetValue(bonusID, out int count);
                            bonusSetCount[bonusID] = count + 1;
                            processedEquip.Add(id);
                        }
                    }

                    if (!allowBonus)
                        continue;
                    foreach (var kv in bonusSetCount)
                    {
                        int key = kv.Key;
                        int count = kv.Value;
                        foreach (SetBonusData setBonusData in setBonusesLookUp.GetValuesForKey(key))
                        {
                            if (count < setBonusData.requiredPieces)
                                continue;
                            var data = setBonusData.conditionData;
                            AddConditionToSummarizedBuffers(data.conditionID, data.value, conditionsTable, conditions, effects);
                        }
                    }

                    bonusSetCount.Clear();
                    processedEquip.Clear();
                    equipped.Clear();
                }

                equipped.Dispose();
                bonusSetCount.Dispose();
                processedEquip.Dispose();
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
            ref var infos = ref ConditionsTable.Value.infos;
            if (id < ConditionID.None || id >= (ConditionID)infos.Length)
            {
                Debug.LogError(string.Format("Condition id {0} is out of bounds for conditions table of length {1}.",
                    (int)id, infos.Length));
                return;
            }

            int effect = (int)infos[(int)id].effect;
            sumConditionsBuffer[(int)id] = new SummarizedConditionsBuffer
            {
                value = sumConditionsBuffer[(int)id].value + value
            };

            sumConditionEffectsBuffer[effect] = new SummarizedConditionEffectsBuffer
            {
                value = sumConditionEffectsBuffer[effect].value + value
            };
        }
    }
}