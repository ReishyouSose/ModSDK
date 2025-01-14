using CoreLib.UserInterface;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.UI
{
    public class TerminalChestUI : MonoBehaviour, IModUI
    {
        public GameObject Root => gameObject;

        public bool showWithPlayerInventory => true;

        public bool shouldPlayerCraftingShow => false;
        internal static TerminalChestUI Ins { get; private set; }
        private void Awake()
        {
            Ins = this;
            HideUI();
        }

        public void HideUI()
        {
            gameObject.SetActive(false);
        }

        public void ShowUI()
        {
            gameObject.SetActive(true);
        }
        private void Update()
        {
            var p = Manager.main.player;
            if (p == null)
            {
                HideUI();
                return;
            }
            var entity = p.activeInventoryHandler.inventoryEntity;
        }
    }
}
