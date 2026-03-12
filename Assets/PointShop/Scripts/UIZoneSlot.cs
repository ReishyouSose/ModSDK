using Assets.GeneralConfigMenu.RUIFramework;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.PointShop.Scripts
{
    public class UIZoneSlot : RUIButton
    {
        [HideInInspector]
        public Zone Zone;

        [HideInInspector]
        public ObjectID Boss;

        public SpriteRenderer Icon;
        public override List<TextAndFormatFields> GetHoverDesc()
        {
            return new List<TextAndFormatFields>
            {
                new()
                {
                    text = Zone is Zone.LarvaHive or Zone.Alien ? $"PointShop/{Zone}"
                        : $"ItemCategory/Environment_{Zone}Biome",
                    color = Color.white,
                },
                new()
                {
                    text ="Names/" + (Zone == Zone.Clay ? "LarvaBoss" : Boss),
                    color = Color.white,
                },
            };
        }
    }
}
