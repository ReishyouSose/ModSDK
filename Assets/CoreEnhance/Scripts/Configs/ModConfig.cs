using CoreLib.Data.Configuration;
using System;
using System.Collections.Generic;

namespace Assets.CoreEnhance.Scripts.Configs
{
    public class ModConfig
    {
        private static ModConfig Ins;
        private readonly Dictionary<(int, int), ConfigData> configs;
        internal static void Load() => Ins = new();
        public ModConfig()
        {
            configs = new();
            ConfigFile file = new("CoreEnhance/config.cfg", true);
            ConfigScope scope = new();
            foreach (var category in Enum.GetValues(typeof(EnhanceCategory)))
            {
                int categoryIndex = (int)category;
                string section = category.ToString();
                foreach (var key in Enum.GetValues(categoryIndex switch
                {
                    0 => typeof(EC_Infinity),
                    1 => typeof(EC_Accelerate),
                    2 => typeof(EC_Industry),
                    _ => null
                }))
                {
                    int keyIndex = (int)key;
                    ConfigDefinition def = new(section, key.ToString());
                    configs.Add((categoryIndex, keyIndex), new(file.Bind(def, true, null, scope)));
                }
            }
            SetValue();
        }

        private void SetValue()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="category"></param>
        /// <param name="ec">具体条目</param>
        /// <param name="entry"></param>
        /// <returns>查询失败或条目未启用均返回false</returns>
        public static bool TryGetEnable(EnhanceCategory category, object ec)
        {
            if (!Ins.configs.TryGetValue(((int)category, (int)ec), out ConfigData entry))
                return false;
            return entry.Enable;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="category"></param>
        /// <param name="ec">具体条目</param>
        /// <param name="value"></param>
        /// <returns>查询失败或条目未启用均返回false</returns>
        public static bool TryGetValue<T>(EnhanceCategory category, object ec, out ConfigEntry<T> value)
        {
            value = null;
            if (!Ins.configs.TryGetValue(((int)category, (int)ec), out var entry))
                return false;
            if (!entry.Enable)
                return false;
            return entry.TryGetValue(out value);
        }
    }
}
