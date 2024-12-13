using Unity.Entities;

namespace Assets.InfiniteArena
{
    public struct ArenaRecordCD : IComponentData
    {
        public float time;
        public bool chest;
    }
}
