using Assets.CoreEnhance.Scripts.Helpers;
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
            var icon = PugDatabase.GetObjectInfo(id).icon;
            Icon.sprite = icon == null ? ObtainLookupUI.ins.Missing : icon;
            var offset = Icon.sprite.textureRectOffset;
            var size = Icon.sprite.texture.GetSize();
            var r = Icon.sprite.textureRect;
            Icon.GetPropertyBlock(materialPropertyBlock); // 获取当前属性块
            materialPropertyBlock.SetVector("_OriginSize", new(size.x, size.y, 0, 0)); // 设置属性
            materialPropertyBlock.SetVector("_SpriteRect", new(r.x, r.y, r.width, r.height)); // 设置属性
            Icon.SetPropertyBlock(materialPropertyBlock); // 应用属性块
        }
        public void SetInfoByLoot(ObjectID id, string info, float chance)
        {
            SetIcon(id);
            Name.Render(id.ToString() + $"[Drop chance {chance.ToPercent(4)}]", false, true);
            Info.Render(info, false, true);
        }
        public void SetInfoByShop(ObjectID id, MerchantItemRequirement info)
        {
            SetIcon(id);
            Name.Render(id.ToString() + "[Shop]", false, true);
            Info.Render("Requires/" + info);
        }
        public void SetInfoByRecipe(ObjectID id, CraftingAuthoring.CraftableObject recipe)
        {
            SetIcon(id);
            Name.Render(id.ToString() + "[Recipe]", false, true);
            Info.Render($"Get {recipe.amount} at a time");
        }
    }
}
