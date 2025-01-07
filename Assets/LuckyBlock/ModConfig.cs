using CoreLib.Data.Configuration;

namespace Assets.LuckyBlock
{
    public class ModConfig
    {
        internal static ModConfig Ins { get; private set; }
        public readonly ConfigEntry<int> SceneChance, DropChance;
        public readonly ConfigEntry<bool> ChallengeMode;
        public ModConfig()
        {
            ConfigFile file = new("LuckyBlock/config.cfg", true);
            SceneChance = file.Bind(new ConfigDefinition("General", nameof(SceneChance)), 1,
                new("生成随机场景的概率\nChance for random scene", new AcceptableValueRange<int>(0, 100)),
                new ConfigScope(ConfigAccessLevel.Admin));
            DropChance = file.Bind(new ConfigDefinition("General", nameof(DropChance)), 5,
                new("幸运方块掉落率\nChance for drop lucky block", new AcceptableValueRange<int>(0, 100)),
                new ConfigScope(ConfigAccessLevel.Admin));
            ChallengeMode = file.Bind("General", nameof(ChallengeMode), false,
                "将所有掉落物替换为幸运方块\nReplaces all drops with lucky blocks", ConfigAccessLevel.Admin, true);
        }
        public static void Load()
        {
            Ins = new();
        }
    }
}
