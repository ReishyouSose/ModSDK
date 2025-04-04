using Assets.CoreEnhance.Scripts.Configs;
using Assets.CoreEnhance.Scripts.Systems.Quick;
using CoreLib.ModResources;
using HarmonyLib;
using Inventory;
using System.Linq;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Patchs
{
    public class ExtraChestButton : MonoBehaviour
    {
        public ExtraChestAction action;
        public float offsetX;
    }

    [HarmonyPatch(typeof(InventoryUI))]
    public static class InventoryUIButtonPatch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        private static void AddExtraButton(InventoryUI __instance)
        {
            var origin = __instance.optionalQuickStackButton;
            if (origin == null)
                return;
            Debug.Log("[CoreEnhance] Add Extra Button!");
            var sprites = ResourcesModule.LoadSprites("Assets/CoreEnhance/Sprites/Misc/ChestUI.png");
            origin.GetComponent<BoxCollider>().size = new(1, 1, 1);
            foreach (var sprite in sprites.Cast<Sprite>())
            {
                switch (sprite.name[^1])
                {
                    case '1':
                        CopyButton(sprite, origin, ExtraChestAction.Replenish, 1.25f, 0f);
                        break;
                    case '2':
                        CopyButton(sprite, origin, ExtraChestAction.PutAll, 0, 2.5f);
                        break;
                    case '3':
                        CopyButton(sprite, origin, ExtraChestAction.TakeAll, 1.25f, 2.5f);
                        break;
                    case '4':
                        var button = __instance.optionalSortButton;
                        button.spritesShownPressed[1].sprite = sprite;
                        button.spritesShownUnpressed[1].sprite = sprite;
                        button.GetComponent<BoxCollider>().size = new(1, 1, 1);
                        break;
                    case '5':
                        CopyButton(sprite, origin, ExtraChestAction.Split, 1.25f, 1.25f);
                        break;
                    default:
                        continue;
                }
            }
            origin.onLeftClick.SetPersistentListenerState(0, UnityEngine.Events.UnityEventCallState.Off);
            var extra = origin.gameObject.AddComponent<ExtraChestButton>();
            extra.action = ExtraChestAction.QuickStack;
            origin.onLeftClick.AddListener(OverrideQuickStack);
        }


        [HarmonyPatch("UpdateContainerSize")]
        [HarmonyPostfix]
        private static void UpdatePosition(InventoryUI __instance)
        {
            var inventoryHandler = __instance.GetInventoryHandler();
            Chest chest = inventoryHandler.entityMonoBehaviour as Chest;
            var quickStack = __instance.optionalQuickStackButton;
            if (quickStack == null)
                return;
            var buttons = quickStack.transform.parent.GetComponentsInChildren<ExtraChestButton>(true);
            if (chest != null && chest.showSortAndQuickStackButtons)
            {
                foreach (var button in buttons)
                {
                    var pos = button.transform.localPosition;
                    var x = -GetSideStartPosition(__instance.visibleColumns, __instance.spread) + 1.375f;
                    button.transform.localPosition = new(x + button.offsetX, pos.y, pos.z);
                    button.gameObject.SetActive(true);
                }
            }
            else
            {
                foreach (var button in buttons)
                {
                    button.gameObject.SetActive(false);
                }
            }
        }
        private static void CopyButton(Sprite icon, ButtonUIElement origin,
            ExtraChestAction action, float x, float y)
        {
            var button = Object.Instantiate(origin, origin.transform.parent);
            var pos = button.transform.localPosition;
            button.transform.localPosition = new(pos.x, pos.y + y, pos.z);
            var info = button.gameObject.AddComponent<ExtraChestButton>();
            info.action = action;
            info.offsetX = x;
            button.spritesShownUnpressed[1].sprite = icon;
            button.spritesShownPressed[1].sprite = icon;
            string label = "CoreEnhance/ExtraChestAction/" + action;
            button.optionalTitle.mTerm = label;
            button.optionalHoverDesc.mTerm = label + "Desc";
            button.onLeftClick.SetPersistentListenerState(0, UnityEngine.Events.UnityEventCallState.Off);
            button.onLeftClick.AddListener(ExtraChestActionClient.Trigger);
        }
        private static float GetSideStartPosition(int size, float spread)
        {
            return -((size - 1) / 2f) * spread;
        }
        private static void OverrideQuickStack()
        {
            var player = Manager.main.player;
            if (EnhanceConfig.IsEnable(EnhanceCategory.Misc, EC_Misc.ReplaceQuickStack))
            {
                ExtraChestActionClient.Trigger();
                return;
            }
            player.QueueInputAction(new()
            {
                action = UIInputAction.InventoryChange,
                inventoryChangeData = Create.QuickStack(player.entity, player.activeInventoryHandler.inventoryEntity)
            });
        }
    }
}
