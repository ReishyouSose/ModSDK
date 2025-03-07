using Assets.CoreEnhance.Scripts.UI.ModScroll;
using CoreLib.UserInterface;
using CoreLib.Util.Extensions;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.UI.ItemLookup
{
    public class ObtainLookupUI : MonoBehaviour, IScrollable, IModUI
    {
        internal static ObtainLookupUI ins;
        public ModUIScrollWindow View;
        public ObtainInfoSlot Template;
        public ItemSelectSlot Focus;
        public Sprite Missing;
        private List<ObtainInfoSlot> slots;
        private float height;
        private static Dictionary<ObjectID, DropLootAuthoring> loots;
        private static Dictionary<ObjectID, MerchantAuthoring> shops;
        private static Dictionary<ObjectID, CraftingAuthoring> recipes;
        private static Dictionary<ObjectID, LootTableID> chests;
        private static Dictionary<ObjectID, ChangeVariationWhenContainingObjectAuthoring> lockedChests;
        public GameObject Root => gameObject;
        public bool showWithPlayerInventory => false;

        public bool shouldPlayerCraftingShow => false;
        private void Awake()
        {
            HideUI();
            ins = this;
            slots = new();
            View.scrollable = this;
        }
        public void HideUI()
        {
            gameObject.SetActive(false);
        }

        public void ShowUI()
        {
            gameObject.SetActive(true);
        }
        public void SearchLoot()
        {
            ObjectID id = Focus.objData.objectID;
            foreach (var info in slots)
            {
                info.gameObject.SetActive(false);
            }
            height = 0;
            int i = 0;
            var lootTables = Manager.mod.LootTable;
            foreach (var (item, lootInfo) in loots)
            {
                if (lootInfo.hasCustomLoot)
                {
                    foreach (var loot in lootInfo.customLoot.Values)
                    {
                        if (loot.lootDropID == id)
                        {
                            AddLootInfo(ref i, ref height, item, "Custom Loot", lootInfo.customLoot.chance);
                            break;
                        }
                    }
                }
                if (lootInfo.hasLootDropsOnTakingDamage)
                {
                    var loot = lootInfo.lootDropsWhenDamaged.dropsLoot;
                    if (loot == id)
                    {
                        AddLootInfo(ref i, ref height, item, "Loot when damaged", 1);
                    }
                }
                if (lootInfo.hasLootDropsOnUse)
                {
                    foreach (var loot in lootInfo.onUseLootDrops.lootDrops)
                    {
                        if (loot.lootDropID == id)
                        {
                            AddLootInfo(ref i, ref height, item, $"On use, get {loot.amount} at a time", loot.chance);
                        }
                    }
                    int index = lootTables.FindIndex(0, x => x.id == lootInfo.onUseLootDrops.lootTableID);
                    if (index >= 0)
                    {
                        AddLootInfoByLootTable(lootTables[index], id, item, ref i, ref height);
                    }
                }
                if (lootInfo.hasLootTable)
                {
                    int index = lootTables.FindIndex(0, x => x.id == lootInfo.lootTableID);
                    if (index >= 0)
                    {
                        AddLootInfoByLootTable(lootTables[index], id, item, ref i, ref height);
                    }
                }
                if (lootInfo.hasSeasonalLoot)
                {
                    foreach (var season in lootInfo.seasonalLootDrops.lootDrops)
                    {
                        foreach (var loot in season.lootDrops)
                        {
                            if (loot.lootDropID == id)
                            {
                                AddLootInfo(ref i, ref height, item, "Season:" + season.season, loot.chance);
                                break;
                            }
                        }
                    }
                }
            }
            foreach (var (npc, shopInfo) in shops)
            {
                foreach (var entry in shopInfo.items)
                {
                    if (entry.objectID == id)
                    {
                        var slot = ActiveSlot(ref i, ref height);
                        slot.SetInfoByShop(npc, entry.requirementToBeAvailable);
                        break;
                    }
                }
            }
            foreach (var (station, recipeInfo) in recipes)
            {
                foreach (var entry in recipeInfo.canCraftObjects)
                {
                    if (entry.objectID == id)
                    {
                        var slot = ActiveSlot(ref i, ref height);
                        slot.SetInfoByRecipe(station, entry);
                        break;
                    }
                }
            }
            foreach (var (chest, lootID) in chests)
            {
                int index = lootTables.FindIndex(0, x => x.id == lootID);
                if (index >= 0)
                {
                    AddLootInfoByLootTable(lootTables[index], id, chest, ref i, ref height);
                }
            }
            foreach (var (chest, chestInfo) in lockedChests)
            {
                foreach (var loot in chestInfo.addItemsToNewObject)
                {
                    if (loot.objectID == id)
                    {
                        AddLootInfo(ref i, ref height, chest, $"Get {loot.amount} at a time", 1);
                        break;
                    }
                }
                int index = lootTables.FindIndex(0, x => x.id == chestInfo.addLootFromTableToNewObject);
                if (index >= 0)
                    AddLootInfoByLootTable(lootTables[index], id, chest, ref i, ref height);
            }
            if (height > 0)
            {
                height -= 0.125f;
            }
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
        internal static void CheckData(Entity entity, GameObject authoringData, EntityManager manager)
        {
            ObjectID id = authoringData.GetEntityObjectID();
            if (authoringData.TryGetComponent<DropLootAuthoring>(out var dropLoot))
            {
                loots ??= new();
                loots[id] = dropLoot;
            }
            if (authoringData.TryGetComponent<MerchantAuthoring>(out var merchant))
            {
                shops ??= new();
                shops[id] = merchant;
            }
            if (authoringData.TryGetComponent<CraftingAuthoring>(out var crafting))
            {
                recipes ??= new();
                recipes[id] = crafting;
            }
            if (authoringData.TryGetComponent<InventoryAuthoring>(out var inventory))
            {
                if (inventory.addLootFromTable != LootTableID.Empty)
                {
                    chests ??= new();
                    chests[id] = inventory.addLootFromTable;
                }
            }
            if (authoringData.TryGetComponent<ChangeVariationWhenContainingObjectAuthoring>(out var chest))
            {
                lockedChests ??= new();
                lockedChests[id] = chest;
            }
        }
        private ObtainInfoSlot ActiveSlot(ref int index, ref float height)
        {
            if (slots.Count <= index)
            {
                slots.Add(Instantiate(Template, View.scrollingContent));
            }
            ObtainInfoSlot slot = slots[index++];
            slot.gameObject.SetActive(true);
            slot.transform.localPosition = new(0, 3 - height, 0);
            height += 2.125f;
            return slot;
        }
        private void AddLootInfo(ref int index, ref float height, ObjectID id, string info, float chance)
        {
            var slot = ActiveSlot(ref index, ref height);
            slot.SetInfoByLoot(id, info, chance);
        }
        private void AddLootInfoByLootTable(LootTable lt, ObjectID target, ObjectID id, ref int i, ref float height)
        {
            foreach (var loot in lt.guaranteedLootInfos)
            {
                if (loot.objectID == target)
                {
                    AddLootInfo(ref i, ref height, id, "Guaranteed Roll/" + lt.id, loot.editorVisualDropChance);
                    break;
                }
            }
            foreach (var loot in lt.lootInfos)
            {
                if (loot.objectID == target)
                {
                    AddLootInfo(ref i, ref height, id, "Normal Rolls/" + lt.id, loot.editorVisualDropChance);
                    break;
                }
            }
        }
        public void ClearSearch()
        {
            HideUI();
            UserInterfaceModule.OpenModUI("CoreEnhance:ItemLookup");
        }
    }
}
