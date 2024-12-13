using CoreLib.Data.Configuration;
using Unity.Mathematics;

namespace Assets.InfiniteArena
{
    internal class ModConfig
    {
        public ConfigEntry<int> Coin { get;private set; }
        public ModConfig()
        {
            ConfigFile file = new("InfiniteArena/config.cfg", true);
            ConfigDescription desc = new("竞技场重生需要多少钱币\nHow much coins for arena respawn", new AcceptableValueRange<int>(100, 9999));
            Coin = file.Bind("General", "coin", 1000, desc);
        }
    }
}
