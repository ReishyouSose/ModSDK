using PugMod;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Datas
{
    public static class QocObjectID
    {
        public static ObjectID ArenaScanner { get; private set; }
        public static ObjectID AutoFisher { get; private set; }
        public static ObjectID AutoFisherTerminal { get; private set; }
        public static ObjectID VerdantShrine { get; private set; }
        public static ObjectID VerdantShrineTerminal { get; private set; }
        public static ObjectID BoulderDemolish { get; private set; }

        internal static void Load()
        {
            var mod = CoreEnhanceMod.InternalName;
            var atr = API.Authoring;
            ArenaScanner = atr.GetObjectID(mod + nameof(ArenaScanner));
            AutoFisher = atr.GetObjectID(mod + nameof(AutoFisher));
            AutoFisherTerminal = atr.GetObjectID(mod + nameof(AutoFisherTerminal));
            VerdantShrine = atr.GetObjectID(mod + nameof(VerdantShrine));
            VerdantShrineTerminal = atr.GetObjectID(mod + nameof(VerdantShrineTerminal));
            BoulderDemolish = atr.GetObjectID(mod + nameof(BoulderDemolish));
        }
    }
}
