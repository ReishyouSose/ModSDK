using UnityEngine;

namespace Assets.GeneralConfigMenu.UIByLimoka
{
    public class RadioButtonUIElement : ButtonUIElement
    {
        public RadioButtonsGroup group;
        public int buttonValue;
        
        private bool showPressed;

        protected override void Awake()
        {
            base.Awake();
            if (group != null)
                group.RegisterButton(this);
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            if (!showPressed) return;

            foreach (SpriteRenderer spriteRenderer in spritesShownUnpressed)
            {
                spriteRenderer.gameObject.SetActive(false);
            }
            foreach (SpriteRenderer spriteRenderer in spritesShownPressed)
            {
                spriteRenderer.gameObject.SetActive(true);
            }
        }

        public void SetIsPressed(bool isPressed)
        {
            showPressed = isPressed;
        }

        public override void OnLeftClicked(bool mod1, bool mod2)
        {
            if (!canBeClicked)
                return;
            
            onLeftClick?.Invoke();
            if (group != null)
                group.OnButtonPressed(buttonValue);
            
            if (!playClickSoundEffect)
                return;
            AudioManager.SfxUI(Manager.audio.InspectorFriendlySfxIDToSfxID(clickSoundEffect), clickSoundPitch, false, pitchDev: 0.0f);
        }
    }
}