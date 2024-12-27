using CoreLib.Data.Configuration;

namespace Assets.THCompass
{
    public class ModConfig
    {
        private readonly ConfigEntry<int> minDrop, maxDrop, minRoll, maxRoll, guaranteed;
        public int MinDrop => minDrop.Value;
        public int MaxDrop => maxDrop.Value;
        public int MinRoll => minRoll.Value;
        public int MaxRoll => maxRoll.Value;
        public int Guaranteed => guaranteed.Value;
        public ModConfig()
        {
            ConfigFile config = new("THCompass/Config.cfg", true);
            var server = ConfigAccessLevel.Server;
            string section = "FromBoss";
            minDrop = config.Bind(section, nameof(minDrop), 1, "单卷最小掉落量。\nMin drop amount per roll.\n(1 <= minDrop <= maxDrop)", server, true);
            maxDrop = config.Bind(section, nameof(maxDrop), 3, "单卷最大掉落量。\n小于1时将不掉落罗盘\nMax drop amount per roll.\nWill not drop when less than 1.", server, true);

            section = "FromCompass";
            minRoll = config.Bind(section, nameof(minRoll), 7, "最小掉落卷数。\nMin drop rolls.\n(1 <= minRoll <= maxRoll)", server, true);
            maxRoll = config.Bind(section, nameof(maxRoll), 7, "最大掉落卷数。\nMax drop rolls.\n(1 <= maxRoll)", server, true);
            guaranteed = config.Bind(section, nameof(guaranteed), 1, "保底掉落量。\n小于1时不掉落保底\nGuaranteed drop amount.\nWill not drop when less than 1.", server, true);
        }
    }
}
