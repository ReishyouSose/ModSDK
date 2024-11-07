using CoreLib.Data.Configuration;

namespace Assets.FasterReplenish
{
    internal class ModConfig
    {
        public readonly ConfigEntry<int> refreshTime;
        public ModConfig()
        {
            ConfigFile config = new("FasterReplenish/Config.cfg", true);
            refreshTime = config.Bind("General", nameof(refreshTime), 0, "商人补货时间间隔，单位为秒\n设置为0则使用“有物品售罄立即补货”模式\nThe merchant's replenishment time interval, in seconds\nSet to 0 to use the \"replenish immediately if any item is sold out\" mode\n60 <= value <= 1500 (1~15 minutes)");
        }
    }
}
