using PugMod;
using UnityEngine;

namespace Assets.TitanNoCD
{
    public class TitanNoCDMod : IMod
    {
        internal static ModConfig Config { get; private set; }
        public void EarlyInit()
        {
            Config = new();
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
