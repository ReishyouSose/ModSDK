using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.GeneralConfigMenu.RUIFramework
{
    public class RUIElement : MonoBehaviour
    {
        private readonly Dictionary<RMouseEventType, Action<GameObject>> Events = new();

        [HideInInspector]
        public bool IsMouseHover;

        public bool CanBeInteract = true;
        public bool Sensitive;
        public string HoverText;
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
        public virtual List<TextAndFormatFields> GetHoverDesc()
        {
            if (string.IsNullOrEmpty(HoverText))
            {
                return null;
            }
            return new()
            {
                new()
                {
                    text = HoverText,
                    formatFields = new string[0],
                }
            };
        }
    }
}
