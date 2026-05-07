using System.Collections.Generic;
using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts
{
    public class UISection : ButtonUIElement
    {
        public PugText Name;

        [HideInInspector]
        public List<UIConfigEntry> Entries = new();
        public GameObject Active;
        public GameObject Inactive;

        private bool state;
        private LinearLayoutUIComponent layout;
        protected override void Awake()
        {
            base.Awake();
            state = true;
            Active.SetActive(true);
            Inactive.SetActive(false);
            layout = GetComponentInParent<LinearLayoutUIComponent>();
        }
        public void SwitchExpandState()
        {
            state = !state;
            Active.SetActive(state);
            Inactive.SetActive(!state);
            foreach (var entry in Entries)
            {
                entry.gameObject.SetActive(state);
            }
            layout.RenderUIComponent(true);
            if (state)
                Manager.menu.AttemptToPlayMenuSfx(SfxID.FIXME_menu_select, 0.6f, 0f, reuse: false);
            else
                AudioManager.SfxUI(SfxID.FIXME_menu_select, 0.4f, false, 1f, 0f, true, true, 0f);
        }
    }
}
