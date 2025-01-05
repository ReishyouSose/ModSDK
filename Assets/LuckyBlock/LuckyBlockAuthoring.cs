using PugConversion;
using Unity.Entities;
using UnityEngine;

namespace Assets.LuckyBlock
{
    public class LuckyBlockAuthoring : MonoBehaviour
    {
    }
    public struct LuckyBlockCD : IComponentData, IEnableableComponent
    {
    }
    public class LuckyBlockConverter : SingleAuthoringComponentConverter<LuckyBlockAuthoring>
    {
        protected override void Convert(LuckyBlockAuthoring authoring)
        {
            AddComponentData(new LuckyBlockCD());
        }
    }
}
