using CoreLib.Data.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.CoreEnhance.Scripts.Configs
{
    public class ModConfig
    {
        private readonly Dictionary<(int, int), ConfigData> configs;
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
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="category"></param>
        /// <param name="ec">具体条目</param>
        /// <param name="entry"></param>
        /// <returns></returns>
        public bool TryGetEntry<T>(EnhanceCategory category, object ec, out ConfigData entry)
            => configs.TryGetValue(((int)category, (int)ec), out entry);
        private void SetValue()
        {

        }
    }
}
