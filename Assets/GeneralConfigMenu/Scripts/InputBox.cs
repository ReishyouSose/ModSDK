using System;
using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts
{
    public class InputBox : TextInputField
    {
        [HideInInspector]
        public Func<bool> AllowInput;
        public override void OnLeftClicked(bool mod1, bool mod2)
        {
            if (AllowInput?.Invoke() == false)
                return;
            base.OnLeftClicked(mod1, mod2);
        }
    }
}
