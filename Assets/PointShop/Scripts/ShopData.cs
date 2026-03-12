using System.Collections.Generic;

namespace Assets.PointShop.Scripts
{
    public struct ShopData
    {
        public ObjectID Boss;
        public List<ShopItem> Items;
    }
    public struct ShopItem
    {
        public ObjectData Item;
        public int Price;
        public static implicit operator ShopItem(ObjectID id)
        {
            return new()
            {
                Item = new()
                {
                    objectID = id,
                    amount = 1,
                },
                Price = 1
            };
        }
        public static implicit operator ShopItem((ObjectID id, int price) value)
        {
            return new()
            {
                Item = new()
                {
                    objectID = value.id,
                    amount = 1,
                },
                Price = value.price
            };
        }
    }
}