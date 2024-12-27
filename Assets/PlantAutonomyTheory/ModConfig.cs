using CoreLib.Data.Configuration;
using System.Text;
using Unity.Mathematics;

namespace Assets.PlantAutonomyTheory
{
    internal class ModConfig
    {
        private readonly ConfigEntry<bool> autoMode;
        private readonly ConfigEntry<int> harvestCycel;
        private readonly ConfigEntry<int> regrowthTimer;
        public bool AutoMode => autoMode.Value;
        public int HarvestCycel { get; private set; }
        public int HarvestCycel_Golden { get; private set; }
        public int RegrowthTimer { get; private set; }
        public ModConfig()
        {
            ConfigFile file = new("PlantAutonomyTheory/config.cfg", true);
            autoMode = file.Bind("General", nameof(autoMode), false, AutoModeDesc());
            harvestCycel = file.Bind("General", nameof(harvestCycel), 600, "收获周期（单位：秒）\n金色植物需要两倍的时间\nThe harvest cycel (in second)\nGolden plants take twice as long\n60 <= value");
            regrowthTimer = file.Bind("General", nameof(regrowthTimer), 200, "重新生长计时（单位：秒）\nThe regrowth timer (in second)\n0 <= value <= 600");
            HarvestCycel = math.max(harvestCycel.Value, 60);
            HarvestCycel_Golden = HarvestCycel * 2;
            RegrowthTimer = math.clamp(regrowthTimer.Value, 0, 600);
        }
        private static string AutoModeDesc()
        {
            var zh = new StringBuilder("自动模式")
                .AppendLine()
                .Append("以下描述均以“植物已完全成长且耕地湿润”为前提")
                .AppendLine()
                .Append("[启用状态]")
                .AppendLine()
                .Append("植物会在“重新生长计时”结束后掉落作物，然后回到种子阶段重新开始生长")
                .AppendLine()
                .Append("重新生长时有概率变为金色作物")
                .AppendLine()
                .Append("概率以服务器中“专业园丁”技能最高级的玩家为准")
                .AppendLine()
                .Append("[禁用状态]")
                .AppendLine()
                .AppendLine("植物每隔一个“收获周期”增加1收获量，这不会增加收获时得到的园艺经验值");
            var en = new StringBuilder("Auto Mode")
                .AppendLine()
                .Append("The following descriptions assume that \"the plant is fully grown and the arable is wet\"")
                .AppendLine()
                .Append("[Enabled]")
                .AppendLine()
                .Append("When \"regrowth timer\" ends, the plant will drop crops and return to the seed stage to restart growth")
                .AppendLine()
                .Append("There is a chance that the plant will become a golden plant when regrowth")
                .AppendLine()
                .Append("The probability is based on the player with the highest skill level of \"Expert Gardener\" in the server")
                .AppendLine()
                .Append("[Disabled]")
                .AppendLine()
                .Append("The plant will increase its harvest count by 1 every \"harvest cycle\", but this will not grant additional gardening experience upon harvest.");
            return zh + "\n" + en;
        }
    }
}
