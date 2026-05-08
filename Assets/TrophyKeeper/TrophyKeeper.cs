using CoreLib.Data.Configuration;
using PugMod;
using UnityEngine;

namespace Assets.TrophyKeeper
{

    public class TrophyKeeper : IMod
    {
        public static ConfigEntry<float> DropChance { get; private set; }
        public static ConfigEntry<bool> TrophyEntityDrop { get; private set;  }
        public void EarlyInit()
        {
            var config = new ConfigFile($"{nameof(TrophyKeeper)}/Config.cfg", true);
            DropChance = config.Bind("General", nameof(DropChance), 0.1f,
                new ConfigDescription(string.Empty, new AcceptableValueRange<float>(0.001f, 1f)));
            TrophyEntityDrop = config.Bind("General", nameof(TrophyEntityDrop), true);
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
