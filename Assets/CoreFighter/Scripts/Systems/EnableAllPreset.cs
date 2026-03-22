using Assets.CoreFighter.Scripts.Cores;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.CoreFighter.Scripts.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation, WorldSystemFilterFlags.Default)]
    [UpdateInGroup(typeof(BeforePredictedFixedStepSimulationSystemGroup), OrderFirst = true)]
    [UpdateAfter(typeof(SummarizeConditionsSystem))]
    public partial class EnableAllPresetSystem : PugSimulationSystemBase
    {
        NativeParallelHashMap<int, SetBonusID> _objectIDToSetBonus;
        NativeParallelMultiHashMap<int, SetBonusData> _setBonusesLookUp;
        private ComponentLookup<DurabilityCD> durabilityLookup;
        private ComponentLookup<LevelCD> levelLookup;
        private BufferLookup<LevelEntitiesBuffer> levelEntitiesLookup;
        private BufferLookup<GivesConditionsWhenEquippedBuffer> givesConditionsWhenEquippedBufferLookup;
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
            _objectIDToSetBonus = new NativeParallelHashMap<int, SetBonusID>(setBonusesTable.setBonuses.Count * 2 * 3, Allocator.Persistent);
            _setBonusesLookUp = new NativeParallelMultiHashMap<int, SetBonusData>(setBonusesTable.setBonuses.Count * 2, Allocator.Persistent);
            foreach (SetBonusInfo setBonusInfo in setBonusesTable.setBonuses)
            {
                foreach (ObjectID objectID in setBonusInfo.availablePieces)
                {
                    _objectIDToSetBonus.Add((int)objectID, setBonusInfo.setBonusID);
                }
                foreach (SetBonusData setBonusData in setBonusInfo.setBonusDatas)
                {
                    _setBonusesLookUp.Add((int)setBonusInfo.setBonusID, setBonusData);
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
            givesConditionsWhenEquippedBufferLookup = SystemAPI.GetBufferLookup<GivesConditionsWhenEquippedBuffer>();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            if (!FighterConfig.TryGetValue<bool>(FighterCategory.EnableAllPreset, out var value))
                return;
            bool allowBonus = value.Value;
            var database = this.database;
            var durabilityLookup = this.durabilityLookup;
            var levelLookup = this.levelLookup;
            var levelEntitiesLookup = this.levelEntitiesLookup;
            var givesConditionsWhenEquippedBufferLookup = this.givesConditionsWhenEquippedBufferLookup;
            var ConditionsTable = SystemAPI.GetSingleton<ConditionsTableCD>().Value;
            var ObjectIDToSetBonus = _objectIDToSetBonus;
            var SetBonusesLookUp = _setBonusesLookUp;
            bool immuneExplosion = FighterConfig.IsEnable(FighterCategory.ImmuneExplosion);
            bool enableAllPreset = FighterConfig.IsEnable(FighterCategory.EnableAllPreset);
            Entities.ForEach((Entity e, DynamicBuffer<SummarizedConditionEffectsBuffer> effects,
                DynamicBuffer<SummarizedConditionsBuffer> conditions,
                in DynamicBuffer<ContainedObjectsBuffer> container,
                in ActiveEquipmentPresetCD active, in DynamicBuffer<EquipmentPresetsBuffer> preset) =>
            {
                if (immuneExplosion)
                {
                    effects[(int)ConditionEffect.ReducedDamageFromExplosions] = new() { value = 100 };
                }
                if (!enableAllPreset)
                    return;
                NativeList<ObjectDataCD> nativeList2 = new(16, Allocator.Temp);
                NativeParallelHashMap<int, int> nativeParallelHashMap = new(16, Allocator.Temp);
                NativeParallelHashSet<int> nativeParallelHashSet = new(16, Allocator.Temp);
                for (int i = 0; i < 3; i++)
                {
                    if (i == active.Value)
                        continue;
                    nativeList2.Clear();
                    EquipmentCD equip = preset[i].equipment;
                    nativeList2.Add(container[equip.helmSlotIndex].objectData);
                    nativeList2.Add(container[equip.breastSlotIndex].objectData);
                    nativeList2.Add(container[equip.pantsSlotIndex].objectData);
                    nativeList2.Add(container[equip.necklaceSlotIndex].objectData);
                    nativeList2.Add(container[equip.ring1SlotIndex].objectData);
                    nativeList2.Add(container[equip.ring2SlotIndex].objectData);
                    nativeList2.Add(container[equip.offHandIndex].objectData);
                    foreach (ObjectDataCD objectDataCD in nativeList2)
                    {
                        Entity primaryPrefabEntity3 = PugDatabase.GetPrimaryPrefabEntity(objectDataCD.objectID, database, 0);
                        if (!durabilityLookup.HasComponent(primaryPrefabEntity3) || objectDataCD.amount > 0)
                        {
                            if (levelEntitiesLookup.TryGetBuffer(primaryPrefabEntity3, out DynamicBuffer<LevelEntitiesBuffer> dynamicBuffer5) && levelLookup.TryGetComponent(primaryPrefabEntity3, out LevelCD levelCD))
                            {
                                int num12 = ((objectDataCD.variation > 0) ? math.min(objectDataCD.variation, LevelScaling.GetMaxLevel()) : levelCD.level);
                                Entity entity = dynamicBuffer5[num12].entity;
                                DynamicBuffer<GivesConditionsWhenEquippedBuffer> dynamicBuffer6 = givesConditionsWhenEquippedBufferLookup[entity];
                                bool flag2 = durabilityLookup.TryGetComponent(primaryPrefabEntity3, out DurabilityCD durabilityCD) && durabilityCD.IsReinforced(objectDataCD.amount);
                                for (int num13 = 0; num13 < dynamicBuffer6.Length; num13++)
                                {
                                    EquipmentCondition equipmentCondition = dynamicBuffer6[num13].equipmentCondition;
                                    int id = (int)equipmentCondition.id;
                                    int num14 = equipmentCondition.value;
                                    if (!ConditionsTable.Value.infos[id].isUnique && flag2 && ConditionsTable.Value.infos[id].effect != ConditionEffect.MaxMinions)
                                    {
                                        int num15;
                                        if (equipmentCondition.value < 0)
                                        {
                                            num15 = (int)math.round(math.min(-1f, (float)equipmentCondition.value * 0.14999998f));
                                            if (ConditionsTable.Value.infos[id].isNegative)
                                            {
                                                num15 = -num15;
                                            }
                                        }
                                        else
                                        {
                                            num15 = (int)math.round(math.max(1f, (float)equipmentCondition.value * 0.14999998f));
                                        }
                                        num14 += num15;
                                    }
                                    AddConditionToSummarizedBuffers(equipmentCondition.id, num14, ConditionsTable, conditions, effects);
                                }
                            }
                            if (ObjectIDToSetBonus.TryGetValue((int)objectDataCD.objectID, out SetBonusID setBonusID) && !nativeParallelHashSet.Contains((int)objectDataCD.objectID))
                            {
                                if (!nativeParallelHashMap.TryGetValue((int)setBonusID, out int num16))
                                {
                                    nativeParallelHashMap.Add((int)setBonusID, 1);
                                }
                                else
                                {
                                    nativeParallelHashMap[(int)setBonusID] = num16 + 1;
                                }
                                nativeParallelHashSet.Add((int)objectDataCD.objectID);
                            }
                        }
                    }
                    if (!allowBonus)
                    {
                        continue;
                    }
                    using NativeParallelHashMap<int, int>.Enumerator enumerator3 = nativeParallelHashMap.GetEnumerator();
                    while (enumerator3.MoveNext())
                    {
                        KeyValue<int, int> keyValue = enumerator3.Current;
                        int key = keyValue.Key;
                        int num17 = keyValue.Value;
                        foreach (SetBonusData setBonusData in SetBonusesLookUp.GetValuesForKey(key))
                        {
                            if (num17 >= setBonusData.requiredPieces)
                            {
                                AddConditionToSummarizedBuffers(setBonusData.conditionData, ConditionsTable, conditions, effects);
                            }
                        }
                    }
                }
                nativeList2.Dispose();
                nativeParallelHashMap.Dispose();
                nativeParallelHashSet.Dispose();
            })
                .WithName("EnableAllPreset")
                .WithBurst()
                .Schedule();
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
