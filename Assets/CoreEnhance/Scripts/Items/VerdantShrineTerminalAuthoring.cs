using Assets.CoreEnhance.Scripts.Buffers;
using PugConversion;
using Unity.Entities;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Items
{
    public class VerdantShrineTerminalAuthoring : MonoBehaviour
    {

    }

    public struct VerdantShrineTerminalCD : IComponentData
    {

    }

    public class VerdantShrineTerminalConverter : SingleAuthoringComponentConverter<VerdantShrineTerminalAuthoring>
    {
        protected override void Convert(VerdantShrineTerminalAuthoring authoring)
        {
            AddComponentData(new VerdantShrineTerminalCD());
            EnsureHasComponent<DistanceToPlayerCD>();
            EnsureHasBuffer<VerdantShrineBuffer>();
        }
    }
}
