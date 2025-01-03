using Assets.CoreEnhance.Scripts.Configs;
using Assets.CoreEnhance.Scripts.Systems.Misc;
using CoreLib;
using CoreLib.RewiredExtension;
using CoreLib.Submodules.ModEntity;
using CoreLib.UserInterface;
using CoreLib.Util.Extensions;
using PugMod;
using Rewired;
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

        private void Authoring_OnObjectTypeAdded(Unity.Entities.Entity entity, GameObject authoringData, Unity.Entities.EntityManager entityManager)
        {
            if (authoringData.GetEntityObjectID() == ObjectID.BatMinion)
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
            Player rewiredPlayer = p.inputModule.rewiredPlayer;
            if (rewiredPlayer.GetButtonDown(ModKeyBind.QuickStack))
            {
                QuickStackClient.SendRequest();
            }
        }
    }
}
