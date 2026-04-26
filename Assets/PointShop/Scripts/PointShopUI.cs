using CoreLib.Submodule.UserInterface.Interface;
using UnityEngine;

namespace Assets.PointShop.Scripts
{
    [RequireComponent(typeof(ShopInfo))]
    [RequireComponent(typeof(UIScrollWindow))]
    public class PointShopUI : UIelement, IModUI, IScrollable
    {
        internal static PointShopUI Ins { get; private set; }
        public GameObject Root => gameObject;

        public bool ShowWithPlayerInventory => true;

        public bool ShouldPlayerCraftingShow => false;
        public UIScrollWindow ZonePanel;
        public UIZoneSlot ZoneTemplate;
        public UIShopSlot ShopSlotTemplate;
        public Transform EmptryPage;
        public Transform PageContainer;
        public PugText Header;
        public PugText PointValue;

        private UIZoneSlot current;
        private ShopInfo info;
        private GridLayoutUIComponent layout;
        private UIScrollWindow scroll;
        private void Awake()
        {
            Ins = this;
            info = GetComponent<ShopInfo>();
            scroll = GetComponent<UIScrollWindow>();
            ZoneTemplate.gameObject.SetActive(false);
            ShopSlotTemplate.gameObject.SetActive(false);
            EmptryPage.gameObject.SetActive(false);
            info.Init();
            int max = (int)Zone.MAX;
            var page = ZonePanel.scrollingContent.GetChild(0);
            for (int i = 0; i < max; i++)
            {
                Zone zone = (Zone)i;
                UIZoneSlot slot = Instantiate(ZoneTemplate, page);
                slot.Zone = zone;
                slot.Boss = info.GetBoss(zone);
                slot.Icon.sprite = SelectZoneIcon(zone);
                slot.gameObject.SetActive(true);
                slot.Page = RegisterShop(zone);
            }
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
            var layout = ZonePanel.scrollingContent.GetComponentInChildren<LinearLayoutUIComponent>();
            layout.RenderUIComponent(true);
            ZonePanel.ResetScroll();
            if (!current)
            {
                OnClickZoneSlot(layout.transform.GetChild(0).GetComponent<UIZoneSlot>());
            }
        }
        private Transform RegisterShop(Zone zone)
        {
            var page = Instantiate(EmptryPage, PageContainer);
            page.gameObject.SetActive(false);
            var contents = page.GetChild(0);
            var items = info.GetShop(zone);
            items.Sort((x, y) => PugDatabase.GetObjectInfo(x.Item.objectID).rarity.CompareTo(PugDatabase.GetObjectInfo(y.Item.objectID).rarity));
            var boss = info.GetBoss(zone);
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                UIShopSlot slot = Instantiate(ShopSlotTemplate, contents);
                slot.Zone = zone;
                slot.Boss = boss;
                slot.SetItem(item.Item, item.Price, item.Currency);
                slot.gameObject.SetActive(true);
            }
            return page;
        }
        public void OnClickZoneSlot(UIZoneSlot slot)
        {
            if (current)
            {
                current.Selected.gameObject.SetActive(false);
                current.Page.gameObject.SetActive(false);
            }
            current = slot;
            current.Selected.gameObject.SetActive(true);
            var page = current.Page;
            page.gameObject.SetActive(true);
            scroll.scrollingContent = page;
            layout = page.GetComponentInChildren<GridLayoutUIComponent>();
            layout.RenderUIComponent(true);
            Header.Render($"ItemCategory/Environment_{slot.Zone}Biome", false, true);
            AudioManager.Sfx(SfxTableID.inventorySFXCreativeModeCategory, Manager.main.player.transform.position);
            scroll.ResetScroll();
        }
        private void Update()
        {
            PointValue.Render(Manager.main.player.playerInventoryHandler.GetExistingAmountOfObject(PointShop.Coin).ToString(), false, true);
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

        public void UpdateContainingElements(float scroll)
        {

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
            if (layout)
                return layout.GetUIComponentRenderHeight();
            return 0;
        }
        public void WarnNotDefeat()
        {
            current.WarnNotDefeat();
        }
    }
}
