using Assets.GeneralConfigMenu.ConfigSync;
using CoreLib;
using CoreLib.Submodule.ControlMapping;
using CoreLib.Submodule.UserInterface;
using PugMod;
using Rewired;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts
{
    public class GeneralConfigMenuMod : IMod
    {
        private static List<MonoBehaviour> needChanges;
        private const string OpenMenu = "GCM_OpenMenu";
        internal const string HorizenScroll = "GCM_HorizenScroll";
        internal static ModConfig config;
        internal static ConfigSyncClient ConfigSync { get; private set; }
        public void EarlyInit()
        {
            CoreLibMod.LoadSubmodule(typeof(ControlMappingModule), typeof(UserInterfaceModule));
            var local = new Dictionary<string, string>()
            {
                { "en", "Open Mod Config Menu" },
                { "zh-CN", "打开模组配置菜单" }
            };
            int cateogry = ControlMappingModule.AddNewCategory("GCM");
            ControlMappingModule.AddKeyboardBind(OpenMenu, KeyboardKeyCode.K, ModifierKey.Control, categoryId: cateogry);
            ControlMappingModule.AddKeyboardBind(HorizenScroll, KeyboardKeyCode.LeftShift, categoryId: cateogry);
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
            var p = Manager.main.player;
            if (p == null)
                return;

            if (needChanges == null)
            {
                var parent = Manager.ui.playerHealthBarUI.transform.parent;
                needChanges = new()
                {
                    parent.GetComponentInChildren<PlayerHealthBarUI>(true),
                    parent.GetComponentInChildren<MagicBarrierBarUI>(true),
                    parent.GetComponentInChildren<PlayerHungerBarUI>(true),
                    parent.GetComponentInChildren<PlayerManaBarUI>(true),
                    parent.GetComponentInChildren<ConditionsContainerUI>(true),
                    parent.GetComponentInChildren<MinionCountUI>(true),
                    parent.GetComponentInChildren<InGameButtonHintsUI>(true)
                };
            }
            var ui = ConfigManager.Instance;
            if (p.inputModule.rewiredPlayer.GetButtonDown(OpenMenu))
            {
                if (ui.Root.activeInHierarchy)
                {
                    ui.HideUI();
                }
                else
                    UserInterfaceModule.OpenModUI("GeneralConfigMenu:Screen");
            }
        }
        public static void SetPlayerStateUI(bool active)
        {
            if (needChanges == null)
                return;
            foreach (var ui in needChanges)
            {
                ui.gameObject.SetActive(active);
            }
        }
    }
}
