using Assets.CoreEnhance.Scripts.Configs;
using Assets.CoreEnhance.Scripts.Systems.Automation;
using Rewired;
using System;

namespace Assets.CoreEnhance.Scripts.Tiles
{
    public abstract class ContainerEM : Chest
    {
        public int Index { get; private set; }
        public bool AllowSwitchPage;
        public override void Use()
        {
            base.Use();
            if (AllowSwitchPage)
                SwitchPage(0);
        }
        public virtual void CloseTerminal()
        {
            OnPlayerLeftChest();
        }
        public override void ManagedLateUpdate()
        {
            if (!AllowSwitchPage)
                return;
            var p = Manager.main.player;
            if (p == null)
                return;
            Player r = p.inputModule.rewiredPlayer;
            var ui = Manager.ui;
            if (ui.isChestInventoryUIShowing)
            {
                if (r.GetButtonDown(ModKeyBind.InvPageUp))
                    SwitchPage(--Index);
                else if (r.GetButtonDown(ModKeyBind.InvPageDown))
                    SwitchPage(++Index);
            }
        }
        private void SwitchPage(int index)
        {
            var chest = Manager.ui.chestInventoryUI;
            int page = Index = Math.Max(index, 0);
            page *= chest.totalVisibleSlots;
            int slotIndex = 0;
            foreach (var slot in chest.itemSlots)
            {
                slot.visibleSlotIndex = page + slotIndex++;
            }
        }
    }
}
