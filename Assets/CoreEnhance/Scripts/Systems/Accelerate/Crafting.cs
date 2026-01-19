using Pug.Automation;
using Unity.Entities;

namespace Assets.CoreEnhance.Scripts.Systems.Accelerate
{
    [UpdateBefore(typeof(PugTimerSystem))]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class AccelerateCraftingSystem : PugSimulationSystemBase
    {
        private ComponentLookup<BigEntityRefCD> bigLookup;
        private ComponentLookup<CraftingCD> craftingLookup;
        private ComponentLookup<CrafterForSlotCD> slotLookup;
        private BufferLookup<CraftingTimerSlotBuffer> timerLookup;
        protected override void OnCreate()
        {
            bigLookup = SystemAPI.GetComponentLookup<BigEntityRefCD>();
            craftingLookup = SystemAPI.GetComponentLookup<CraftingCD>();
            slotLookup = SystemAPI.GetComponentLookup<CrafterForSlotCD>();
            timerLookup = SystemAPI.GetBufferLookup<CraftingTimerSlotBuffer>();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var bigLookup = this.bigLookup;
            var slotLookup = this.slotLookup;
            var timerLookup = this.timerLookup;
            var craftingLookup = this.craftingLookup;
            Entities.ForEach((ref PugTimerCD timer, in PugTimerRefCD timerRef) =>
            {
                var entity = timerRef.entity;
                if (entity == Entity.Null)
                    return;
                if (!bigLookup.TryGetComponent(entity, out var big))
                    return;
                if (!slotLookup.TryGetComponent(entity, out var slot))
                    return;
                var bigEntity = big.Value;
                int index = slot.slotIndex;
                if (!craftingLookup.HasComponent(bigEntity))
                    return;
                if (!timerLookup.TryGetBuffer(bigEntity, out var craftTimerSlot))
                    return;
                ref var craftTimer = ref craftTimerSlot.ElementAt(index);
                craftTimer.timeLeftToCraft = 0;
                timer.timer = 0;
            })
                .WithName("Accelerate_Crafting")
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
    }
}
