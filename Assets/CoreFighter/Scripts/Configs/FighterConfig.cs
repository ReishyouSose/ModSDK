using Assets.CoreEnhance.Scripts.Configs;
using CoreLib.Data.Configuration;
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Assets.CoreFighter.Scripts.Configs
{
    public class FighterConfig
    {
        private static FighterConfig Ins;
        private readonly Dictionary<(int, int), ConfigData> configs;
        private readonly Dictionary<float, AttackSpeedModifier> speedModifiers;
        private readonly ConfigEntry<bool> EnableATKSpeedModifier;
        private readonly int count;
        public static bool ATKSpeedModifierIsEnable => Ins.EnableATKSpeedModifier?.Value ?? false;
        internal static void Load() => Ins = new();
        public FighterConfig()
        {
            configs = new();
            ConfigFile file = new("CoreFighter/Switch.cfg", true);
            foreach (var category in Enum.GetValues(typeof(FighterCategory)))
            {
                int categoryIndex = (int)category;
                string section = category.ToString();
                foreach (var key in Enum.GetValues(categoryIndex switch
                {
                    0 => typeof(FC_Infinity),
                    1 => typeof(FC_Equip),
                    2 => typeof(FC_Misc),
                    _ => null
                }))
                {
                    int keyIndex = (int)key;
                    ConfigDefinition def = new(section, key.ToString());
                    configs.Add((categoryIndex, keyIndex), new(file.Bind(def, true, null, new())));
                }
            }
            AddValue(new("CoreFighter/Value.cfg", true));

            speedModifiers = new();
            ConfigFile atkSpeed = new("CoreFighter/AttackSpeed.cfg", true);
            EnableATKSpeedModifier = atkSpeed.Bind(new("General", "MainSwitch"),
                false, null, new(ConfigAccessLevel.Admin));
            List<float> target = new()
            {
                0.2f,
                0.3f,
                0.4f,
                0.5f,
                0.6f,
                0.7f,
                1.0f,
                1.5f,
                2.0f,
                5.0f
            };
            count = target.Count;
            foreach (var speed in target)
                speedModifiers[speed] = new(speed, atkSpeed);
        }

        private void AddValue(ConfigFile file)
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
        public static bool IsEnable(FighterCategory category, object ec)
        {
            if (!Ins.configs.TryGetValue(((int)category, (int)ec), out ConfigData entry))
                return false;
            return entry.Enable;
        }
        private bool TryAddValue<T>(ConfigFile file, FighterCategory category, object ec,
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
        public static bool TryGetValue<T>(FighterCategory category, object ec,
            out ConfigEntry<T> value, string key = "")
        {
            value = null;
            if (!Ins.configs.TryGetValue(((int)category, (int)ec), out var entry))
                return false;
            if (!entry.Enable)
                return false;
            return entry.TryGetValue(key, out value);
        }
        public static bool TryGetValues(FighterCategory category, object ec, out Dictionary<string, ConfigEntryBase> values)
        {
            values = null;
            if (Ins.configs.TryGetValue(((int)category, (int)ec), out var entry))
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
