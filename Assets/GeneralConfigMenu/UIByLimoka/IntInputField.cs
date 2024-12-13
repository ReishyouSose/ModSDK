using UnityEngine;
using UnityEngine.UI;

namespace Assets.GeneralConfigMenu.UIByLimoka
{
    public class IntInputField : BetterInputField
    {
        // Use this event as an int Unity Event
        public Dropdown.DropdownEvent onValueChanged;

        protected override string ValidateInput(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            
            if (int.TryParse(text, out int value))
            {
                return value.ToString();
            }

            return "";
        }

        protected override void OnCommit()
        {
            if (int.TryParse(pugText.textString, out int value))
            {
                onValueChanged?.Invoke(value);
                return;
            }
            
            if (float.TryParse(pugText.textString, out float floatValue))
            {
                onValueChanged?.Invoke(Mathf.RoundToInt(floatValue));
            }
        }
    }
}