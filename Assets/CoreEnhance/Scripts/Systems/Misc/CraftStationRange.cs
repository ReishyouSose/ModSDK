//using Assets.CoreEnhance.Scripts.Cores;
//using Assets.CoreEnhance.Scripts.Helpers;
//using Interaction;
//using Inventory;
//using Outlines.Systems;
//using Pug.UnityExtensions;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Physics;
//using Unity.Transforms;
//using UnityEngine;

//namespace Assets.CoreEnhance.Scripts.Systems.Quick
//{
//    [UpdateAfter(typeof(VisualOutlineDisplaySystem))]
//    [UpdateInGroup(typeof(PresentationSystemGroup))]
//    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation, WorldSystemFilterFlags.Default)]
//    public partial class CraftStationRangeSystem : PugSimulationSystemBase
//    {
//        private struct CraftStationRangeCD : IComponentData
//        {
//            public bool Hovered;
//        }

//        private ComponentLookup<DirectionCD> directionLookup;
//        private ComponentLookup<InteractableObjectReferenceCD> interactLookup;
//        private ComponentLookup<InventoryAutoTransferEnabledCD> autoTransferLookup;
//        private ComponentLookup<LocalTransform> transLookup;
//        private CollisionWorld collision;
//        protected override void OnCreate()
//        {
//            NeedDatabase();
//            directionLookup = SystemAPI.GetComponentLookup<DirectionCD>();
//            interactLookup = SystemAPI.GetComponentLookup<InteractableObjectReferenceCD>();
//            autoTransferLookup = SystemAPI.GetComponentLookup<InventoryAutoTransferEnabledCD>();
//            transLookup = SystemAPI.GetComponentLookup<LocalTransform>();
//            base.OnCreate();
//        }
//        protected override void OnStartRunning()
//        {
//            collision = GetPhysicsWorld().CollisionWorld;
//            base.OnStartRunning();
//        }
//        protected override void OnUpdate()
//        {
//            var player = Manager.main.player;
//            if (player == null)
//                return;
//            bool press = player.inputModule.rewiredPlayer.GetButton(ModKeyBind.CraftStationRange);
//            var mouse = MiscHelper.MouseWorld.ToFloat2();
//            var database = this.database;
//            var ecb = CreateCommandBuffer();
//            var directionLookup = this.directionLookup;
//            var interactLookup = this.interactLookup;
//            var transLookup = this.transLookup;
//            var autoTransferLookup = this.autoTransferLookup;
//            var collision = this.collision;
//            Entities.ForEach((Entity entity, ref CraftStationRangeCD craft, in LocalTransform trans, in InteractableCD interactable, in InteractableObjectReferenceCD interact) =>
//            {
//                directionLookup.TryGetComponent(entity, out var direction);
//                if (press)
//                {
//                    if (craft.Hovered)
//                        return;
//                    craft.Hovered = true;
//                    var rect = GraphicEntityHelper.GetInteractableRect(trans, interactable, Direction.FromVector(direction.direction).id);
//                    if (rect.Contains(mouse))
//                    {
//                        NativeList<Entity> list = new(20, Allocator.Temp);
//                        InventoryUtility.GetNearbyChestsForCraftingByDistance(trans.Position, collision, autoTransferLookup, transLookup, ref list);
//                        foreach (var e in list)
//                        {
//                            if (interactLookup.TryGetComponent(e, out var interactRef))
//                                GraphicEntityHelper.UpdateOutline(interactRef, Color.cyan);
//                        }
//                        list.Dispose();
//                    }
//                }
//                else if(craft.Hovered)
//                {
//                    craft.Hovered = false;
//                    GraphicEntityHelper.UpdateOutline(interact, Color.clear);
//                }
//            })
//                .WithName("ChooseChest")
//                .WithoutBurst()
//                .Run();
//            base.OnUpdate();
//        }

//        internal static void MarkCraftStation(Entity e, GameObject authoringData, EntityManager manager)
//        {
//            if (!authoringData.HasComponent<PlaceableObjectAuthoring>())
//                return;
//            if (!authoringData.TryGetComponent<CraftingAuthoring>(out var craft) || craft.craftingType != CraftingType.Simple)
//                return;
//            manager.AddComponent<CraftStationRangeCD>(e);
//        }
//    }
//}