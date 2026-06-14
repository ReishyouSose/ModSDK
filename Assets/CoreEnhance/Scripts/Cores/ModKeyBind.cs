using Assets.CoreEnhance.Scripts.Systems.Misc;
using Assets.CoreEnhance.Scripts.Systems.Quick;
using CoreLib.Submodule.ControlMapping;
using Rewired;

namespace Assets.CoreEnhance.Scripts.Cores
{
    public static class ModKeyBind
    {
        private const string CoreEnhance = "CoreEnhance_";
        internal const string QuickOpenLockedChest = CoreEnhance + nameof(QuickOpenLockedChest);
        internal const string ContainerHighLight = CoreEnhance + nameof(ContainerHighLight);
        //internal const string CraftStationRange = CoreEnhance + nameof(CraftStationRange);
        internal const string ClearDropItem = CoreEnhance + nameof(ClearDropItem);
        internal const string MoveChest = CoreEnhance + nameof(MoveChest);
        public static void Load()
        {
            int cateogry = ControlMappingModule.AddNewCategory("CoreEnhance");
            ControlMappingModule.AddKeyboardBind(QuickOpenLockedChest, KeyboardKeyCode.O, ModifierKey.Control, categoryId: cateogry);
            ControlMappingModule.AddKeyboardBind(ContainerHighLight, KeyboardKeyCode.LeftAlt, categoryId: cateogry);
            //ControlMappingModule.AddKeyboardBind(CraftStationRange, KeyboardKeyCode.LeftAlt, categoryId: cateogry);
            ControlMappingModule.AddKeyboardBind(ClearDropItem, KeyboardKeyCode.D, ModifierKey.Control, ModifierKey.Shift, ModifierKey.Alt, categoryId: cateogry);
            ControlMappingModule.AddKeyboardBind(MoveChest, KeyboardKeyCode.LeftAlt, categoryId: cateogry);
        }
        public static void Handle(PlayerController p)
        {
            Player r = p.inputModule.rewiredPlayer;
            if (r.GetButtonDown(QuickOpenLockedChest))
                QuickOpenLockedChestClient.Trigger(p);
            if (r.GetButtonDown(ClearDropItem))
                ClearDropItemRPCClient.Clear(p);
        }
    }
}
