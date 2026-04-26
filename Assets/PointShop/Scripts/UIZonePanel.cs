using UnityEngine;

namespace Assets.PointShop.Scripts
{
    public class UIZonePanel : UIelement, IScrollable
    {
        private LinearLayoutUIComponent layout;
        private Transform container;
        private void Awake()
        {
            layout = GetComponentInChildren<LinearLayoutUIComponent>();
            container = layout.transform;
        }
        public float GetCurrentWindowHeight() => layout.GetUIComponentRenderHeight();

        public bool IsBottomElementSelected()
        {
            int index = container.childCount - 1;
            if (index < 0)
                return false;
            return container.GetChild(index) == Manager.ui.currentSelectedUIElement;
        }

        public bool IsTopElementSelected()
        {
            int count = container.childCount;
            if (count <= 0)
                return false;
            return container.GetChild(0) == Manager.ui.currentSelectedUIElement;
        }

        public void UpdateContainingElements(float scroll)
        {
        }
    }
}
