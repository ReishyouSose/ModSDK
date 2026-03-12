using Assets.GeneralConfigMenu.RUIFramework;
using PugMod;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.PointShop.Scripts
{
    public class UIShopSlot : RUIButton
    {
        [HideInInspector]
        public ObjectData objectData;

        [HideInInspector]
        public int Price;

        [HideInInspector]
        public Zone Zone;

        [HideInInspector]
        public ObjectID Boss;

        public SpriteRenderer icon;
        public PugText PriceText;
        public void Awake()
        {
            AddEvent(RMouseEventType.LeftClick, OnLeftClick);
        }
        public override List<TextAndFormatFields> GetHoverDesc()
        {
            ObjectID objectID = objectData.objectID;
            if (objectID != 0)
            {
                objectID = PlayerController.GetAnyObjectIDReplaceForNameAndDesc(objectID);
                if (!API.Authoring.ObjectProperties.TryGetPropertyString(objectID, "name", out var value))
                {
                    value = objectID.ToString();
                }

                string nameTermOverride = Manager.ui.itemOverridesTable.GetNameTermOverride(objectData);
                if (nameTermOverride != null)
                {
                    value = nameTermOverride;
                }

                return new List<TextAndFormatFields>
                {
                    GetHoverTitle(),
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
        public void SetItem(ObjectData objData, int sellPrice)
        {
            Price = sellPrice;
            PriceText.Render(sellPrice.ToString(), false, true);
            ObjectID id = objData.objectID;
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
            objectData = objData;
            icon.sprite = sprite;
            icon.gameObject.SetActive(true);
            icon.transform.localPosition = offset;
        }
        public TextAndFormatFields GetHoverTitle()
        {
            ContainedObjectsBuffer slotObject = new() { objectData = objectData };
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
        private static void OnLeftClick(GameObject go)
        {
            UIShopSlot slot = go.GetComponent<UIShopSlot>();
            slot.OnLeftClick();
        }
        private void OnLeftClick()
        {
            PointShopClient.TryBuyItem(Manager.main.player.entity, objectData,
                Boss, Price, PointShop.IsScale);
        }
    }
}
