using PugConversion;
using Unity.Entities;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Items
{
    public class AutoFisherAuthoring : MonoBehaviour
    {
    }
    public struct AutoFisherCD : IComponentData
    {

    }
    public class AutoFisherConverter : SingleAuthoringComponentConverter<AutoFisherAuthoring>
    {
        protected override void Convert(AutoFisherAuthoring authoring)
        {
            AddComponentData(new AutoFisherCD());
        }
    }
}
