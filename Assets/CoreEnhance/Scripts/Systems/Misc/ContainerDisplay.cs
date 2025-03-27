using Assets.CoreEnhance.Scripts.Configs;
using Interaction;
using Unity.Entities;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Systems.Misc
{
    public struct ContainerHighLightCD : IComponentData
    {
        public bool needRefresh;
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class ContainerDisplaySystem : PugSimulationSystemBase
    {
        private ComponentLookup<InteractorCD> interactorLookup;
        protected override void OnCreate()
        {
            interactorLookup = SystemAPI.GetComponentLookup<InteractorCD>();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            if (!EnhanceConfig.IsEnable(EnhanceCategory.Misc, EC_Misc.ContainerDisplay))
                return;
            var ecb = CreateCommandBuffer();
            Entities.ForEach((Entity e) =>
            {
                ecb.AddComponent<ContainerHighLightCD>(e);
            })
                .WithName("CheckInteractable")
                .WithAll<InteractableObjectReferenceCD>()
                .WithAll<ContainedObjectsBuffer>()
                .WithNone<ContainerHighLightCD>()
                .WithBurst()
                .Schedule();

            var player = Manager.main.player;
            if (player == null)
                return;
            var selected = Manager.ui.currentSelectedUIElement;
            ObjectID target = ObjectID.None;
            bool needLight = false;
            if (player.inputModule.rewiredPlayer.GetButton(ModKeyBind.ContainerHighLight) && selected != null)
            {
                target = selected.GetContainedObject().objectID;
                needLight = target != ObjectID.None;
            }
            var closet = interactorLookup.GetRefRO(player.entity).ValueRO.currentClosestInteractable;
            Entities.ForEach((Entity e, ref ContainerHighLightCD highLight, in InteractableObjectReferenceCD interact,
                in DynamicBuffer<ContainedObjectsBuffer> containers) =>
            {
                if (e == closet)
                    return;
                ref bool refresh = ref highLight.needRefresh;
                if (needLight)
                {
                    foreach (var container in containers)
                    {
                        if (container.objectID == target)
                        {
                            UpdateHighLight(interact, Color.cyan);
                            refresh = true;
                            return;
                        }
                    }
                }
                if (!refresh)
                    return;
                refresh = false;
                UpdateHighLight(interact, Color.clear);
            })
                .WithName("ContainerHighLight")
                .WithBurst()
                .Schedule();
        }
        private static void UpdateHighLight(InteractableObjectReferenceCD interact, Color color)
        {
            foreach (var sprite in interact.Value.Value.spriteObjects)
            {
                sprite.outlineColor = color;
            }
        }
    }
}
