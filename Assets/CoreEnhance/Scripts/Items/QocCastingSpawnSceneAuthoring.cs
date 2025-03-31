using PugConversion;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Items
{
    [RequireComponent(typeof(CastItemAuthoring))]
    public class QocCastingSpawnSceneAuthoring : MonoBehaviour
    {
        public List<string> SceneNames;
        public bool LimitRange;
        public int MinRadiums;
        public int MaxRadiums;
        public bool Consume;
        public Biome Biome;
    }

    public struct QocCastingSpawnSceneCD : IComponentData
    {
        public NativeList<FixedString32Bytes> SceneNames;
        public bool LimitRange;
        public int MinRadiums;
        public int MaxRadiums;
        public bool Consume;
        public Biome Biome;
    }

    public class QocCastingSpawnSceneConverter : SingleAuthoringComponentConverter<QocCastingSpawnSceneAuthoring>
    {
        protected override void Convert(QocCastingSpawnSceneAuthoring authoring)
        {
            NativeList<FixedString32Bytes> scenes = new(Allocator.Persistent);
            foreach (var scene in authoring.SceneNames)
            {
                scenes.Add(new(scene));
            }
            AddComponentData(new QocCastingSpawnSceneCD()
            {
                SceneNames = scenes,
                LimitRange = authoring.LimitRange,
                MinRadiums = authoring.MinRadiums,
                MaxRadiums = authoring.MaxRadiums,
                Consume = authoring.Consume,
                Biome = authoring.Biome,
            });
        }
    }
}
