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
        public int Nature;
        public int Sea;
        public int Desert;
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
