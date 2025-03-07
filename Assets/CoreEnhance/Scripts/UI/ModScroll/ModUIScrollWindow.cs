using Pug.Sprite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.CoreEnhance.Scripts.UI.ModScroll
{
    public class ModUIScrollWindow : MonoBehaviour
    {
        [FormerlySerializedAs("scroll")]
        public Transform scrollingContent;

        public MonoBehaviour scrollable;

        public float windowHeight = 9.25f;

        public float windowWidth = 9.25f;

        public Vector2 windowLocalCenter;

        public float minScrollPos;

        public ModScrollBar scrollBar;

        public SpriteRenderer arrowUp;

        public SpriteRenderer arrowUpInactive;

        public SpriteRenderer arrowDown;

        public SpriteRenderer arrowDownInactive;

        public bool dontForcePixelPerfect;

        [Header("Navigation")]
        public float scrollMultiplierSpeedMouse = 0.5f;

        public float scrollMultiplierSpeedJoystick = 0.3f;

        public bool scrollWithLeftStick;

        public bool allowScrollWithArrowKeys;

        public bool cursorMustBeInsideWindowToScroll;

        public bool anyElementMustBeSelectedToScrollWithController;

        [Header("If content is smaller than window:")]
        public bool autoHideScrollbar;

        public bool centerVertically;

        private IScrollable _scrollable;

        private float _previousPosition = -1f;

        private float _previousHeight = -1f;

        private bool _isCreditsMenu;

        private List<SpriteObject> _spriteObjects = new List<SpriteObject>();

        private List<Vector3> _spriteObjectStartPositions = new List<Vector3>();

        private Vector3 _spriteObjectOffset = Vector3.zero;

        public float VisibleRatio => windowHeight / (windowHeight + ScrollHeight);

        public float ScrollHeight { get; private set; }

        private void Awake()
        {
            if (scrollable is IScrollable)
            {
                _scrollable = (IScrollable)scrollable;
            }
            else
            {
                Debug.LogError(scrollable?.ToString() + " does not implement IScrollable, disabling UIScrollWindow");
                enabled = false;
            }

            if (scrollable != null && scrollable is RadicalCreditsMenu)
            {
                _isCreditsMenu = true;
                SpriteObject[] componentsInChildren = transform.GetComponentsInChildren<SpriteObject>(includeInactive: true);
                foreach (SpriteObject spriteObject in componentsInChildren)
                {
                    _spriteObjects.Add(spriteObject);
                    _spriteObjectStartPositions.Add(spriteObject.transform.localPosition);
                }
            }
        }

        private void LateUpdate()
        {
            UpdateScrollHeight();
            UpdateScroll();
            if (Math.Abs(_previousPosition - scrollingContent.localPosition.y) > 0.001f || Math.Abs(_previousHeight - ScrollHeight) > 0.001f)
            {
                UpdateArrows();
                UpdateScrollbar();
                _previousPosition = scrollingContent.localPosition.y;
                _previousHeight = ScrollHeight;
            }
        }

        public void ResetScroll()
        {
            SetScrollValue(1f);
        }

        private bool IsMouseWithinScrollArea()
        {
            Vector3 position = Manager.ui.mouse.pointer.transform.position;
            Vector2 size = new Vector2(windowWidth, windowHeight);
            Vector2 vector = new Vector2(windowWidth, windowHeight) / 2f;
            Vector3 position2 = transform.position;
            Vector2 vector2 = new Vector2(position2.x, position2.y);
            return new Rect(vector2 + windowLocalCenter - vector, size).Contains(position);
        }

        private void UpdateScroll()
        {
            if (VisibleRatio >= 1f)
            {
                float num = minScrollPos;
                if (centerVertically)
                {
                    float num2 = minScrollPos;
                    float num3 = minScrollPos + windowHeight - _scrollable.GetCurrentWindowHeight();
                    num = math.lerp(num2, num3, 0.5f);
                }

                SetScrollablePosition(0f - num);
                return;
            }

            bool flag = !Manager.input.SystemPrefersKeyboardAndMouse();
            UIelement uIelement = Manager.ui.currentSelectedUIElement;
            bool flag3;
            if (flag)
            {
                bool flag2 = uIelement != null && uIelement.uiScrollWindow == this;
                flag3 = !anyElementMustBeSelectedToScrollWithController || flag2;
            }
            else
            {
                flag3 = !cursorMustBeInsideWindowToScroll || IsMouseWithinScrollArea() && scrollable is UIelement uIelement2 && uIelement2.isShowing;
            }

            float num4 = 0f;
            float num5 = 0f;
            if (flag3)
            {
                num4 = 0f - Manager.input.GetScrollValue();
                num5 = scrollMultiplierSpeedMouse;
                if (flag)
                {
                    num5 = scrollMultiplierSpeedJoystick;
                }

                if (allowScrollWithArrowKeys)
                {
                    float num6 = Manager.input.IsMenuUpButtonPressed() ? -5f : Manager.input.IsMenuDownButtonPressed() ? 5f : 0f;
                    if (num6 != 0f)
                    {
                        num4 = Time.unscaledDeltaTime * num6 * 5f;
                    }
                }

                if (flag)
                {
                    num4 *= Time.unscaledDeltaTime * 50f;
                }
            }

            MoveScroll(num4 * num5);
            if (!flag)
            {
                return;
            }

            bool flag4 = false;
            float num7 = 0f - Manager.input.singleplayerInputModule.GetRawAxisInput().y;
            if (scrollWithLeftStick)
            {
                flag4 = true;
            }
            else if (_scrollable.IsTopElementSelected() && num7 < 0f)
            {
                flag4 = true;
            }
            else if (_scrollable.IsBottomElementSelected() && num7 > 0f)
            {
                flag4 = true;
            }

            if (flag4 && Math.Abs(num7) > 0.1f)
            {
                MoveScroll(num7 * num5 * Time.unscaledDeltaTime * 50f);
            }

            if (flag3 && uIelement != null && !IsShowingPosition(uIelement.localScrollPosition))
            {
                bool flag5 = uIelement.localScrollPosition + scrollingContent.transform.localPosition.y > 0f;
                while (uIelement != null && !IsShowingPosition(uIelement.localScrollPosition))
                {
                    uIelement = flag5 ? uIelement.GetAdjacentUIElement(Direction.Id.back, uIelement.transform.position) : uIelement.GetAdjacentUIElement(Direction.Id.forward, uIelement.transform.position);
                }

                if (uIelement != null)
                {
                    uIelement.Select();
                    Manager.ui.mouse.PlaceMousePositionOnSelectedUIElementWhenControlledByJoystick();
                }
            }
        }

        private void SetScrollablePosition(float verticalOffsetFromParent)
        {
            if (!dontForcePixelPerfect)
            {
                verticalOffsetFromParent -= verticalOffsetFromParent % 0.0625f;
            }

            Vector3 localPosition = scrollingContent.localPosition;
            localPosition.y = verticalOffsetFromParent;
            scrollingContent.localPosition = localPosition;
            _scrollable.UpdateContainingElements(verticalOffsetFromParent);
        }

        public void SetScrollValue(float normalizedScrollValue)
        {
            float scrollablePosition = math.lerp(ScrollHeight, minScrollPos, normalizedScrollValue);
            SetScrollablePosition(scrollablePosition);
        }

        public void MoveScroll(float scrollValue)
        {
            float num = scrollingContent.localPosition.y + scrollValue;
            num = math.clamp(num, minScrollPos, ScrollHeight);
            if (_isCreditsMenu)
            {
                _spriteObjectOffset.y = math.abs(scrollValue * 0.5f);
                for (int i = 0; i < _spriteObjects.Count; i++)
                {
                    _spriteObjects[i].transform.localPosition = _spriteObjectStartPositions[i] + _spriteObjectOffset;
                }
            }

            SetScrollablePosition(num);
            Manager.ui.mouse.PlaceMousePositionOnSelectedUIElementWhenControlledByJoystick();
        }

        private void UpdateScrollHeight()
        {
            ScrollHeight = math.max(0f, _scrollable.GetCurrentWindowHeight() - windowHeight);
        }

        private void UpdateArrows()
        {
            if (arrowUp != null)
            {
                bool flag = scrollingContent.localPosition.y - 0.0625f > 0f;
                arrowUp.gameObject.SetActive(flag);
                if (arrowUpInactive != null)
                {
                    arrowUpInactive.gameObject.SetActive(!flag);
                }
            }

            if (arrowDown != null)
            {
                bool flag2 = scrollingContent.localPosition.y + 0.0625f < ScrollHeight;
                arrowDown.gameObject.SetActive(flag2);
                if (arrowDownInactive != null)
                {
                    arrowDownInactive.gameObject.SetActive(!flag2);
                }
            }
        }

        private void UpdateScrollbar()
        {
            if (scrollBar == null)
            {
                return;
            }

            if (VisibleRatio >= 1f)
            {
                if (autoHideScrollbar)
                {
                    Debug.Log("Deactive Scroll");
                    scrollBar.gameObject.SetActive(value: false);
                }
            }
            else
            {
                scrollBar.gameObject.SetActive(value: true);
                float normalizedPosition = math.clamp(1f - (scrollingContent.localPosition.y - minScrollPos) / (ScrollHeight - minScrollPos), 0f, 1f);
                scrollBar.UpdateScrollBarPosition(normalizedPosition);
            }
        }

        public void MoveScrollToIncludePosition(float positionRelativeScrollRoot, float padding)
        {
            if (!Manager.input.SystemPrefersKeyboardAndMouse() || Manager.input.SystemIsUsingKeyboard() && (Manager.input.IsMenuDownButtonPressed() || Manager.input.IsMenuUpButtonPressed()))
            {
                float num = scrollingContent.localPosition.y + positionRelativeScrollRoot;
                if (num - padding < 0f - windowHeight)
                {
                    float scrollValue = 0f - windowHeight - num + padding;
                    MoveScroll(scrollValue);
                }
                else if (num + padding > 0f)
                {
                    float scrollValue2 = 0f - (num + padding);
                    MoveScroll(scrollValue2);
                }
            }
        }

        public bool IsShowingPosition(float posToInclude, float padding = 0f)
        {
            float num = scrollingContent.localPosition.y + posToInclude;
            if (num + padding >= 0f - windowHeight)
            {
                return num - padding <= 0f;
            }

            return false;
        }

        public bool IsAtBottom()
        {
            return scrollingContent.localPosition.y + 0.0625f >= ScrollHeight;
        }
    }
}
