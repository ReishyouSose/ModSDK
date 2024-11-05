using CoreLib.Data.Configuration;

namespace Assets.ImproveEquipment
{
    public class ModConfig
    {
        public ConfigEntry<bool> Sledge_Enable { get; private set; }
        public ConfigEntry<float> Sledge_Range { get; private set; }
        public ConfigEntry<float> Sledge_Speed { get; private set; }

        public ConfigEntry<bool> Ray_Enable { get; private set; }
        public ConfigEntry<float> Ray_MoveSpeed { get; private set; }
        private const string EnableSection = "是否启用这个部分？\nEenable this sction?";
        public ModConfig()
        {
            ConfigFile config = new("ImproveEquipment/Config.cfg", true);
            SledgeBind(config);
            RayBind(config);
        }
        private void SledgeBind(ConfigFile config)
        {
            const string section = "Sledge";
            Sledge_Enable = config.Bind(section, "Enable", true, EnableSection);
            Sledge_Range = config.Bind(section, "Range", 2.5f,
                 "大锤的攻击范围。\r\n原版是1.4（能攻击3格）。\r\n默认为2.5（能攻击5格）。\r\n不会低于1.4.\r\nThe attack range of the sledge hammer.\r\nThe original version is 1.4 (can attack 3 blocks).\r\nThe default is 2.5 (can attack 5 blocks).\r\nWill not be lower than 1.4.");
            Sledge_Speed = config.Bind(section, "AttackSpeed", 0.4f,
                "大锤攻击速度。\r\n原版是0.7（1.4次/秒）。\r\n默认是0.4（2.5次/秒）\r\n不会高于0.7.\r\n公式为  攻速 = 1 / 数值。\r\nSledge hammer attack speed.\r\nThe original version is 0.7 (1.4 times/second).\r\nThe default is 0.4 (2.5 times/second).\r\nIt will not be higher than 0.7.\r\nThe formula is attack speed = 1 / value.");
        }
        private void RayBind(ConfigFile config)
        {
            const string section = "Ray";
            Ray_Enable = config.Bind(section, "Enable", true, EnableSection);
            Ray_MoveSpeed = config.Bind(section, "MoveSpeed", 1f, "射线类武器的使用时移动速度倍数。\r\n0~1表示减速。\r\n1表示无效果。\r\n超过1表示使用时加速。\r\n速度过高会导致闪电链和武器纹理的位置偏移较大。\r\n此项不会小于或等于0，也不会大于2。\nMove speed ​​multiplier when using for ray-type weapons.\r\n0~1 means slowdown.\r\n1 means no effect.\r\nover 1 means speed up when used.\r\nToo high a speed will cause the lightning chain and weapon texture to have a higher position offset.\r\nThis item will not be less than or equal to 0 nor greater than 2.");
        }
    }
}
