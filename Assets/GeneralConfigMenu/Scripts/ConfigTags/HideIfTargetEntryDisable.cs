using CoreLib.Data.Configuration;

namespace Assets.GeneralConfigMenu.Scripts.ConfigTags
{
    public class HideIfTargetEntryDisable
    {
        public readonly ConfigEntry<bool> Target;
        public HideIfTargetEntryDisable(ConfigEntry<bool> target)
        {
            Target = target;
        }
    }
}
