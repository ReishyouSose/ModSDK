using Pug.Sprite;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Helpers
{
    public static class SpriteObjectHelper
    {
        public static void PlayAnimationByIndex(this SpriteObject sprite, int index, int variation)
        {
            var asset = sprite.asset;
            if (asset.animationCount <= index)
            {
                Debug.Log("Animation index out of range");
                return;
            }
            var anim = asset.GetAnimationAt(index);
            variation--;
            if (variation > -1 && anim.variantCount <= variation)
            {
                Debug.Log("Animation variation out of range");
                return;
            }
            int hash = SpriteAsset.StringToHash(anim.name);
            int vari = anim.GetVariantHash(variation);
            sprite.PlayAnimation(hash, vari);
        }
    }
}
