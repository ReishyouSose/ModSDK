using PugConversion;
using Unity.Entities;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Items
{
    public class VerdantShrineAuthoring : MonoBehaviour
    {
        public int radiums = 16;
    }
    public struct VerdantShrineCD : IComponentData
    {
        public int radiums;
        public bool Nature;
        public bool Sea;
        public bool Desert;
        public float timer;
    }

    public class VerdantShrineConverter : SingleAuthoringComponentConverter<VerdantShrineAuthoring>
    {
        protected override void Convert(VerdantShrineAuthoring authoring)
        {
            AddComponentData(new VerdantShrineCD()
            {
                radiums = authoring.radiums,
                timer = 1
            });
        }
    }
}
