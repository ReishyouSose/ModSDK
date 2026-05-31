using Assets.CoreEnhance.Scripts.Cores;
using Pug.Automation;
using Pug.UnityExtensions;
using PugTilemap;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace Assets.CoreEnhance.Scripts.Systems.Automation
{
    [UpdateInGroup(typeof(PugAutomationFinishCraftingSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation, WorldSystemFilterFlags.Default)]
    [UpdateAfter(typeof(PugAutomationFishingSystem))]
    public partial class FishingCanGetItemSystem : PugSimulationSystemBase
    {
        public int simulationTickRate;
        public ComponentLookup<PugAutomationCD> pugAutomationLookup;
        public ComponentLookup<BigEntityRefCD> bigEntityRefLookup;
        public ComponentLookup<CrafterForSlotCD> crafterForSlotLookup;
        public ComponentLookup<ObjectCategoryTagsCD> objectCategoryTagsLookup;
        public ComponentLookup<LocalTransform> localTransformLookup;
        public ComponentLookup<RandomCD> randomLookup;
        public BufferLookup<ContainedObjectsBuffer> containedObjectsBufferLookup;
        public BufferLookup<CraftingTimerSlotBuffer> craftingTimerSlotBufferLookup;
        public BufferLookup<CraftingByConsumedObjectSlotBuffer> craftingByConsumedObjectSlotBufferLookup;
        public PugTimerSystem.Timer pugTimer;
        public FishingTableCD fishingTableCD;
        public LootTableBankCD lootTableBankCD;
        public TileAccessor tileAccessor;
        public BiomeLookup biomeLookup;
        public PugDatabase.DatabaseBankCD databaseBankCD;
        protected override void OnCreate()
        {
            RequireForUpdate<PugDatabase.DatabaseBankCD>();
            RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            RequireForUpdate<FishingTableCD>();
            RequireForUpdate<LootTableBankCD>();
            simulationTickRate = PlatformConfiguration.Instance.SessionConfiguration.SimulationTickRate;
            pugAutomationLookup = SystemAPI.GetComponentLookup<PugAutomationCD>();
            bigEntityRefLookup = SystemAPI.GetComponentLookup<BigEntityRefCD>();
            crafterForSlotLookup = SystemAPI.GetComponentLookup<CrafterForSlotCD>();
            objectCategoryTagsLookup = SystemAPI.GetComponentLookup<ObjectCategoryTagsCD>();
            localTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>();
            randomLookup = SystemAPI.GetComponentLookup<RandomCD>();
            containedObjectsBufferLookup = SystemAPI.GetBufferLookup<ContainedObjectsBuffer>();
            craftingTimerSlotBufferLookup = SystemAPI.GetBufferLookup<CraftingTimerSlotBuffer>();
            craftingByConsumedObjectSlotBufferLookup = SystemAPI.GetBufferLookup<CraftingByConsumedObjectSlotBuffer>();
            pugTimer = PugTimerSystem.Timer.Create(ref CheckedStateRef);
        }
        protected override void OnStartRunning()
        {
            tileAccessor = CreateTileAccessor();
            biomeLookup = SystemAPI.TryGetSingleton(out BiomeSamplesCD biomeSamplesCD) ? new BiomeLookup(biomeSamplesCD)
                : new BiomeLookup(SystemAPI.GetSingleton<BiomeRangesCD>().Value, Allocator.Persistent);
            base.OnStartRunning();
        }
        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingleton<FishingTableCD>(out var fishingTableCD))
                return;
            if (!SystemAPI.TryGetSingleton<LootTableBankCD>(out var lootTableBankCD))
                return;
            if (!SystemAPI.TryGetSingleton<PugDatabase.DatabaseBankCD>(out var databaseBankCD))
                return;
            if (!SystemAPI.TryGetSingleton<ClientServerTickRate>(out var tickRate))
                return;

            bool fishingNoBait = EnhanceConfig.IsEnable(EnhanceCategory.FishingNetNoCritter);
            bool canGetItem = EnhanceConfig.TryGetValue<float>(EnhanceCategory.FishingNetCanGetItem, out var chance);
            var itemChance = chance.Value;
            var simulationTickRate = tickRate.SimulationTickRate;
            var ecb = CreateCommandBuffer();
            var bigEntityRefLookup = this.bigEntityRefLookup;
            var pugAutomationLookup = this.pugAutomationLookup;
            var crafterForSlotLookup = this.crafterForSlotLookup;
            var craftingTimerSlotBufferLookup = this.craftingTimerSlotBufferLookup;
            var craftingByConsumedObjectSlotBufferLookup = this.craftingByConsumedObjectSlotBufferLookup;
            var containedObjectsBufferLookup = this.containedObjectsBufferLookup;
            var objectCategoryTagsLookup = this.objectCategoryTagsLookup;
            var localTransformLookup = this.localTransformLookup;
            var tileAccessor = this.tileAccessor;
            var biomeLookup = this.biomeLookup;
            var randomLookup = this.randomLookup;
            var pugTimer = this.pugTimer;

            Entities.ForEach((Entity entity, ref PugTimerRefCD pugTimerRef) =>
            {
                ecb.DestroyEntity(entity);
                Entity entity2 = pugTimerRef.entity;

                // ========== 第一层：基础组件检查（最轻量） ==========
                if (!bigEntityRefLookup.TryGetComponent(entity2, out BigEntityRefCD bigEntityRefCD))
                    return;

                Entity value = bigEntityRefCD.Value;

                // 第二层：RandomCD 检查（极轻量，提前失败）
                if (!randomLookup.HasComponent(value))
                    return;

                // 第三层：自动化状态检查
                if (pugAutomationLookup.TryGetComponent(value, out PugAutomationCD pugAutomationCD) && !pugAutomationCD.isActive)
                    return;

                // 第四层：Crafter 相关检查
                RefRO<CrafterForSlotCD> refROOptional = crafterForSlotLookup.GetRefROOptional(entity2);
                if (!refROOptional.IsValid)
                    return;

                int slotIndex = refROOptional.ValueRO.slotIndex;

                // 第五层：Timer Buffer 检查
                if (!craftingTimerSlotBufferLookup.TryGetBuffer(value, out var dynamicBuffer) ||
                    slotIndex >= dynamicBuffer.Length)
                    return;

                ref CraftingTimerSlotBuffer ptr = ref dynamicBuffer.ElementAt(slotIndex);
                if (--ptr.timeLeftToCraft > 0f)
                {
                    pugTimer.StartTimer(ecb, entity2, 1f, simulationTickRate);
                    return;
                }

                // 第六层：清理 consumed slot
                if (craftingByConsumedObjectSlotBufferLookup.TryGetBuffer(value, out var dynamicBuffer2))
                {
                    dynamicBuffer2.ElementAt(slotIndex).previousConsumedItem = default;
                }

                // ========== 第七层：钓鱼前置检查（中等开销） ==========
                // 检查 container buffer
                if (!containedObjectsBufferLookup.TryGetBuffer(value, out var containedBuffer) || slotIndex >= containedBuffer.Length)
                    return;

                ObjectDataCD objectData = containedBuffer[slotIndex].objectData;
                if (fishingNoBait)
                {
                    if (objectData.objectID != ObjectID.None)
                        return;
                }
                else
                {
                    if (objectData.objectID == ObjectID.None)
                        return;

                    // 第八层：鱼饵类型检查（需要查询数据库，相对较重）
                    Entity primaryPrefabEntity = PugDatabase.GetPrimaryPrefabEntity(objectData.objectID, databaseBankCD.databaseBankBlob, objectData.variation);

                    if (!objectCategoryTagsLookup.TryGetComponent(primaryPrefabEntity, out ObjectCategoryTagsCD objectCategoryTagsCD) ||
                        !ObjectCategoryTagsCD.HasTag(objectCategoryTagsCD.tagsBitMask, ObjectCategoryTag.Critter))
                        return;
                }

                // ========== 第九层：执行钓鱼（最重操作） ==========
                RefRW<RandomCD> randomRef = randomLookup.GetRefRW(value);
                localTransformLookup.TryGetComponent(value, out LocalTransform localTransform);
                float3 position = localTransform.Position;

                int2 worldPosition = position.RoundToInt2();
                Tileset tileset = (Tileset)tileAccessor.GetTop(worldPosition).tileset;
                Biome biome = biomeLookup.GetBiome(worldPosition);
                fishingTableCD.GetFishingStats(tileset, biome, out var fishingInfo, out var _);

                ref var rng = ref randomRef.ValueRW.Value;
                var lootID = rng.NextFloat() < itemChance ? fishingInfo.lootTableID : fishingInfo.fishLootTableID;
                using var randomLoot = PugDatabase.GetRandomLoot(fishingInfo.fishLootTableID, ref rng, lootTableBankCD.Value, databaseBankCD.databaseBankBlob, biome);

                if (randomLoot.Length != 0)
                {
                    containedBuffer[slotIndex] = new ContainedObjectsBuffer
                    {
                        objectData = new ObjectDataCD
                        {
                            objectID = randomLoot[0].objectID,
                            amount = 1
                        }
                    };
                }

                ptr.timeLeftToCraft = 0f;
            })
            .WithName("BlockFishingNet")
            .WithBurst()
            .Schedule();

            base.OnUpdate();
        }
        protected override void OnStopRunning()
        {
            biomeLookup.Dispose();
            base.OnStopRunning();
        }
    }
}
