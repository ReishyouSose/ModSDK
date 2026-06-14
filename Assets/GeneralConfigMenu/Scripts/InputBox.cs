using System;
using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts
{
    public class InputBox : TextInputField
    {
        public string Desc;

        [HideInInspector]
        public Func<bool> AllowInput;
        public override void OnLeftClicked(bool mod1, bool mod2)
        {
            if (AllowInput?.Invoke() == false)
                return;
            base.OnLeftClicked(mod1, mod2);
            MoveCharMarker(9999);
        }
        public override void OnSelected()
        {
            base.OnSelected();
            ModConfigMenu.PlaySelectedSound();
        }
        public override TextAndFormatFields GetHoverTitle()
        {
            return new()
            {
                text = Desc,
            };
        }
    }
}
