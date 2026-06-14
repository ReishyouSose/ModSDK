using Assets.CoreEnhance.Scripts.Cores;
using Assets.CoreEnhance.Scripts.Helpers;
using Interaction;
using Outlines.Systems;
using System.ComponentModel;
using Unity.Entities;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Systems.Misc
{
    public struct ContainerHighLightCD : IComponentData
    {
        public bool marking;
    }

    [UpdateAfter(typeof(VisualOutlineDisplaySystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation, WorldSystemFilterFlags.Default)]
    public partial class ContainerDisplaySystem : PugSimulationSystemBase
    {
        private ComponentLookup<InteractorCD> interactorLookup;
        private BufferLookup<ContainedObjectsBuffer> containerLookup;
        private BufferLookup<CanCraftObjectsBuffer> craftLookup;
        private BufferLookup<VendingMachineItemBuffer> vendingLookup;
        private float timer;
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
            ContainedObjectsBuffer target = default;
            bool needLight = false;
            if (player.inputModule.rewiredPlayer.GetButton(ModKeyBind.ContainerHighLight) && selected != null)
            {
                target = selected.GetContainedObject();
                needLight = target.objectID != ObjectID.None;
                timer = 0.1f;
            }
            if (timer <= 0)
                return;
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
                            if (container.objectID == target.objectID && container.variation == target.variation)
                            {
                                GraphicEntityHelper.UpdateOutline(interact, Color.cyan);
                                marking = true;
                                Debug.Log("highlight chest");
                                return;
                            }
                        }
                    }
                    if (craftLookup.TryGetBuffer(e, out var crafts))
                    {
                        foreach (var craft in crafts)
                        {
                            if (craft.objectID == target.objectID)
                            {
                                GraphicEntityHelper.UpdateOutline(interact, Color.cyan);
                                marking = true;
                                return;
                            }
                        }
                    }
                    if (vendingLookup.TryGetBuffer(e, out var vendings))
                    {
                        foreach (var vending in vendings)
                        {
                            if (vending.objectID == target.objectID)
                            {
                                GraphicEntityHelper.UpdateOutline(interact, Color.cyan);
                                marking = true;
                                return;
                            }
                        }
                    }
                }
                if (!marking)
                    return;
                marking = false;
                GraphicEntityHelper.UpdateOutline(interact, Color.clear);
            })
                .WithName("ContainerHighLight")
                .WithoutBurst()
                .Schedule();
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
