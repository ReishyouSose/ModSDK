using CoreLib.UserInterface;
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
        public override TextAndFormatFields GetHoverTitle()
        {
            return base.GetHoverTitle();
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
            icon.sprite = another.icon.sprite;
        }
    }
}
