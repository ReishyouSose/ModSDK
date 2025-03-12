using Assets.GeneralConfigMenu.ConfigSync;
using CoreLib;
using CoreLib.RewiredExtension;
using CoreLib.UserInterface;
using PugMod;
using Rewired;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts
{
    public class Main : IMod
    {
        private Player RewiredPlayer;
        private const string OpenMenu = "GCM:OpenMenu";
        internal const string HorizenScroll = "GCM:HorizenScroll";
        internal static ModConfig config;
        internal static ConfigSyncClient ConfigSync { get; private set; }
        public void EarlyInit()
        {
            CoreLibMod.LoadModules(typeof(RewiredExtensionModule));
            RewiredExtensionModule.rewiredStart += () => RewiredPlayer = ReInput.players.GetPlayer(0);
            var local = new Dictionary<string, string>()
            {
                { "en", "Open Mod Config Menu" },
                { "zh-CN", "打开模组配置菜单" }
            };
            RewiredExtensionModule.AddKeybind(OpenMenu, local, KeyboardKeyCode.K, ModifierKey.Control);
            RewiredExtensionModule.AddKeybind(HorizenScroll, "GCM:Horizen Scroll(Useless now)", KeyboardKeyCode.LeftShift);
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
            if (Manager.main.player == null)
                return;
            var ui = ConfigManager.Instance;
            if (RewiredPlayer.GetButtonDown(OpenMenu))
            {
                if (ui.Root.activeInHierarchy)
                {
                    ui.HideUI();
                }
                else
                    UserInterfaceModule.OpenModUI("GeneralConfigMenu:Screen");
            }
        }
    }
}
