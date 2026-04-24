using Assets.GeneralConfigMenu.ConfigSync;
using Assets.GeneralConfigMenu.Scripts;
using CoreLib;
using CoreLib.Submodule.ControlMapping;
using CoreLib.Submodule.UserInterface;
using HarmonyLib;
using PugMod;
using UnityEngine;

namespace Assets.GeneralConfigMenu
{
    [HarmonyPatch]
    public class GeneralConfigMenuMod : IMod
    {
        internal static GameObject MenuPrefab;
        internal static ModConfig config;
        internal static ConfigSyncClient ConfigSync { get; private set; }
        internal static RadicalMenu.MenuType Menu { get; private set; }
        public void EarlyInit()
        {
            Menu = (RadicalMenu.MenuType)1493;
            CoreLibMod.LoadSubmodule(typeof(ControlMappingModule), typeof(UserInterfaceModule));
            int cateogry = ControlMappingModule.AddNewCategory("GCM");
            config = new();
            API.Client.OnWorldCreated += Client_OnWorldCreated;
        }

        private void Client_OnWorldCreated()
        {
            var world = API.Client.World;
            world.GetOrCreateSystem<ConfigSyncClient>();
            ConfigSync = world.GetExistingSystemManaged<ConfigSyncClient>();
        }

        public void Init()
        {
        }

        public void ModObjectLoaded(Object obj)
        {
            if (obj is not GameObject gameObject)
                return;
            if (gameObject.GetComponent<ModConfigMenu>())
                MenuPrefab = gameObject;
        }

        public void Shutdown()
        {
        }

        public void Update()
        {
        }
    }
}
