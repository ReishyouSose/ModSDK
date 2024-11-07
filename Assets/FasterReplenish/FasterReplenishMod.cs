using Assets.FasterReplenish;
using PugMod;
using UnityEngine;

namespace Assets.FasterReplenish
{
    public class FasterReplenishMod : IMod
    {
        private static ModConfig config;
        internal static ModConfig Config => config ??= new();
        public void EarlyInit()
        {
        }

        public void Init()
        {
        }

        public void ModObjectLoaded(Object obj)
        {
        }

        public void Shutdown()
        {
        }

        public void Update()
        {
        }
    }
}
