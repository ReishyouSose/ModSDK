using CoreLib.Data.Configuration;

namespace Assets.CoreEnhance.Scripts.Configs
{
    public class ConfigData
    {
        public ConfigEntry<bool> Switch { get; private set; }
        public ConfigEntryBase Value { get; private set; }
        public ConfigData(ConfigEntry<bool> @switch)
        {
            Switch = @switch;
        }
        public bool Enable => Switch.Value;
        public void SetValue(ConfigEntryBase value) => Value = value;
        public bool TryGetValue<T>(out ConfigEntry<T> value)
        {
            value = null;
            if (Value == null)
                return false;
            if (typeof(T) != Value.SettingType)
                return false;
            value = Value as ConfigEntry<T>;
            return true;
        }
    }
}
