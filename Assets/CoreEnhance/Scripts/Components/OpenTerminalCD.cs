using Unity.Entities;
using Unity.NetCode;

namespace Assets.CoreEnhance.Scripts.Components
{
    public struct OpenTerminalCD : IComponentData
    {
        public Entity Player;
        public int TerminalType;
        public OpenTerminalCD(Entity player, TerminalType terminal)
        {
            Player = player;
            TerminalType = (int)terminal;
        }
    }
    public struct OpenTerminalRPC : IRpcCommand
    {
        public Entity Player;
        public int TerminalType;
        public OpenTerminalRPC(Entity player, TerminalType terminal)
        {
            Player = player;
            TerminalType = (int)terminal;
        }
    }
    public enum TerminalType
    {
        AutoFisher,
        VerdantShrine
    }
}
