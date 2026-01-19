using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.CoreEnhance.Scripts
{
    public class ModConfig
    {
        public static ModConfig Ins { get; private set;  }
        public ModAPIConfig Config;
        public ModConfig()
        {
            Config = new(PugMod.API.ConfigFilesystem);
            Config.re
        }
        public static void Load()
        {
            Ins = new();
        }
    }
}
