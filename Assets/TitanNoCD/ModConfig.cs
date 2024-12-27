using CoreLib.Data.Configuration;
using System.Text;

namespace Assets.TitanNoCD
{
    public class ModConfig
    {
        public ConfigEntry<float> SoulOrbDuration { get; private set; }
        public ConfigEntry<float> DetectionInterval { get; private set; }

        public ModConfig()
        {
            ConfigFile file = new("TitanNoCD/config.cfg", true);
            var server = ConfigAccessLevel.Server;
            StringBuilder builder = new();
            builder.Append("泰坦灵魂球的持续时间").AppendLine()
                .Append("原版为300s").AppendLine()
                .Append("The duration of the Titan Soul Orb").AppendLine()
                .Append("Original version was 300 seconds");
            SoulOrbDuration = file.Bind("General", nameof(SoulOrbDuration), 15f,
                new ConfigDescription(builder.ToString(), new AcceptableValueRange<float>(5, 300)), new ConfigScope(server));

            builder = new();
            builder.Append("泰坦生成的检测间隔").AppendLine()
                .Append("原版为每60s检测一次").AppendLine()
                .Append("注意，即使设置为0也会有大约10s的间隔").AppendLine()
                .Append("The detection interval of Titan spawn").AppendLine()
                .Append("The original version detects once every 60 seconds").AppendLine()
                .Append("Note that even if set to 0, there will still be an interval of about 10 seconds");
            DetectionInterval = file.Bind("General", nameof(DetectionInterval), 15f,
                new ConfigDescription(builder.ToString(), new AcceptableValueRange<float>(0, 60)), 
                new ConfigScope(server));
        }
    }
}
