using UnityEngine;

namespace Assets.GeneralConfigMenu.RUIFramework
{
    public class RUIText : RUIElement
    {
        public PugText Text;
        public Color Origin = Color.gray;
        public Color Hover = Color.white;
        private void OnValidate()
        {
            if (Text != null)
            {
                Text.color = Origin;
            }
        }
        public void NeedHoverColor()
        {
            AddEvent(RMouseEventType.MouseEnter, _ => Text.SetTempColor(Hover));
            AddEvent(RMouseEventType.MouseLeave, _ => Text.SetTempColor(Origin));
        }
    }
}
