using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts
{
    [RequireComponent(typeof(ButtonUIElement))]
    public class UIConfigValueBool : UIConfigValueBox
    {
        public GameObject Active;
        public GameObject Inactive;
        private bool state;
        private ButtonUIElement button;

        private void Awake()
        {
            button = GetComponent<ButtonUIElement>();
            state = bool.Parse(Entry.GetSerializedValue());
            SetState(state, true);
        }
        private void Update()
        {
            button.canBeClicked = Editable;
        }
        public void Click()
        {
            SetState(state = !state, false);
        }
        protected override void UpdateDisplayValue(string value)
        {
            SetState(bool.Parse(value), true);
        }
        private void SetState(bool state, bool visualOnly)
        {
            Active.SetActive(state);
            Inactive.SetActive(!state);
            if (!visualOnly)
                SetValue(state.ToString());
        }
    }
}
