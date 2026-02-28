using CoreLib.Submodule.UserInterface;
using PugMod;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.UI.ItemLookup
{
    public class ItemSelectSlot : SlotUIBase
    {
        [HideInInspector]
        public ContainedObjectsBuffer objData;
        public bool focus;
        protected override void Awake()
        {
            base.Awake();
        }
        protected override ContainedObjectsBuffer GetSlotObject()
        {
            return objData;
        }
        public override List<TextAndFormatFields> GetHoverDescription()
        {
            ContainedObjectsBuffer slotObject = GetSlotObject();
            ObjectID objectID = slotObject.objectID;
            if (objectID != 0)
            {
                objectID = PlayerController.GetAnyObjectIDReplaceForNameAndDesc(objectID);
                if (!API.Authoring.ObjectProperties.TryGetPropertyString(objectID, "name", out var value))
                {
                    value = objectID.ToString();
                }

                string nameTermOverride = Manager.ui.itemOverridesTable.GetNameTermOverride(slotObject.objectData);
                if (nameTermOverride != null)
                {
                    value = nameTermOverride;
                }

                return new List<TextAndFormatFields>
                {
                    new()
                    {
                        text = objectID.ToString()+$"({(int)objectID})",
                        color = Color.cyan
                    },
                    new() { text = "Items/" + value + "Desc" }
                };
            }
            return null;
        }
        public override void OnLeftClicked(bool mod1, bool mod2)
        {
            var obtain = ObtainLookupUI.ins;
            if (focus)
            {
                obtain.ClearSearch();
            }
            else
            {
                ItemLookupUI.ins.HideUI();
                UserInterfaceModule.OpenModUI("CoreEnhance:ObtainLookup");
                obtain.Focus.SetItem(this);
                obtain.SearchLoot();
            }
        }
        public void SetItem(ObjectID id)
        {
            if (id == ObjectID.None)
            {
                objData = default;
                icon.gameObject.SetActive(false);
                return;
            }
            var sprite = PugDatabase.GetObjectInfo(id)?.icon;
            if (sprite == null)
                return;
            objData = new()
            {
                objectData = new()
                {
                    objectID = id,
                    amount = 1
                }
            };
            icon.gameObject.SetActive(true);
            icon.sprite = sprite;
        }
        public void SetItem(ItemSelectSlot another)
        {
            objData = another.objData;
            var icon = another.icon.sprite;
            this.icon.sprite = icon == null ? ObtainLookupUI.ins.Missing : icon;
        }
    }
}
