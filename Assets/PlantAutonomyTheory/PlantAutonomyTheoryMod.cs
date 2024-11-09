using PugMod;
using UnityEngine;

namespace Assets.PlantAutonomyTheory
{
    public class PlantAutonomyTheoryMod : IMod
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
