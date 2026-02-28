using CoreLib.Data.Configuration;

namespace Assets.GeneralConfigMenu.Scripts
{
    public class ModConfig
    {
        public ConfigEntry<bool> ChangeClientWhenSync, AutoStoC, AutoCtoS;
        public ModConfig()
        {
            ConfigFile config = new("GeneralConfigMenu/config.cfg", true);
            ChangeClientWhenSync = config.Bind("Sync", nameof(ChangeClientWhenSync), false,
                "When sync data from server, the client data will also be changed and save locally" +
                "\n当接收服务器的数据时，客户端的数据也将一同更改并保存到本地", ConfigAccessLevel.Client);
            AutoStoC = config.Bind("Manual", nameof(AutoStoC), false,
                "When set server data manually, the client data will also be changed and save locally" +
                "\n当手动设置服务器的数据时，客户端的数据也将一同更改并保存到本地", ConfigAccessLevel.Client);
            AutoCtoS = config.Bind("Manual", nameof(AutoCtoS), false,
                "When set client data manually, the server data will also be changed and sync" +
                "\n当手动设置客户端的数据时，服务器的数据也将一同更改并进行同步", ConfigAccessLevel.Client);
        }
    }
}
