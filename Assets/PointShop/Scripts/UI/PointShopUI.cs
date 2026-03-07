using Assets.GeneralConfigMenu.RUIFramework;
using Assets.GeneralConfigMenu.RUIFramework.Extend;
using CoreLib.Submodule.UserInterface.Interface;
using Pug.UnityExtensions;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.PointShop.Scripts
{
    public class PointShopUI : UIelement, IModUI
    {
        public GameObject Root => gameObject;

        public bool ShowWithPlayerInventory => false;

        public bool ShouldPlayerCraftingShow => false;
        public RUIScrollView BiomeView;
        public Transform ShopParent;
        public RUIScrollView ShopView;
        public UIZoneSlot ZoneTemplate;
        public UIShopSlot ShopSlotTemplate;
        public PugText Header;
        public PugText PointValue;
        public RUIManager Manager;
        private ShopInfo info;
        private RUIScrollView[] shops;
        private UIShopSlot hoverSlot;
        public void Awake()
        {
            info = new();
            BiomeView.Reload(RegisterBiome);
            BiomeView.gameObject.SetActive(true);
            HideUI();
        }

        public void Update()
        {
            if (Manager.hoverElement is UIShopSlot slot)
                hoverSlot = slot;
            else
                hoverSlot = null;
        }

        public override ContainedObjectsBuffer GetContainedObject()
        {
            if (hoverSlot)
                return hoverSlot.GetSlotObject();
            return default;
        }
        public override TextAndFormatFields GetHoverTitle()
        {
            if (hoverSlot)
                return hoverSlot.GetHoverTitle();
            return null;
        }
        public override List<TextAndFormatFields> GetHoverDescription()
        {
            if (hoverSlot)
                return hoverSlot.GetHoverDescription();
            return null;
        }

        public void HideUI()
        {
            Root.SetActive(false);
        }

        public void ShowUI()
        {
            Root.SetActive(true);
        }
        private void RegisterBiome(RUIScrollView view, Transform parent)
        {
            int max = (int)Zone.MAX;
            shops = new RUIScrollView[max];
            ZoneTemplate.gameObject.SetActive(false);
            ShopView.gameObject.SetActive(false);
            ShopSlotTemplate.gameObject.SetActive(false);
            for (int i = 0; i < max; i++)
            {
                Zone zone = (Zone)i;
                UIZoneSlot slot = Instantiate(ZoneTemplate, parent);
                slot.AddEvent(RMouseEventType.LeftClick, OnClickZoneSlot);
                slot.ToggleAtFirst = false;
                AddZoneMark(slot, zone);
                slot.gameObject.SetActive(true);
                view.AddChild(slot);

                var shop = shops[i] = Instantiate(ShopView, ShopParent);
                AddZoneMark(shop, zone);
                shop.Reload(RegisterShop);
                shop.gameObject.SetActive(false);
            }
        }
        private void RegisterShop(RUIScrollView view, Transform parent)
        {
            var zone = view.GetComponent<ZoneMark>().Zone;
            info.TryGetShopItem(zone, out var shop);
            foreach (var item in shop)
            {
                UIShopSlot slot = Instantiate(ShopSlotTemplate, parent);
                slot.SetItem(item, 100);
                slot.gameObject.SetActive(true);
                view.AddChild(slot.gameObject);
            }
        }
        public void OnClickZoneSlot(GameObject go)
        {
            info.CheckDefeat();
            var zone = go.GetComponent<ZoneMark>().Zone;
            int zoneInt = (int)zone;
            for (int i = 0; i < shops.Length; i++)
            {
                var shop = shops[i];
                if (i == zoneInt)
                {
                    shop.gameObject.SetActive(true);
                }
                else
                    shop.gameObject.SetActive(false);
            }
            Header.Render(zone.ToString(), false, true);
        }
        private static void AddZoneMark(MonoBehaviour go, Zone zone)
        {
            var zoneMark = go.gameObject.AddComponent<ZoneMark>();
            zoneMark.Zone = zone;
            if (!go.TryGetComponent<UIZoneSlot>(out var slot))
                return;
            ObjectID id = zone switch
            {
                Zone.Dirt => ObjectID.WallDirtBlock,
                Zone.Clay => ObjectID.WallClayBlock,
                Zone.LarvaHive => ObjectID.WallHiveBlock,
                Zone.Stone => ObjectID.WallStoneBlock,
                Zone.Nature => ObjectID.WallGlassBlock,
                Zone.Mold => ObjectID.WallMoldBlock,
                Zone.Sea => ObjectID.WallLimestoneBlock,
                Zone.City => ObjectID.WallCityBlock,
                Zone.Desert => ObjectID.WallDesertBlock,
                Zone.Lava => ObjectID.WallLavaBlock,
                Zone.Crystal => ObjectID.WallCrystalBlock,
                Zone.Oasis => ObjectID.WallOasisBlock,
                Zone.Alien => ObjectID.WallAlienBlock,
                Zone.Passage => ObjectID.WallPassageBlock,
                Zone.Excavation => ObjectID.WallExcavationBlock,
                _ => ObjectID.None,
            };
            if (id == ObjectID.None)
                return;
            ObjectInfo info = PugDatabase.GetObjectInfo(id);
            slot.Icon.sprite = info.smallIcon;
            //slot.Icon.transform.localPosition = info.iconOffset;
        }
    }
}
