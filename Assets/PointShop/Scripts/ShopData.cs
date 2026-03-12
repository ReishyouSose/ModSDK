using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.PointShop.Scripts
{
    [CreateAssetMenu(fileName = "ShopData", menuName = "PointShop/ShopData")]
    public class ShopData : ScriptableObject
    {
        public List<ZoneShopSerialize> shopList;
    }

    [Serializable]
    public struct ZoneShopSerialize
    {
        public Zone Zone;
        public string Boss;
        public List<ShopItem> Items;
    }


    [Serializable]
    public struct ShopItem
    {
        public string ObjectID;
        public int Amount;
    }

    public struct ZoneShop
    {
        public Zone Zone;
        public ObjectID Boss;
        public List<ObjectData> Items;
    }
}
