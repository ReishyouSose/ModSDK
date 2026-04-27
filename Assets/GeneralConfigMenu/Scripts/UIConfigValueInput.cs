using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts
{
    [RequireComponent(typeof(InputBox))]
    public class UIConfigValueInput : UIConfigValueBox
    {
        private InputBox input;
        private void Awake()
        {
            input = GetComponent<InputBox>();
            input.AllowInput += AllowEdit;
            input.onInputFieldDone.AddListener(OnTextChange);
            input.pugText.Render(Entry.GetSerializedValue());
        }
        protected override void UpdateDisplayValue(string value)
        {
            input.pugText.Render(Entry.GetSerializedValue());
        }
        private bool AllowEdit() => Editable;
        private void OnTextChange()
        {
            SetValue(input.GetInputText());
            input.pugText.Render(Entry.GetSerializedValue(), false, true);
        }
    }
}