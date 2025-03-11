using System;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.UI.ModScroll
{
    public class ModScrollBar : UIelement
    {
        private const float MIN_HANDLE_SIZE = 0.625f;

        public ModUIScrollWindow scrollWindow;

        public GameObject root;

        public SpriteRenderer background;

        public ScrollBarHandle handle;

        public bool handleIsPressed;

        private float prevScrollHeight;

        public override bool isShowing => root.activeInHierarchy;

        protected override void OnDisable()
        {
            handleIsPressed = false;
            base.OnDisable();
        }

        private void Update()
        {
            float scrollHeight = scrollWindow.ScrollHeight;
            bool flag = scrollHeight > 0f;
            //root.SetActive(flag);
            if (!flag)
            {
                return;
            }

            if (!Manager.input.LeftClickPressed())
            {
                handleIsPressed = false;
            }

            bool flag2 = Math.Abs(scrollHeight - prevScrollHeight) > 0.001f;
            if (handleIsPressed || flag2)
            {
                prevScrollHeight = scrollHeight;
                UpdateHandleSize();
                float num = 1f - scrollWindow.scrollingContent.localPosition.y / scrollHeight;
                if (handleIsPressed)
                {
                    float y = handle.handleSpriteRenderer.size.y;
                    float num2 = background.size.y - y;
                    Vector2 mouseUIViewPosition = Manager.ui.mouse.GetMouseUIViewPosition();
                    num = Mathf.Clamp01((background.transform.InverseTransformPoint(mouseUIViewPosition).y + num2 / 2f) / num2);
                    scrollWindow.SetScrollValue(num);
                }

                UpdateScrollBarPosition(num);
            }
        }

        private void UpdateHandleSize()
        {
            float y = background.size.y;
            float y2 = math.max(scrollWindow.VisibleRatio * y, 0.625f);
            Vector2 size = new(handle.handleSpriteRenderer.size.x, y2);
            foreach (SpriteRenderer item in handle.handleSpritesToResize)
            {
                item.size = size;
            }

            Vector3 size2 = handle.handleCollider.size;
            size2.y = y2;
            handle.handleCollider.size = size2;
        }

        public void UpdateScrollBarPosition(float normalizedPosition)
        {
            float y = handle.handleSpriteRenderer.size.y;
            float num = background.size.y - y;
            float num2 = num / 2f;
            Transform obj = handle.transform;
            Vector3 localPosition = obj.localPosition;
            obj.localPosition = new Vector3(localPosition.x, normalizedPosition * num - num2, localPosition.z);
        }

        public void OnHandleLeftClick()
        {
            handleIsPressed = true;
        }
    }
}
