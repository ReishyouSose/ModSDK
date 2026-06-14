using Assets.GeneralConfigMenu.Scripts;
using Assets.GeneralConfigMenu.Scripts.ConfigSync;
using CoreLib;
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
        internal static RadicalMenu.MenuType Menu { get; private set; }
        internal static ConfigSyncSystem Sync;
        public void EarlyInit()
        {
            Menu = (RadicalMenu.MenuType)1493;
            CoreLibMod.LoadSubmodule(typeof(UserInterfaceModule));
            config = new();
            API.Client.OnWorldCreated += Client_OnWorldCreated;
        }

        private void Client_OnWorldCreated()
        {
            var world = API.Client.World;
            world.GetOrCreateSystem<ConfigSyncSystem>();
            Sync = world.GetExistingSystemManaged<ConfigSyncSystem>();
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
            if (!Manager.main.player)
            {
                if (ModConfigMenu.Instance)
                    ModConfigMenu.Instance.TryResetConnect();
            }
        }
    }
}
