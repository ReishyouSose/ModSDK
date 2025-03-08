using Assets.CoreEnhance.Scripts.Configs;
using Assets.CoreEnhance.Scripts.Edits;
using Assets.CoreEnhance.Scripts.UI;
using Assets.CoreEnhance.Scripts.UI.ItemLookup;
using CoreLib;
using CoreLib.RewiredExtension;
using CoreLib.Submodules.ModEntity;
using CoreLib.UserInterface;
using PugMod;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts
{
    public class Main : IMod
    {
        private int timer;
        public void EarlyInit()
        {
            ModConfig.Load();
            CoreLibMod.LoadModules(typeof(EntityModule), typeof(UserInterfaceModule), typeof(RewiredExtensionModule));
            ModKeyBind.Load();
            var authoring = API.Authoring;
            authoring.OnObjectTypeAdded += NoRecoil;
            authoring.OnObjectTypeAdded += ModRecipes.EditWorkbench;
            authoring.OnObjectTypeAdded += PlaceSizeEdit.EditResizeableTool;
            authoring.OnObjectTypeAdded += ObtainLookupUI.CheckData;
            API.Server.OnWorldCreated += Server_OnWorldCreated;
        }

        private void Server_OnWorldCreated()
        {

        }

        private void NoRecoil(Entity entity, GameObject authoringData, EntityManager entityManager)
        {
            if (!ModConfig.IsEnable(EnhanceCategory.Misc, EC_Misc.NoRecoil))
                return;
            if (authoringData.TryGetComponent<WeaponAuthoring>(out var weapon))
            {
                //weapon.isMoveFreelyWeapon = true;
                ref var move = ref weapon.moveSpeedMultiplier;
                move = math.max(move, 1f);
            }
            /*if(authoringData.TryGetComponent<RangeWeaponAuthoring>(out var range))
            {
                range.recoilForce = 0;
            }*/
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
            /*else if (obj is WorkbenchDefinition workbenchDefinition)
            {
                EntityModule.AddModWorkbench(workbenchDefinition);
            }*/

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
