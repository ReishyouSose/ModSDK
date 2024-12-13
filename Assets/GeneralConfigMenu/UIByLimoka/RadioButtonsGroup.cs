using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Assets.GeneralConfigMenu.UIByLimoka
{
    public class RadioButtonsGroup : MonoBehaviour
    {
        private List<RadioButtonUIElement> buttons = new List<RadioButtonUIElement>();
        
        // Use this event as an int Unity Event
        public Dropdown.DropdownEvent onValueChanged;

        public void RegisterButton(RadioButtonUIElement button)
        {
            buttons.Add(button);
        }
        
        public void OnButtonPressed(int value)
        {
            SetCurrentValue(value);
        }
        
        public void SetCurrentValue(int speed)
        {
            UpdateUI(speed);
            onValueChanged?.Invoke(speed);
        }

        public void UpdateUI(int value)
        {
            foreach (RadioButtonUIElement button in buttons)
            {
                button.SetIsPressed(button.buttonValue == value);
            }
        }
    }
}