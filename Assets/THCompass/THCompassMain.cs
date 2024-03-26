using Assets.THCompass.Compasses;
using Assets.THCompass.System;
using PugMod;
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
            var world = API.Client.World;
            //world.GetOrCreateSystem<ClientSpawnRoomSystem>();
            //roomSpawnSystem = world.GetExistingSystemManaged<ClientSpawnRoomSystem>();
            world.GetOrCreateSystem<ClientCompassLootSystem>();
            compassLootSystem = world.GetExistingSystemManaged<ClientCompassLootSystem>();
        }
        public void EarlyInit()
        {
            /*CoreLibMod.LoadModules(typeof(LocalizationModule));
            ResourcesModule.RegisterBundles(this.GetModInfo());
            CoreLibMod.LoadModules(typeof(EntityModule));*/
        }
        public void Init()
        {
            API.Client.OnWorldCreated += ClientWorldInit;
            CompassLoader.Load();
        }

        public void ModObjectLoaded(Object obj)
        {
            /*if (obj is WorkbenchDefinition workbenchDefinition)
            {
                EntityModule.AddModWorkbench(workbenchDefinition);
                return;
            }*/
        }

        public void Shutdown()
        {

        }
        public void Update()
        {
            /*if (Input.GetKeyDown(KeyCode.T))
            {
                ObjectDataCD select = Manager.main.player.GetEquippedSlot().objectData;
                ItemHelper.DropItem(ItemHelper.GetItemID("CastingTableEntity"));
                Debug.Log(select.objectID);
            }*/
        }
    }
}
