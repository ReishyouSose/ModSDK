using UnityEngine;
using UnityEngine.UI;

namespace Assets.GeneralConfigMenu.UIByLimoka
{
    public class ToggleButton : ButtonUIElement
    {
        public SpriteRenderer onSprite;
        public SpriteRenderer offSprite;
        public SpriteRenderer hoverSprite;
        public bool initialValue;
        public bool AutoChangeVisual;
        private bool state;

        public Toggle.ToggleEvent onValueChanged;

        protected override void Awake()
        {
            base.Awake();
            state = initialValue;
        }

        public void SetState(bool state)
        {
            this.state = state;
            if (!AutoChangeVisual)
                return;
            onSprite.gameObject.SetActive(state);
            offSprite.gameObject.SetActive(!state);
        }
        public override void OnLeftClicked(bool mod1, bool mod2)
        {
            if (!canBeClicked)
                return;
            onLeftClick?.Invoke();

            SetState(!state);
            onValueChanged?.Invoke(state);

            if (!playClickSoundEffect)
                return;
            AudioManager.SfxUI(Manager.audio.InspectorFriendlySfxIDToSfxID(clickSoundEffect), clickSoundPitch, false, pitchDev: 0.0f);
        }

    }
}