using Assets.CoreEnhance.Scripts.Tiles;
using CoreLib.UserInterface;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.UI
{
    public class AutoFisherUI : InventoryUI, IModUI
    {
        public static AutoFisherUI Ins { get; private set; }
        public GameObject Root => gameObject;

        public bool showWithPlayerInventory => true;

        public bool shouldPlayerCraftingShow => false;
        public AutoFisherEM AutoFisherIns { get; private set; }
        public override int MAX_COLUMNS => 1;
        public override int MAX_ROWS => 3;
        protected override void Awake()
        {
            Ins = this;
            base.Awake();
        }
        public void SetAutoFisher(AutoFisherEM autoFisher) => AutoFisherIns = autoFisher;

        public void HideUI()
        {
            Root.SetActive(false);
            SetAutoFisher(null);
        }

        public void ShowUI()
        {
            if (AutoFisherIns == null)
            {
                Debug.Log("Not set auto fisher ins");
                return;
            }
            Root.SetActive(true);
        }
    }
}
