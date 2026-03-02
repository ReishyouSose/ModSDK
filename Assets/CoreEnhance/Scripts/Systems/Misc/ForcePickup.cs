using Assets.CoreEnhance.Scripts.Components;
using Assets.CoreEnhance.Scripts.Cores;
using Pug.UnityExtensions;
using PugTilemap;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

namespace Assets.CoreEnhance.Scripts.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [UpdateBefore(typeof(PickUpItemSystem))]
    public partial class ForcePickupSystem : PugSimulationSystemBase
    {
        protected override void OnCreate()
        {
            NeedTileUpdateBuffer();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingletonBuffer<TileUpdateBuffer>(out var buffer, true))
                return;
            if (!EnhanceConfig.IsEnable(EnhanceCategory.IgnoreRayChecksForPickup))
                return;
            var ecb = CreateCommandBuffer();
            Entities.ForEach((Entity e, ref PickUpItemCD pickup, in LocalTransform trans) =>
            {
                if (pickup.state != PickUpItemState.ForcePickUp)
                    return;
                if (pickup.targetEntity == Entity.Null)
                    return;
                ecb.AddComponent<ProcessedTagCD>(e);
                var pos = trans.Position.RoundToInt2();
                foreach (var update in buffer)
                {
                    if (!update.position.Equals(pos))
                        continue;
                    if (update.tile.tileType != TileType.immune)
                        continue;
                    if (update.command != TileUpdateBuffer.Command.Add)
                        continue;
                    return;
                }
                pickup.ignoreRayChecksForPickup = true;
            })
                .WithName("ForcePickup")
                .WithNone<EntityDestroyedCD>()
                .WithNone<ProcessedTagCD>()
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
    }
}
