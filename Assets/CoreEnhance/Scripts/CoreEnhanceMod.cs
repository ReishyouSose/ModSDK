using Assets.CoreEnhance.Scripts.Configs;
using Assets.CoreEnhance.Scripts.Datas;
using Assets.CoreEnhance.Scripts.Edits;
using Assets.CoreEnhance.Scripts.Helpers;
using Assets.CoreEnhance.Scripts.Systems.Automation;
using Assets.CoreEnhance.Scripts.Systems.Infinity;
using Assets.CoreEnhance.Scripts.UI;
using Assets.CoreEnhance.Scripts.UI.ItemLookup;
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
    public class CoreEnhanceMod : IMod
    {
        public const string InternalName = "CoreEnhance:";
        private int timer;
        public void EarlyInit()
        {
            EnhanceConfig.Load();
            CoreLibMod.LoadModules(typeof(EntityModule), typeof(UserInterfaceModule), typeof(RewiredExtensionModule));
            ModKeyBind.Load();
            var authoring = API.Authoring;
            authoring.OnObjectTypeAdded += InfinityArenaSystem.RecordArena;
            authoring.OnObjectTypeAdded += ModRecipes.EditWorkbench;
            //authoring.OnObjectTypeAdded += PlaceSizeEdit.EditResizeableTool;
            authoring.OnObjectTypeAdded += ObtainLookupUI.CheckData;
            authoring.OnObjectTypeAdded += AutoDoorSystem.AddDistanceCD;
            //authoring.OnObjectTypeAdded += Test;
        }

        private void Test(Entity entity, GameObject authoringData, EntityManager entityManager)
        {
            ItemHelper.LogComponent(authoringData, ObjectID.PandoriumCrystal);
            ItemHelper.LogComponent(authoringData, ObjectID.SmallPandoriumCrystal);
        }

        public void Init()
        {
            QocObjectID.Load();
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
            ModKeyBind.Handle(p);
            if (timer < 60)
            {
                timer++;
                return;
            }
            timer = 0;
            ArenaScannerUI.CheckScanner(p);
        }
    }
}
