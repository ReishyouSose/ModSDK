using PlayerState;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace Assets.CoreEnhance.Scripts.Systems.Misc
{
    public enum QuickConsumeType : byte
    {
        Health,
        Potion,
        Cooked,
    }
    public struct QuickConsumeRpc : IRpcCommand
    {
        public byte ConsumeType;
        public int PlayerIndex;
        public QuickConsumeRpc(QuickConsumeType type, int player)
        {
            ConsumeType = (byte)type;
            PlayerIndex = player;
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class QuickConsumableClient : PugSimulationSystemBase
    {
        private static QuickConsumableClient ins;
        private NativeQueue<QuickConsumeRpc> queue;
        private EntityArchetype archetype;
        protected override void OnCreate()
        {
            ins = this;
            queue = new(Allocator.Persistent);
            archetype = EntityManager.CreateArchetype(typeof(QuickConsumeRpc), typeof(SendRpcCommandRequest));
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            while (queue.TryDequeue(out var rpc))
            {
                Entity e = ecb.CreateEntity(archetype);
                ecb.SetComponent(e, rpc);
            }
            base.OnUpdate();
        }
        public static void QuickConsume(QuickConsumeType type)
        {
            if (ins == null)
                return;
            var p = Manager.main.player;
            if (p == null)
                return;
            ins.queue.Enqueue(new(type, p.playerIndex));
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class QuickConsumableServer : PugSimulationSystemBase
    {
        private NativeQueue<QuickConsumeRpc> queue;
        private ComponentLookup<CookedFoodCD> cookLookup;
        private ComponentLookup<FishCD> fishLookup;
        private ComponentLookup<FlowerCD> flowerLookup;
        private ComponentLookup<PotionCD> potionLookup;
        private ComponentLookup<PlayerGhost> playerLookup;
        private BufferLookup<GivesConditionsWhenConsumedBuffer> giveConditionLookup;
        private BufferLookup<HealthChangeBuffer> healthChangeLookup;
        private uint tickRate;
        protected override void OnCreate()
        {
            queue = new(Allocator.Persistent);
            cookLookup = SystemAPI.GetComponentLookup<CookedFoodCD>();
            fishLookup = SystemAPI.GetComponentLookup<FishCD>();
            flowerLookup = SystemAPI.GetComponentLookup<FlowerCD>();
            potionLookup = SystemAPI.GetComponentLookup<PotionCD>();
            playerLookup = SystemAPI.GetComponentLookup<PlayerGhost>();
            giveConditionLookup = SystemAPI.GetBufferLookup<GivesConditionsWhenConsumedBuffer>();
            healthChangeLookup = SystemAPI.GetBufferLookup<HealthChangeBuffer>();
            tickRate = (uint)NetworkingManager.GetSimulationTickRateForPlatform();
            RequireForUpdate<PugDatabase.DatabaseBankCD>();
            RequireForUpdate<ConditionsTableCD>();
            NeedDatabase();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            var queue = this.queue;
            Entities.ForEach((Entity e, in QuickConsumeRpc rpc) =>
            {
                queue.Enqueue(rpc);
                ecb.DestroyEntity(e);
            })
                .WithName("QuickConsume_Receive")
                .WithAll<ReceiveRpcCommandRequest>()
                .WithBurst()
                .Schedule();

            while (queue.TryDequeue(out var rpc))
            {
                var cookLookup = this.cookLookup;
                var database = this.database;
                var conditionsTableCD = SystemAPI.GetSingleton<ConditionsTableCD>();
                var databasebankCD = SystemAPI.GetSingleton<PugDatabase.DatabaseBankCD>();
                var flowerLookup = this.flowerLookup;
                var fishLookup = this.fishLookup;
                var giveConditionLookup = this.giveConditionLookup;
                var potionLookup = this.potionLookup;
                var playerLookup = this.playerLookup;
                var tickRate = this.tickRate;
                var healthChangeBuffer = healthChangeLookup[SystemAPI.GetSingletonEntity<HealthChangeBuffer>()];
                var currentTick = GetServerTick();
                Entities.ForEach((Entity entity, DynamicBuffer<ContainedObjectsBuffer> containers,
                    DynamicBuffer<SummarizedConditionEffectsBuffer> summarizedConditionEffectsBuffer,
                    DynamicBuffer<SummarizedConditionsBuffer> summarizedConditionsBuffer,
                    DynamicBuffer<ConditionsBuffer> conditionsBuffer, ref HealthCD healthCD,
                    ref HungerCD hungerCD, in PlayerStateCD playerStateCD) =>
                {
                    if (!playerLookup.TryGetComponent(entity, out var player) || player.playerIndex != rpc.PlayerIndex)
                        return;
                    for (int j = 0; j < containers.Length; j++)
                    {
                        ObjectDataCD objectData = containers[j].objectData;
                        FixedList64Bytes<ObjectDataCD> ingredients = new() { objectData };
                        if (PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType != ObjectType.Eatable)
                            continue;
                        Entity item = PugDatabase.GetPrimaryPrefabEntity(objectData.objectID, database, objectData.variation);
                        bool cook = false;
                        switch ((QuickConsumeType)rpc.ConsumeType)
                        {
                            case QuickConsumeType.Health:
                                if (objectData.objectID is not ObjectID.HealingPotion or ObjectID.GreaterHealingPotion)
                                    continue;
                                break;
                            case QuickConsumeType.Potion:
                                if (potionLookup.HasComponent(item))
                                    continue;
                                break;
                            case QuickConsumeType.Cooked:
                                if (!cookLookup.HasComponent(item))
                                    continue;
                                cook = true;
                                ObjectID primaryIngredientFromVariation = CookedFoodCD.GetPrimaryIngredientFromVariation(objectData.variation);
                                ObjectID secondaryIngredientFromVariation = CookedFoodCD.GetSecondaryIngredientFromVariation(objectData.variation);
                                ingredients.Add(new()
                                {
                                    objectID = primaryIngredientFromVariation,
                                    amount = 1
                                });
                                ingredients.Add(new()
                                {
                                    objectID = secondaryIngredientFromVariation,
                                    amount = 1
                                });
                                break;
                        }
                        using NativeArray<ConditionData> conditionsOnConsume
                            = ConditionUIExtensions.GetConditionsOnConsume(objectData, ingredients, cook,
                            entity, databasebankCD, conditionsTableCD, flowerLookup, fishLookup,
                            giveConditionLookup, summarizedConditionsBuffer, Allocator.Temp);
                        ObjectID objectID = objectData.objectID;
                        for (int i = 0; i < conditionsOnConsume.Length; i++)
                        {
                            ConditionID conditionID = conditionsOnConsume[i].conditionID;
                            if (conditionID <= ConditionID.HungerAddition)
                            {
                                if (conditionID != ConditionID.HealthAddition)
                                {
                                    if (conditionID != ConditionID.HungerAddition)
                                    {
                                        continue;
                                    }
                                    PlayerController.AddHunger(conditionsOnConsume[i].value, playerStateCD, ref hungerCD);
                                }
                                else
                                {
                                    PlayerController.HealPlayer(conditionsOnConsume[i].value, ref healthCD, playerStateCD, summarizedConditionEffectsBuffer);
                                }
                            }
                            else if (conditionID != ConditionID.HealthReduction)
                            {
                                if (conditionID != ConditionID.HealthAdditionPercentage)
                                {
                                    continue;
                                }
                                int maxHealthWithConditions = healthCD.GetMaxHealthWithConditions(summarizedConditionEffectsBuffer);
                                int num = (int)math.round(math.clamp(conditionsOnConsume[i].value / 100f * maxHealthWithConditions, 0f, maxHealthWithConditions));
                                PlayerController.HealPlayer(num, ref healthCD, playerStateCD, summarizedConditionEffectsBuffer);

                                if (objectID == ObjectID.HealingPotion || objectID == ObjectID.GreaterHealingPotion)
                                {
                                    float num2 = summarizedConditionsBuffer[113].value / 10f;
                                    if (num2 > 0f)
                                    {
                                        EntityUtility.AddOrRefreshCondition(new ConditionData
                                        {
                                            conditionID = ConditionID.HealOverTimeFromPotion,
                                            value = (int)math.round(num * num2 / 20f),
                                            duration = 20f
                                        }, conditionsBuffer, conditionsTableCD, currentTick, tickRate, summarizedConditionsBuffer);
                                    }
                                }
                            }
                            else
                            {
                                healthChangeBuffer.Add(new HealthChangeBuffer
                                {
                                    healthChange = new HealthChange
                                    {
                                        entity = entity,
                                        amount = conditionsOnConsume[i].value
                                    }
                                });
                            }
                            EntityUtility.AddOrRefreshCondition(conditionsOnConsume[i], conditionsBuffer, conditionsTableCD, currentTick, tickRate, summarizedConditionsBuffer);
                        }

                    }
                })
                    .WithName("QuickConsume")
                    .WithBurst()
                    .Schedule();
            }
            base.OnUpdate();
        }
    }
}
