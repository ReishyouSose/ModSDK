using Assets.GeneralConfigMenu.RUIFramework;
using Assets.GeneralConfigMenu.RUIFramework.Extend;
using CoreLib.Submodule.UserInterface.Interface;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.PointShop.Scripts
{
    [RequireComponent(typeof(ShopInfo))]
    public class PointShopUI : MonoBehaviour, IModUI
    {
        internal static PointShopUI Ins { get; private set; }
        public GameObject Root => gameObject;

        public bool ShowWithPlayerInventory => true;

        public bool ShouldPlayerCraftingShow => false;
        public RUIScrollView ZonePanel;
        public RUIScrollView ShopPanel;
        public UIZoneSlot ZoneTemplate;
        public UIShopSlot ShopSlotTemplate;
        public PugText Header;
        public PugText PointValue;
        public PugText ScaleTip;

        [HideInInspector]
        public UIZoneSlot CurrentZoneSlot;

        [HideInInspector]
        public UIShopSlot CurrentShopSlot;

        private ShopInfo info;
        private Zone currentZone;
        private Dictionary<Zone, RUIScrollView> shops;
        public void Awake()
        {
            Ins = this;
            info = GetComponent<ShopInfo>();
            ZoneTemplate.gameObject.SetActive(false);
            ShopSlotTemplate.gameObject.SetActive(false);
            ShopPanel.gameObject.SetActive(false);
            info.Init();
            shops = new();
            ZonePanel.Reload(RegisterZone);
            ZonePanel.gameObject.SetActive(true);
            HideUI();
        }

        public void HideUI()
        {
            Root.SetActive(false);
        }

        public void ShowUI()
        {
            Manager.ui.HideAllInventoryAndCraftingUI();
            Root.SetActive(true);
        }
        private void RegisterZone(RUIScrollView view, Transform parent)
        {
            int max = (int)Zone.MAX;
            for (int i = 0; i < max; i++)
            {
                Zone zone = (Zone)i;
                UIZoneSlot slot = Instantiate(ZoneTemplate, parent);
                slot.AddEvent(RMouseEventType.LeftClick, OnClickZoneSlot);
                slot.Zone = zone;
                slot.Boss = info.GetBoss(zone);
                slot.Icon.sprite = SelectZoneIcon(zone);
                slot.gameObject.SetActive(true);
                view.AddChild(slot);
                RUIScrollView shop = shops[zone] = Instantiate(ShopPanel, ShopPanel.transform.parent);
                shop.Reload((shopView, shopParent) => RegisterShop(shopView, shopParent, zone));
            }
            Header.Render($"ItemCategory/Environment_{currentZone}Biome", false, true);
            shops[currentZone].gameObject.SetActive(true);
        }
        private void RegisterShop(RUIScrollView view, Transform parent, Zone zone)
        {
            var items = info.GetShop(zone);
            var boss = info.GetBoss(zone);
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                UIShopSlot slot = Instantiate(ShopSlotTemplate, parent);
                slot.Zone = zone;
                slot.Boss = boss;
                slot.SetItem(item.Item, item.Price, item.Currency);
                slot.gameObject.SetActive(true);
                view.AddChild(slot.gameObject);
            }
        }
        public void OnClickZoneSlot(GameObject go)
        {
            var slot = go.GetComponent<UIZoneSlot>();
            CurrentZoneSlot = slot;
            currentZone = slot.Zone;
            foreach (var (_, shop) in shops)
            {
                shop.gameObject.SetActive(false);
            }
            shops[currentZone].gameObject.SetActive(true);
            Header.Render($"ItemCategory/Environment_{currentZone}Biome", false, true);
            AudioManager.Sfx(SfxTableID.inventorySFXCreativeModeCategory, Manager.main.player.transform.position);
        }
        private void Update()
        {
            PointValue.Render(Manager.main.player.playerInventoryHandler.GetExistingAmountOfObject(PointShop.Coin).ToString(), false, true);
            ScaleTip.SetTempColor(Input.GetKey(KeyCode.LeftControl) ? Color.yellow : Color.white);
        }
        private static Sprite SelectZoneIcon(Zone zone)
        {
            ObjectID id = zone switch
            {
                Zone.Dirt => ObjectID.WallDirtBlock,
                Zone.Clay => ObjectID.WallClayBlock,
                Zone.LarvaHive => ObjectID.WallHiveBlock,
                Zone.Stone => ObjectID.WallStoneBlock,
                Zone.Nature => ObjectID.WallGrassBlock,
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
                _ => ObjectID.WallObsidianBlock,
            };
            ObjectInfo info = PugDatabase.GetObjectInfo(id);
            return info.smallIcon;
            //slot.Icon.transform.localPosition = info.iconOffset;
        }
    }
}
