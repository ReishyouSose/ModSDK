namespace Assets.PointShop.Scripts
{
    using PimDeWitte.UnityMainThreadDispatcher;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class GridLayoutUIComponent : UIComponentMonoBehaviour
    {
        [Header("Layout Settings")]
        public bool horizontal = true;
        public int gapHorizontal = 0;
        public int gapVertical = 0;

        [Header("Padding")]
        public float paddingLeft = 0f;
        public float paddingRight = 0f;
        public float paddingTop = 0f;
        public float paddingBottom = 0f;

        [Header("Other")]
        public SpriteRenderer background;
        public UIScrollWindow ScrollWindow;
        public bool renderFrameLate;

        private float totalWidth;
        private float totalHeight;
        public override void RenderUIComponent(bool force = false)
        {
            if (!(Dirty || force))
            {
                return;
            }

            if (renderFrameLate)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(delegate
                {
                    RenderUIComponentLate(force);
                });
            }
            else
            {
                RenderUIComponentLate(force);
            }
        }

        private void RenderUIComponentLate(bool force)
        {
            base.RenderUIComponent(force);
            ArrangeGridChildren();
            UpdateBackground();
        }

        protected override bool IsUIComponentRenderingDependentOnChildren()
        {
            return true;
        }

        private void ArrangeGridChildren()
        {
            List<UIComponentMonoBehaviour> activeChildren = GetActiveChildren();
            if (activeChildren.Count == 0)
            {
                totalWidth = 0;
                totalHeight = 0;
                return;
            }
            SpaceElement(activeChildren);
        }

        private List<UIComponentMonoBehaviour> GetActiveChildren()
        {
            List<UIComponentMonoBehaviour> allChildren = GetDirectUIComponentChildren();
            List<UIComponentMonoBehaviour> activeChildren = new();

            foreach (UIComponentMonoBehaviour child in allChildren)
            {
                if (PlatformStorefrontUtility.MatchesCurrent(child.activeInPlatforms, child.activeInStoreFronts)
                    && child.gameObject.activeInHierarchy && child.GetUIComponentPivotPosition() == PivotPosition.TopLeft)
                {
                    activeChildren.Add(child);
                }
            }

            return activeChildren;
        }

        private void SpaceElement(List<UIComponentMonoBehaviour> children)
        {
            float startX = 0.0625f * paddingLeft;
            float startY = 0.0625f * paddingTop;
            float gapX = 0.0625f * gapHorizontal;
            float gapY = 0.0625f * gapVertical;
            float currentX = startX;
            float currentY = startY;
            float maxWidth = ScrollWindow.windowWidth;
            float maxHeight = ScrollWindow.windowHeight;
            float maxLength = 0f;
            int itemsInCurrent = 0;
            totalWidth = 0f;
            totalHeight = 0f;

            for (int i = 0; i < children.Count; i++)
            {
                UIComponentMonoBehaviour child = children[i];
                float width = child.GetUIComponentRenderWidth();
                float height = child.GetUIComponentRenderHeight();
                bool wrap = false;
                if (horizontal && currentX + width > maxWidth)
                {
                    currentY += maxLength + gapY;
                    currentX = startX;
                    wrap = true;
                }
                else if (!horizontal && currentY + height > maxHeight)
                {
                    currentX += maxLength + gapX;
                    currentY = startY;
                    wrap = true;
                }
                if (wrap)
                {
                    itemsInCurrent = 0;
                    maxLength = 0;
                }

                Vector3 pos = child.transform.localPosition;
                pos.x = currentX;
                pos.y = -currentY;
                child.transform.localPosition = pos;
                totalWidth = Math.Max(totalWidth, currentX + width);
                totalHeight = Math.Max(totalHeight, currentY + height);
                if (horizontal)
                    currentX += width + gapX;
                else
                    currentY += height + gapY;
                maxLength = Math.Max(maxLength, horizontal ? height : width);
                itemsInCurrent++;
            }

            totalWidth += 0.0625f * paddingRight;
            totalHeight += 0.0625f * paddingBottom;
        }

        private void UpdateBackground()
        {
            if (background == null)
                return;

            Vector2 size = new(totalWidth, totalHeight);
            background.transform.localPosition = new Vector3((0f - size.x) / 2f, (0f - size.y) / 2f, 0f);
            background.size = size;
        }

        public override float GetUIComponentRenderWidth()
        {
            return totalWidth;
        }

        public override float GetUIComponentRenderHeight()
        {
            return totalHeight;
        }
    }
}
