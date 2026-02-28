using Assets.GeneralConfigMenu.Scripts;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.GeneralConfigMenu.ConfigSync
{
    public struct ConfigDataRPC : IRpcCommand
    {
        public FixedString128Bytes data;
        public int playerIndex;
        public ConfigDataRPC(string data, int? index = null)
        {
            playerIndex = index ?? Manager.main.player.playerIndex;
            this.data = data;
        }
        public void TryChangeConfig()
        {
            ConfigManager.Instance.TryReceiveSync(playerIndex, data.Value);
        }
    }

    public struct JoinRequest : IRpcCommand
    {
    }
}
