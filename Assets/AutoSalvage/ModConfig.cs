using CoreLib.Data.Configuration;

namespace Assets.AutoSalvage
{
    internal class ModConfig
    {
        private readonly ConfigEntry<int> checkCount;
        private readonly ConfigEntry<float> salvageTime;
        public int CheckCount => checkCount.Value;
        public int SalvageTime => (int)(salvageTime.Value * 20);
        public ModConfig()
        {
            ConfigFile file = new("AutoSalvage/config.cfg", true);
            checkCount = file.Bind("General", nameof(checkCount), 6,
                new ConfigDescription("检测数量，拆解台内物品数量大于等于检测数量时开始进行自动拆解倒计时" +
                "\nDetection quantity," +
                " when the number of items in the salvage station is greater than or equal to the detection quantity," +
                " the automatic salvage countdown begins.",
                new AcceptableValueList<int>(1, 2, 3, 4, 5, 6)), new(ConfigAccessLevel.Server));
            salvageTime = file.Bind("General", nameof(salvageTime), 3f,
               new ConfigDescription("自动拆解倒计时（单位：秒）\nAutomatic salvage countdown (in second)",
               new AcceptableValueRange<float>(0, 600)), new(ConfigAccessLevel.Server));
        }
    }
}
