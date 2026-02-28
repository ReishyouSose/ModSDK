using CoreLib.Data.Configuration;
using System.Collections.Generic;

namespace Assets.GeneralConfigMenu.Scripts
{
    public class ConfigData
    {
        public ConfigEntry<bool> Switch { get; private set; }
        public Dictionary<string, ConfigEntryBase> Values { get; private set; }
        public ConfigData(ConfigEntry<bool> @switch)
        {
            Switch = @switch;
        }
        public bool Enable => Switch.Value;
        public bool SetValue(string key, ConfigEntryBase value)
        {
            Values ??= new();
            return Values.TryAdd(key, value);
        }
        public bool TryGetValue<T>(string key, out ConfigEntry<T> value)
        {
            value = null;
            if (Values?.TryGetValue(key, out var v) != true)
                return false;
            if (typeof(T) != v.SettingType)
                return false;
            value = v as ConfigEntry<T>;
            return true;
        }
    }
}
