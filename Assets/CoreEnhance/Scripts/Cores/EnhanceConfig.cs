using Assets.GeneralConfigMenu.Scripts;
using CoreLib.Data.Configuration;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Cores
{
    public class EnhanceConfig
    {
        internal static EnhanceConfig Ins => ins ??= new();
        private static EnhanceConfig ins;
        private readonly Dictionary<EnhanceCategory, ConfigData> configs;
        internal static void Load() => ins ??= new();
        public EnhanceConfig()
        {
            configs = new();
            ConfigFile file = new("CoreEnhance/Switch.cfg", true);
            string section = string.Empty;
            foreach (var category in Enum.GetValues(typeof(EnhanceCategory)))
            {
                EnhanceCategory index = (EnhanceCategory)category;
                string function = category.ToString();
                if (function.StartsWith('_'))
                {
                    section = function[1..];
                    continue;
                }
                ConfigDefinition def = new(section, function);
                ConfigScope scope = null;
                bool _default = true;
                switch (index)
                {
                    case EnhanceCategory.Boulder:
                    case EnhanceCategory.Titan:
                    case EnhanceCategory.Crafting:
                    case EnhanceCategory.FishingNetNoCritter:
                    case EnhanceCategory.FishingNetCanGetItem:
                        _default = false;
                        break;
                    case EnhanceCategory.RollSkill:
                        _default = false;
                        scope = new(ConfigAccessLevel.Admin, false);
                        break;
                    case EnhanceCategory.Level:
                        _default = false;
                        scope = new(ConfigAccessLevel.Admin, false);
                        break;
                    case EnhanceCategory.GoldenPlantToSeed:
                        _default = false;
                        scope = new(ConfigAccessLevel.Server, true);
                        break;
                }
                configs.Add(index, new(file.Bind(def, _default, null, scope ?? new())));
            }
            AddValue(new("CoreEnhance/Value.cfg", true));
        }

        private void AddValue(ConfigFile file)
        {
            TryAddValue(file, EnhanceCategory.Arena, 1000, new AcceptableValueRange<int>(100, 9999));
            TryAddValue(file, EnhanceCategory.Merchant, 0, new AcceptableValueRange<int>(0, 3500));
            TryAddValue(file, EnhanceCategory.Titan, 5, new AcceptableValueRange<int>(5, 300));
            TryAddValue(file, EnhanceCategory.Crafting, true, null, "Animals");
            TryAddValue(file, EnhanceCategory.Crafting, true, null, "FishingNet");
            TryAddValue(file, EnhanceCategory.FishingNetCanGetItem, 0.4f, new AcceptableValueRange<float>(0, 0.5f));
            TryAddValue(file, EnhanceCategory.GoldenPlantToSeed, 8, new AcceptableValueRange<int>(0, 8));
            TryAddValue(file, EnhanceCategory.Level, 10, new AcceptableValueRange<int>(1, 100));
            TryAddValue(file, EnhanceCategory.ChainMining, true, null, "Adsorption");
            TryAddValue(file, EnhanceCategory.ChainMining, true, null, "NeedPlayer");
            TryAddValue(file, EnhanceCategory.ChainMining, true, null, "GiveExp");
            TryAddValue(file, EnhanceCategory.ModifySledgeRange, 2.5f, new AcceptableValueRange<float>(1.4f, 5f));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="category"></param>
        /// <param name="ec">具体条目</param>
        /// <param name="entry"></param>
        /// <returns>查询失败或条目未启用均返回false</returns>
        public static bool IsEnable(EnhanceCategory category)
        {
            if (!Ins.configs.TryGetValue(category, out ConfigData entry))
                return false;
            return entry.Enable;
        }
        private bool TryAddValue<T>(ConfigFile file, EnhanceCategory category,
            T defaultV, AcceptableValueBase accept = null, string key = "")
        {
            if (configs.TryGetValue(category, out ConfigData entry))
            {
                ConfigDefinition def = entry.Switch.Definition;
                ConfigScope scope = entry.Switch.Scope;
                entry.SetValue(key, file.Bind(new(def.Section, def.Key + key),
                    defaultV, new(string.Empty, accept), scope ?? new()));
                return true;
            }
            Debug.Log($"Can't find {category} config");
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
        public static bool TryGetValue<T>(EnhanceCategory category, out ConfigEntry<T> value, string key = "")
        {
            value = null;
            if (!Ins.configs.TryGetValue(category, out var entry))
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
        public static bool TryGetValues(EnhanceCategory category, out Dictionary<string, ConfigEntryBase> values)
        {
            values = null;
            if (!Ins.configs.TryGetValue(category, out var entry))
                return false;
            if (!entry.Enable)
                return false;
            values = entry.Values;
            return true;
        }
    }
}
