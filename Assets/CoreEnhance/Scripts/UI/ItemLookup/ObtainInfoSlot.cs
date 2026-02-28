using Assets.CoreEnhance.Scripts.Helpers;
using Pug.UnityExtensions;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.UI.ItemLookup
{
    public class ObtainInfoSlot : MonoBehaviour
    {
        public SpriteMask Mask;
        public SpriteRenderer Icon;
        public PugText Name;
        public PugText Info;
        private MaterialPropertyBlock materialPropertyBlock;
        private void Awake()
        {
            // 初始化 MaterialPropertyBlock
            materialPropertyBlock = new MaterialPropertyBlock();
        }
        public void SetIcon(ObjectID id)
        {
            var info = PugDatabase.GetObjectInfo(id);
            var icon = info.icon;
            var offset = info.iconOffset;
            Icon.sprite = icon == null ? ObtainLookupUI.ins.Missing : icon;
            Icon.transform.localPosition = new(-9 + offset.x, offset.y, 0);
            var size = Icon.sprite.texture.GetSize();
            var r = Icon.sprite.textureRect;
            Icon.GetPropertyBlock(materialPropertyBlock); // 获取当前属性块
            materialPropertyBlock.SetVector("_OriginSize", new(size.x, size.y, 0, 0)); // 设置属性
            materialPropertyBlock.SetVector("_SpriteRect", new(r.x, r.y, r.width, r.height)); // 设置属性
            Icon.SetPropertyBlock(materialPropertyBlock); // 应用属性块
        }
        private string GetName(ObjectID id)
        {
            ContainedObjectsBuffer buffer = new()
            {
                objectData = new() { objectID = id },
            };
            var name = PlayerController.GetObjectName(buffer, true).text;
            return (string.IsNullOrEmpty(name) ? id.ToString() : name) + $"({(int)id})";
        }
        public void SetInfoByLoot(ObjectID id, string info, float chance)
        {
            SetIcon(id);
            Name.Render(GetName(id) + $"[Drop chance {chance.ToPercent(4)}]", false, true);
            Info.Render(info, false, true);
        }
        public void SetInfoByShop(ObjectID id, MerchantItemRequirement info)
        {
            SetIcon(id);
            Name.Render(GetName(id) + "[Shop]", false, true);
            Info.Render("Requires/" + info);
        }
        public void SetInfoByRecipe(ObjectID id, CraftingAuthoring.CraftableObject recipe)
        {
            SetIcon(id);
            Name.Render(GetName(id) + "[Recipe]", false, true);
            Info.Render($"Get {recipe.amount} at a time");
        }
    }
}
