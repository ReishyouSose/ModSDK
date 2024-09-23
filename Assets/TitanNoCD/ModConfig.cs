using CoreLib.Data.Configuration;

namespace Assets.TitanNoCD
{
    public class ModConfig
    {
        public ConfigEntry<float> SoulOrbDuration { get; private set; }
        public ConfigEntry<float> DetectionInterval { get; private set; }

        public ModConfig()
        {
            ConfigFile file = new("TitanNoCD/config.cfg", true);
            SoulOrbDuration = file.Bind("General", nameof(SoulOrbDuration), 15f, "泰坦灵魂球的持续时间。\r\n原版为300s。\r\n默认为15s。\nThe duration of the Titan Soul Orb.\r\nOriginal version was 300 seconds.\r\nDefault is 15 seconds.\n(5 <= duration <= 300)");
            DetectionInterval = file.Bind("General", nameof(DetectionInterval), 15f, "泰坦生成的检测间隔。\r\n原版为每60s检测一次。\r\n默认为15s。\n注意，即使设置为0也会有大约10s的间隔。\nThe detection interval of Titan spawn.\r\nThe original version detects once every 60 seconds.\r\nThe default is 15 seconds.\nNote that even if it is set to 0, there will still be an interval of about 10 seconds.\n(0 <= interval <= 60)");
        }
    }
}
