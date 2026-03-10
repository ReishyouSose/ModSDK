using Assets.GeneralConfigMenu.RUIFramework;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.PointShop.Scripts
{
    public class UIZoneSlot : RUIButton
    {
        [HideInInspector]
        public Zone Zone
        {
            get => zone;
            set
            {
                zone = value;
                buffer = new()
                {
                    objectData = new()
                    {
                        objectID = ShopInfo.Ins.GetBoss(value)
                    }
                };
            }
        }
        private Zone zone;
        private ContainedObjectsBuffer buffer;

        public SpriteRenderer Icon;
        public override List<TextAndFormatFields> GetHoverDesc()
        {
            return new List<TextAndFormatFields>
            {
                new()
                {
                    text = zone is Zone.LarvaHive or Zone.Alien ? $"PointShop/{zone}"
                        : $"ItemCategory/Environment_{zone}Biome",
                    color = Color.white,
                },
                new()
                {
                    text ="Names/" + (zone == Zone.Clay ? "LarvaBoss": ShopInfo.Ins.GetBoss(zone)),
                    color = Color.white,
                },
            };
        }
    }
}
