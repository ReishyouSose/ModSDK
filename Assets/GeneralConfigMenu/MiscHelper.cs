using CoreLib.Data.Configuration;
using I2.Loc;
using System.Linq;
using System.Text;

namespace Assets.GeneralConfigMenu
{
    public static class MiscHelper
    {
        public static bool IsNeedReload(this ConfigEntryBase config)
            => config.Description.Tags.Any(x => x.ToString().ToLower().Contains("needreload"));
        public static void WirteNeedReloadText(this StringBuilder builder)
            => builder.Append("[!]")
                    .Append(LocalizationManager.GetTranslation("GeneralConfigMenu/NeedReload"))
                    .AppendLine();
    }
}
