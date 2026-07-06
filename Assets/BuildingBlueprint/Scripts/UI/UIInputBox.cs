using I2.Loc;
using System;
using System.Collections.Generic;

namespace Assets.BuildingBlueprint.Scripts.UI
{
    public class UIInputBox : TextInputField
    {
        public Func<bool> AllowInput;
        public bool ShowHoverTitle;
        public LocalizedString HoverTile;
        public bool ShowHoverDesc;
        public LocalizedString HoverDesc;
        public override void OnLeftClicked(bool mod1, bool mod2)
        {
            if (AllowInput?.Invoke() == false)
                return;
            base.OnLeftClicked(mod1, mod2);
        }
        public override TextAndFormatFields GetHoverTitle()
        {
            return ShowHoverTitle ? new()
            {
                text = HoverTile.mTerm
            } : null;
        }
        public override List<TextAndFormatFields> GetHoverDescription()
        {
            return ShowHoverDesc ? new()
            {
               new()
               {
                   text = HoverDesc.mTerm
               }
            } : null;
        }
    }
}
