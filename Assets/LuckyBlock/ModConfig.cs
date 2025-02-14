using CoreLib.Data.Configuration;

namespace Assets.LuckyBlock
{
    public class ModConfig
    {
        internal static ModConfig Ins { get; private set; }
        public readonly ConfigEntry<int> SceneChance, DropChance, MaxStack;
        public readonly ConfigEntry<float> Equip, NPC, Food, Misc;
        public readonly ConfigEntry<bool> ChallengeMode, NoMultiBoss;
        public ModConfig()
        {
            ConfigFile file = new("LuckyBlock/config.cfg", true);
            SceneChance = file.Bind(new ConfigDefinition("General", nameof(SceneChance)), 1,
                new("生成随机场景的概率\nChance for random scene", new AcceptableValueRange<int>(0, 100)),
                new ConfigScope(ConfigAccessLevel.Admin));
            DropChance = file.Bind(new ConfigDefinition("General", nameof(DropChance)), 5,
                new("幸运方块掉落率\nChance for drop lucky block", new AcceptableValueRange<int>(0, 100)),
                new ConfigScope(ConfigAccessLevel.Admin));
            MaxStack = file.Bind(new ConfigDefinition("General", nameof(MaxStack)), 10,
                new("幸运方块内含物的最大数量（不影响NPC）\nMax stack for the contents from lucky block (doesn't affect NPCs)",
                new AcceptableValueRange<int>(1, 9999), new ConfigScope(ConfigAccessLevel.Admin, false)));
            ChallengeMode = file.Bind("General", nameof(ChallengeMode), false,
                "将所有掉落物替换为幸运方块\nReplaces all drops with lucky blocks", ConfigAccessLevel.Admin, true);
            NoMultiBoss = file.Bind("General", nameof(NoMultiBoss), true,
                "开出Boss时不出现复数个\nNot appear multiper bosses when opening", ConfigAccessLevel.Admin, false);

            string empty = string.Empty;
            Equip = file.Bind("Weight", nameof(Equip), 1f, empty, ConfigAccessLevel.Admin, false);
            NPC = file.Bind("Weight", nameof(NPC), 1f, empty, ConfigAccessLevel.Admin, false);
            Food = file.Bind("Weight", nameof(Food), 1f, empty, ConfigAccessLevel.Admin, false);
            Misc = file.Bind("Weight", nameof(Misc), 1f, empty, ConfigAccessLevel.Admin, false);
        }
        public static void Load()
        {
            Ins = new();
        }
    }
}
