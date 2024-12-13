using Assets.GeneralConfigMenu.ConfigSync;
using CoreLib;
using CoreLib.RewiredExtension;
using CoreLib.UserInterface;
using PugMod;
using Rewired;
using UnityEngine;

namespace Assets.GeneralConfigMenu
{
    public class Main : IMod
    {
        private Player RewiredPlayer;
        private const string OpenMenu = "GCM:OpenMenu";
        private static ModConfig config;
        internal static ConfigSyncClient ConfigSync { get; private set; }
        public void EarlyInit()
        {
            CoreLibMod.LoadModules(typeof(RewiredExtensionModule));
            RewiredExtensionModule.rewiredStart += () => RewiredPlayer = ReInput.players.GetPlayer(0);
            RewiredExtensionModule.AddKeybind(OpenMenu, "Open Mod Config Menu", KeyboardKeyCode.K, ModifierKey.Control);
            CoreLibMod.LoadModule(typeof(UserInterfaceModule));
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
            UserInterfaceModule.RegisterModUI(gameObject);
        }

        public void Shutdown()
        {
        }

        public void Update()
        {
            if (RewiredPlayer.GetButtonDown(OpenMenu))
            {
                UserInterfaceModule.OpenModUI("GeneralConfigMenu:Menu");
            }
        }
    }
}
