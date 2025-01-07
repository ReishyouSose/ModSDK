using Assets.CoreEnhance.Scripts.UI;
using CoreLib.UserInterface;
using Unity.Entities;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Tiles
{
    public class AutoFisherEM : Chest
    {
        public EquipmentSlot FishRod { get; private set; }
        private const string UI = "CoreEnhance:AutoFisherUI";
        public override void Use()
        {
            base.Use();
            AutoFisherUI.Ins.SetAutoFisher(this);
            UserInterfaceModule.OpenModUI(UI);
        }
        public new void Close()
        {
            OnPlayerLeftChest();
            AutoFisherUI.Ins.HideUI();
        }
    }
}
