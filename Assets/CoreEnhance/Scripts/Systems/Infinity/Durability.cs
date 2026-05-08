using Assets.CoreEnhance.Scripts.Cores;
using Inventory;
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
            var database = this.database;
            var invChangeEntity = SystemAPI.GetSingletonEntity<InventoryChangeBuffer>();
            var invChangeBuffer = invLookup[invChangeEntity];
            var durabilityLookup = this.durabilityLookup;
            Entities.ForEach((Entity e, in EquippedObjectCD held, in EquipmentCD equip, in DynamicBuffer<ContainedObjectsBuffer> containers) =>
            {
                int length = containers.Length;
                for(int i = 0; i < length; i++)
                {
                    ContainedObjectsBuffer item = containers[i];
                    Entity primaryPrefabEntity = GetPrimaryPrefabEntity(item.objectID, database, item.variation);
                    if (!durabilityLookup.TryGetComponent(primaryPrefabEntity, out var durability))
                    {
                        continue;
                    }
                    invChangeBuffer.Add(new InventoryChangeBuffer
                    {
                        inventoryChangeData = Create.SetAmount(e, i, item.objectID, (durability.IsReinforced(item.amount) ? 2 : 1) * durability.maxDurability),
                        playerEntity = e
                    });
                }
            })
                .WithName("Infinity_Durability")
                .WithAll<PlayerGhost>()
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
    }
}
