using CoreLib.Data.Configuration;

namespace Assets.THCompass
{
    public class ModConfig
    {
        private ConfigEntry<int> minDrop, maxDrop, minRoll, maxRoll, guaranteed;
        public int MinDrop => minDrop.Value;
        public int MaxDrop => maxDrop.Value;
        public int MinRoll => minRoll.Value;
        public int MaxRoll => maxRoll.Value;
        public int Guaranteed => guaranteed.Value;
        public ModConfig()
        {
            ConfigFile config = new("THCompass/config.cfg", true);
            string section = "FromBoss";
            minDrop = config.Bind(section, nameof(minDrop), 1, "单卷最小掉落量。\nMin drop amount per roll.");
            maxDrop = config.Bind(section, nameof(maxDrop), 3, "单卷最大掉落量。\nMax drop amount per roll.");

            section = "FromCompass";
            minRoll = config.Bind(section, nameof(minRoll), 7, "最小掉落卷数。\nMin drop rolls");
            maxRoll = config.Bind(section, nameof(maxRoll), 7, "最大掉落卷数。\nMax drop rolls");
            guaranteed = config.Bind(section, nameof(guaranteed), 1, "保底掉落量。\nGuaranteed drop amount");
        }
    }
}
