using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.PointShop.Scripts
{
    public class UIZoneSlot : ButtonUIElement
    {
        private static readonly WaitForSeconds time = new(0.1f);
        [HideInInspector]
        public Zone Zone;

        [HideInInspector]
        public ObjectID Boss;

        public GameObject WarnBorder;
        public SpriteRenderer Icon;
        public override void OnLeftClicked(bool mod1, bool mod2)
        {
            base.OnLeftClicked(mod1, mod2);
            PointShopUI.Ins.OnClickZoneSlot(this);
        }
        public override List<TextAndFormatFields> GetHoverDescription()
        {
            var list = new List<TextAndFormatFields>()
            {
                new()
                {
                    text = $"ItemCategory/Environment_{Zone}Biome",
                    color = Color.white,
                },
                new()
                {
                    text = "PointShop/NeedDefeat",
                    color = Color.white,
                }
            };
            if (Zone != Zone.None)
            {
                list.Add(new()
                {
                    text = "Names/" + (Zone == Zone.Clay ? "LarvaBoss" : Boss),
                    color = Color.white,
                });
            }
            return list;
        }
        public void WarnNotDefeat()
        {
            StartCoroutine(NotDefeat());
        }
        private IEnumerator NotDefeat()
        {
            WarnBorder.SetActive(true);
            yield return time;
            WarnBorder.SetActive(false);
            yield return time;
            WarnBorder.SetActive(true);
            yield return time;
            WarnBorder.SetActive(false);
            yield return time;
            WarnBorder.SetActive(true);
            yield return time;
            WarnBorder.SetActive(false);
            yield return time;
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            WarnBorder.SetActive(false);
        }
    }
}
