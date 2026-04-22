using System.Collections.Generic;
using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts.Vanilla
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
        }
    }
}
