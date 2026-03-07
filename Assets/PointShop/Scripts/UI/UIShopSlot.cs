using Assets.GeneralConfigMenu.RUIFramework;
using PugMod;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.PointShop.Scripts
{
    public class UIShopSlot : RUIButton
    {
        [HideInInspector]
        public ContainedObjectsBuffer objectData;

        [HideInInspector]
        public int Price;

        public SpriteRenderer icon;
        public PugText PriceText;
        public ContainedObjectsBuffer GetSlotObject()
        {
            return objectData;
        }
        public List<TextAndFormatFields> GetHoverDescription()
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
        public void SetItem(ObjectID id, int sellPrice)
        {
            Price = sellPrice;
            PriceText.Render(sellPrice.ToString(), false, true);
            if (id == ObjectID.None)
            {
                objectData = default;
                icon.gameObject.SetActive(false);
                return;
            }
            var info = PugDatabase.GetObjectInfo(id);
            if (info == null)
                return;
            var sprite = info.icon;
            var offset = info.iconOffset;
            objectData = new()
            {
                objectData = new()
                {
                    objectID = id,
                    amount = 1
                }
            };
            icon.sprite = sprite;
            icon.gameObject.SetActive(true);
            icon.transform.localPosition = offset;
        }
        public TextAndFormatFields GetHoverTitle()
        {
            ContainedObjectsBuffer slotObject = GetSlotObject();
            ObjectID objectID = slotObject.objectID;
            TextAndFormatFields textAndFormatFields = null;
            if (objectID != ObjectID.None)
            {
                textAndFormatFields = PlayerController.GetObjectName(slotObject, localize: false);
                Rarity objectRarity = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation)?.rarity ?? Rarity.Common;
                textAndFormatFields.color = Manager.text.GetRarityColor(objectRarity);
            }
            return textAndFormatFields;
        }
    }
}
