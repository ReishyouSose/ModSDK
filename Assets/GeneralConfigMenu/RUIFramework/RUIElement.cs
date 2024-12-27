using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.GeneralConfigMenu.RUIFramework
{
    public class RUIElement : MonoBehaviour
    {
        private readonly Dictionary<RMouseEventType, Action<GameObject>> Events = new();
        private readonly Dictionary<RMouseEventType, Action<GameObject>> ExceptEvents = new();

        [HideInInspector]
        public bool IsMouseHover;

        public bool CanBeInteract = true;
        public bool Sensitive;
        public List<string> HoverText = new();
        public Func<int, string, TextAndFormatFields> SpecialHoverTextSnip;

        [HideInInspector]
        public bool LockByOther;

        [HideInInspector]
        public Func<bool> AllowEvent;

        public object[] customData;
        public void AddEvent(RMouseEventType eventType, Action<GameObject> evt)
        {
            if (!Events.ContainsKey(eventType))
                Events.Add(eventType, delegate
                { });
            Events[eventType] += evt;
        }
        public bool TryDoEvent(RMouseEventType eventType)
        {
            if (Events.TryGetValue(eventType, out var events))
            {
                events.Invoke(gameObject);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Only Click Type will take effect
        /// </summary>
        public void AddExceptEvent(RMouseEventType eventType, Action<GameObject> evt)
        {
            if (!ExceptEvents.ContainsKey(eventType))
                ExceptEvents.Add(eventType, delegate
                { });
            ExceptEvents[eventType] += evt;
        }

        /// <summary>
        /// Only Click Type will take effect
        /// </summary>
        public bool TryDoExceptEvent(RMouseEventType eventType)
        {
            if (ExceptEvents.TryGetValue(eventType, out var events))
            {
                events.Invoke(gameObject);
                return true;
            }
            return false;
        }
        public virtual List<TextAndFormatFields> GetHoverDesc()
        {
            List<TextAndFormatFields> list = new();
            int count = HoverText.Count;
            for (int i = 0; i < count; i++)
            {
                var t = HoverText[i];
                if (string.IsNullOrEmpty(t))
                {
                    continue;
                }
                list.Add(SpecialHoverTextSnip?.Invoke(i, t) ?? new()
                {
                    text = t,
                    formatFields = new string[0],
                });

            }
            return list;
        }

        public static void SetLockState(Transform trans, bool state, bool ignoreSelf = false)
        {
            RUIElement uie = trans.GetComponent<RUIElement>();
            if (!ignoreSelf)
            {
                uie.LockByOther = state;
            }
            foreach (Transform child in trans)
            {
                SetLockState(child, state, false);
            }
        }
        public void SetCustomData(params object[] customData)
        {
            this.customData = customData;
        }
        public static implicit operator GameObject(RUIElement uie) => uie.gameObject;
        public static implicit operator Transform(RUIElement uie) => uie.transform;
    }
}
