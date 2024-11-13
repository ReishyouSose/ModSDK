using CoreLib.Data.Configuration;

namespace Assets.InfiniteArena
{
    internal class ModConfig
    {
        private readonly ConfigEntry<float> chargeTime;
        private readonly ConfigEntry<int> checkInterval;
        public int ChargeTime { get; private set; }
        public int CheckInterval { get; private set; }
        public ModConfig()
        {
            ConfigFile file = new("InfiniteArena/config.cfg", true);
            chargeTime = file.Bind("General", nameof(chargeTime), 10f, "竞技场重激活充能时间（单位：分）\nArena re-actice charge times (in minutes)");
            checkInterval = file.Bind("General", nameof(checkInterval), 3, "检测竞技场状态的时间间隔（单位：秒）\nThe time interval for checking the state of the arena (in second)\n1 <= value");
            ChargeTime =/* (int)(chargeTime.Value * 1200)*/0;
            CheckInterval = checkInterval.Value;
        }
    }
}
