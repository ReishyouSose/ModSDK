using Assets.CoreEnhance.Scripts.Configs;
using CoreLib;
using CoreLib.Submodules.ModEntity;
using CoreLib.UserInterface;
using CoreLib.Util.Extensions;
using PugMod;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts
{
    public class Main : IMod
    {
        public void EarlyInit()
        {
            ModConfig.Load();
            CoreLibMod.LoadModules(typeof(EntityModule), typeof(UserInterfaceModule));
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
        }
    }
}
