using CoreLib.Data.Configuration;

namespace Assets.GeneralConfigMenu.Scripts
{
    public class ModConfig
    {
        public ConfigEntry<bool> ChangeClientWhenSync, AutoSave, AdminOnly;
        public ModConfig()
        {
            ConfigFile config = new("GeneralConfigMenu/Config.cfg", true);
            ChangeClientWhenSync = config.Bind("Sync", nameof(ChangeClientWhenSync), false, string.Empty, ConfigAccessLevel.Client);
            AutoSave = config.Bind("Manual", nameof(AutoSave), false, string.Empty, ConfigAccessLevel.Client);
            AdminOnly = config.Bind("Manual", nameof(AdminOnly), false, string.Empty, ConfigAccessLevel.Admin);
        }
    }
}
