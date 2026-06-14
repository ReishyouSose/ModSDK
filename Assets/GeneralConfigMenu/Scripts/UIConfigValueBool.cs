using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts
{
    [RequireComponent(typeof(ButtonUIElement))]
    public class UIConfigValueBool : UIConfigValueBox
    {
        public GameObject Active;
        public GameObject Inactive;

        private bool state;
        protected override string ConvertValue(string value) => value.ToLower();
        public override void Init()
        {
            name = "Bool" + (IsServerBox ? "(Server)" : "(Client)");
            GetComponent<ButtonUIElement>().optionalTitle.mTerm = "GeneralConfigMenu/" + (IsServerBox ? "ServerValue" : "ClientValue");
        }

        public void Click()
        {
            if (!editable)
            {
                UEntry.ShowUnEditableWarning();
                return;
            }
            SetState(!state);
            ApplyUserChange(state.ToString());
        }

        protected override void UpdateDisplayValue()
        {
            SetState(bool.Parse(ValidValue.ToString()));
        }

        private void SetState(bool newState)
        {
            state = newState;
            Active.SetActive(state);
            Inactive.SetActive(!state);
        }
    }
}