using Assets.GeneralConfigMenu.RUIFramework.Extend;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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
        public Toggle.ToggleEvent OnValueChanged;
        public bool IsToggle { get; private set; }

        [HideInInspector]
        public RUIButtonGroup ButtonGroup;
        private void Start()
        {
            IsToggle = ToggleAtFirst;
            OnSprite.gameObject.SetActive(IsToggle);
            OffSprite.gameObject.SetActive(!IsToggle);
            SetHoverSR(false);
        }
        private void Awake()
        {
            AddEvent(RMouseEventType.LeftDown, OnLeftDown);
            AddEvent(RMouseEventType.MouseEnter, _ => SetHoverSR(true));
            AddEvent(RMouseEventType.MouseLeave, _ => SetHoverSR(false));
        }
        public void SetState(bool state)
        {
            IsToggle = state;
            OnValueChanged?.Invoke(IsToggle);
            OnSprite.gameObject.SetActive(IsToggle);
            OffSprite.gameObject.SetActive(!IsToggle);
        }
        private void OnLeftDown(GameObject go)
        {
            if (ButtonGroup != null)
            {
                var buttons = ButtonGroup.Buttons;
                int max = ButtonGroup.MaxSelected;
                if (max == 1)
                {
                    OnValueChanged?.Invoke(IsToggle = !IsToggle);
                    foreach (var button in buttons)
                    {
                        if (button != this)
                        {
                            button.SetState(false);
                        }
                    }

                }
                else
                {
                    int selected = buttons.Count(x => x.IsToggle);
                    if (selected < max)
                    {
                        OnValueChanged?.Invoke(IsToggle = !IsToggle);
                    }
                }
            }
            else
            {
                IsToggle = !IsToggle;
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
