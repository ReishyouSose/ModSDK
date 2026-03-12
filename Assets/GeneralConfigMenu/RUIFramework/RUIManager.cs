using I2.Loc;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.GeneralConfigMenu.RUIFramework
{
    public class RUIManager : MonoBehaviour
    {

        public RUIElement HoverElement { get; private set; }

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
        public readonly List<TextAndFormatFields> hoverTips = new();
        private void Awake()
        {
            RUIHoverTextContainer.managers.Add(this);
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
            HoverElement = interactedBuffer.LastOrDefault();

            //UpdateHoverText(mouse);

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
    }
}
