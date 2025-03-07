using Assets.CoreEnhance.Scripts.UI.ModScroll;
using CoreLib.UserInterface;
using I2.Loc;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.UI.ItemLookup
{
    public class ItemLookupUI : MonoBehaviour, IScrollable, IModUI
    {
        public GameObject Root => gameObject;
        public bool showWithPlayerInventory => false;

        public bool shouldPlayerCraftingShow => false;
        internal static ItemLookupUI ins;
        public ModUIScrollWindow View;
        public ItemSelectSlot Template;
        public TextInputField Searcher;
        private List<ItemSelectSlot> slots;
        private float height;
        private ObjectID[] ids;

        private void Awake()
        {
            HideUI();
            ins = this;
            slots = new();
            View.scrollable = this;
            ids = Enum.GetValues(typeof(ObjectID)).Cast<ObjectID>().ToArray();
        }
        public void HideUI()
        {
            gameObject.SetActive(false);
        }

        public void ShowUI()
        {
            gameObject.SetActive(true);
        }
        public void SearchItem()
        {
            foreach (SlotUIBase slot in slots)
            {
                slot.gameObject.SetActive(false);
            }
            string input = Searcher.GetInputText().ToLower();
            int i = 0;
            bool any = false;
            foreach (ObjectID id in ids)
            {
                string idName = id.ToString().ToLower();
                bool idMatch = idName.Contains(input);
                bool isItem = LocalizationManager.TryGetTranslation("Items/" + id, out string local);
                if (!isItem)
                    continue;
                bool nameMatch = local.ToLower().Contains(input);
                if (!idMatch && !nameMatch)
                    continue;
                any = true;
                if (slots.Count <= i)
                {
                    var slot = Instantiate(Template, View.scrollingContent);
                    slot.focus = false;
                    slot.visibleSlotIndex = i;
                    slots.Add(slot);
                }
                var info = slots[i];
                info.gameObject.SetActive(true);
                info.SetItem(id);
                info.transform.localPosition = new((i % 16) * 1.25f + 0.625f, 4 - (i / 16) * 1.25f - 0.625f, 0);
                i++;
            }
            height = any ? ((i / 16 + 1) * 1.25f - 0.125f) : 0;
        }
        public void UpdateContainingElements(float scroll)
        {
            /*foreach (Transform trans in transform)
            {
                Vector3 p = trans.position;
                trans.position = new(p.x, p.y - scroll, p.z);
            }*/
        }

        public bool IsBottomElementSelected()
        {
            return false;
        }
        public bool IsTopElementSelected()
        {
            return false;
        }

        public float GetCurrentWindowHeight()
        {
            return height;
        }
        private void Update()
        {
        }
    }
}
