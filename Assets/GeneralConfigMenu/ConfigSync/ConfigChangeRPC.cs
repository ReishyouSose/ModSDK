using Unity.Collections;
using Unity.NetCode;

namespace Assets.GeneralConfigMenu.ConfigSync
{
    public struct ConfigDataRPC : IRpcCommand
    {
        public FixedString128Bytes data;
        public ConfigDataRPC(string data)
        {
            this.data = data;
        }
    }
}
