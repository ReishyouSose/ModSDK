using Assets.GeneralConfigMenu.Scripts;
using CoreLib.Data.Configuration;
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Assets.CoreFighter.Scripts.Cores
{
    public class FighterConfig
    {
        private static FighterConfig Ins;
        private readonly Dictionary<FighterCategory, ConfigData> configs;
        private readonly Dictionary<float, AttackSpeedModifier> speedModifiers;
        private readonly ConfigEntry<bool> EnableATKSpeedModifier;
        private readonly int count;
        public static bool ATKSpeedModifierIsEnable => Ins.EnableATKSpeedModifier?.Value ?? false;
        internal static void Load() => Ins = new();
        public FighterConfig()
        {
            configs = new();
            ConfigFile file = new("CoreFighter/Switch.cfg", true);
            string section = string.Empty;
            foreach (var category in Enum.GetValues(typeof(FighterCategory)))
            {
                FighterCategory index = (FighterCategory)category;
                string function = category.ToString();
                if (function.StartsWith('_'))
                {
                    section = function[1..];
                    continue;
                }
                ConfigDefinition def = new(section, function);
                configs.Add(index, new(file, def, true, new()));
            }
            AddValue(new("CoreFighter/Value.cfg", true));

            speedModifiers = new();
            ConfigFile atkSpeed = new("CoreFighter/AttackSpeed.cfg", true);
            EnableATKSpeedModifier = atkSpeed.Bind(new("General", "MainSwitch"),
                false, null, new(ConfigAccessLevel.Admin));
            List<string> target = new()
            {
                "0.125",
                "0.2",
                "0.25",
                "0.3",
                "0.4",
                "0.5",
                "0.6",
                "0.7",
                "1.0",
                "1.5",
                "2.0",
                "5.0"
            };
            count = target.Count;
            foreach (var s in target)
            {
                var speed = float.Parse(s);
                speedModifiers[speed] = new(s,speed, atkSpeed);
            }
        }

        private void AddValue(ConfigFile file)
        {
            TryAddValue(file, FighterCategory.MapMarkerTeleport, true, null);
            TryAddValue(file, FighterCategory.Vampire, 0.01f, new AcceptableValueRange<float>(0.01f, 1f));
            TryAddValue(file, FighterCategory.EnableAllPreset, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="category"></param>
        /// <param name="ec">具体条目</param>
        /// <param name="entry"></param>
        /// <returns>查询失败或条目未启用均返回false</returns>
        public static bool IsEnable(FighterCategory category)
        {
            if (!Ins.configs.TryGetValue(category, out ConfigData entry))
                return false;
            return entry.Enable;
        }
        private bool TryAddValue<T>(ConfigFile file, FighterCategory category,
            T defaultV, AcceptableValueBase accept = null, string key = "")
        {
            if (configs.TryGetValue(category, out ConfigData entry))
            {
                ConfigDefinition def = entry.Switch.Definition;
                entry.SetValue(key, file.Bind(new(def.Section, def.Key + key),
                    defaultV, new(string.Empty, accept), new()));
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
        public static bool TryGetValue<T>(FighterCategory category,
            out ConfigEntry<T> value, string key = "")
        {
            value = null;
            if (!Ins.configs.TryGetValue(category, out var entry))
                return false;
            if (!entry.Enable)
                return false;
            return entry.TryGetValue(key, out value);
        }
        public static bool TryGetValues(FighterCategory category, out Dictionary<string, ConfigEntryBase> values)
        {
            values = null;
            if (Ins.configs.TryGetValue(category, out var entry))
                values = entry.Values;
            return values != null;
        }
        public static NativeHashMap<float, ATKSpeedModifer> GetModifiers()
        {
            NativeHashMap<float, ATKSpeedModifer> modifiers = new(Ins.count, Allocator.Temp);
            foreach (var (speed, modifier) in Ins.speedModifiers)
            {
                modifiers[speed] = modifier.ToStruct();
            }
            return modifiers;
        }
    }
}
