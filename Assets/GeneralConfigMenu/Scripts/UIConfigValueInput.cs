using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts
{
    [RequireComponent(typeof(InputBox))]
    public class UIConfigValueInput : UIConfigValueBox
    {
        private InputBox inputBox;

        public override void Init()
        {
            name = "Input" + (IsServerBox ? "(Server)" : "(Client)");
            inputBox = GetComponent<InputBox>();
            inputBox.AllowInput += AllowEdit;
            inputBox.onInputFieldDone.AddListener(OnTextChange);
            inputBox.Desc = "GeneralConfigMenu/" + (IsServerBox ? "ServerValue" : "ClientValue");
        }

        protected override void UpdateDisplayValue()
        {
            inputBox.pugText.Render(ValidValue.ToString(), false, true);
        }

        private bool AllowEdit()
        {
            if (editable)
            {
                return true;
            }
            UEntry.ShowUnEditableWarning();
            return false;
        }

        private void OnTextChange()
        {
            ApplyUserChange(inputBox.GetInputText());
        }
    }
}