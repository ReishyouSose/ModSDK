using Assets.THCompass.Compasses;
using Assets.THCompass.DataStruct;
using Assets.THCompass.Helper;
using Assets.THCompass.System;
using CoreLib;
using CoreLib.Drops;
using PugMod;
using System;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.THCompass
{
    public class THCompassMain : IMod
    {
        /*internal static ClientSpawnRoomSystem roomSpawnSystem;
        internal static ClientSpawnBossSystem spawnBossSystem;
        private static CustomScenesDataTable sceneData;
        internal static CustomScenesDataTable SceneData
        {
            get
            {
                if (sceneData == null)
                {
                    sceneData = Resources.Load<CustomScenesDataTable>("Scenes/CustomScenesDataTable");
                }
                return sceneData;
            }
        }*/
        internal static ClientCompassLootSystem compassLootSystem;

        private void ClientWorldInit()
        {
            /*var world = API.Client.World;
            //world.GetOrCreateSystem<ClientSpawnRoomSystem>();
            //roomSpawnSystem = world.GetExistingSystemManaged<ClientSpawnRoomSystem>();
            world.GetOrCreateSystem<ClientCompassLootSystem>();
            compassLootSystem = world.GetExistingSystemManaged<ClientCompassLootSystem>();*/
        }
        public void EarlyInit()
        {
            CoreLibMod.LoadModules(typeof(DropTablesModule));
            CompassLoader.Load();
            /*CoreLibMod.LoadModules(typeof(LocalizationModule));
            ResourcesModule.RegisterBundles(this.GetModInfo());
            CoreLibMod.LoadModules(typeof(EntityModule));*/
        }
        public void Init()
        {
            API.Client.OnWorldCreated += ClientWorldInit;
            foreach (var loot in Manager.mod.LootTable)
            {
                LootTableID ltID = loot.id;
                if (ltID.ToString().Contains("Boss") || ltID == LootTableID.CoreCommander)
                {
                    loot.minUniqueDrops++;
                    loot.maxUniqueDrops++;
                    loot.dontAllowDuplicates = true;
                    LootInfo cps = null;
                    float sumWeight = 0;
                    foreach (var info in loot.lootInfos)
                    {
                        if (info.objectID.ToString() != ((int)info.objectID).ToString())
                        {
                            sumWeight += info.weight;
                        }
                        else
                            cps ??= info;
                    }
                    foreach (var info in loot.lootInfos)
                    {
                        if (info == cps)
                        {
                            info.weight = sumWeight * 0.1f;
                            StringBuilder log = new StringBuilder(ltID.ToString())
                                .Append(" SumWeight: ").Append(sumWeight)
                                .Append(" Compass: ").Append(info.objectID)
                                .Append(' ').Append(info.weight);
                            Debug.Log(log);
                        }
                        else
                        {
                            info.weight *= 0.9f;
                        }
                    }
                }
                if (CompassLoader.CompassLootByID.ContainsValue(ltID))
                {
                    loot.dontAllowDuplicates = true;
                    Debug.Log("Dont allow " + ltID + " duplicates");
                }
            }
        }

        public void Shutdown()
        {

        }
        public void Update()
        {
            //if (Input.GetKeyDown(KeyCode.T))
            {
                //ObjectDataCD select = Manager.main.player.GetEquippedSlot().objectData;
                //ItemHelper.DropItem(ItemHelper.GetItemID("Compass_Slime"), 100);
                //ObjectDataCD select = Manager.main.player.GetEquippedSlot().objectData;
                /*if (select.TryGetComponent<SpawnsItemsOnUseCD>(out var dropCD))
                {
                    Debug.Log(dropCD.lootTable);
                }*/
                //ItemHelper.DropItem(ObjectID.SlimeBossSummoningItem, 100);
                //Debug.Log(select.objectID);
                //compassLootSystem.CompassLoot(BossID.Slime, false, Manager.main.player.WorldPosition.RountToFloat3(), 0);
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                var lt = Manager.mod.LootTable[(int)LootTableID.SlimeBoss];
                Debug.Log(string.Join("\n", lt.lootInfos.Select(x => (x.objectID, x.weight))));
            }
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.G))
            {
                foreach (BossID id in Enum.GetValues(typeof(BossID)))
                {
                    ItemHelper.DropItem(ItemHelper.GetItemID("Compass_" + id), 100);
                }
                foreach (Compasses.Compass cps in CompassLoader.CompassByID.Values)
                {
                    if (cps.BossSummoner > ObjectID.None)
                    {
                        ItemHelper.DropItem(cps.BossSummoner, 100);
                    }
                }
                ItemHelper.DropItem(ObjectID.SlimeBossSummoningItem, 100);
            }
        }

        public void ModObjectLoaded(UnityEngine.Object obj)
        {
        }
    }
}
