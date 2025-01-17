using Assets.CoreEnhance.Scripts.Systems.Misc;
using System;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Tiles
{
    public class AutoFisherTerminalEM : Chest
    {
        private int index;
        private bool interacting;
        public override void Use()
        {
            base.Use();
            interacting = true;
            SwitchPage(0);
            AutoFisherTerminalClient.OpenAFTerminal();
        }
        public void CloseTerminal()
        {
            OnPlayerLeftChest();
            interacting = false;
        }
        public override void ManagedLateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.PageUp))
            {
                SwitchPage(--index);
            }
            if (Input.GetKeyDown(KeyCode.PageDown))
            {
                SwitchPage(++index);
            }
        }
        private void SwitchPage(int index)
        {
            if (!interacting)
                return;
            int page = this.index = Math.Max(index, 0);
            page *= 36;
            int slotIndex = 0;
            foreach (var slot in Manager.ui.chestInventoryUI.itemSlots)
            {
                slot.visibleSlotIndex = page + slotIndex++;
            }
        }
    }
}
