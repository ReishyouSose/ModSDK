using Assets.BuildingBlueprint.Scripts.Components;
using PlayerEquipment;
using Unity.Entities;

namespace Assets.BuildingBlueprint.Scripts.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation, WorldSystemFilterFlags.Default)]
    [UpdateInGroup(typeof(EquipmentUpdateSystemGroup))]
    [UpdateBefore(typeof(EquipmentUpdateSystem))]
    public partial class BlockInputSystem : PugSimulationSystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref ClientInput input, in SelectionOptionCD option) =>
            {
                if (option.Open)
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
