using Assets.CoreEnhance.Scripts.Systems.Quick;
using Assets.CoreEnhance.Scripts.UI.ItemLookup;
using CoreLib.Submodule.ControlMapping;
using CoreLib.Submodule.UserInterface;
using Rewired;

namespace Assets.CoreEnhance.Scripts.Cores
{
    public static class ModKeyBind
    {
        private const string CoreEnhance = "CoreEnhance_";
        internal const string ItemLookup = CoreEnhance + nameof(ItemLookup);
        internal const string QuickOpenLockedChest = CoreEnhance + nameof(QuickOpenLockedChest);
        internal const string ContainerHighLight = CoreEnhance + nameof(ContainerHighLight);
        //internal const string QuickHealth = CoreEnhance + nameof(QuickHealth);
        //internal const string QuickPotion = CoreEnhance + nameof(QuickPotion);
        //internal const string QuickCooked = CoreEnhance + nameof(QuickCooked);
        public static void Load()
        {
            int cateogry = ControlMappingModule.AddNewCategory("CoreEnhance");
            ControlMappingModule.AddKeyboardBind(ItemLookup, KeyboardKeyCode.F, ModifierKey.Control, categoryId: cateogry);
            /*{ "en", "Item Lookup" },
                { "zh-CN","查询获取途径" }*/

            ControlMappingModule.AddKeyboardBind(QuickOpenLockedChest, KeyboardKeyCode.O, ModifierKey.Control, categoryId: cateogry);
            /*{ "en", "Quick Open Locked Chest" },
                { "zh-CN","快速开启上锁宝箱" }*/

            ControlMappingModule.AddKeyboardBind(ContainerHighLight, KeyboardKeyCode.LeftAlt, categoryId: cateogry);
            /*{ "en", "Container HighLight (Hold)" },
                { "zh-CN","容器高亮（按住）" }*/
            //RewiredExtensionModule.AddKeybind(QuickHealth, "Quick Health", KeyboardKeyCode.Q, ModifierKey.Control);
            //RewiredExtensionModule.AddKeybind(QuickPotion, "Quick Potion", KeyboardKeyCode.W, ModifierKey.Control);
            //RewiredExtensionModule.AddKeybind(QuickCooked, "Quick Cooked", KeyboardKeyCode.E, ModifierKey.Control);
        }
        public static void Handle(PlayerController p)
        {
            Player r = p.inputModule.rewiredPlayer;
            var lookup = ItemLookupUI.ins;
            var obtain = ObtainLookupUI.ins;
            if (r.GetButtonDown(ItemLookup))
            {
                if (lookup.Root.activeInHierarchy)
                    lookup.HideUI();
                else if (obtain.Root.activeInHierarchy)
                    obtain.HideUI();
                else
                {
                    var select = Manager.ui.currentSelectedUIElement;
                    if (select is SlotUIBase slot)
                    {
                        UserInterfaceModule.OpenModUI("CoreEnhance:ObtainLookup");
                        obtain.Focus.SetItem(slot.GetContainedObject().objectID);
                        obtain.SearchLoot();
                    }
                    else
                    {
                        UserInterfaceModule.OpenModUI("CoreEnhance:ItemLookup");
                    }
                }
            }

            if (r.GetButtonDown(QuickOpenLockedChest))
                QuickOpenLockedChestClient.Trigger(p);
            //if (r.GetButtonDown(QuickHealth))
            //    QuickConsumableClient.QuickConsume(QuickConsumeType.Health);
            //if (r.GetButtonDown(QuickPotion))
            //    QuickConsumableClient.QuickConsume(QuickConsumeType.Potion);
            //if (r.GetButtonDown(QuickCooked))
            //    QuickConsumableClient.QuickConsume(QuickConsumeType.Cooked);
        }
    }
}
