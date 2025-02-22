using Assets.CoreEnhance.Scripts.Configs;
using Assets.CoreEnhance.Scripts.Edits;
using Assets.CoreEnhance.Scripts.Helpers;
using Assets.CoreEnhance.Scripts.UI;
using CoreLib;
using CoreLib.RewiredExtension;
using CoreLib.Submodules.ModEntity;
using CoreLib.UserInterface;
using CoreLib.Util.Extensions;
using PugMod;
using Unity.Entities;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts
{
    public class Main : IMod
    {
        public void EarlyInit()
        {
            ModConfig.Load();
            CoreLibMod.LoadModules(typeof(EntityModule), typeof(UserInterfaceModule), typeof(RewiredExtensionModule));
            ModKeyBind.Load();
            API.Authoring.OnObjectTypeAdded += Authoring_OnObjectTypeAdded;
            API.Authoring.OnObjectTypeAdded += ModRecipes.EditWorkbench;
            API.Authoring.OnObjectTypeAdded += PlaceSizeEdit.EditResizeableTool;
            API.Server.OnWorldCreated += Server_OnWorldCreated;
        }

        private void Server_OnWorldCreated()
        {

        }

        private void Authoring_OnObjectTypeAdded(Entity entity, GameObject authoringData, EntityManager entityManager)
        {
            if (authoringData.GetEntityObjectID() == ObjectID.WallDirtBlock)
            {
                ItemHelper.LogComponent(authoringData);
            }
        }

        public void Init()
        {
        }

        public void ModObjectLoaded(Object obj)
        {
            if (obj is GameObject gameObject)
            {
                if (gameObject.TryGetComponent<EntityMonoBehaviour>(out _))
                {
                    EntityModule.EnablePooling(gameObject);
                }

                UserInterfaceModule.RegisterModUI(gameObject);
            }
            else if (obj is WorkbenchDefinition workbenchDefinition)
            {
                EntityModule.AddModWorkbench(workbenchDefinition);
            }

        }

        public void Shutdown()
        {
        }

        public void Update()
        {
            var p = Manager.main.player;
            if (p == null)
                return;
            ArenaScannerUI.CheckScanner(p);
            ModKeyBind.Handle(p);
        }
    }
}
