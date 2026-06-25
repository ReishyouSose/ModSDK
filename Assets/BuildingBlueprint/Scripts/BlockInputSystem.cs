using PlayerEquipment;
using Unity.Entities;

namespace Assets.BuildingBlueprint.Scripts
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation, WorldSystemFilterFlags.Default)]
    [UpdateInGroup(typeof(EquipmentUpdateSystemGroup))]
    [UpdateBefore(typeof(EquipmentUpdateSystem))]
    public partial class BlockInputSystem : PugSimulationSystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref ClientInput input, in ProtectStateCD protect) =>
            {
                if (protect.State)
                {
                    input.SetButtonState(CommandInputButtonStateNames.Interact_HeldDown, false);
                    input.SetButtonState(CommandInputButtonStateNames.SecondInteract_HeldDown, false);
                }
            })
                .WithName("BlockInputWhenBlueprint")
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
    }
}
