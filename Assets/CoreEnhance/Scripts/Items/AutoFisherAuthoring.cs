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
        public bool init;
        public int require;
        public int timer;
        public int wait;
        public bool enable;
        public ObjectID rod;
        public LootTableID fishes;
        public LootTableID items;
    }
    public class AutoFisherConverter : SingleAuthoringComponentConverter<AutoFisherAuthoring>
    {
        protected override void Convert(AutoFisherAuthoring authoring)
        {
            AddComponentData(new AutoFisherCD());
            EnsureHasComponent<DistanceToPlayerCD>();
        }
    }
}
