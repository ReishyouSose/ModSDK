using Assets.CoreEnhance.Scripts.UI;
using CoreLib.UserInterface;

namespace Assets.CoreEnhance.Scripts.Tiles
{
    public class AutoFisherEM : Chest
    {
        public EquipmentSlot FishRod { get; private set; }
        private const string UI = "CoreEnhance:AutoFisherUI";
        public override void Use()
        {
            base.Use();
            return;
            AutoFisherUI.Ins.SetAutoFisher(this);
            UserInterfaceModule.OpenModUI(UI);
        }
        public override void OnFree()
        {
            base.OnFree();
            return;
            AutoFisherUI.Ins.HideUI();
        }
    }
}
