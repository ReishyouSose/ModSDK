using CoreLib.Data.Configuration;
using Unity.Mathematics;

namespace Assets.AutoSalvage
{
    internal class ModConfig
    {
        private readonly ConfigEntry<int> checkCount;
        private readonly ConfigEntry<float> salvageTime;
        public int CheckCount { get; private set; }
        public int SalvageTime { get; private set; }
        public ModConfig()
        {
            ConfigFile file = new("AutoSalvage/config.cfg", true);
            checkCount = file.Bind("General", nameof(checkCount), 6, "检测数量，拆解台内物品数量大于等于检测数量时开始进行自动拆解倒计时\nDetection quantity, when the number of items in the salvage station is greater than or equal to the detection quantity, the automatic salvage countdown begins\n(1 <= value <= 6)");
            salvageTime = file.Bind("General", nameof(salvageTime), 3f, "自动拆解计时（单位：秒）\nAutomatic salvage countdown (in second)");
            CheckCount = math.clamp(checkCount.Value, 1, 6);
            SalvageTime = (int)(salvageTime.Value * 20);
        }
    }
}
