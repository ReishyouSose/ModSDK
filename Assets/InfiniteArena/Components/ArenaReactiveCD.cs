using Unity.Entities;
using Unity.NetCode;

namespace Assets.InfiniteArena.Components
{
    public struct ArenaReactiveCD : IComponentData
    {
        public const int StyleMax = 5;

        public int style;

        public float waitTime;

        public bool chargeCompleted;

        public bool checkCompleted;

    }
}
