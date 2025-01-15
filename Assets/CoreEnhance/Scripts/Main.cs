using Assets.CoreEnhance.Scripts.Configs;
using Assets.CoreEnhance.Scripts.Systems.Misc;
using CoreLib;
using CoreLib.RewiredExtension;
using CoreLib.Submodules.ModEntity;
using CoreLib.Submodules.ModEntity.Patches;
using CoreLib.UserInterface;
using CoreLib.Util.Extensions;
using PugMod;
using Rewired;
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
            RewiredExtensionModule.AddKeybind(ModKeyBind.QuickStack, "Quick Stack", KeyboardKeyCode.I, ModifierKey.Control);
            API.Authoring.OnObjectTypeAdded += Authoring_OnObjectTypeAdded;
        }

        private void Authoring_OnObjectTypeAdded(Entity entity, GameObject authoringData, EntityManager entityManager)
        {
            if (entityManager.HasBuffer<InventorySlotRequirementBuffer>(entity))
            {
                if (authoringData.GetEntityObjectID() != API.Authoring.GetObjectID("CoreEnhance:AutoFisher"))
                    return;
                var id = API.Authoring.GetObjectID("CoreEnhance:IndustrialBaitCan");
                var requires = entityManager.GetBuffer<InventorySlotRequirementBuffer>(entity);
                var req = requires[1];
                req.acceptsObjectIds[0] = id;
                requires[1] = req;

            }
            return;
            if (authoringData.GetEntityObjectID() == API.Authoring.GetObjectID("CoreEnhance:IndustrialBaitCan"))
            {
                int count = authoringData.GetComponentCount();
                for (int i = 0; i < count; i++)
                {
                    Debug.Log(authoringData.GetComponentAtIndex(i));
                }
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
        }
    }
}
