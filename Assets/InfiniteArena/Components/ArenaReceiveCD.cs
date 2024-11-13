using Unity.Entities;
using Unity.Mathematics;

namespace Assets.InfiniteArena.Components
{
    public struct ArenaReceiveCD : IComponentData
    {
        public int messageID;
        public int2 local;
        //统一由服务器通知客户端竞技场状态变动信息
    }
}
