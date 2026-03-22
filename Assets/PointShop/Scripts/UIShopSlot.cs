using Assets.GeneralConfigMenu.RUIFramework;
using PugMod;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.PointShop.Scripts
{
    public class UIShopSlot : RUIButton
    {
        private static readonly WaitForSeconds time = new(0.1f);

        [HideInInspector]
        public ObjectData objectData;

        [HideInInspector]
        public int Price;

        [HideInInspector]
        public Zone Zone;

        [HideInInspector]
        public ObjectID Boss;

        [HideInInspector]
        public ObjectID Currency;

        public SpriteRenderer ItemIcon;
        public SpriteRenderer CurrencyIcon;
        public PugText PriceText;
        public PugText PriceRed;
        public PugText Amount;
        protected override void Awake()
        {
            base.Awake();
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
                    new() { text = "Items/" + value + "Desc" }
                };
            }
            return null;
        }
        public void SetItem(ObjectData objData, int sellPrice, ObjectID currency = ObjectID.None)
        {
            ObjectID id = objData.objectID;
            if (id == ObjectID.None)
            {
                objectData = default;
                ItemIcon.gameObject.SetActive(false);
                return;
            }
            var info = PugDatabase.GetObjectInfo(id, objData.variation);
            if (info == null)
                return;
            var sprite = info.icon;
            var offset = info.iconOffset;
            objectData = objData;
            Price = sellPrice;
            PriceText.Render(sellPrice.ToString(), false, true);
            PriceRed.Render(sellPrice.ToString(), false, true);
            PriceRed.gameObject.SetActive(false);
            ItemIcon.sprite = sprite;
            ItemIcon.transform.localPosition = offset;
            int amount = objData.amount;
            if (amount > 1)
            {
                Amount.Render(amount.ToString(), false, true);
            }
            if (currency != ObjectID.None)
            {
                Currency = currency;
                CurrencyIcon.sprite = PugDatabase.GetObjectInfo(currency).smallIcon;
            }
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
            PointShopUI.Ins.CurrentShopSlot = slot;
            slot.OnLeftClick();
        }
        private void OnLeftClick()
        {
            PointShopClient.TryBuyItem(Manager.main.player.entity, objectData,
                Boss, Currency, Price, PointShop.IsScale);
            AudioManager.Sfx(SfxID.twitch, Manager.main.player.transform.position, 0.1f, 0.55f, 0.1f, reuse: true);
        }
        public void WarnNotEnough()
        {
            StartCoroutine(NotEnough());
        }
        private IEnumerator NotEnough()
        {
            var obj = PriceRed.gameObject;
            obj.SetActive(true);
            yield return time;
            obj.SetActive(false);
            yield return time;
            obj.SetActive(true);
            yield return time;
            obj.SetActive(false);
            yield return time;
            obj.SetActive(true);
            yield return time;
            obj.SetActive(false);
            yield return time;
        }
    }
}
