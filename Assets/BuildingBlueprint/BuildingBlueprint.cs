using Assets.BuildingBlueprint.Scripts;
using CoreLib;
using CoreLib.Submodule.ControlMapping;
using CoreLib.Submodule.UserInterface;
using PugMod;
using UnityEngine;

namespace Assets.BuildingBlueprint
{
    public class BuildingBlueprint : IMod
    {
        internal const string OpenUI = "BuildingBlueprint_OpenUI";
        public void EarlyInit()
        {
            CoreLibMod.LoadSubmodule(typeof(UserInterfaceModule));
            CoreLibMod.LoadSubmodule(typeof(ControlMappingModule));
            ControlMappingModule.AddKeyboardBind(OpenUI, Rewired.KeyboardKeyCode.V);
            API.Authoring.OnObjectTypeAdded += Authoring_OnObjectTypeAdded;
        }

        private void Authoring_OnObjectTypeAdded(Unity.Entities.Entity entity, GameObject authoringData, Unity.Entities.EntityManager entityManager)
        {
            if (authoringData.TryGetComponent<TileAuthoring>(out _))
                return;
            if (!authoringData.TryGetComponent<PlaceableObjectAuthoring>(out _))
                return;
            if (authoringData.TryGetComponent<MineableAuthoring>(out _) || authoringData.TryGetComponent<DiggableAuthoring>(out _))
            {
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
            if (!Manager.main.player)
                return;
            if (Manager.main.player.inputModule.rewiredPlayer.GetButtonDown(OpenUI))
            {
                BlueprintUI.Ins.Switch();
            }
        }
    }
}
