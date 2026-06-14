using Assets.CoreEnhance.Scripts.Cores;
using Inventory;
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
    [UpdateBefore(typeof(PugAutomationFishingSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class FishingNetCanGetItemSystem : PugSimulationSystemBase
    {
        private TileAccessor tileAccessor;
        private BiomeLookup biomeLookup;
        private PugTimerSystem.Timer pugTimer;
        private ComponentLookup<BigEntityRefCD> bigEntityRefLookup;
        private ComponentLookup<PugAutomationCD> pugAutomationLookup;
        private ComponentLookup<CrafterForSlotCD> crafterForSlotLookup;
        private BufferLookup<CraftingTimerSlotBuffer> craftingTimerSlotBufferLookup;
        private BufferLookup<CraftingByConsumedObjectSlotBuffer> craftingByConsumedObjectSlotBufferLookup;
        private ComponentLookup<LocalTransform> localTransformLookup;
        private BufferLookup<ContainedObjectsBuffer> containedObjectsBufferLookup;
        private ComponentLookup<ObjectCategoryTagsCD> objectCategoryTagsLookup;
        private ComponentLookup<RandomCD> randomLookup;
        protected override void OnCreate()
        {
            RequireForUpdate<BiomeRangesCD>();
            RequireForUpdate<BiomeSamplesCD>();
            RequireForUpdate<PugDatabase.DatabaseBankCD>();
            RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            RequireForUpdate<FishingTableCD>();
            RequireForUpdate<LootTableBankCD>();
            RequireForUpdate<ClientServerTickRate>();
            RequireForUpdate<NetworkTime>();
            bigEntityRefLookup = SystemAPI.GetComponentLookup<BigEntityRefCD>();
            pugAutomationLookup = SystemAPI.GetComponentLookup<PugAutomationCD>();
            crafterForSlotLookup = SystemAPI.GetComponentLookup<CrafterForSlotCD>();
            craftingTimerSlotBufferLookup = SystemAPI.GetBufferLookup<CraftingTimerSlotBuffer>();
            craftingByConsumedObjectSlotBufferLookup = SystemAPI.GetBufferLookup<CraftingByConsumedObjectSlotBuffer>();
            localTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>();
            containedObjectsBufferLookup = SystemAPI.GetBufferLookup<ContainedObjectsBuffer>();
            objectCategoryTagsLookup = SystemAPI.GetComponentLookup<ObjectCategoryTagsCD>();
            randomLookup = SystemAPI.GetComponentLookup<RandomCD>();
            pugTimer = PugTimerSystem.Timer.Create(ref CheckedStateRef);
            base.OnCreate();
        }
        protected override void OnStartRunning()
        {
            tileAccessor = CreateTileAccessor();
            biomeLookup = (SystemAPI.TryGetSingleton(out BiomeSamplesCD biomeSamplesCD) ? new BiomeLookup(biomeSamplesCD) : new BiomeLookup(SystemAPI.GetSingleton<BiomeRangesCD>().Value, Allocator.Persistent));
            base.OnStartRunning();
        }
        protected override void OnUpdate()
        {
            if (!EnhanceConfig.TryGetValue<float>(EnhanceCategory.FishingNetCanGetItem, out var value))
                return;
            float chance = value.Value;
            var ecb = CreateCommandBuffer();
            var biomeLookup = this.biomeLookup;
            var tileAccessor = this.tileAccessor;
            var pugTimer = this.pugTimer;
            var bigEntityRefLookup = this.bigEntityRefLookup;
            var pugAutomationLookup = this.pugAutomationLookup;
            var crafterForSlotLookup = this.crafterForSlotLookup;
            var craftingTimerSlotBufferLookup = this.craftingTimerSlotBufferLookup;
            var craftingByConsumedObjectSlotBufferLookup = this.craftingByConsumedObjectSlotBufferLookup;
            var localTransformLookup = this.localTransformLookup;
            var containedObjectsBufferLookup = this.containedObjectsBufferLookup;
            var objectCategoryTagsLookup = this.objectCategoryTagsLookup;
            var randomLookup = this.randomLookup;
            var databaseBankCD = SystemAPI.GetSingleton<PugDatabase.DatabaseBankCD>();
            var simulationTickRate = SystemAPI.GetSingleton<ClientServerTickRate>().SimulationTickRate;
            var fishingTableCD = SystemAPI.GetSingleton<FishingTableCD>();
            var lootTableBankCD = SystemAPI.GetSingleton<LootTableBankCD>();
            Entities.ForEach((Entity entity, ref PugTimerRefCD pugTimerRef) =>
            {
                ecb.DestroyEntity(entity);
                Entity entity2 = pugTimerRef.entity;
                if (!bigEntityRefLookup.TryGetComponent(entity2, out BigEntityRefCD bigEntityRefCD))
                {
                    return;
                }
                Entity value = bigEntityRefCD.Value;
                if (pugAutomationLookup.TryGetComponent(value, out PugAutomationCD pugAutomationCD) && !pugAutomationCD.isActive)
                {
                    return;
                }
                RefRO<CrafterForSlotCD> refROOptional = crafterForSlotLookup.GetRefROOptional(entity2);
                if (!refROOptional.IsValid)
                {
                    return;
                }
                int slotIndex = refROOptional.ValueRO.slotIndex;
                if (!craftingTimerSlotBufferLookup.TryGetBuffer(value, out DynamicBuffer<CraftingTimerSlotBuffer> dynamicBuffer) || slotIndex >= dynamicBuffer.Length)
                {
                    return;
                }
                ref CraftingTimerSlotBuffer ptr = ref dynamicBuffer.ElementAt(slotIndex);
                ptr.timeLeftToCraft -= 1f;
                if (ptr.timeLeftToCraft > 0f)
                {
                    pugTimer.StartTimer(ecb, entity2, 1f, simulationTickRate);
                    return;
                }
                if (craftingByConsumedObjectSlotBufferLookup.TryGetBuffer(value, out DynamicBuffer<CraftingByConsumedObjectSlotBuffer> dynamicBuffer2))
                {
                    dynamicBuffer2.ElementAt(slotIndex).previousConsumedItem = default;
                }
                if (!InventoryUtility.CanFish(value, slotIndex, containedObjectsBufferLookup, objectCategoryTagsLookup, databaseBankCD))
                {
                    return;
                }
                if (!localTransformLookup.TryGetComponent(value, out LocalTransform localTransform))
                {
                    return;
                }
                float3 position = localTransform.Position;
                Fish(value, slotIndex, chance, position, fishingTableCD, lootTableBankCD, tileAccessor, biomeLookup, containedObjectsBufferLookup, randomLookup, databaseBankCD);
                ptr.timeLeftToCraft = 0f;
            })
                .WithName("FishingNetBlockSystem")
                .WithBurst()
                .WithAll<IsFishingTimerTriggerCD>()
                .Schedule();
            base.OnUpdate();
        }
        protected override void OnStopRunning()
        {
            biomeLookup.Dispose();
            base.OnStopRunning();
        }
        private static void Fish(Entity inventory, int fishSlotIndex, float itemChance, float3 baitPosition, FishingTableCD fishingTableCD, LootTableBankCD lootTableBankCD, TileAccessor tileAccessor, BiomeLookup biomeLookup, BufferLookup<ContainedObjectsBuffer> containedObjectsBufferLookup, ComponentLookup<RandomCD> randomLookup, PugDatabase.DatabaseBankCD databaseBankCD)
        {
            if (!containedObjectsBufferLookup.TryGetBuffer(inventory, out var val) || val[fishSlotIndex].objectID == ObjectID.None)
            {
                return;
            }

            RefRW<RandomCD> refRWOptional = randomLookup.GetRefRWOptional(inventory);
            if (!refRWOptional.IsValid)
            {
                return;
            }

            int2 worldPosition = baitPosition.RoundToInt2();
            Tileset tileset = (Tileset)tileAccessor.GetTop(worldPosition).tileset;
            Biome biome = biomeLookup.GetBiome(worldPosition);
            fishingTableCD.GetFishingStats(tileset, biome, out var fishingInfo, out var _);
            ref var rng = ref refRWOptional.ValueRW.Value;
            var id = rng.NextFloat() < itemChance ? fishingInfo.fishLootTableID : fishingInfo.lootTableID;
            using var randomLoot = PugDatabase.GetRandomLoot(id, ref rng, lootTableBankCD.Value, databaseBankCD.databaseBankBlob, biome);
            if (randomLoot.Length != 0)
            {
                var loot = randomLoot[0];
                val[fishSlotIndex] = new ContainedObjectsBuffer
                {
                    objectData = new ObjectDataCD
                    {
                        objectID = loot.objectID,
                        amount = loot.amount
                    }
                };
            }
        }
    }
}
