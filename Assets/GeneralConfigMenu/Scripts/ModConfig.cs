using CoreLib.Data.Configuration;

namespace Assets.GeneralConfigMenu.Scripts
{
    public class ModConfig
    {
        public ConfigEntry<bool> ChangeClientWhenSync, AutoStoC, AutoCtoS, AdminOnly;
        public ModConfig()
        {
            ConfigFile config = new("GeneralConfigMenu/Config.cfg", true);
            ChangeClientWhenSync = config.Bind("Sync", nameof(ChangeClientWhenSync), false, string.Empty, ConfigAccessLevel.Client);
            AutoStoC = config.Bind("Manual", nameof(AutoStoC), false, string.Empty, ConfigAccessLevel.Client);
            AutoCtoS = config.Bind("Manual", nameof(AutoCtoS), false, string.Empty, ConfigAccessLevel.Client);
            AdminOnly = config.Bind("Manual", nameof(AdminOnly), false, string.Empty, ConfigAccessLevel.Admin);
        }
    }
}
