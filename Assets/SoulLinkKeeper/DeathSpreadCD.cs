using Unity.Entities;
using Unity.NetCode;

namespace Assets.SoulLinkKeeper
{
    [GhostComponent]
    public struct DeathSpreadCD : IComponentData
    {
        [GhostField]
        public int Server;
        public int Client;
    }

}
