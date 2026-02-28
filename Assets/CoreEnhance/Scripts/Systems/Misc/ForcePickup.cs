using Assets.CoreEnhance.Scripts.Cores;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.CoreEnhance.Scripts.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [UpdateBefore(typeof(PickUpItemSystem))]
    public partial class ForcePickupSystem : PugSimulationSystemBase
    {
        protected override void OnUpdate()
        {
            if (!EnhanceConfig.IsEnable(EnhanceCategory.IgnoreRayChecksForPickup))
                return;
            Entities.ForEach((ref PickUpItemCD pickup) =>
            {
                if (pickup.state != PickUpItemState.ForcePickUp)
                    return;
                if (pickup.targetEntity == Entity.Null)
                    return;
                pickup.ignoreRayChecksForPickup = true;
            })
                .WithName("ForcePickup")
                .WithNone<EntityDestroyedCD>()
                .WithBurst()
                .Schedule();
        }
    }
}
