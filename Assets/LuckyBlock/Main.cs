using CoreLib;
using CoreLib.Submodules.ModEntity;
using PugMod;
using UnityEngine;

namespace Assets.LuckyBlock
{
    public class Main : IMod
    {
        public void EarlyInit()
        {
            CoreLibMod.LoadModules(typeof(EntityModule));
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
