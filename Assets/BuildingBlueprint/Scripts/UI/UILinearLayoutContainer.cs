using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts.UI
{
    [RequireComponent(typeof(UIScrollWindow))]
    public class UILinearLayoutContainer : UIelement, IScrollable
    {
        [HideInInspector]
        public LinearLayoutUIComponent Layout;
        protected Transform Container;
        protected UIScrollWindow Scroll;
        public virtual void Awake()
        {
            Layout = GetComponentInChildren<LinearLayoutUIComponent>();
            Scroll = GetComponent<UIScrollWindow>();
            Container = Layout.transform;
        }
        public float GetCurrentWindowHeight() => Layout.GetUIComponentRenderHeight();

        public bool IsBottomElementSelected()
        {
            int index = Container.childCount - 1;
            if (index < 0)
                return false;
            return Container.GetChild(index) == Manager.ui.currentSelectedUIElement;
        }

        public bool IsTopElementSelected()
        {
            int count = Container.childCount;
            if (count <= 0)
                return false;
            return Container.GetChild(0) == Manager.ui.currentSelectedUIElement;
        }
        public void UpdateContainingElements(float _)
        {
            if (!Layout)
                return;
            Vector3 center = Scroll.transform.position + (Vector3)Scroll.windowLocalCenter;
            Rect rect = new(center.x - Scroll.windowWidth / 2f, center.y - Scroll.windowHeight / 2f, Scroll.windowWidth, Scroll.windowHeight);
            foreach (Transform trans in Layout.transform)
            {
                if (trans.TryGetComponent<UIComponentMonoBehaviour>(out var ui))
                {
                    var boxs = trans.GetComponentsInChildren<BoxCollider>();
                    if (boxs.Length <= 0)
                        continue;

                    float width = ui.GetUIComponentRenderWidth();
                    float height = ui.GetUIComponentRenderHeight();

                    // 根据 Pivot 计算左下角
                    float left = trans.position.x;
                    float bottom = trans.position.y;

                    if (ui.GetUIComponentPivotPosition() == UIComponentMonoBehaviour.PivotPosition.TopLeft)
                        bottom -= height;
                    else // MiddleLeft
                        bottom -= height / 2f;

                    foreach (var box in boxs)
                    {
                        box.enabled = new Rect(left, bottom, width, height).Overlaps(rect);
                    }
                }
            }
        }
    }
}
