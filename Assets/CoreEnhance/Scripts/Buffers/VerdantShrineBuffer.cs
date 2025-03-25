using Assets.CoreEnhance.Scripts.Items;
using Unity.Entities;
using Unity.Transforms;

namespace Assets.CoreEnhance.Scripts.Buffers
{
    public struct VerdantShrineBuffer : IBufferElementData
    {
        public int radiums;
        public int Nature;
        public int Sea;
        public int Desert;
        public LocalTransform trans;
    }
}
