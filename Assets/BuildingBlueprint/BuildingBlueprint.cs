using Assets.BuildingBlueprint.Scripts;
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
        internal static ConfigFile File;
        public void EarlyInit()
        {
            CoreLibMod.LoadSubmodule(typeof(UserInterfaceModule));
            CoreLibMod.LoadSubmodule(typeof(ControlMappingModule));
            ControlMappingModule.AddKeyboardBind(OpenUI, Rewired.KeyboardKeyCode.V);
            API.Authoring.OnObjectTypeAdded += Authoring_OnObjectTypeAdded;
            File = new ConfigFile("BuildingBlueprint/Saves.cfg", true);
            Saves = File.Bind("General", "Saves", "[]", scope: new(ConfigAccessLevel.ViewOnly));
        }

        private void Authoring_OnObjectTypeAdded(Unity.Entities.Entity entity, GameObject authoringData, Unity.Entities.EntityManager entityManager)
        {
            if (authoringData.TryGetComponent<TileAuthoring>(out _))
                return;
            if (authoringData.TryGetComponent<MineableAuthoring>(out _) || authoringData.TryGetComponent<DiggableAuthoring>(out _))
            {
                if (authoringData.TryGetComponent(out EntityMonoBehaviourData data))
                {
                    if (data.objectInfo.icon == null)
                        return;
                }
                else if (!authoringData.TryGetComponent(out InventoryItemAuthoring _))
                    return;
                entityManager.AddComponent<PlaceableCD>(entity);
            }
        }

        public void Init()
        {
        }

        public void ModObjectLoaded(Object obj)
        {
            if (obj is GameObject go)
                UserInterfaceModule.RegisterModUI(go);
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
                else
                    ui.HideUI();
            }
        }
    }
}
