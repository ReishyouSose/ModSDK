using HarmonyLib;
using I2.Loc;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.GeneralConfigMenu.RUIFramework
{
    [HarmonyPatch]
    public class RUIManager : MonoBehaviour
    {
        [Tooltip("Should with 75%+ opacity BG")]
        public Transform HoverTextBG;

        [Tooltip("Just create an empty")]
        public Transform HoverTopLeft;

        [Tooltip("Align Top Left")]
        public PugText HoverTextTemplate;

        public RUIElement hoverElement { get; private set; }

        private bool mouseLeftDown;
        private bool mouseRightDown;
        private readonly RKeyCD mouseLeftCD = new(() => Input.GetMouseButton(0));
        private readonly RKeyCD mouseRightCD = new(() => Input.GetMouseButton(1));
        private List<RUIElement> interactedBuffer = new();
        private readonly List<RUIElement> leftUpBuffer = new();
        private readonly List<RUIElement> rightUpBuffer = new();
        private readonly List<RUIElement> expectLeftUpBuffer = new();
        private readonly List<RUIElement> exceptRightUpBuffer = new();
        private readonly List<PugText> TempTexts = new();
        private readonly List<TextAndFormatFields> hoverTips = new();
        private readonly List<RUIManager> managers = new();
        private void Awake()
        {
            HoverTextBG.gameObject.SetActive(false);
            HoverTextTemplate.gameObject.SetActive(false);
            managers.Add(this);
        }
        private void Update()
        {
            hoverTips.Clear();
            Dictionary<int, List<RUIElement>> hierarchy = new();
            GetHierarchy(hierarchy, transform);
            List<RUIElement> all = new(), interact = new();
            var mouse = Manager.ui.mouse.pointer.position;
            foreach (var (layer, uies) in hierarchy)
            {
                foreach (var uie in uies)
                {
                    all.Add(uie);
                    if (!uie.CanBeInteract || uie.LockByOther)
                        continue;
                    if (uie.TryGetComponent<BoxCollider>(out var collider) && collider.bounds.Contains(mouse))
                    {
                        interact.Add(uie);
                    }
                }
            }
            interact = interact.Where(x => x.Sensitive || interact.Last() == x).ToList();
            var except = all.Except(interact).ToList();

            foreach (var uie in interact)
            {
                if (interactedBuffer.Contains(uie))
                    continue;
                uie.IsMouseHover = true;
                uie.TryDoEvent(RMouseEventType.MouseEnter);
            }

            foreach (var uie in interactedBuffer)
            {
                if (interact.Contains(uie))
                    continue;
                uie.IsMouseHover = false;
                uie.TryDoEvent(RMouseEventType.MouseLeave);
            }

            interactedBuffer = interact;
            foreach (var uie in interactedBuffer)
            {
                if (uie.IsMouseHover)
                {
                    uie.TryDoEvent(RMouseEventType.MouseHover);
                    var tips = uie.GetHoverDesc();
                    if (tips != null)
                        hoverTips.InsertRange(0, uie.GetHoverDesc());
                }
            }
            hoverElement = interactedBuffer.LastOrDefault();

            UpdateHoverText(mouse);

            bool nowMouseLeft = Input.GetMouseButton(0);
            if (mouseLeftDown != nowMouseLeft)
            {
                if (nowMouseLeft)
                {
                    interact.ForEach(x => x.TryDoEvent(RMouseEventType.LeftDown));
                    except.ForEach(x => x.TryDoExceptEvent(RMouseEventType.LeftDown));
                    leftUpBuffer.AddRange(interact);
                    expectLeftUpBuffer.AddRange(except);
                }
                else
                {
                    if (mouseLeftCD.IsCoolDown())
                    {
                        leftUpBuffer.ForEach(x => x.TryDoEvent(RMouseEventType.LeftClick));
                        expectLeftUpBuffer.ForEach(x => x.TryDoExceptEvent(RMouseEventType.LeftClick));
                        mouseLeftCD.ResetCoolDown();
                    }
                    else
                    {
                        leftUpBuffer.ForEach(x => x.TryDoEvent(RMouseEventType.LeftDoubleClick));
                        leftUpBuffer.ForEach(x => x.TryDoExceptEvent(RMouseEventType.LeftDoubleClick));
                        mouseLeftCD.CoolDown();
                    }
                    leftUpBuffer.ForEach(x => x.TryDoEvent(RMouseEventType.LeftUp));
                    expectLeftUpBuffer.ForEach(x => x.TryDoExceptEvent(RMouseEventType.LeftUp));
                    leftUpBuffer.Clear();
                    expectLeftUpBuffer.Clear();
                }

                mouseLeftDown = nowMouseLeft;
            }
            leftUpBuffer.ForEach(x => x.TryDoEvent(RMouseEventType.LeftHolding));
            expectLeftUpBuffer.ForEach(x => x.TryDoExceptEvent(RMouseEventType.LeftHolding));

            bool nowMouseRight = Input.GetMouseButton(1);
            if (mouseRightDown != nowMouseRight)
            {
                if (nowMouseRight)
                {
                    interact.ForEach(x => x.TryDoEvent(RMouseEventType.RightDown));
                    except.ForEach(x => x.TryDoExceptEvent(RMouseEventType.RightDown));
                    rightUpBuffer.AddRange(interact);
                    exceptRightUpBuffer.AddRange(except);
                }
                else
                {
                    if (mouseRightCD.IsCoolDown())
                    {
                        rightUpBuffer.ForEach(x => x.TryDoEvent(RMouseEventType.RightClick));
                        exceptRightUpBuffer.ForEach(x => x.TryDoExceptEvent(RMouseEventType.RightClick));
                        mouseRightCD.ResetCoolDown();
                    }
                    else
                    {
                        rightUpBuffer.ForEach(x => x.TryDoEvent(RMouseEventType.RightDoubleClick));
                        exceptRightUpBuffer.ForEach(x => x.TryDoExceptEvent(RMouseEventType.RightDoubleClick));
                        mouseRightCD.CoolDown();
                    }
                    rightUpBuffer.ForEach(x => x.TryDoEvent(RMouseEventType.RightUp));
                    exceptRightUpBuffer.ForEach(x => x.TryDoExceptEvent(RMouseEventType.RightUp));
                    rightUpBuffer.Clear();
                    exceptRightUpBuffer.Clear();
                }
                mouseRightDown = nowMouseRight;
            }
            rightUpBuffer.ForEach(x => x.TryDoEvent(RMouseEventType.RightHolding));
            exceptRightUpBuffer.ForEach(x => x.TryDoExceptEvent(RMouseEventType.RightHolding));

            mouseLeftCD.Update();
            mouseRightCD.Update();
        }
        public static void GetHierarchy(Dictionary<int, List<RUIElement>> hierarchy, Transform transform, int layer = 0)
        {
            if (!hierarchy.ContainsKey(layer))
                hierarchy[layer] = new();
            List<RUIElement> list = hierarchy[layer];
            foreach (Transform trans in transform)
            {
                if (!trans.gameObject.activeInHierarchy)
                    continue;
                if (trans.TryGetComponent<RUIElement>(out var uie))
                    list.Add(uie);
                GetHierarchy(hierarchy, trans, layer + 1);
            }
        }
        private void OnDisable()
        {
            hoverTips.Clear();
        }

        private void UpdateHoverText(Vector3 mouse, float maxWidthToUse = 9f)
        {
            int count = hoverTips.Count;
            if (count == 0)
            {
                HoverTextBG.gameObject.SetActive(false);
                return;
            }
            HoverTextBG.gameObject.SetActive(true);
            Vector3 vector = new(0, 0, 0);
            float num = 0f;
            float num3 = maxWidthToUse;
            float offset = -1;
            foreach (var temp in TempTexts)
            {
                temp.gameObject.SetActive(false);
            }
            for (int i = 0; i < count; i++)
            {
                if (TempTexts.Count <= i)
                {
                    TempTexts.Add(Instantiate(HoverTextTemplate, HoverTopLeft));
                }
                var text = TempTexts[i];
                var hoverTip = hoverTips[i];
                text.localize = !string.IsNullOrEmpty(LocalizationManager.GetTranslation(hoverTip.text));
                text.gameObject.SetActive(true);
                text.maxWidth = num3;
                text.formatFields = hoverTip.formatFields;
                text.Render(hoverTip.text, false, false);
                Color color = hoverTips[i].color;
                text.SetTempColor(color);
                text.transform.localPosition = vector;
                var size = text.dimensions.size;
                vector.y -= size.y;
                if (offset < 0)
                {
                    float offY = size.y / text.displayedTextStringLinesAmount / 2f;
                    offset = offY;
                }
                float width5 = size.x;
                num = Mathf.Max(width5, num);
            }
            float width = num;
            float height = -vector.y;
            //超出屏幕高度，加宽然后重新计算高度
            if (height > 16.5f && maxWidthToUse + 4f < 30f)
            {
                UpdateHoverText(mouse, maxWidthToUse + 4f);
                return;
            }
            HoverTopLeft.transform.localPosition = new(-width / 2f, height / 2f - offset, 0);
            width += 0.5f;
            height += 0.25f;
            //var (x, y) = GetPanelCenter(30, 17, width, height, mouse);
            HoverTextBG.transform.localPosition = new(14.5f - width / 2f, height / 2f - 8f, 0f);
            HoverTextBG.GetComponent<SpriteRenderer>().size = new(width, height);
        }
        public (float x, float y) GetPanelCenter(float screenWidth, float screenHeight, float width, float height, Vector3 mouse)
        {
            // 初步设置悬浮面板的 topLeft
            screenWidth /= 2;
            screenHeight /= 2;
            float topLeftX = mouse.x + 0.5f;
            float topLeftY = mouse.y - 0.5f;

            // 计算面板的 bottomRight
            float bottomRightX = topLeftX + width;
            float bottomRightY = topLeftY - height;

            // 调整 topLeft 使 bottomRight 不超出屏幕范围
            if (bottomRightX > screenWidth)
            {
                topLeftX -= bottomRightX - screenWidth;
            }
            if (bottomRightY < screenHeight)
            {
                topLeftY += screenHeight - screenHeight;
            }

            return (topLeftX + width / 2f, topLeftY - height / 2f);
        }

    }
}
