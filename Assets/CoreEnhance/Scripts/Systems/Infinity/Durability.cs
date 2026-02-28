using Assets.CoreEnhance.Scripts.Cores;
using Inventory;
using Unity.Collections;
using Unity.Entities;
using static PugDatabase;

namespace Assets.CoreEnhance.Scripts.Systems.Infinity
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class InfinityDurabilitySystem : PugSimulationSystemBase
    {
        private ComponentLookup<DurabilityCD> durabilityLookup;
        private BufferLookup<InventoryChangeBuffer> invLookup;
        private float timer;
        protected override void OnCreate()
        {
            durabilityLookup = SystemAPI.GetComponentLookup<DurabilityCD>();
            invLookup = SystemAPI.GetBufferLookup<InventoryChangeBuffer>();
            NeedDatabase();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            if (!EnhanceConfig.IsEnable(EnhanceCategory.Durability))
                return;
            if (timer < 3)
            {
                timer += World.Time.DeltaTime;
                return;
            }
            timer = 0;
            var localDatabase = database;
            var invChangeEntity = SystemAPI.GetSingletonEntity<InventoryChangeBuffer>();
            var invChangeBuffer = invLookup[invChangeEntity];
            var durabilityLookup = this.durabilityLookup;
            Entities.ForEach((Entity e, in EquippedObjectCD held, in EquipmentCD equip, in DynamicBuffer<ContainedObjectsBuffer> containers) =>
            {
                IncreaseDrb(e, held.equippedSlotIndex, equip, durabilityLookup, localDatabase, containers, invChangeBuffer);
            })
                .WithName("Infinity_Durability")
                .WithAll<PlayerGhost>()
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }

        // Token: 0x06007554 RID: 30036 RVA: 0x001EA7EC File Offset: 0x001E89EC
        private static void IncreaseDrb(Entity playerEntity, int held, EquipmentCD equip,
            ComponentLookup<DurabilityCD> durabilityLookup,
            BlobAssetReference<PugDatabaseBank> bank,
            DynamicBuffer<ContainedObjectsBuffer> containers,
            DynamicBuffer<InventoryChangeBuffer> inventoryChangeBuffer)
        {
            using NativeList<int> slot = new(4, Allocator.Temp)
            {
                held, equip.helmSlotIndex, equip.breastSlotIndex, equip.pantsSlotIndex
            };
            foreach (var slotIndex in slot)
            {
                ContainedObjectsBuffer item = containers[slotIndex];
                Entity primaryPrefabEntity = GetPrimaryPrefabEntity(item.objectID, bank, item.variation);
                if (!durabilityLookup.TryGetComponent(primaryPrefabEntity, out var durability))
                {
                    continue;
                }
                inventoryChangeBuffer.Add(new InventoryChangeBuffer
                {
                    inventoryChangeData = Create.SetAmount(playerEntity, slotIndex, item.objectID,
                   (durability.IsReinforced(item.amount) ? 2 : 1) * durability.maxDurability),
                    playerEntity = playerEntity
                });
            }
        }
    }
}
