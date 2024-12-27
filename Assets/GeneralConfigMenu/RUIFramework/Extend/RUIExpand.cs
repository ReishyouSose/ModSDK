using UnityEngine;

namespace Assets.GeneralConfigMenu.RUIFramework.Extend
{
    public class RUIExpand : RUIButton
    {
        public RUIScrollView ExpandView;
        public GameObject InnerTemplate;

        [HideInInspector]
        public RUIElement LockElement;

        private GameObject[] changeWhenExpand;

        public override void Start()
        {
            base.Start();
            SetExpand(ToggleAtFirst);
            AddEvent(RMouseEventType.LeftDown, _ =>SetExpand(IsToggle));
            ExpandView.AddExceptEvent(RMouseEventType.LeftDown, _ => SetExpand(false));
        }
        private void Update()
        {
            if (IsToggle && LockElement != null)
            {
                LockElement.LockByOther = ExpandView.IsMouseHover;
            }
        }
        public void SetExpand(bool active)
        {
            if (active && AllowEvent?.Invoke() == false)
                return;
            SetState(active);
            LockElement.LockByOther = active;
            ExpandView.gameObject.SetActive(active);
            SetChangeWhenExpand(active);
        }
        private void SetChangeWhenExpand(bool active)
        {
            active = !active;
            if (changeWhenExpand == null)
                return;
            foreach (var go in changeWhenExpand)
            {
                go.SetActive(active);
            }
        }
        private void OnDisable()
        {
            SetExpand(false);
        }
        public void SetChangeWhenExpand(params GameObject[] gos)
        {
            changeWhenExpand = gos;
        }
    }
}
