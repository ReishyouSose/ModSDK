using CoreLib.Data.Configuration;

namespace Assets.GeneralConfigMenu.Scripts
{
    public class ModConfig
    {
        public ConfigEntry<bool> AdminOnly;
        public ConfigEntry<int> UIVersion;
        public ModConfig()
        {
            ConfigFile config = new("GeneralConfigMenu/Config.cfg", true);
            string general = "General";
            AdminOnly = config.Bind(general, nameof(AdminOnly), false, string.Empty, ConfigAccessLevel.Admin);
        }
    }
}
