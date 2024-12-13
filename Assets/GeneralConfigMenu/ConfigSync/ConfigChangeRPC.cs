using Assets.GeneralConfigMenu.MonoBehaivours;
using CoreLib.Data.Configuration;
using Unity.Collections;
using Unity.NetCode;

namespace Assets.GeneralConfigMenu.ConfigSync
{
    public struct ConfigChangeRPC : IRpcCommand
    {
        public FixedString32Bytes mod;
        public FixedString32Bytes file;
        public FixedString32Bytes section;
        public FixedString32Bytes key;
        public FixedString32Bytes value;
        public int playerIndex;
        public ConfigChangeRPC(string mod, string file, ConfigDefinition def, string value)
        {
            playerIndex = Manager.main.player.playerIndex;
            this.mod = mod;
            this.file = file;
            section = def.Section;
            key = def.Key;
            this.value = value;
        }
        public void TryChangeConfig()
        {
            ModConfigMenu.Ins.TryRecieveSync(playerIndex, mod.Value, file.Value, section.Value, key.Value, value.Value);
        }
    }
}
