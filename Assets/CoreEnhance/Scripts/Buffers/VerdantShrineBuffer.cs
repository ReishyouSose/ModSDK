using Assets.CoreEnhance.Scripts.Items;
using Unity.Entities;
using Unity.Transforms;

namespace Assets.CoreEnhance.Scripts.Buffers
{
    public struct VerdantShrineBuffer : IBufferElementData
    {
        public VerdantShrineCD shrine;
        public LocalTransform trans;
    }
}
