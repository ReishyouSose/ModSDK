using PugConversion;
using Unity.Entities;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Components
{
    [RequireComponent(typeof(PlaceableObjectAuthoring))]
    public class UniquePlaceableAuthoring : MonoBehaviour
    {
    }
    public class UniquePlaceableConverter : SingleAuthoringComponentConverter<UniquePlaceableAuthoring>
    {
        public static int Count { get; private set; }
        protected override void Convert(UniquePlaceableAuthoring authoring)
        {
            Count++;
            AddComponentData(new UniquePlaceableCD());
        }
    }

    public struct UniquePlaceableCD : IComponentData
    {
        public bool init;
        public double placeTime;
    }
}
