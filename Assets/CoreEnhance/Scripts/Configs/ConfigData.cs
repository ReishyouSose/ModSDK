using CoreLib.Data.Configuration;

namespace Assets.CoreEnhance.Scripts.Configs
{
    public class ConfigData
    {
        private readonly ConfigEntry<bool> _switch;
        private ConfigEntryBase _value;
        public ConfigData(ConfigEntry<bool> @switch)
        {
            _switch = @switch;
        }
        public bool Enable => _switch.Value;
        public void SetValue(ConfigEntryBase value) => _value = value;
        public bool TryGetValue<T>(out ConfigEntry<T> value)
        {
            value = null;
            if (_value == null)
                return false;
            if (typeof(T) != _value.SettingType)
                return false;
            value = _value as ConfigEntry<T>;
            return true;
        }
    }
}
