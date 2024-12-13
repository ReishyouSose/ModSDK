using System.Collections.Generic;
using UnityEngine;

namespace Assets.GeneralConfigMenu.RUIFramework
{
    public class RUIManager : MonoBehaviour
    {
        public Transform HoverTextBG;
        public PugText HoverText;
        private bool mouseLeftDown;
        private bool mouseRightDown;
        private readonly RKeyCD mouseLeftCD = new(() => Input.GetMouseButton(0));
        private readonly RKeyCD mouseRightCD = new(() => Input.GetMouseButton(1));
        private List<RUIElement> interactedBuffer = new();
        private readonly List<RUIElement> leftUpBuffer = new();
        private readonly List<RUIElement> rightUpBuffer = new();
        private static readonly List<TextAndFormatFields> hoverTips = new();
        private readonly List<PugText> registerTips = new();
        private void Awake()
        {
            HoverTextBG.gameObject.SetActive(false);
        }
        private void Update()
        {
            var all = GetAllRUIEs(transform);
            var interact = GetElementsContainsPoint(transform, Manager.ui.mouse.pointer.position);
            hoverTips.Clear();

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
                        hoverTips.AddRange(uie.GetHoverDesc());
                }
            }

            HandleHoverText();

            bool nowMouseLeft = Input.GetMouseButton(0);
            if (mouseLeftDown != nowMouseLeft)
            {
                if (nowMouseLeft)
                {
                    interact.ForEach(x => x.TryDoEvent(RMouseEventType.LeftDown));
                    leftUpBuffer.AddRange(interact);
                }
                else
                {
                    if (mouseLeftCD.IsCoolDown())
                    {
                        leftUpBuffer.ForEach(x => x.TryDoEvent(RMouseEventType.LeftClick));
                        mouseLeftCD.ResetCoolDown();
                    }
                    else
                    {
                        leftUpBuffer.ForEach(x => x.TryDoEvent(RMouseEventType.LeftDoubleClick));
                        mouseLeftCD.CoolDown();
                    }
                    leftUpBuffer.ForEach(x => x.TryDoEvent(RMouseEventType.LeftUp));
                    leftUpBuffer.Clear();
                }

                mouseLeftDown = nowMouseLeft;
            }
            leftUpBuffer.ForEach(x => x.TryDoEvent(RMouseEventType.LeftHolding));

            bool nowMouseRight = Input.GetMouseButton(1);
            if (mouseRightDown != nowMouseRight)
            {
                if (nowMouseRight)
                {
                    interact.ForEach(x => x.TryDoEvent(RMouseEventType.RightDown));
                    rightUpBuffer.AddRange(interact);
                }
                else
                {
                    if (mouseRightCD.IsCoolDown())
                    {
                        rightUpBuffer.ForEach(x => x.TryDoEvent(RMouseEventType.RightClick));
                        mouseRightCD.ResetCoolDown();
                    }
                    else
                    {
                        rightUpBuffer.ForEach(x => x.TryDoEvent(RMouseEventType.RightDoubleClick));
                        mouseRightCD.CoolDown();
                    }
                    rightUpBuffer.ForEach(x => x.TryDoEvent(RMouseEventType.RightUp));
                    rightUpBuffer.Clear();
                }
                mouseRightDown = nowMouseRight;
            }
            rightUpBuffer.ForEach(x => x.TryDoEvent(RMouseEventType.RightHolding));

            mouseLeftCD.Update();
            mouseRightCD.Update();
        }
        public static List<RUIElement> GetAllRUIEs(Transform transform)
        {
            List<RUIElement> list = new();
            foreach (Transform trans in transform)
            {
                if (!trans.gameObject.activeInHierarchy)
                    continue;
                if (trans.TryGetComponent<RUIElement>(out var uie))
                    list.Add(uie);
                list.AddRange(GetAllRUIEs(trans));
            }
            return list;
        }
        public static List<RUIElement> GetElementsContainsPoint(Transform trans, Vector3 point)
        {
            List<RUIElement> elements = new();
            if (trans.TryGetComponent<RUIElement>(out var uie))
            {
                bool contains = uie.gameObject.activeInHierarchy
                    && uie.TryGetComponent<BoxCollider>(out var collider) && collider.bounds.Contains(point);
                if (contains && uie.Sensitive && uie.CanBeInteract)
                {
                    elements.Add(uie);
                }

                foreach (Transform child in trans)
                {
                    if (!child.gameObject.activeInHierarchy)
                        continue;
                    elements.AddRange(GetElementsContainsPoint(child, point));
                }

                if (elements.Count == 0 && contains && uie.CanBeInteract && !elements.Contains(uie))
                {
                    elements.Add(uie);
                }
            }
            else
            {
                foreach (Transform child in trans)
                {
                    if (!child.gameObject.activeInHierarchy)
                        continue;
                    elements.AddRange(GetElementsContainsPoint(child, point));
                }
            }
            return elements;
        }
        private void HandleHoverText()
        {
            bool hasTip = hoverTips?.Count > 0;
            if (!hasTip)
            {
                HoverTextBG.gameObject.SetActive(false);
                return;
            }
            HoverTextBG.gameObject.SetActive(true);
            foreach (var text in registerTips)
            {
                text.gameObject.SetActive(false);
            }
            float height = 0;
            for (int i = 0; i < hoverTips.Count; i++)
            {
                if (registerTips.Count < i + 1)
                    registerTips.Add(Instantiate(HoverText, HoverTextBG.transform));
                var text = registerTips[i];
                var hover = hoverTips[i];
                text.gameObject.SetActive(true);
                text.Render(hover.text);
                text.SetTempColor(text.color);
                float h = text.dimensions.height;
                text.transform.localPosition = new(-4.5f, height + h / 2f, 0);
                height += h;
            }
            float halfH = height / 2f;
            foreach (Transform tip in HoverTextBG.transform)
            {
                if (tip.gameObject.activeInHierarchy)
                {
                    var local = tip.transform.localPosition;
                    tip.transform.localPosition = new(-4.5f, halfH - local.y, 0);
                }
            }
            halfH += 0.5f;
            var mouse = Manager.ui.mouse.pointer.position;
            HoverTextBG.transform.localPosition = new(mouse.x + 5.5f, mouse.y - halfH - 0.5f, 0);
            HoverTextBG.GetComponent<SpriteRenderer>().size = new(10, halfH * 2);
        }
    }
}
