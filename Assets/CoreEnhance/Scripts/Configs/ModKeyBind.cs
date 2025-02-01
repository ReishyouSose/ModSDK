using Assets.CoreEnhance.Scripts.Systems.Misc;
using CoreLib.RewiredExtension;
using Rewired;

namespace Assets.CoreEnhance.Scripts.Configs
{
    public static class ModKeyBind
    {
        private const string CoreEnhance = "CoreEnhance:";
        internal const string QuickStack = CoreEnhance + nameof(QuickStack);
        internal const string QuickHealth = CoreEnhance + nameof(QuickHealth);
        internal const string QuickPotion = CoreEnhance + nameof(QuickPotion);
        internal const string QuickCooked = CoreEnhance + nameof(QuickCooked);
        public static void Load()
        {
            RewiredExtensionModule.AddKeybind(QuickStack, "Quick Stack", KeyboardKeyCode.I, ModifierKey.Control);
            RewiredExtensionModule.AddKeybind(QuickHealth, "Quick Health", KeyboardKeyCode.Q, ModifierKey.Control);
            RewiredExtensionModule.AddKeybind(QuickPotion, "Quick Potion", KeyboardKeyCode.W, ModifierKey.Control);
            RewiredExtensionModule.AddKeybind(QuickCooked, "Quick Cooked", KeyboardKeyCode.E, ModifierKey.Control);
        }
        public static void Handle(PlayerController p)
        {
            Player r = p.inputModule.rewiredPlayer;
            if (r.GetButtonDown(QuickStack))
                QuickStackClient.SendRequest();
            if (r.GetButtonDown(QuickHealth))
                QuickConsumableClient.QuickConsume(QuickConsumeType.Health);
            if (r.GetButtonDown(QuickPotion))
                QuickConsumableClient.QuickConsume(QuickConsumeType.Potion);
            if (r.GetButtonDown(QuickCooked))
                QuickConsumableClient.QuickConsume(QuickConsumeType.Cooked);
        }
    }
}
