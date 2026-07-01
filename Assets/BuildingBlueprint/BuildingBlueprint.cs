using Assets.BuildingBlueprint.Scripts.Systems;
using Assets.BuildingBlueprint.Scripts.UI;
using CoreLib;
using CoreLib.Data.Configuration;
using CoreLib.Submodule.ControlMapping;
using CoreLib.Submodule.UserInterface;
using PugMod;
using UnityEngine;

namespace Assets.BuildingBlueprint
{
    public class BuildingBlueprint : IMod
    {
        internal const string Key = "BuildingBlueprint_";
        internal const string Menu = Key + "Menu";
        internal const string OpenUI = Key + "OpenUI";
        internal static ConfigEntry<string> Saves;
        internal static bool InputActive;
        public void EarlyInit()
        {
            CoreLibMod.LoadSubmodule(typeof(UserInterfaceModule));
            CoreLibMod.LoadSubmodule(typeof(ControlMappingModule));
            int category = ControlMappingModule.AddNewCategory(Key[..^1]);
            ControlMappingModule.AddKeyboardBind(OpenUI, Rewired.KeyboardKeyCode.V, categoryId: category);
            API.Authoring.OnObjectTypeAdded += Authoring_OnObjectTypeAdded;
            var file = new ConfigFile("BuildingBlueprint/Saves.cfg", true);
            Saves = file.Bind("General", "Saves", "[]", scope: new(ConfigAccessLevel.ViewOnly));
        }

        private void Authoring_OnObjectTypeAdded(Unity.Entities.Entity entity, GameObject authoringData, Unity.Entities.EntityManager entityManager)
        {
            BuildingSelectServer.AddSelectBuffer(entity, authoringData, entityManager);
            BuildingSelectServer.MarkPlaceable(entity, authoringData, entityManager);
            BlueprintStateChangeServer.AddItemInteracBlockToPlayer(entity, authoringData, entityManager);
        }

        public void Init()
        {
        }

        public void ModObjectLoaded(Object obj)
        {
            if (obj is GameObject go)
            {
                UserInterfaceModule.RegisterModUI(go);
            }
        }

        public void Shutdown()
        {
        }

        public void Update()
        {
            var player = Manager.main.player;
            if (!player)
                return;
            if (player.inputModule.rewiredPlayer.GetButtonDown(OpenUI))
            {
                var ui = BlueprintUI.Ins;
                if (!ui.Root.activeSelf)
                    ui.ShowUI();
                else if (!InputActive)
                    ui.HideUI();
            }
        }
    }
}
