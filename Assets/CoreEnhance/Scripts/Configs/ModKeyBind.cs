using Assets.CoreEnhance.Scripts.UI.ItemLookup;
using CoreLib.RewiredExtension;
using CoreLib.UserInterface;
using Rewired;

namespace Assets.CoreEnhance.Scripts.Configs
{
    public static class ModKeyBind
    {
        private const string CoreEnhance = "CoreEnhance:";
        internal const string ItemLookup = CoreEnhance + nameof(ItemLookup);
        //internal const string QuickHealth = CoreEnhance + nameof(QuickHealth);
        //internal const string QuickPotion = CoreEnhance + nameof(QuickPotion);
        //internal const string QuickCooked = CoreEnhance + nameof(QuickCooked);
        public static void Load()
        {
            RewiredExtensionModule.AddKeybind(ItemLookup, "Item Lookup", KeyboardKeyCode.F, ModifierKey.Control);
            //RewiredExtensionModule.AddKeybind(QuickHealth, "Quick Health", KeyboardKeyCode.Q, ModifierKey.Control);
            //RewiredExtensionModule.AddKeybind(QuickPotion, "Quick Potion", KeyboardKeyCode.W, ModifierKey.Control);
            //RewiredExtensionModule.AddKeybind(QuickCooked, "Quick Cooked", KeyboardKeyCode.E, ModifierKey.Control);
        }
        public static void Handle(PlayerController p)
        {
            Player r = p.inputModule.rewiredPlayer;
            if (r.GetButtonDown(ItemLookup))
            {
                if (ItemLookupUI.ins.Root.activeInHierarchy)
                    ItemLookupUI.ins.HideUI();
                else if (ObtainLookupUI.ins.Root.activeInHierarchy)
                    ObtainLookupUI.ins.HideUI();
                else
                {
                    var select = Manager.ui.currentSelectedUIElement;
                    if (select is SlotUIBase slot)
                    {
                        UserInterfaceModule.OpenModUI("CoreEnhance:ObtainLookup");
                        ObtainLookupUI.ins.Focus.SetItem(slot.GetContainedObject().objectID);
                        ObtainLookupUI.ins.SearchLoot();
                    }
                    else
                    {
                        UserInterfaceModule.OpenModUI("CoreEnhance:ItemLookup");
                    }
                }
            }
            //if (r.GetButtonDown(QuickHealth))
            //    QuickConsumableClient.QuickConsume(QuickConsumeType.Health);
            //if (r.GetButtonDown(QuickPotion))
            //    QuickConsumableClient.QuickConsume(QuickConsumeType.Potion);
            //if (r.GetButtonDown(QuickCooked))
            //    QuickConsumableClient.QuickConsume(QuickConsumeType.Cooked);
        }
    }
}
