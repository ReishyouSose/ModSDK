using CoreLib.Data.Configuration;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Configs
{
    public class EnhanceConfig
    {
        private static EnhanceConfig Ins;
        private readonly Dictionary<(int, int), ConfigData> configs;
        internal static void Load() => Ins = new();
        public EnhanceConfig()
        {
            configs = new();
            ConfigFile file = new("CoreEnhance/Switch.cfg", true);
            foreach (var category in Enum.GetValues(typeof(EnhanceCategory)))
            {
                int categoryIndex = (int)category;
                string section = category.ToString();
                foreach (var key in Enum.GetValues(categoryIndex switch
                {
                    0 => typeof(EC_Infinity),
                    1 => typeof(EC_Accelerate),
                    2 => typeof(EC_Automation),
                    3 => typeof(EC_Misc),
                    _ => null
                }))
                {
                    int keyIndex = (int)key;
                    ConfigDefinition def = new(section, key.ToString());
                    configs.Add((categoryIndex, keyIndex), new(file.Bind(def, true, null, new())));
                }
            }
            AddValue(new("CoreEnhance/Value.cfg", true));
        }

        private void AddValue(ConfigFile file)
        {
            TryAddValue(file, EnhanceCategory.Infinity, EC_Infinity.Arena, 1000, new AcceptableValueRange<int>(100, 9999));
            TryAddValue(file, EnhanceCategory.Accelerate, EC_Accelerate.Merchant, 0, new AcceptableValueRange<int>(0, 3500));
            TryAddValue(file, EnhanceCategory.Accelerate, EC_Accelerate.Titan, 5, new AcceptableValueRange<int>(5, 300));
            TryAddValue(file, EnhanceCategory.Accelerate, EC_Accelerate.Crafting, false, null, "Animals");
            TryAddValue(file, EnhanceCategory.Automation, EC_Automation.Salvage, 6, new AcceptableValueList<int>(1, 2, 3, 4, 5, 6), "Amount");
            TryAddValue(file, EnhanceCategory.Automation, EC_Automation.Salvage, 5, new AcceptableValueRange<int>(0, 60), "Timer");
            TryAddValue(file, EnhanceCategory.Automation, EC_Automation.GiveExp, 100, new AcceptableValueRange<int>(0, 100), "Gardening");
            TryAddValue(file, EnhanceCategory.Automation, EC_Automation.GiveExp, 10, new AcceptableValueRange<int>(0, 100), "Fishing");
            TryAddValue(file, EnhanceCategory.Misc, EC_Misc.ChainMining, true, null, "Adsorption");
            TryAddValue(file, EnhanceCategory.Misc, EC_Misc.ChainMining, true, null, "NeedPlayer");
            TryAddValue(file, EnhanceCategory.Automation, EC_Automation.Plant, 10, new AcceptableValueRange<int>(10, 100), "Nature");
            TryAddValue(file, EnhanceCategory.Automation, EC_Automation.Plant, 10, new AcceptableValueRange<int>(10, 100), "Sea");
            TryAddValue(file, EnhanceCategory.Automation, EC_Automation.Plant, 10, new AcceptableValueRange<int>(10, 100), "Desert");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="category"></param>
        /// <param name="ec">具体条目</param>
        /// <param name="entry"></param>
        /// <returns>查询失败或条目未启用均返回false</returns>
        public static bool IsEnable(EnhanceCategory category, object ec)
        {
            if (!Ins.configs.TryGetValue(((int)category, (int)ec), out ConfigData entry))
                return false;
            return entry.Enable;
        }
        private bool TryAddValue<T>(ConfigFile file, EnhanceCategory category, object ec,
            T defaultV, AcceptableValueBase accept = null, string key = "")
        {
            if (configs.TryGetValue(((int)category, (int)ec), out ConfigData entry))
            {
                ConfigDefinition def = entry.Switch.Definition;
                entry.SetValue(key, file.Bind(new(def.Section, def.Key + key),
                    defaultV, new(string.Empty, accept), new()));
                return true;
            }
            Debug.Log($"Can't find {category} {ec} config");
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="category"></param>
        /// <param name="ec">具体条目</param>
        /// <param name="value"></param>
        /// <returns>查询失败或条目未启用均返回false</returns>
        public static bool TryGetValue<T>(EnhanceCategory category, object ec, out ConfigEntry<T> value, string key = "")
        {
            value = null;
            if (!Ins.configs.TryGetValue(((int)category, (int)ec), out var entry))
                return false;
            if (!entry.Enable)
                return false;
            return entry.TryGetValue(key, out value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="category"></param>
        /// <param name="ec"></param>
        /// <param name="values">Extra Configs</param>
        /// <returns>查询失败或条目未启用均返回false</returns>
        public static bool TryGetValues(EnhanceCategory category, object ec,
            out Dictionary<string, ConfigEntryBase> values)
        {
            values = null;
            if (!Ins.configs.TryGetValue(((int)category, (int)ec), out var entry))
                return false;
            if (!entry.Enable)
                return false;
            values = entry.Values;
            return true;
        }
    }
}
