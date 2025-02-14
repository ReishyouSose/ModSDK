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

    public class VerdantShrineTerminalConverter : SingleAuthoringComponentConverter<VerdantShrineAuthoring>
    {
        protected override void Convert(VerdantShrineAuthoring authoring)
        {
            AddComponentData(new VerdantShrineTerminalCD());
        }
    }
}
