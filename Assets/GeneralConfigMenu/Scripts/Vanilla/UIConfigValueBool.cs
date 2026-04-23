using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts.Vanilla
{
    [RequireComponent(typeof(ButtonUIElement))]
    public class UIConfigValueBool : UIConfigValueBox
    {
        public GameObject Active;
        public GameObject Inactive;

        [HideInInspector]
        public UIConfigEntry UEntry;

        private ButtonUIElement button;
        private bool state;
        protected override void Awake()
        {
            base.Awake();
            button = GetComponent<ButtonUIElement>();
            //TODO:获取entry的初始值
            Active.SetActive(true);
            Inactive.SetActive(false);
        }
        private void Update()
        {
            button.canBeClicked = Editable;
        }
        public void Click()
        {
            state = !state;
            Active.SetActive(state);
            Inactive.SetActive(!state);
            Manager.menu.AttemptToPlayMenuSfx(SfxID.FIXME_menu_select, 0.6f, 0f, reuse: false);
            //UEntry.SetValue(state.ToString());
        }
    }
}
