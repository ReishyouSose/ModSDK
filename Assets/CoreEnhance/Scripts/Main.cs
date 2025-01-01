using CoreLib;
using CoreLib.Submodules.ModEntity;
using CoreLib.UserInterface;
using PugMod;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts
{
    public class Main : IMod
    {
        public void EarlyInit()
        {
            CoreLibMod.LoadModules(typeof(EntityModule),typeof(UserInterfaceModule));
        }

        public void Init()
        {
        }

        public void ModObjectLoaded(Object obj)
        {
            if (obj is  GameObject gameObject)
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
