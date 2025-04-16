using PugConversion;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Items
{
    public enum AutoFisherState
    {
        Idle,
        Start,
        Catching,
        End
    }
    public class AutoFisherAuthoring : MonoBehaviour
    {
    }

    [GhostComponent]
    public struct AutoFisherCD : IComponentData
    {
        public int require;
        public int timer;
        public int wait;
        public bool enable;
        public ObjectID rod;
        public LootTableID fishes;
        public LootTableID items;
        public Biome biome;
        public bool biomeIsMatch;

        [GhostField]
        public int rodLevel;

        [GhostField]
        public AutoFisherState state;
        public void CheckRod(ObjectID id, BufferLookup<GivesConditionsWhenEquippedBuffer> lookup,
            BlobAssetReference<PugDatabase.PugDatabaseBank> database)
        {
            rod = id;
            rodLevel = 0;
            var rodEntity = PugDatabase.GetPrimaryPrefabEntity(id, database);
            if (!lookup.TryGetBuffer(rodEntity, out var conditions))
                return;
            foreach (var condition in conditions)
            {
                var c = condition.equipmentCondition;
                if (c.id == ConditionID.IncreasedFishing)
                {
                    rodLevel = c.value;
                    return;
                }
            }
        }
        public readonly bool CheckAllCondition(bool power, bool requireBiomeMatch)
        {
            if (!power)
                return false;
            if (rodLevel < require)
                return false;
            if (requireBiomeMatch && !biomeIsMatch)
                return false;
            return true;
        }
        public void ForceIdle()
        {
            state = AutoFisherState.Idle;
            timer = 0;
        }
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
