using Assets.CoreEnhance.Scripts.Cores;
using Assets.CoreEnhance.Scripts.Helpers;
using Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Systems.Misc
{
    public struct ContainerHighLightCD : IComponentData
    {
        public bool marking;
    }

    [UpdateInGroup(typeof(LocalPresentationCueSystemGroup))]
    [UpdateAfter(typeof(InteractableVisualUpdateSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class ContainerDisplaySystem : PugSimulationSystemBase
    {
        private ComponentLookup<InteractorCD> interactorLookup;
        private BufferLookup<ContainedObjectsBuffer> containerLookup;
        private BufferLookup<CanCraftObjectsBuffer> craftLookup;
        private BufferLookup<VendingMachineItemBuffer> vendingLookup;
        protected override void OnCreate()
        {
            interactorLookup = SystemAPI.GetComponentLookup<InteractorCD>();
            containerLookup = SystemAPI.GetBufferLookup<ContainedObjectsBuffer>();
            craftLookup = SystemAPI.GetBufferLookup<CanCraftObjectsBuffer>();
            vendingLookup = SystemAPI.GetBufferLookup<VendingMachineItemBuffer>();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            if (!EnhanceConfig.IsEnable(EnhanceCategory.ContainerDisplay))
                return;

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
            var containerLookup = this.containerLookup;
            var craftLookup = this.craftLookup;
            var vendingLookup = this.vendingLookup;
            Entities.ForEach((Entity e, ref ContainerHighLightCD hl, in InteractableObjectReferenceCD interact) =>
            {
                if (e == closet)
                    return;
                ref bool marking = ref hl.marking;
                if (needLight)
                {
                    if (containerLookup.TryGetBuffer(e, out var containers))
                    {
                        foreach (var container in containers)
                        {
                            if (container.objectID == target)
                            {
                                UpdateHighLight(interact, Color.cyan);
                                marking = true;
                                return;
                            }
                        }
                    }
                    if (craftLookup.TryGetBuffer(e, out var crafts))
                    {
                        foreach (var craft in crafts)
                        {
                            if (craft.objectID == target)
                            {
                                UpdateHighLight(interact, Color.cyan);
                                marking = true;
                                return;
                            }
                        }
                    }
                    if (vendingLookup.TryGetBuffer(e, out var vendings))
                    {
                        foreach (var vending in vendings)
                        {
                            if (vending.objectID == target)
                            {
                                UpdateHighLight(interact, Color.cyan);
                                marking = true;
                                return;
                            }
                        }
                    }
                }
                if (!marking)
                    return;
                marking = false;
                UpdateHighLight(interact, Color.clear);
            })
                .WithName("ContainerHighLight")
                .WithBurst()
                .Run();
        }
        private static void UpdateHighLight(InteractableObjectReferenceCD interact, Color color)
        {
            var interactObject = interact.Value.Value;
            var optionalOutlineController = interactObject.optionalOutlineController;
            if (optionalOutlineController != null)
            {
                optionalOutlineController.showOutline = true;
                optionalOutlineController.SetColor(color);
            }

            foreach (OutlineController additionalOutlineController in interactObject.additionalOutlineControllers)
            {
                additionalOutlineController.showOutline = true;
                additionalOutlineController.SetColor(color);
            }

            var spriteObjects = interactObject.spriteObjects;
            if (spriteObjects == null)
            {
                return;
            }
            if (spriteObjects == null)
            {
                return;
            }
            foreach (var sprite in spriteObjects)
            {
                sprite.outlineColor = color;
            }
        }
        internal static void MarkHighLight(Entity e, GameObject authoringData, EntityManager manager)
        {
            if (!authoringData.HasComponent<LocalInteractableAuthoring>())
                return;
            if (authoringData.HasComponent<InventoryAuthoring>()
                || authoringData.HasComponent<MerchantAuthoring>()
                || authoringData.HasComponent<CraftingAuthoring>()
                || authoringData.HasComponent<VendingMachineAuthoring>())
            {
                //Debug.Log("[CoreEnhance] Container Display: Mark " + authoringData.GetEntityObjectID());
                manager.AddComponent<ContainerHighLightCD>(e);
            }
        }
    }
}
