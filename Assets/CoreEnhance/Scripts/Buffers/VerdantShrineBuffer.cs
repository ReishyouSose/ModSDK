using Assets.CoreEnhance.Scripts.Items;
using Unity.Entities;
using Unity.Transforms;

namespace Assets.CoreEnhance.Scripts.Buffers
{
    public struct VerdantShrineBuffer : IBufferElementData
    {
        public int radiums;
        public bool Nature;
        public bool Sea;
        public bool Desert;
        public LocalTransform trans;
    }
}
