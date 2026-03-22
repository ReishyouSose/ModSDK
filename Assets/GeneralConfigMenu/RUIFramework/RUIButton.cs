using Assets.GeneralConfigMenu.RUIFramework.Extend;
using System;
using System.Linq;
using UnityEngine;

namespace Assets.GeneralConfigMenu.RUIFramework
{
    [RequireComponent(typeof(BoxCollider))]
    public class RUIButton : RUIElement
    {
        public SpriteRenderer OnSprite;
        public SpriteRenderer OffSprite;
        public SpriteRenderer HoverSprite;

        public bool ToggleAtFirst;
        public bool AutoChangeVisual;
        public bool IsToggle { get; private set; }

        [HideInInspector]
        public RUIButtonGroup ButtonGroup;

        [HideInInspector]
        public Action<RUIButton> OnValueChange;

        public virtual void Start()
        {
            IsToggle = ToggleAtFirst;
            OnSprite.gameObject.SetActive(IsToggle);
            OffSprite.gameObject.SetActive(!IsToggle);
            SetHoverSR(false);
        }
        protected virtual void Awake()
        {
            AddEvent(RMouseEventType.LeftDown, OnLeftDown);
            AddEvent(RMouseEventType.MouseEnter, _ => SetHoverSR(true));
            AddEvent(RMouseEventType.MouseLeave, _ => SetHoverSR(false));
        }
        public void SetState(bool state, bool doEvt = true, bool ignoreCheck = false)
        {
            if (!ignoreCheck && AllowEvent?.Invoke() == false)
                return;
            IsToggle = state;
            OnSprite.gameObject.SetActive(IsToggle);
            OffSprite.gameObject.SetActive(!IsToggle);
            if (!doEvt)
                return;
            OnValueChange?.Invoke(this);
        }
        private void OnLeftDown(GameObject go)
        {
            if (ButtonGroup != null)
            {
                var buttons = ButtonGroup.Buttons;
                int max = ButtonGroup.MaxSelected;
                if (max == 1)
                {
                    SetState(true);
                    foreach (var button in buttons)
                    {
                        if (button != this)
                        {
                            button.SetState(false, false);
                        }
                    }

                }
                else
                {
                    int selected = buttons.Count(x => x.IsToggle);
                    if (selected < max)
                    {
                        SetState(true);
                    }
                }
            }
            else
            {
                SetState(!IsToggle);
            }
            if (AutoChangeVisual)
            {
                OnSprite.gameObject.SetActive(IsToggle);
                OffSprite.gameObject.SetActive(!IsToggle);
            }
        }
        public void SetHoverSR(bool active)
        {
            if (HoverSprite != null)
                HoverSprite.gameObject.SetActive(active);
        }
    }
}
