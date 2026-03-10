using Assets.GeneralConfigMenu.RUIFramework;
using Assets.GeneralConfigMenu.RUIFramework.Extend;
using CoreLib.Submodule.UserInterface.Interface;
using PugMod;
using UnityEngine;

namespace Assets.PointShop.Scripts
{
    public class PointShopUI : MonoBehaviour, IModUI
    {
        public GameObject Root => gameObject;

        public bool ShowWithPlayerInventory => true;

        public bool ShouldPlayerCraftingShow => false;
        public RUIScrollView ZonePanel;
        public RUIScrollView ShopPanel;
        public UIZoneSlot ZoneTemplate;
        public UIShopSlot ShopSlotTemplate;
        public PugText Header;
        public PugText PointValue;
        private ShopInfo info;
        private Zone currentZone;
        private ObjectID currency;
        public void Awake()
        {
            info = new();
            currency = API.Authoring.GetObjectID("PointShop_Currency");
            currentZone = Zone.Dirt;
            ZoneTemplate.gameObject.SetActive(false);
            ShopSlotTemplate.gameObject.SetActive(false);
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
                slot.Icon.sprite = SelectZoneIcon(zone);
                slot.gameObject.SetActive(true);
                view.AddChild(slot);
                if (i == 0)
                    slot.TryDoEvent(RMouseEventType.LeftClick);
            }
        }
        private void RegisterShop(RUIScrollView view, Transform parent)
        {
            info.TryGetShopItem(currentZone, out var shop);
            for (int i = 0; i < shop.Count; i++)
            {
                UIShopSlot slot = Instantiate(ShopSlotTemplate, parent);
                slot.SetItem(shop[i], 100);
                slot.gameObject.SetActive(true);
                view.AddChild(slot.gameObject);
            }
        }
        public void OnClickZoneSlot(GameObject go)
        {
            currentZone = go.GetComponent<UIZoneSlot>().Zone;
            foreach (Transform slot in ZonePanel.View)
            {
                if (slot.TryGetComponent(out UIZoneSlot zone) && zone.Zone != currentZone)
                {
                    zone.SetState(false, false);
                }
            }
            ShopPanel.Reload(RegisterShop);
            ShopPanel.gameObject.SetActive(true);
            Header.Render(currentZone is Zone.LarvaHive or Zone.Alien ? $"PointShop/{currentZone}"
                        : $"ItemCategory/Environment_{currentZone}Biome", false, true);
        }
        private void Update()
        {
            PointValue.Render(Manager.main.player.playerInventoryHandler.GetExistingAmountOfObject(currency).ToString(), false, true);
        }
        private static Sprite SelectZoneIcon(Zone zone)
        {
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
                return null;
            ObjectInfo info = PugDatabase.GetObjectInfo(id);
            return info.smallIcon;
            //slot.Icon.transform.localPosition = info.iconOffset;
        }
    }
}
