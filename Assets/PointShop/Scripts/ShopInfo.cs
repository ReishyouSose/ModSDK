using PugMod;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.PointShop.Scripts
{
    public class ShopInfo : MonoBehaviour
    {
        internal static ShopInfo Ins { get; private set; }

        [SerializeField]
        private ShopData data;

        private Dictionary<Zone, ZoneShop> shops;
        public void Awake()
        {
            Ins = this;
            shops = new();
            foreach (var value in data.shopList)
            {
                Zone zone = value.Zone;
                ObjectID boss = API.Authoring.GetObjectID(value.Boss);
                if (!PugDatabase.HasComponent<BossAuthoring>(boss))
                {
                    Debug.LogError($"Shop [{zone}] boss error!");
                }
                List<ObjectData> result = new();
                foreach (var item in value.Items)
                {
                    ObjectID id = API.Authoring.GetObjectID(item.ObjectID);
                    if(id == ObjectID.None)
                    {
                        Debug.LogError($"Shop [{zone}] objectid [{item.ObjectID}] not exsist!");
                    }
                    result.Add(new()
                    {
                        objectID = id,
                        amount = Math.Max(1, item.Amount),
                    });
                }
                shops[value.Zone] = new()
                {
                    Zone = value.Zone,
                    Boss = boss,
                    Items = result,
                };
            }
        }
        public List<ObjectData> GetShop(Zone zone) => shops[zone].Items;
        public ObjectID GetBoss(Zone zone) => shops[zone].Boss;
    }
}