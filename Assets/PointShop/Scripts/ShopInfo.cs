using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.PointShop.Scripts
{
    public class ShopInfo : MonoBehaviour
    {
        private Dictionary<Zone, ShopData> shops;
        public void Init()
        {
            shops = new();
            Init(Zone.Dirt, ObjectID.SlimeBoss, Dirt());
            Init(Zone.Clay, ObjectID.BossLarva, Clay());
            Init(Zone.LarvaHive, ObjectID.LarvaHiveBoss, LarvaHive());
            Init(Zone.Stone, ObjectID.ShamanBoss, Stone());
            Init(Zone.Nature, ObjectID.BirdBoss, Nature());
            Init(Zone.Mold, ObjectID.PoisonSlimeBoss, Mold());
            Init(Zone.Sea, ObjectID.OctopusBoss, Sea());
            Init(Zone.City, ObjectID.SlipperySlimeBoss, City());
            Init(Zone.Desert, ObjectID.ScarabBoss, Desert());
            Init(Zone.Lava, ObjectID.LavaSlimeBoss, Lava());
            Init(Zone.Oasis, ObjectID.GiantCicadaBoss, Oasis());
            Init(Zone.Crystal, ObjectID.HydraBossDesert, Crystal());
            Init(Zone.Alien, ObjectID.CoreBoss, Alien());
            Init(Zone.Passage, ObjectID.WallBoss, Passage());
            Init(Zone.Excavation, ObjectID.RobotBoss, Excavation());
        }
        public List<ShopItem> GetShop(Zone zone) => shops[zone].Items;
        public ObjectID GetBoss(Zone zone) => shops[zone].Boss;
        private void Init(Zone zone, ObjectID boss, List<ShopItem> items)
        {
            shops[zone] = new() { Boss = boss, Items = items.ToList() };
        }
        private List<ShopItem> Dirt()
        {
            return new()
            {

            };
        }
        private List<ShopItem> Clay()
        {
            return new()
            {
            };
        }
        private List<ShopItem> LarvaHive()
        {
            return new()
            {

            };
        }
        private List<ShopItem> Stone()
        {
            return new()
            {

            };
        }
        private List<ShopItem> Nature()
        {
            return new()
            {

            };
        }
        private List<ShopItem> Mold()
        {
            return new()
            {

            };
        }
        private List<ShopItem> Sea()
        {
            return new()
            {

            };
        }
        private List<ShopItem> City()
        {
            return new()
            {

            };
        }
        private List<ShopItem> Desert()
        {
            return new()
            {

            };
        }
        private List<ShopItem> Lava()
        {
            return new()
            {

            };
        }
        private List<ShopItem> Oasis()
        {
            return new()
            {

            };
        }
        private List<ShopItem> Crystal()
        {
            return new()
            {

            };
        }
        private List<ShopItem> Alien()
        {
            return new()
            {

            };
        }
        private List<ShopItem> Passage()
        {
            return new()
            {

            };
        }
        private List<ShopItem> Excavation()
        {
            return new()
            {

            };
        }
    }
}