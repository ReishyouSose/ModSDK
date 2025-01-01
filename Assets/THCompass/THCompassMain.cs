using Assets.THCompass.Compasses;
using Assets.THCompass.DataStruct;
using Assets.THCompass.Helper;
using Assets.THCompass.System;
using CoreLib;
using CoreLib.Drops;
using CoreLib.Submodules.ModEntity;
using CoreLib.Util.Extensions;
using PugMod;
using System;
using Unity.Entities;
using UnityEngine;

namespace Assets.THCompass
{
    public class THCompassMain : IMod
    {
        private static ModConfig config;
        internal static ModConfig Config => config ??= new();
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
        private  bool Testing => false;

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
            CoreLibMod.LoadModules(typeof(EntityModule));
            CoreLibMod.LoadModules(typeof(DropTablesModule));
            CompassLoader.Load();
            Debug.Log("grt: " + Config.Guaranteed);
            API.Authoring.OnObjectTypeAdded += Authoring_OnObjectTypeAdded;
            /*CoreLibMod.LoadModules(typeof(LocalizationModule));
            ResourcesModule.RegisterBundles(this.GetModInfo());
            CoreLibMod.LoadModules(typeof(EntityModule));*/
        }

        private void Authoring_OnObjectTypeAdded(Entity entity, GameObject authoringData, EntityManager entityManager)
        {
            int grt = Config.Guaranteed;
            if (grt <= 0) return;
            ObjectID id = authoringData.GetEntityObjectID();
            if (CompassLoader.BossIDByObjID.TryGetValue(id, out var boss))
            {
                ObjectID cps = ItemHelper.GetItemID("Compass_" + boss);
                DynamicBuffer<DropsLootBuffer> drBuffer;
                if (entityManager.HasBuffer<DropsLootBuffer>(entity))
                {
                    drBuffer = entityManager.GetBuffer<DropsLootBuffer>(entity);
                }
                else
                    drBuffer = entityManager.AddBuffer<DropsLootBuffer>(entity);
                drBuffer.Add(new DropsLootBuffer()
                {
                    lootDrop = new()
                    {
                        lootDropID = cps,
                        amount = Config.Guaranteed,
                    }
                });
                Debug.Log(id + "Add guaranteed compass " + boss);
            }
        }

        public void Init()
        {
            API.Client.OnWorldCreated += ClientWorldInit;
        }

        public void Shutdown()
        {

        }
        public void Update()
        {
            //if (Input.GetKeyDown(KeyCode.T))
            {
                // ObjectDataCD select = Manager.main.player.GetEquippedSlot().objectData;
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
            /*if (Input.GetKeyDown(KeyCode.K))
            {
                var lt = Manager.mod.LootTable[(int)LootTableID.SlimeBoss];
                Debug.Log(string.Join("\n", lt.lootInfos.Select(x => (x.objectID, x.weight))));
            }*/
            if (!Testing) return;
            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKeyDown(KeyCode.T))
                {
                    foreach (BossID id in Enum.GetValues(typeof(BossID)))
                    {
                        ItemHelper.DropItem(ItemHelper.GetItemID("Compass_" + id), 100);
                    }
                }
                else if (Input.GetKey(KeyCode.B))
                {
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
        }

        public void ModObjectLoaded(UnityEngine.Object obj)
        {
        }
    }
}
