using PugConversion;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Items
{
    public class AutoFisherAuthoring : MonoBehaviour
    {
    }

    public struct AutoFisherCD : IComponentData
    {
        public bool init;
        public LootTableID fishes;
        public LootTableID items;
        public Biome biome;
        public float timer;
    }
    public class AutoFisherConverter : SingleAuthoringComponentConverter<AutoFisherAuthoring>
    {
        protected override void Convert(AutoFisherAuthoring authoring)
        {
            AddComponentData(new AutoFisherCD());
            AddComponentData(new RandomCD() { Value = PugRandom.GetRng() });
        }
    }
}
