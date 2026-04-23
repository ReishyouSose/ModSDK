using Assets.GeneralConfigMenu.Scripts;
using CoreLib.Data.Configuration;
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Assets.CoreFighter.Scripts.Cores
{
    public class FighterConfig : CombindConfigPage
    {
        private static FighterConfig ins;
        private Dictionary<FighterCategory, ConfigEntry<bool>> configs;
        private Dictionary<float, AttackSpeedModifier> speedModifiers;
        private ConfigEntry<bool> EnableATKSpeedModifier;
        private int count;
        public static bool ATKSpeedModifierIsEnable => ins.EnableATKSpeedModifier.Value;

        public override string FilePath => "CoreFighter/Switch";

        public override void Init(ConfigFile file)
        {
            ins = this;
            configs = new();
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
                var entry = file.Bind(def, true);
                Add(entry, out _);
                configs.Add(index, entry);
            }
            AddValue();

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
                speedModifiers[speed] = new(s, speed, atkSpeed);
            }
        }

        private void AddValue()
        {
            TryAddValue(FighterCategory.MapMarkerTeleport, true, null);
            TryAddValue(FighterCategory.Vampire, 0.01f, new AcceptableValueRange<float>(0.01f, 1f));
            TryAddValue(FighterCategory.EnableAllPreset, true);
        }

        /// <returns>查询失败或条目未启用均返回false</returns>
        public static bool IsEnable(FighterCategory category)
        {
            if (!ins.configs.TryGetValue(category, out var entry))
                return false;
            return entry.Value;
        }
        private bool TryAddValue<T>(FighterCategory category, T defaultV, AcceptableValueBase accept = null, string key = null)
        {
            if (!ins.configs.TryGetValue(category, out var entry))
                return false;
            return TryAddValue(entry, defaultV, accept, key);
        }

        /// <returns>查询失败或条目未启用均返回false</returns>
        public static bool TryGetValue<T>(FighterCategory category, out ConfigEntry<T> value, string key =null)
        {
            value = null;
            if (!ins.configs.TryGetValue(category, out var entry))
                return false;
            return ins.TryGetValue(entry, out value, key);
        }

        /// <returns>查询失败或条目未启用均返回false</returns>
        public static bool TryGetValues(FighterCategory category, out Dictionary<string, ConfigEntryBase> values)
        {
            values = null;
            if (!ins.configs.TryGetValue(category, out var entry))
                return false;
            return ins.TryGetValues(entry, out values);
        }
        public static NativeHashMap<float, ATKSpeedModifer> GetModifiers()
        {
            NativeHashMap<float, ATKSpeedModifer> modifiers = new(ins.count, Allocator.Temp);
            foreach (var (speed, modifier) in ins.speedModifiers)
            {
                modifiers[speed] = modifier.ToStruct();
            }
            return modifiers;
        }
    }
}
